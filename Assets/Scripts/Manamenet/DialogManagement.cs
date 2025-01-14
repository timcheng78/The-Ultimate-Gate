using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogManagement : MonoBehaviour
{
    /// <summary>
    /// 遮罩
    /// </summary>
    public GameObject maskCanvas;
    /// <summary>
    /// 半遮罩
    /// </summary>
    public GameObject halfCanvas;
    /// <summary>
    /// 結束顯示區塊
    /// </summary>
    public GameObject endgameCanvas;
    /// <summary>
    /// 結束背景
    /// </summary>
    public GameObject endgameBackground;
    /// <summary>
    /// 結束返回主選單按鈕
    /// </summary>
    public GameObject endgameButton;
    /// <summary>
    /// 結束返回主選單按鈕(Fin)
    /// </summary>
    public GameObject endgameFinButton;
    /// <summary>
    /// 結束遊戲報紙
    /// </summary>
    public GameObject endgameNewspaper;
    /// <summary>
    /// 準心
    /// </summary>
    public GameObject accurateImage;
    /// <summary>
    /// 互動提示
    /// </summary>
    public GameObject interactDialog;
    /// <summary>
    /// 取消提示
    /// </summary>
    public GameObject cancelDialog;
    /// <summary>
    /// 縮放/取消
    /// </summary>
    public GameObject interactHelp1;
    /// <summary>
    /// 移動/旋轉
    /// </summary>
    public GameObject interactHelp2;
    /// <summary>
    /// 上一頁/下一頁
    /// </summary>
    public GameObject interactHelp3;
    /// <summary>
    /// 跳頁
    /// </summary>
    public GameObject interactHelp4;
    /// <summary>
    /// 旋轉提示
    /// </summary>
    public GameObject rotateHelp;
    /// <summary>
    /// 筆記本
    /// </summary>
    public GameObject noteBookCanvas;
    /// <summary>
    /// 開始時的字幕
    /// </summary>
    public GameObject startSubTitleCanvas;
    /// <summary>
    /// 字幕
    /// </summary>
    public GameObject[] helpTexts;
    /// <summary>
    /// 結束文字
    /// </summary>
    public TMP_Text endGameText;
    /// <summary>
    /// 結束回到主選單按鈕
    /// </summary>
    public GameObject endGameMenuButton;
    /// <summary>
    /// 結束繼續按鈕
    /// </summary>
    public GameObject endGameContinueButton;
    /// <summary>
    /// 結束製作人員名單
    /// </summary>
    public GameObject endGameCredits;
    /// <summary>
    /// 結束時間文字
    /// </summary>
    public TMP_Text endTimeText;

    [Header("help text fade speed")]
    [SerializeField] private float _fadeSpeed = .05f;
    [SerializeField] private float _displayTime = 5f;

    [Header("open/close book speed")]
    [SerializeField] private float _bookPositionY = -375f;
    [SerializeField] private float _openSpeed = 1500f;
    [SerializeField] private float _closeSpeed = 1500f;

    [Header("subtitle timers")]
    [SerializeField] private int _randomSentencesTimes = 50;

    private bool _displaying = false;
    private List<int> _playListIndex = new();
    private bool _startTimer = false;
    private float _timer_f = 0f;
    private int _timer_i = 0;

    public bool _animating = false;

    public static DialogManagement Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one DialogManagement in the scene.");
        }
        Instance = this;
    }

    private void Update()
    {
        if (_playListIndex.Count > 0) ReadyToPlayHelpText();
        if (_startTimer)
        {
            _timer_f += Time.deltaTime;
            _timer_i = (int)_timer_f;
        }
        // 當秒數達到會隨機出嘲諷語句
        if (_timer_i % _randomSentencesTimes == 0 && _timer_i != 0)
        {
            SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { "any", "any", "t" });
            _timer_f += 1f;
        }
    }

    public void ToggleMaskCanvas(bool status)
    {
        maskCanvas.SetActive(status);
    }

    public void ToggleHalfCanvas(bool status)
    {
        halfCanvas.SetActive(status);
    }

    public void ToggleEndgameCanvas(string endText)
    {
        endgameCanvas.SetActive(!endgameCanvas.activeSelf);
        endGameText.text = endText;
    }

    public void ToggleEndgameNewspaper(bool status)
    {
        endgameNewspaper.SetActive(status);
    }

    public void ToggleAccurateImage(bool status)
    {
        accurateImage.SetActive(status);
    }

    public void ToggleInteractHelp1(bool status)
    {
        interactHelp1.SetActive(status);
        _startTimer = status;
    }

    public void ToggleInteractHelp2(bool status)
    {
        return;
    }

    public void ToggleInteractHelp3(bool status)
    {
        interactHelp3.SetActive(status);
    }

    public void ToggleInteractHelp4(bool status)
    {
        interactHelp4.SetActive(status);
    }

    public void StartRotateHint()
    {
        rotateHelp.SetActive(true);
        rotateHelp.GetComponent<ArrowIamgeFade>().StartFade();
    }

    public void CloseRotateHint()
    {
        rotateHelp.SetActive(false);
    }

    public void ToggleNoteBookCanvas(bool status)
    {
        if (status)
        {
            // open book
            noteBookCanvas.SetActive(status);
            StartCoroutine(OpenBook());
        }
        else
        {
            // close book
            StartCoroutine(CloseBook());
        }
    }

    public void AddHelpTextToPlay(int[] indexs)
    {
        foreach (int index in indexs)
        {
            _playListIndex.Add(index);
        }
    }

    public void ToggleEndGameButton()
    {
        endGameMenuButton.SetActive(!endGameMenuButton.activeSelf);
        endGameContinueButton.SetActive(!endGameContinueButton.activeSelf);
    }

    private void ReadyToPlayHelpText()
    {
        if (_displaying) return;
        StartCoroutine(PlayHelpText(helpTexts[_playListIndex[0]]));
        _playListIndex.RemoveAt(0);
    }

    IEnumerator PlayHelpText(GameObject helpText)
    {
        _displaying = true;
        CanvasGroup cGroup = startSubTitleCanvas.GetComponent<CanvasGroup>();
        ToggleGameObject(helpText);
        while (1 > cGroup.alpha)
        {
            cGroup.alpha += _fadeSpeed * Time.deltaTime * 60;
            yield return null;
        }
        yield return new WaitForSeconds(_displayTime);
        while (0 < cGroup.alpha)
        {
            cGroup.alpha -= _fadeSpeed * Time.deltaTime * 60;
            yield return null;
        }
        ToggleGameObject(helpText);
        _displaying = false;
    }

    IEnumerator OpenBook()
    {
        _animating = true;
        Book book = noteBookCanvas.GetComponentInChildren<Book>();
        RectTransform rect = book.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, _bookPositionY);
        while (rect.anchoredPosition.y < 0)
        {
            rect.anchoredPosition += new Vector2(0, _openSpeed * Time.deltaTime);

            // 確保不超過目標位置
            if (rect.anchoredPosition.y > 0)
            {
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0);
            }
            yield return null;
        }
        _animating = false;
    }

    IEnumerator CloseBook()
    {
        _animating = true;
        Book book = noteBookCanvas.GetComponentInChildren<Book>();
        RectTransform rect = book.GetComponent<RectTransform>();
        while (rect.anchoredPosition.y > _bookPositionY)
        {
            rect.anchoredPosition -= new Vector2(0, _closeSpeed * Time.deltaTime);

            // 確保不超過目標位置
            if (rect.anchoredPosition.y < _bookPositionY)
            {
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, _bookPositionY);
            }
            yield return null;
        }
        noteBookCanvas.SetActive(false);
        _animating = false;
    }

    private void ToggleGameObject(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
