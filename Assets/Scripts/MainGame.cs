using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MainGame : MonoBehaviour
{
    [SerializeField] private GameObject _startObject;
    [SerializeField] private GameObject _secondObject;
    [SerializeField] private GameObject _maskCanvas;
    [SerializeField] private GameObject _dialogCanvas;
    [SerializeField] private GameObject _step1Hint;
    [SerializeField] private GameObject _step2Hint;
    [SerializeField] private GameObject _secondPhaseLookatObject;
    [SerializeField] private bool _skip;
    [SerializeField] private AudioClip _tutorialPassSoundFX;
    [SerializeField] private Light _SceneSpot;
    [SerializeField] private GameObject _outsideView;

    [Header("BGM Fade Speed")]
    [SerializeField] private float _loudSpeed = 0.01f;
    [SerializeField] private float _silenceSpeed = 0.01f;

    [Header("Demo use")]
    [SerializeField] private GameObject _shopButton;

    [Header("End Game Setting")]
    [SerializeField] private AnimationCamera _endGameCamera1;
    [SerializeField] private AnimationCamera _endGameCamera2;
    [SerializeField] private AnimationCamera _endGameCamera3;
    [SerializeField] private AnimationCamera _endGameCamera4;
    [SerializeField] private Animator _endAnimator1;
    [SerializeField] private Animator _endAnimator2;
    [SerializeField] private WordType _endDialogWord;
    [SerializeField] private AudioClip _normalEndClip;
    [SerializeField] private AudioSource _truthEndClip;
    [SerializeField] private AudioSource _outDoorEndAudio;
    [SerializeField] private float _endWaitTime = 0.0f;
    [SerializeField] private ObjectFlashEffect _flashEffect;
    [SerializeField] private GameObject[] _truthEndHideObjects;
    [SerializeField] private GameObject _truthEndGameObjectShow;
    [SerializeField] private GameObject _truthEndGameObjectHide;
    [SerializeField] private Animator _truthEndExitDoorAnimator;
    [SerializeField] private PlayableDirector _normalEndDirector;
    [SerializeField] private PlayableDirector _truthEndDirector;
    [SerializeField] private PlayableDirector _crazyEndDirector;

    [Header("Step 1")]
    [SerializeField] private GameObject _step1Object;

    [Header("Step 2")]
    [SerializeField] private GameObject _step2Object;

    private Animator _animator;
    private AudioSource _BGM;
    private CameraAttributes _cameraAttributes;
    private int _endType = 0;
    private int _callback = 0;
    private int _step = 0;
    private int _endGameCallback = 0;
    private int _endGameCallbackCount = 0;
    private int _truthEndCount = 8;
    private int _normalEndCount = 0;
    private int _crazyEndCount = 0;

    [Header("debug")]
    [SerializeField] private string _useTest = "";

    public static MainGame Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one MainGame in the scene.");
        }
        Instance = this;
        _cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        _BGM = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (DataPersistenceManagement.Instance == null)
        {
            // debug mode
            _skip = true;
            StartGame(_useTest);
        }
    }

    private void StartGame(string type)
    {
        switch (type)
        {
            case "demo":
                {
                    if (_skip)
                    {
                        StartDemo();
                    }
                    else
                    {
                        _maskCanvas.SetActive(true);
                        StartCoroutine(SubtitleManagement.Instance.SubWordType.PlayStoryPlot("Starting Plot Table", () => StartDemo()));
                        StartCoroutine(CloseMask());
                    }

                    break;
                }
            case "main":
                {
                    CameraManagement.Instance.animationVirtualCamera1.enabled = false;
                    CameraManagement.Instance.TogglePlayerVirtualCamera(true);
                    PlayerAttributes.Instance._height = new Vector3(0.65f, 0.9f, 0.65f);
                    PlayerAttributes.Instance.SetHeight();
                    PlayerController.Instance.SetPlayerStartPosition();
                    StartCoroutine(WaitToStart());
                    break;
                }
            default:
                break;
        }
    }

    public void PreloadGameSetting()
    {
        if (DataPersistenceManagement.Instance.IsNewGame) return;
        PlayerAttributes.Instance._location = DataPersistenceManagement.Instance.GameDatas.playerData.location;
        SharedUtils.SwitchGameScenes(_step1Object, _step2Object);
        if (_step2Object.activeSelf) _outsideView.SetActive(true);
        DataPersistenceManagement.Instance.ReloadDataPersistenceObjects();
        DataPersistenceManagement.Instance.LoadGame();
        DataPersistenceManagement.Instance.ResetLoadFile();
        CameraManagement.Instance.animationVirtualCamera1.enabled = false;
        CameraManagement.Instance.TogglePlayerVirtualCamera(true);
        Camera.main.GetComponent<CameraAttributes>().ZeroCameraSpeed();
        
    }

    public void StartGame()
    {
        // Play Animation
        if (DataPersistenceManagement.Instance.IsNewGame)
        {
            if (Enviroment.Instance.SkipTutorial) StartGame("main");
            else StartGame("demo");
        }
        else
        {
            StartDemo(DataPersistenceManagement.Instance.IsNewGame);
        }
    }

    private void PlayStartMovie(string animationName, int index = 1)
    {
        if (index.Equals(1))
            _animator = _startObject.GetComponent<Animator>();
        else if (index.Equals(2))
            _animator = _secondObject.GetComponent<Animator>();
        _animator.Play(animationName, -1, 0f);
    }

    private void StartDemo(bool isNewGame = true)
    {
        // start animation end
        CameraManagement.Instance.animationVirtualCamera1.enabled = false;
        CameraManagement.Instance.TogglePlayerVirtualCamera(true);
        Camera.main.GetComponent<CameraAttributes>().ZeroCameraSpeed();
        StartCoroutine(WaitToStart());
        if (isNewGame) DialogManagement.Instance.AddHelpTextToPlay(new int[] { 0, 1, 2 });
    }

    private IEnumerator StartMask(Action nextFunction = null, float maskSpeed = 5)
    {
        RectTransform rTransform = _maskCanvas.transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform mTransform = _maskCanvas.GetComponent<RectTransform>();
        float percent = 0;
        rTransform.sizeDelta = new Vector2(rTransform.sizeDelta.x, 0);
        while (((mTransform.sizeDelta.y * 2) - rTransform.sizeDelta.y) > 0)
        {
            rTransform.sizeDelta = new Vector2(rTransform.sizeDelta.x, percent);
            yield return null;
            percent += maskSpeed * Time.deltaTime * 60;
            if (percent > 500) PlayerMovement.Instance._footStepSoundFX.Pause();
        }
        nextFunction?.Invoke();
    }

    private IEnumerator CloseMask()
    {
        RectTransform rTransform = _maskCanvas.transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform mTransform = _maskCanvas.GetComponent<RectTransform>();
        float percent = mTransform.sizeDelta.y * 2;
        rTransform.sizeDelta = new Vector2(rTransform.sizeDelta.x, percent);
        yield return new WaitForSeconds(2);
        while (rTransform.sizeDelta.y > 0)
        {
            rTransform.sizeDelta = new Vector2(rTransform.sizeDelta.x, percent);
            yield return null;
            percent -= 15 * Time.deltaTime * 60;
        }
        _maskCanvas.SetActive(false);
        PlayStartMovie("start_animation");
    }

    private void PlayStory()
    {
        CameraManagement.Instance.animationVirtualCamera2.enabled = false;
        CameraManagement.Instance.TogglePlayerVirtualCamera(true);
        if (_skip) NextGame();
        else StartCoroutine(SubtitleManagement.Instance.SubWordType.PlayStoryPlot("Second Plot Table", () => NextGame(), new List<int>() { 0, 1, 2, 3, 4, 5, 6 }));
    }

    private void NextGame()
    {
        PlayerAttributes.Instance._height = new Vector3(0.65f, 0.9f, 0.65f);
        PlayerAttributes.Instance.SetHeight();
        Camera.main.GetComponent<CameraAttributes>().ZeroCameraSpeed();
        StartCoroutine(WaitToStart());
        SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { "book_room", "entrance", "t" });
        SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_BOOK_ROOM_COUNT_SEC, Timer.Instance.GetNowSec());
    }

    private IEnumerator WaitToStart()
    {
        yield return new WaitForSeconds(1);
        SharedUtils.SwitchGameScenes(_step1Object, _step2Object);
        StartCoroutine(Camera.main.GetComponent<CameraAttributes>().RestoreCameraSpeed());
        DialogManagement.Instance.ToggleAccurateImage(true);
        PlayerMovement.Instance.ToggleMove(true);
        Enviroment.Instance.IsStartPlay = true;
        _maskCanvas.SetActive(false);
        _cameraAttributes._screenLock = false;
        _cameraAttributes._cursorVisable = false;
        _cameraAttributes._cursorLockMode = CursorLockMode.Locked;
        PlayStartMovie("Empty");
        _BGM.Play();
        StartCoroutine(PlayBGMFade());
    }

    public IEnumerator PlayBGMFade()
    {
        while (_BGM.volume < 1)
        {
            _BGM.volume += _loudSpeed;
            yield return null;
        }
    }

    public IEnumerator StopBGMFade()
    {
        while (_BGM.volume > 0)
        {
            _BGM.volume -= _silenceSpeed;
            yield return new WaitForEndOfFrame();
        }
        _BGM.Stop();
    }

    public void DemoEnd()
    {
        SharedUtils.GameOver();
        CameraManagement.Instance.demoEndVirtualCamera.enabled = true;
        SoundManagement.Instance.PlaySoundFXClip(_tutorialPassSoundFX, Enviroment.Instance.Player.transform, 1f);
        StartCoroutine(StopBGMFade());
        StartCoroutine(GameOver());
    }

    public void TutorialEnd()
    {
        _outsideView.SetActive(true);
        SharedUtils.GameOver();
        CameraManagement.Instance.demoEndVirtualCamera.enabled = true;
        SoundManagement.Instance.PlaySoundFXClip(_tutorialPassSoundFX, Enviroment.Instance.Player.transform, 1f);
        StartCoroutine(StopBGMFade());
        StartCoroutine(WaitForOneSec());
    }

    public void OutDoorEnd()
    {
        SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_NORMAL_END);
        SubtitleManagement.Instance.SpeedUpSentences();
        SharedUtils.GameOver(true);
        SharedUtils.SaveNormalEnd();
        _endType = 1;
        _endGameCallbackCount = 2;
        _step = 1;
        StartCoroutine(StopBGMFade());
        SoundManagement.Instance.PlaySoundFXClip(_normalEndClip, Enviroment.Instance.Player.transform, 1f);
        //_outDoorEndAudio.Play();
        _endGameCamera1.GetComponent<CinemachineVirtualCamera>().enabled = true;
        //_endGameCamera1.StartToPlay(NormalEndAnimation);
        //_endGameCamera3.type = 1;
        //_endAnimator1.Play("Step4", -1, 0f);
        _normalEndDirector.Play();
        _normalEndDirector.stopped += _normalEndDirector_played;
        StartCoroutine(SubtitleManagement.Instance.SubWordType.PlayStoryPlot("Truth End Plot Table", EndGameAnimationEnded, new List<int>() { 3, 4, 5, 6, 7, 8 }, true));
        StartCoroutine(EngGamePhotoShot());
    }

    private void _normalEndDirector_played(PlayableDirector obj)
    {
        Debug.Log("Normal The end");
    }

    public void DirectorPause(PlayableDirector playable)
    {
        playable.Pause();

        SharedUtils.WaitingForSec(2.0f, () => playable.Play());
    }

    public void ExitDoorEnd()
    {
        SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_TRUE_ENDING);
        foreach (GameObject gameObject in _truthEndHideObjects)
        {
            gameObject.SetActive(false);
        }
        SubtitleManagement.Instance.SpeedUpSentences();
        SharedUtils.GameOver(true);
        CameraManagement.Instance.TruthEndCameraSwitch();
        _endType = 2;
        _endGameCallbackCount = 2;
        StartCoroutine(StopBGMFade());
        _truthEndClip.Play();
        _truthEndDirector.Play();
        _truthEndDirector.stopped += _truthEndDirector_stopped;
        SubtitleManagement.Instance.SubWordType.TextSpeed = .15f;
        SubtitleManagement.Instance.SubWordType.SoundFX.pitch = .7f;
        StartCoroutine(SubtitleManagement.Instance.SubWordType.PlayStoryPlot("Truth End Plot Table", EndGameAnimationEnded, new List<int>() { 0, 1, 2 }));
    }
    public void _truthEndDirector_stopped(PlayableDirector obj)
    {
        Debug.Log("Truth The end");
        CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        cameraAttributes._cursorVisable = true;
        cameraAttributes._cursorLockMode = CursorLockMode.Confined;
        DialogManagement.Instance.endgameCanvas.SetActive(true);
        DialogManagement.Instance.endgameBackground.SetActive(false);
        DialogManagement.Instance.endgameButton.SetActive(false);
        DialogManagement.Instance.endgameFinButton.SetActive(true);
        DialogManagement.Instance.endTimeText.gameObject.SetActive(false);
    }

    IEnumerator EngGamePhotoShot()
    {
        int count = 0;
        yield return new WaitForSeconds(2);
        while (count < 13)
        {
            _flashEffect.TriggerFlicker(); ;
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 2f));
            count++;
        }
    } 

    public void NormalEndAnimation()
    {
        SubtitleManagement.Instance.SubWordType.GetComponent<RectTransform>().offsetMax = new Vector2(-800f, 0);
        SubtitleManagement.Instance.SubWordType.TextSpeed = .3f;
        SubtitleManagement.Instance.SubWordType.SoundFX.pitch = .6f;
        SubtitleManagement.Instance.SubWordType.GetComponent<TMP_Text>().color = Color.white;
        SubtitleManagement.Instance.ToggleActiveCanvas(true);
        StartCoroutine(SubtitleManagement.Instance.SubWordType.PlayStoryPlot("Normal End Credit Table", null, new List<int>() { _normalEndCount++ }));
    }

    private void NormalNextStage()
    {
        StartCoroutine(LaterEndWait());
    }

    public void TruthEndAnimation()
    {
        if (_truthEndCount == 21)
        {
            SubtitleManagement.Instance.ToggleActiveCanvas(false);
            return;
        }
        if (_truthEndCount > 15)
        {
            SubtitleManagement.Instance.SubWordType.GetComponent<RectTransform>().offsetMax = new Vector2(-800f, 0);
            SubtitleManagement.Instance.SubWordType.TextSpeed = .3f;
            SubtitleManagement.Instance.SubWordType.SoundFX.pitch = .6f;
        } 
        else if (_truthEndCount > 11)
        {
            SubtitleManagement.Instance.SubWordType.GetComponent<TMP_Text>().color = Color.white;
        }
        else
        {
            SubtitleManagement.Instance.SubWordType.GetComponent<TMP_Text>().color = Color.red;
        }
        SubtitleManagement.Instance.ToggleActiveCanvas(true);
        StartCoroutine(SubtitleManagement.Instance.SubWordType.PlayStoryPlot("Truth End Plot Table", null, new List<int>() { ++_truthEndCount }));
    }

    public void CrazyEndAnimation()
    {
        if (_crazyEndCount < 16)
        {
            SubtitleManagement.Instance.SubWordType.GetComponent<TMP_Text>().color = Color.white;
        }
        else
        {
            SubtitleManagement.Instance.SubWordType.GetComponent<TMP_Text>().color = Color.red;
            SubtitleManagement.Instance.SubWordType.SentencesSpeed = 2.5f;
        }
        SubtitleManagement.Instance.SubWordType.GetComponent<RectTransform>().offsetMax = new Vector2(0, -800f);
        SubtitleManagement.Instance.SubWordType.TextSpeed = .15f;
        SubtitleManagement.Instance.SubWordType.SoundFX.pitch = .7f;
        SubtitleManagement.Instance.ToggleActiveCanvas(true);
        StartCoroutine(SubtitleManagement.Instance.SubWordType.PlayStoryPlot("Crazy End Plot Table", null, new List<int>() { _crazyEndCount++ }));
    }

    public void NormalEndStopped()
    {
        Debug.Log("Normal The end");
        CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        cameraAttributes._cursorVisable = true;
        cameraAttributes._cursorLockMode = CursorLockMode.Confined;
        DialogManagement.Instance.ToggleEndgameCanvas("");
        DialogManagement.Instance.endgameCanvas.SetActive(true);
        DialogManagement.Instance.endgameBackground.SetActive(true);
        DialogManagement.Instance.endgameButton.SetActive(true);
        DialogManagement.Instance.endTimeText.gameObject.SetActive(false);
        _outDoorEndAudio.Play();
    }

    public void CrazyEndStopped()
    {
        Debug.Log("Crazy The end");
        CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        cameraAttributes._cursorVisable = true;
        cameraAttributes._cursorLockMode = CursorLockMode.Confined;
        var localString = LocalizationSettings.StringDatabase.GetTableEntry("Ending Plot Table", "crazy_ending_plot.1");
        DialogManagement.Instance.endGameText.color = Color.red;
        DialogManagement.Instance.endGameText.fontSize = 48;
        DialogManagement.Instance.endgameBackground.GetComponent<Image>().color = Color.black;
        DialogManagement.Instance.ToggleEndgameCanvas(localString.Entry.GetLocalizedString());
        DialogManagement.Instance.endgameCanvas.SetActive(true);
        DialogManagement.Instance.endgameBackground.SetActive(true);
        DialogManagement.Instance.endgameButton.SetActive(true);
        DialogManagement.Instance.endTimeText.gameObject.SetActive(false);
    }

    public void TruthNewsPaper()
    {
        _truthEndDirector.Pause();
        ShowNewPaper();
    }

    private void TruthNextStage()
    {
        StartCoroutine(SharedUtils.WaitingForSec(3f, ShowNewPaper));
        SubtitleManagement.Instance.ToggleActiveCanvas(false);
        _truthEndGameObjectHide.SetActive(false);
        _truthEndGameObjectShow.SetActive(true);
    }

    private void ShowNewPaper()
    {
        CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        cameraAttributes._cursorVisable = true;
        cameraAttributes._cursorLockMode = CursorLockMode.Confined;
        DialogManagement.Instance.ToggleEndgameCanvas("");
        DialogManagement.Instance.ToggleEndGameButton();
        DialogManagement.Instance.ToggleEndgameNewspaper(true);
        DialogManagement.Instance.endTimeText.gameObject.SetActive(false);
    }

    private void TruthNextStage2()
    {
        CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        cameraAttributes._cursorVisable = false;
        cameraAttributes._cursorLockMode = CursorLockMode.Locked;
        DialogManagement.Instance.ToggleEndgameCanvas("");
        // open the door
        _endGameCamera3.GetComponent<CinemachineVirtualCamera>().enabled = false;
        _endGameCamera4.GetComponent<CinemachineVirtualCamera>().enabled = true;
        StartCoroutine(SharedUtils.WaitingForSec(2f, TruthNextStage3));
    }

    private void TruthNextStage3()
    {
        _endGameCamera4.StartToPlay(NormalNextStage);
        StartCoroutine(SharedUtils.WaitingForSec(5f, TruthEndOpenDoor));
    }

    private void TruthEndOpenDoor()
    {
        _truthEndExitDoorAnimator.Play("Exit Door Open");
    }

    private IEnumerator LaterEndWait()
    {
        yield return new WaitForSeconds(3);
        EndGameAnimationEnded();
    }

    private void EndGameAnimationEnded()
    {
        if (++_endGameCallback < _endGameCallbackCount) return;
        _maskCanvas.SetActive(true);
        CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        cameraAttributes._cursorVisable = true;
        cameraAttributes._cursorLockMode = CursorLockMode.Confined;
        StartCoroutine(StartMask(BackToMenu, 20));
    }

    private void EndGameSubtitle()
    {
        if (_endType.Equals(1))
        {
            SubtitleManagement.Instance.ToggleActiveCanvas(true);
            _endWaitTime = 4.0f;
            StartCoroutine(SubtitleManagement.Instance.SubWordType.ShowSentences(LocalizationSettings.StringDatabase.GetTableEntry("Ending Plot Table", "normal_ending_plot.1").Entry.GetLocalizedString().Split(" "), EndGameShowPaper, null, true));
        }
        else if (_endType.Equals(2))
        {
            SubtitleManagement.Instance.ToggleActiveCanvas(true);
            _endWaitTime = 4.0f;
            SubtitleManagement.Instance.SubWordType.TextSpeed = .3f;
            SubtitleManagement.Instance.SubWordType.SoundFX.pitch = .6f;
            StartCoroutine(SubtitleManagement.Instance.SubWordType.ShowSentences(LocalizationSettings.StringDatabase.GetTableEntry("Ending Plot Table", "truth_ending_plot.1").Entry.GetLocalizedString().Split(" "), EndGameShowPaper));
        }
        
    }

    private void EndGameShowPaper(string empty = null)
    {
        StartCoroutine(EndGameWaitSec());
    }

    IEnumerator EndGameWaitSec()
    {
        yield return new WaitForSeconds(_endWaitTime);
        SubtitleManagement.Instance.ToggleActiveCanvas(false);
        SubtitleManagement.Instance.SubWordType.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        if (_endType == 1) TheEnd();
    }

    public void CrazyEnd()
    {
        _crazyEndDirector.Play();
        _crazyEndDirector.stopped += _normalEndDirector_played;
    }

    private void TheEnd()
    {
        string tempText = "";
        string localizaedString = LocalizationSettings.StringDatabase.GetTableEntry("Ending Plot Table", "normal_ending_plot.1").Entry.GetLocalizedString();
        if (_endType.Equals(2)) localizaedString = LocalizationSettings.StringDatabase.GetTableEntry("Ending Plot Table", "truth_ending_plot.1").Entry.GetLocalizedString();

        // Eng
        if (LocalizationSettings.AvailableLocales.Locales[0].Equals(LocalizationSettings.SelectedLocale))
        {
            tempText = localizaedString;
        }
        else
        {
            tempText = String.Join("", localizaedString.Split(" "));
        }

        DialogManagement.Instance.endGameText.fontSize = 42;
        DialogManagement.Instance.ToggleEndgameCanvas(tempText);
        SubtitleManagement.Instance.ToggleActiveCanvas(false);
        CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        cameraAttributes._cursorVisable = true;
        cameraAttributes._cursorLockMode = CursorLockMode.Confined;
        DialogManagement.Instance.ToggleEndGameButton();
        if (_endType.Equals(1))
        {
            SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_NORMAL_END);
            SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_NORMAL_END_COUNT_SEC, Timer.Instance.GetNowSec());
        }
        else if (_endType.Equals(2))
        {
            SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_TRUE_ENDING);
            SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_TRUTH_END_COUNT_SEC, Timer.Instance.GetNowSec());
        }
        DialogManagement.Instance.endTimeText.SetText(Timer.Instance.GetFormatTime());
    }

    IEnumerator GameOver()
    {
        SubtitleManagement.Instance.SpeedUpSentences();
        yield return new WaitForSeconds(5);
        SubtitleManagement.Instance.AddSentencesToShow("Resolve Puzzle Statement Table", new string[] { "demo", "door", "t" });
        yield return new WaitForSeconds(2);
        Enviroment.Instance.IsStartPlay = false;
        SubtitleManagement.Instance.SpeedUpSentences();
        yield return new WaitForSeconds(3);
        // End Game
        var localString = LocalizationSettings.StringDatabase.GetTableEntry("String Table", "DemoEndText");
        DialogManagement.Instance.ToggleEndgameCanvas(localString.Entry.GetLocalizedString());

        _cameraAttributes._screenLock = true;
        _cameraAttributes._cursorVisable = true;
        _cameraAttributes._cursorLockMode = CursorLockMode.Confined;
        _shopButton.SetActive(true);
    }

    private IEnumerator WaitForOneSec()
    {
        SubtitleManagement.Instance.SpeedUpSentences();
        yield return new WaitForSeconds(5);
        Enviroment.Instance.IsStartPlay = false;
        CameraManagement.Instance.demoEndVirtualCamera.enabled = false;
        CameraManagement.Instance.animationVirtualCamera2.enabled = true;
        _SceneSpot.enabled = true;
        yield return new WaitForSeconds(1);
        PlayStartMovie("step2_animation", 2);
        yield return new WaitForSeconds(1);
        DialogManagement.Instance.ToggleAccurateImage(false);
        CameraManagement.Instance.animationVirtualCamera2.GetComponent<AnimationCamera>().StartToPlay(EnterSecondPhase);
        yield return new WaitForSeconds(5);
        StartCoroutine(SubtitleManagement.Instance.SubWordType.PlayStoryPlot("Second Plot Table", EnterSecondPhase, new List<int>() { 7, 8 }));
        PlayerController.Instance.SetPlayerStartPosition();
    }

    private void EnterSecondPhase()
    {
        if (++_callback < 2) return;
        _SceneSpot.enabled = false;
        _maskCanvas.SetActive(true);
        StartCoroutine(StartMask(PlayStory, 20));
        PlayerAttributes.Instance._location = "book_room";
    }

    public void BackToMenu()
    {
        
        DataPersistenceManagement.Instance.DeleteGame();
        DataPersistenceManagement.Instance.HasData = false;
        DataPersistenceManagement.Instance.BackToMainMenu();
    }

    public void ShowCredit()
    {
        if (_step.Equals(0))
        {
            CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
            cameraAttributes._cursorVisable = false;
            cameraAttributes._cursorLockMode = CursorLockMode.Locked;
            DialogManagement.Instance.ToggleEndgameCanvas("");
            DialogManagement.Instance.ToggleEndGameButton();
            DialogManagement.Instance.ToggleEndgameNewspaper(false);
            _truthEndDirector.Resume();
        }
        else if (_step.Equals(1))
        {
            if (!(_truthEndClip.isPlaying || _outDoorEndAudio.isPlaying)) _outDoorEndAudio.Play();
            DialogManagement.Instance.endGameText.SetText("");
            DialogManagement.Instance.ToggleEndgameNewspaper(false);
            DialogManagement.Instance.ToggleEndGameButton();
            DialogManagement.Instance.endGameCredits.SetActive(true);
            DialogManagement.Instance.endTimeText.gameObject.SetActive(false);
            StartCoroutine(CreditsMove());
        }
    }

    IEnumerator CreditsMove()
    {
        RectTransform trans = DialogManagement.Instance.endGameCredits.GetComponent<RectTransform>();
        while (trans.anchoredPosition.y < 1110)
        {
            Vector2 movement = new Vector2(0, 3).normalized;
            trans.anchoredPosition += movement * Time.deltaTime * 60;
            yield return null;
        }
    }
}
