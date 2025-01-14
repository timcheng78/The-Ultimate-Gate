using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogManagement : MonoBehaviour
{
    /// <summary>
    /// �B�n
    /// </summary>
    public GameObject maskCanvas;
    /// <summary>
    /// �b�B�n
    /// </summary>
    public GameObject halfCanvas;
    /// <summary>
    /// ������ܰ϶�
    /// </summary>
    public GameObject endgameCanvas;
    /// <summary>
    /// �����I��
    /// </summary>
    public GameObject endgameBackground;
    /// <summary>
    /// ������^�D�����s
    /// </summary>
    public GameObject endgameButton;
    /// <summary>
    /// ������^�D�����s(Fin)
    /// </summary>
    public GameObject endgameFinButton;
    /// <summary>
    /// �����C������
    /// </summary>
    public GameObject endgameNewspaper;
    /// <summary>
    /// �Ǥ�
    /// </summary>
    public GameObject accurateImage;
    /// <summary>
    /// ���ʴ���
    /// </summary>
    public GameObject interactDialog;
    /// <summary>
    /// ��������
    /// </summary>
    public GameObject cancelDialog;
    /// <summary>
    /// �Y��/����
    /// </summary>
    public GameObject interactHelp1;
    /// <summary>
    /// ����/����
    /// </summary>
    public GameObject interactHelp2;
    /// <summary>
    /// �W�@��/�U�@��
    /// </summary>
    public GameObject interactHelp3;
    /// <summary>
    /// ����
    /// </summary>
    public GameObject interactHelp4;
    /// <summary>
    /// ���ണ��
    /// </summary>
    public GameObject rotateHelp;
    /// <summary>
    /// ���O��
    /// </summary>
    public GameObject noteBookCanvas;
    /// <summary>
    /// �}�l�ɪ��r��
    /// </summary>
    public GameObject startSubTitleCanvas;
    /// <summary>
    /// �r��
    /// </summary>
    public GameObject[] helpTexts;
    /// <summary>
    /// ������r
    /// </summary>
    public TMP_Text endGameText;
    /// <summary>
    /// �����^��D�����s
    /// </summary>
    public GameObject endGameMenuButton;
    /// <summary>
    /// �����~����s
    /// </summary>
    public GameObject endGameContinueButton;
    /// <summary>
    /// �����s�@�H���W��
    /// </summary>
    public GameObject endGameCredits;
    /// <summary>
    /// �����ɶ���r
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
        // ���ƹF��|�H���X�J�ػy�y
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

            // �T�O���W�L�ؼЦ�m
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

            // �T�O���W�L�ؼЦ�m
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
