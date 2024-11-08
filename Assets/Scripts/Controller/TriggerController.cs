using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class TriggerController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [SerializeField] private Animator _animator;
    [SerializeField] private Animator _yogAnimator;
    [SerializeField] private GameObject _yog;
    [SerializeField] private GameObject[] _needHide;
    [SerializeField] private AudioClip _crazyClip;
    [SerializeField] private Material _newMaterial;
    [SerializeField] private Renderer _scenes;

    private AudioSource _endGameBGM;

    private void Awake()
    {
        TryGetComponent<AudioSource>(out _endGameBGM);
    }
    private void OnTriggerEnter()
    {
        _yog.SetActive(true);
        SubtitleManagement.Instance.SpeedUpSentences();
        // GameOver - µ²§½(ºÆ)
        SharedUtils.GameOver(true, true);
        //SoundManagement.Instance.PlaySoundFXClip(_crazyClip, Enviroment.Instance.Player.transform, 1f);
        StartCoroutine(MainGame.Instance.StopBGMFade());
        if (_endGameBGM) _endGameBGM.Play();
        _virtualCamera.enabled = true;
        AnimationCamera animationCamera = _virtualCamera.GetComponent<AnimationCamera>();
        _animator.Play("Step3", -1, 0f);
        _yogAnimator.Play("Yog", -1, 0f);
        _scenes.material = _newMaterial;
        foreach (GameObject gameObject in _needHide)
        {
            gameObject.SetActive(false);
        }
        animationCamera.StartToPlay(AnimationEnd);
        SubtitleManagement.Instance.SubWordType.TextSpeed = .3f;
        SubtitleManagement.Instance.SubWordType.SentencesSpeed = 1f;
        SubtitleManagement.Instance.SubWordType.SoundFX.pitch = .8f;
        SubtitleManagement.Instance.SubWordType.TextArea.color = Color.red;
        StartCoroutine(SubtitleManagement.Instance.SubWordType.PlayStoryPlot("Crazy End Plot Table"));
    }

    private void AnimationEnd()
    {
        StartCoroutine(WatingSecond());
    }
    
    private IEnumerator WatingSecond()
    {
        DialogManagement.Instance.endgameCanvas.transform.GetChild(0).GetComponent<Image>().color = new Color(0, 0, 0, 1);
        yield return new WaitForSeconds(3);
        if (_endGameBGM) _endGameBGM.Stop();
        var localString = LocalizationSettings.StringDatabase.GetTableEntry("Ending Plot Table", "crazy_ending_plot.1");
        DialogManagement.Instance.endGameText.color = Color.red;
        DialogManagement.Instance.endGameText.fontSize = 48;
        DialogManagement.Instance.ToggleEndgameCanvas(localString.Entry.GetLocalizedString());
        CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        cameraAttributes._cursorVisable = true;
        cameraAttributes._cursorLockMode = CursorLockMode.Confined;
        SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_CRAZY_ENDING);
        SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_CRAZY_END_COUNT_SEC, Timer.Instance.GetNowSec());
        DialogManagement.Instance.endTimeText.SetText(Timer.Instance.GetFormatTime());
    }
}
