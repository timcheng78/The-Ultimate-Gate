using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

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
    [SerializeField] private Animator _endAnimator1;
    [SerializeField] private Animator _endAnimator2;
    [SerializeField] private WordType _endDialogWord;
    [SerializeField] private Light[] _lights;
    [SerializeField] private float _lightSpeed = .5f;
    [SerializeField] private AudioClip _truthEndClip;
    [SerializeField] private AudioSource _outDoorEndAudio;

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
        SubtitleManagement.Instance.SpeedUpSentences();
        SharedUtils.GameOver(true);
        _endType = 1;
        _endGameCallbackCount = 1;
        StartCoroutine(StopBGMFade());
        _outDoorEndAudio.Play();
        _endGameCamera1.GetComponent<CinemachineVirtualCamera>().enabled = true;
        _endGameCamera1.StartToPlay(EndGameAnimationEnded);
        _endAnimator1.Play("Step4", -1, 0f);
        StartCoroutine(EndGameLightOpen());
    }

    public void ExitDoorEnd()
    {
        SubtitleManagement.Instance.SpeedUpSentences();
        SharedUtils.GameOver(true);
        _endType = 2;
        _endGameCallbackCount = 2;
        StartCoroutine(StopBGMFade());
        SoundManagement.Instance.PlaySoundFXClip(_truthEndClip, Enviroment.Instance.Player.transform, 1f);
        _endGameCamera2.GetComponent<CinemachineVirtualCamera>().enabled = true;
        _endGameCamera2.StartToPlay(EndGameAnimationEnded);
        SubtitleManagement.Instance.SubWordType.TextSpeed = .15f;
        SubtitleManagement.Instance.SubWordType.SoundFX.pitch = .7f;
        StartCoroutine(SubtitleManagement.Instance.SubWordType.PlayStoryPlot("Truth End Plot Table", EndGameAnimationEnded));
        _endAnimator2.Play("Step5", -1, 0f);
    }

    IEnumerator EndGameLightOpen()
    {
        float intensity = 0.0f;
        while (intensity < 15)
        {
            foreach (Light light in _lights)
            {
                light.enabled = true;
                light.intensity = intensity;
            }
            yield return null;
            intensity += _lightSpeed * Time.deltaTime * 60; ;
        }
    }

    private void EndGameAnimationEnded()
    {
        if (++_endGameCallback < _endGameCallbackCount) return;
        _maskCanvas.SetActive(true);
        StartCoroutine(StartMask(EndGameSubtitle, 20));
    }

    private void EndGameSubtitle()
    {
        if (_endType.Equals(1))
        {
            SubtitleManagement.Instance.ToggleActiveCanvas(true);
            SubtitleManagement.Instance.SubWordType.TextSpeed = .3f;
            SubtitleManagement.Instance.SubWordType.SoundFX.pitch = .6f;
            StartCoroutine(SubtitleManagement.Instance.SubWordType.ShowSentences(LocalizationSettings.StringDatabase.GetTableEntry("Ending Plot Table", "normal_ending_plot.1").Entry.GetLocalizedString().Split(" "), EndGameShowPaper));
        }
        else if (_endType.Equals(2))
        {
            SubtitleManagement.Instance.ToggleActiveCanvas(true);
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
        yield return new WaitForSeconds(4);
        //if (_endType.Equals(2)) DialogManagement.Instance.ToggleEndgameNewspaper(true);
        TheEnd();
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
        if (_endType.Equals(2)) DialogManagement.Instance.ToggleEndGameButton();
        if (_endType.Equals(1))
        {
            if (_outDoorEndAudio) _outDoorEndAudio.Stop();
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
            DialogManagement.Instance.endGameText.SetText("");
            DialogManagement.Instance.ToggleEndgameNewspaper(true);
            _step++;
        }
        else if (_step.Equals(1))
        {
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
