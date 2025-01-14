using Coffee.UIExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class WordType : MonoBehaviour
{
    [SerializeField] private float _textSpeed = .2f;
    [SerializeField] private float _sentencesSpeed = 3f;
    [SerializeField] private float _fadeSpeed = 0.4f;
    [SerializeField] private float _speedUp = 0.4f;
    [SerializeField] private TMP_FontAsset _asset;
    private TMP_Text _textArea;
    private AudioSource _soundFX;
    private string _text;
    private bool _isTyping = false;
    private UIDissolve dissolveEffect;
    private Locale _english;
    private Locale _jap;

    public float TextSpeed { set => _textSpeed = value; }
    public float SentencesSpeed { set => _sentencesSpeed = value; }
    public AudioSource SoundFX { get => _soundFX; }
    public TMP_Text TextArea { get => _textArea; }

    private void Awake()
    {
        _textArea = GetComponent<TMP_Text>();
        _soundFX = GetComponent<AudioSource>();
        dissolveEffect = GetComponent<UIDissolve>();
    }

    private void Start()
    {
        _english = LocalizationSettings.AvailableLocales.Locales[0];
        _jap = LocalizationSettings.AvailableLocales.Locales[1];
    }

    public bool IsTyping { get => _isTyping; }

    private void Update()
    {
        if (Enviroment.Instance.IsPause) _soundFX.Pause();
        else if (!Enviroment.Instance.IsPause && _isTyping && !_soundFX.isPlaying) _soundFX.Play();
        CheckFontAsset();
        if (dissolveEffect == null) return;
        _textArea.material.SetFloat("_OutlineWidth", 0.2f);
        if (!dissolveEffect.enabled) return;
        dissolveEffect.effectFactor += _fadeSpeed * Time.deltaTime;
        if (dissolveEffect.effectFactor.Equals(1.0f))
        {
            _text = "";
            _textArea.SetText(_text);
            dissolveEffect.effectFactor = 0.0f;
            Destroy(gameObject);
        }
    }

    public void ChangeFadeSpeed(float newSpeed)
    {
        _fadeSpeed = newSpeed;
    }

    public void SpeedUpFade()
    {
        _fadeSpeed += _speedUp;
        _textSpeed = 0.01f;
    }

    private void CheckFontAsset()
    {
        if (LocalizationSettings.SelectedLocale.Equals(_jap) && dissolveEffect == null)
        {
            _textArea.font = _asset;
        }
    }

    private List<string> SortTableEntry(ICollection<StringTableEntry> values)
    {
        List<string> result = new();
        foreach (var key in values)
        {
            string keyName = key.Key;
            result.Add(keyName);
        }
        BubbleSortByNumber(result);
        return result;
    }

    private void BubbleSortByNumber(List<string> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; ++i)
        {
            for (int j = 0; j < n - 1; j++)
            {
                if (ExtractNumber(list[j]) > ExtractNumber(list[j + 1]))
                {
                    string temp = list[j];
                    list[j] = list[j + 1];
                    list[j + 1] = temp;
                }
            }
        }
    }

    private int ExtractNumber(string str)
    {
        string[] parts = str.Split("_");
        return int.Parse(parts[parts.Length - 1]);
    }

    public IEnumerator PlayStoryPlot(string tableName, Action finishAction = null, List<int> range = null, bool singleWords = false)
    {
        SubtitleManagement.Instance.ToggleActiveCanvas(true);
        StringTable plotTable = DataPersistenceManagement.Instance.StringTables[tableName];
        var entryKeys = plotTable.Values;
        List<string> sortString = SortTableEntry(entryKeys);
        int count = 0;
        bool isEng = LocalizationSettings.SelectedLocale.Equals(_english);
        foreach (string key in sortString)
        {
            if (range != null && !range.Contains(count++)) continue;
            int engCount = 0;
            _isTyping = true;
            _soundFX.Play();
            foreach (string str in LocalizationSettings.StringDatabase.GetTableEntry(tableName, key).Entry.GetLocalizedString().Split(" "))
            {
                if (singleWords && isEng)
                {
                    foreach (char word in str)
                    {
                        _text += word;
                        _textArea.SetText(_text);
                        yield return new WaitForSeconds(.05f);
                    }
                    _text += " ";
                }
                else
                {
                    _text += str;
                    // 英文要加空白
                    if (isEng) _text += " ";
                    _textArea.SetText(_text);
                    yield return new WaitForSeconds(_textSpeed);
                    engCount++;
                }
            }
            _isTyping = false;
            _soundFX.Pause();
            yield return new WaitForSeconds(_sentencesSpeed + (isEng ? engCount * 0.1f : 0));
            _text = "";
            _textArea.SetText(_text);
        }
        SubtitleManagement.Instance.ToggleActiveCanvas(false);
        yield return new WaitForSeconds(1);
        finishAction?.Invoke();
        _isTyping = false;
    }

    public IEnumerator ShowSentences(string[] showTexts, Action<string> callback = null, string callbackParam = null, bool singleWords = false)
    {
        _isTyping = true;
        _soundFX.Play();
        yield return LocalizationSettings.InitializationOperation;
        bool isEng = LocalizationSettings.SelectedLocale.Equals(_english);
        foreach (string str in showTexts)
        {
            if (singleWords && isEng)
            {
                foreach (char word in str)
                {
                    _text += word;
                    _textArea.SetText(_text);
                    yield return new WaitForSeconds(_textSpeed);
                }
                _text += " ";
            }
            else
            {
                _text += str;
                // 英文要加空白
                if (isEng) _text += " ";
                _textArea.SetText(_text);
                yield return new WaitForSeconds(_textSpeed);
            }
        }
        _soundFX.Pause();
        if (dissolveEffect) dissolveEffect.enabled = true;
        _isTyping = false;
        callback?.Invoke(callbackParam);
    }

    internal IEnumerator ShowSentences(string[] vs, object p)
    {
        throw new NotImplementedException();
    }
}
