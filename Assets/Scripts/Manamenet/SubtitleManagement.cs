using Coffee.UIExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using static UnityEngine.EventSystems.EventTrigger;

public class SubtitleManagement : MonoBehaviour, IDataPersistence
{
    [SerializeField] private GameObject _subTitleGameObject;
    [SerializeField] private GameObject _sentencesGameObject;
    [SerializeField] private TMP_Text _subTitleArea;
    [SerializeField] private TMP_Text _contentTitleArea;
    [SerializeField] private float _subTitleWaitTime;
    [SerializeField] private TMP_Text[] _notePages;
    [SerializeField] private int _bookLines = 15;
    [SerializeField] private int _chineseWordsPerLine = 17;
    [SerializeField] private int _englishWordsPerLine = 10;
    [SerializeField] private int _japanesWordsPerLine = 15;

    private WordType _subWordType;
    private WordType _contentWordType;
    private Dictionary<string, object> _talkingStatement = new();
    private Dictionary<string, object> _allStatement = new();
    private float _timer_f = 0f;
    private int _timer_i = 0;
    private int _count = 0;
    private bool _isTyping = false;
    private Dictionary<string, List<string>> _readyToShow = new();
    private List<string> _usedSentences = new();
    private List<string> _hintUsedSentences = new();
    private List<string> _usedLocation = new();
    private List<List<string>> _pageContent = new();
    private List<List<string>> _normalCotent = new();
    private List<List<string>> _hintContent = new();
    private List<string> _alreadyShowHints = new List<string>();

    public int Count { get => _count; set => _count = value; }
    public WordType SubWordType { get => _subWordType; }

    public GameObject SentencesGameObject { get => _sentencesGameObject; }

    public List<List<string>> PageContent { get => _pageContent; }


    public bool IsTyping { get => _isTyping; }

    public static SubtitleManagement Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one SubtitleManagement in the scene.");
        }
        Instance = this;
        _subWordType = _subTitleArea.GetComponent<WordType>();
        SettingStringTables();
    }

    public void ToggleActiveCanvas(bool active)
    {
        _subTitleGameObject.SetActive(active);
    }

    public string GetNotePageText(int index)
    {
        return _notePages[index].text;
    }

    public void SetNotePageText(int index, string content)
    {
        _notePages[index].SetText(content);
    }

    private void SentencesEnd(string tableName)
    {
        Locale currentSelectedLocale = LocalizationSettings.SelectedLocale;
        ILocalesProvider availableLocales = LocalizationSettings.AvailableLocales;
        // add to page
        var localString = LocalizationSettings.StringDatabase.GetTableEntry(tableName, _readyToShow[tableName][0]);
        int type = 0;
        if (!_hintUsedSentences.Contains(_readyToShow[tableName][0]))
        {
            if (currentSelectedLocale == availableLocales.GetLocale("en"))
            {
                AddToPage(localString.Entry.GetLocalizedString(), type);
            }
            else
            {
                AddToPage(string.Join("", localString.Entry.GetLocalizedString().Split(" ")), type);
            }
            _hintUsedSentences.Add(_readyToShow[tableName][0]);
        }
        _readyToShow[tableName].RemoveAt(0);
        if (_readyToShow[tableName].Count.Equals(0)) _readyToShow.Remove(tableName);
        _isTyping = false;
    }

    private void SettingTalkStatementWithLocation()
    {
        if (_talkingStatement.Keys.Count.Equals(0)) return;
        string location = PlayerAttributes.Instance._location;
        if (location.Equals("none")) location = "";
        bool hasLight = LightManagement.Instance.CheckLightExist(location);
        string param = hasLight ? "" : "_nolight";
        string nodeName = $"{location}{ param }";
        int locationSize = _usedLocation.Count;
        // 檢查該區是否有需要顯示的文字
        if (!string.IsNullOrEmpty(nodeName) && _talkingStatement.ContainsKey(nodeName) && ((List<string>) _talkingStatement[nodeName]).Count > 0 && !_usedLocation.Contains(nodeName))
        {
            if (_count < ((List<string>) _talkingStatement[nodeName]).Count) 
            {
                StackSentences("Talk Statement Table", ((List<string>) _talkingStatement[nodeName])[_count++]);
            }
            else
            {
                _usedLocation.Add(nodeName);
                _count = 0;
            }
            
        }
        if ((!locationSize.Equals(_usedLocation.Count) || String.IsNullOrEmpty(nodeName) || !_talkingStatement.ContainsKey(nodeName)) && _count < ((List<string>)_talkingStatement["any"]).Count) 
            StackSentences("Talk Statement Table", ((List<string>) _talkingStatement["any"])[_count++]);
    }

    public void StackSentences(string tableName, string sentences)
    {
        if (_usedSentences.Contains(sentences)) return;
        if (!_readyToShow.ContainsKey(tableName))
        {
            _readyToShow.Add(tableName, new List<string>());
        }
        _readyToShow[tableName].Add(sentences);
        Debug.Log(sentences);
        if (!tableName.Equals("Hint Table") && sentences.IndexOf("living_room.") == -1 && sentences.IndexOf("_lock_noKey") == -1) _usedSentences.Add(sentences);
        _timer_f = 0f;
        _timer_i = 0;
    }

    public bool AddSentencesToShow(string table, string[] nodes)
    {
        string sentence = FindAllStatements(table, _allStatement, nodes);
        Debug.Log(sentence);
        if (sentence != null)
        {
            StackSentences(table, sentence);
            return true;
        }
        return false;
    }

    public void SpeedUpSentences()
    {
        foreach (WordType text in _sentencesGameObject.transform.GetComponentsInChildren<WordType>())
        {
            text.SpeedUpFade();
        }
    }

    private void SettingSentencesPosition()
    {
        foreach (TMP_Text text in _sentencesGameObject.transform.GetComponentsInChildren<TMP_Text>())
        {
            text.text += "\n\n\n\n\n";
        }
    }

    private void PlaySentences()
    {
        if (_isTyping) return;
        _isTyping = true;
        SettingSentencesPosition();
        TMP_Text textObject = Instantiate(_contentTitleArea);
        textObject.transform.SetParent(_sentencesGameObject.transform, false);
        WordType contentType = textObject.GetComponent<WordType>();
        UIDissolve dissolve = textObject.GetComponent<UIDissolve>();
        dissolve.enabled = false;
        string[] showTexts = new string[] { };
        string useTableName = "";
        if (_readyToShow.ContainsKey("Close Statement Table"))
        {
            var localString = LocalizationSettings.StringDatabase.GetTableEntry("Close Statement Table", _readyToShow["Close Statement Table"][0]);
            showTexts = localString.Entry.GetLocalizedString().Split(" ");
            useTableName = "Close Statement Table";
        }
        else if(_readyToShow.ContainsKey("Resolve Puzzle Statement Table"))
        {
            var localString = LocalizationSettings.StringDatabase.GetTableEntry("Resolve Puzzle Statement Table", _readyToShow["Resolve Puzzle Statement Table"][0]);
            showTexts = localString.Entry.GetLocalizedString().Split(" ");
            useTableName = "Resolve Puzzle Statement Table";
        }
        else if (_readyToShow.ContainsKey("Interact Statement Table"))
        {
            var localString = LocalizationSettings.StringDatabase.GetTableEntry("Interact Statement Table", _readyToShow["Interact Statement Table"][0]);
            showTexts = localString.Entry.GetLocalizedString().Split(" ");
            useTableName = "Interact Statement Table";
            // 攻擊文字正中間
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchoredPosition = new(rect.sizeDelta.x, rect.sizeDelta.y + 350);
            // 攻擊文字消失速度加快
            contentType.ChangeFadeSpeed(0.1f);
        }
        else if (_readyToShow.ContainsKey("Talk Statement Table"))
        {
            var localString = LocalizationSettings.StringDatabase.GetTableEntry("Talk Statement Table", _readyToShow["Talk Statement Table"][0]);
            showTexts = localString.Entry.GetLocalizedString().Split(" ");
            useTableName = "Talk Statement Table";
        }
        else if (_readyToShow.ContainsKey("Hint Table"))
        {
            var localString = LocalizationSettings.StringDatabase.GetTableEntry("Hint Table", _readyToShow["Hint Table"][0]);
            showTexts = localString.Entry.GetLocalizedString().Split(" ");
            useTableName = "Hint Table";
        }
        StartCoroutine(contentType.ShowSentences(showTexts, SentencesEnd, useTableName));
    }

    private void AddToPage(string sentences, int type)
    {
        SetContentSize();
        string[] sortSentences = BreakLineCheck(sentences);


        foreach(string singleLine in sortSentences)
        {
            int page = -1;
            switch (type)
            {
                case 0:
                    // normal
                    //page = CheckChangePage(_normalCotent);
                    //if (page != -1) _normalCotent[page].Add(singleLine);
                    page = CheckChangePage(_pageContent);
                    if (page != -1) _pageContent[page].Add(singleLine);
                    break;
                case 1:
                    // hint
                    page = CheckChangePage(_hintContent);
                    if (page != -1) _hintContent[page].Add(singleLine);
                    break;
            }
        }

        //CombineContent();
    }

    private void SetContentSize()
    {
        if (_pageContent.Count.Equals(22)) return;
        for (int i = 0; i < 22; ++i)
        {
            if (i >= 0 && i < _pageContent.Count && _pageContent[i] != null) _pageContent[i] = new List<string>();
            else _pageContent.Add(new List<string>());
            //if (i < 11)
            //{
            //    if (!(i >= 0 && i < _normalCotent.Count)) _normalCotent.Add(new List<string>());
            //    if (!(i >= 0 && i < _hintContent.Count)) _hintContent.Add(new List<string>());
            //}
        }
    }

    private void CombineContent()
    {
        int page = 0;
        // 處理一般頁面
        page = AddContentToPage(_normalCotent, page);
        // 處理提示頁面
        //if (page % 2 == 0) page += 2;
        //else page += 1;
        //_hintPage = page;
        //page = AddContentToPage(_hintContent, page);
        
    }

    private int AddContentToPage(List<List<string>> pageContents, int page)
    {
        foreach (List<string> pageContent in pageContents)
        {
            if (pageContent.Count > 0)
            {
                foreach (string content in pageContent)
                {
                    _pageContent[page].Add(content);
                }
                page++;
            }
        }
        return page;
    }

    private int CheckChangePage(List<List<string>> content)
    {
        int page = 0;
        while (content[page].Count >= _bookLines)
        {
            page++;
        }
        return page;
    }
    
    private string[] BreakLineCheck(string sentences)
    {
        Locale currentSelectedLocale = LocalizationSettings.SelectedLocale;
        ILocalesProvider availableLocales = LocalizationSettings.AvailableLocales;
        List<string> result = new();
        result.Add(sentences);
        if (currentSelectedLocale == availableLocales.GetLocale("zh-TW")) 
        {
            // chinese
            if (sentences.Length > _chineseWordsPerLine)
            {
                // remove first
                result.RemoveAt(0);
                // break word
                string singleLine = "";
                foreach (char word in sentences)
                {
                    if (word.Equals('\n')) continue;
                    singleLine += word;
                    if (singleLine.Length.Equals(_chineseWordsPerLine))
                    {
                        result.Add(singleLine);
                        singleLine = "";
                    }
                }
                if (singleLine.Length > 0) result.Add(singleLine);
            }
        }
        if (currentSelectedLocale == availableLocales.GetLocale("en"))
        {
            // english
            if (sentences.Length > _englishWordsPerLine)
            {
                // remove first
                result.RemoveAt(0);
                // split words
                string[] vocabularies = sentences.Split(" ");
                string lastVocabulary = "";
                string singleLine = "";
                foreach (string vocabulary in vocabularies)
                {
                    string nonBreakVocabulary = vocabulary.Replace('\n', ' ');
                    // count words to break line
                    if (!String.IsNullOrEmpty(lastVocabulary))
                    {
                        // check break line
                        if ((singleLine.Length + lastVocabulary.Length) > _englishWordsPerLine)
                        {
                            result.Add(singleLine);
                            singleLine = "";
                        }
                        // check add blank
                        if (singleLine.Length > 0) singleLine += " ";
                        singleLine += lastVocabulary;

                        // The color of the
                    }
                    // add to lastVocabulary
                    lastVocabulary = nonBreakVocabulary;
                }
                if ((singleLine.Length + lastVocabulary.Length) > _englishWordsPerLine)
                {
                    result.Add(singleLine);
                    singleLine = "";
                }
                // check add blank
                if (singleLine.Length > 0) singleLine += " ";
                singleLine += lastVocabulary;
                result.Add(singleLine);
            }
        }
        if (currentSelectedLocale == availableLocales.GetLocale("ja-JP"))
        {
            // japan
            if (sentences.Length > _japanesWordsPerLine)
            {
                // remove first
                result.RemoveAt(0);
                // break word
                string singleLine = "";
                foreach (char word in sentences)
                {
                    if (word.Equals('\n')) continue;
                    singleLine += word;
                    if (singleLine.Length.Equals(_japanesWordsPerLine))
                    {
                        result.Add(singleLine);
                        singleLine = "";
                    }
                }
                if (singleLine.Length > 0) result.Add(singleLine);
            }
        }
        return result.ToArray();
    }

    private void Update()
    {
        if (Enviroment.Instance.IsPause || !Enviroment.Instance.IsStartPlay) return;

        _timer_f += Time.deltaTime;
        _timer_i = (int)_timer_f;

        if (_timer_i >= _subTitleWaitTime) SettingTalkStatementWithLocation();
        if (_readyToShow.Count > 0) PlaySentences();
    }

    private void SettingStringTables()
    {
        if (DataPersistenceManagement.Instance == null) return;
        StringTable plotTable = DataPersistenceManagement.Instance.StringTables["Talk Statement Table"];
        StringTable interactTable = DataPersistenceManagement.Instance.StringTables["Interact Statement Table"];
        StringTable resolvePuzzleTable = DataPersistenceManagement.Instance.StringTables["Resolve Puzzle Statement Table"];
        StringTable closeStatementTable = DataPersistenceManagement.Instance.StringTables["Close Statement Table"];
        StringTable hintTable = DataPersistenceManagement.Instance.StringTables["Hint Table"];
        AddStatementKeys(_talkingStatement, plotTable.Values, true);
        SharedUtils.RandomStringArray((List<string>) _talkingStatement["any"]);
        AddStatementKeys(_allStatement, interactTable.Values);
        RandomListData(_allStatement);
        AddStatementKeys(_allStatement, resolvePuzzleTable.Values);
        AddStatementKeys(_allStatement, closeStatementTable.Values);
        AddStatementKeys(_allStatement, hintTable.Values);
    }

    private void AddStatementKeys(Dictionary<string, object> dictionary, ICollection<StringTableEntry> entryKeys, bool noHardMode = false)
    {
        foreach (var key in entryKeys)
        {
            string keyName = key.Key;
            string[] keyNames = keyName.Split(".");
            SetAllStatements(dictionary, keyNames, keyName, noHardMode);
        }
    }

    private void RandomListData(Dictionary<string, object> dictionary)
    {
        foreach (KeyValuePair<string, object> map in dictionary)
        {
            if (map.Value is List<string>) SharedUtils.RandomStringArray((List<string>)map.Value);
            else RandomListData((Dictionary<string, object>) map.Value);
        }
    }

    private string FindAllStatements(string table, Dictionary<string, object> dictionary, string[] nodes)
    {
        if (nodes.Length.Equals(1))
        {
            // return
            if (dictionary.ContainsKey(nodes[0]))
            {
                List<string> list = (List<string>) dictionary[nodes[0]];
                if (table.Equals("Hint Table"))
                {
                    int x = 0;
                    while (x < list.Count)
                    {
                        if (_alreadyShowHints.Contains(list[x])) x++;
                        else
                        {
                            _alreadyShowHints.Add(list[x]);
                            return list[x];
                        }
                    }
                    // fully end
                    _alreadyShowHints.Clear();
                    _alreadyShowHints.Add(list[0]);
                    return list[0];
                }
                else
                {
                    string result = list[0];
                    //if (list.Count.Equals(1))
                    //{
                    //    dictionary.Remove(nodes[0]);
                    //}
                    //else
                    //{
                    //    list.RemoveAt(0);
                    //}
                    return result;
                }
                
            }
            return null;
        }
        nodes = Shift(nodes, out string node);
        if (dictionary.ContainsKey(node))
        {
            string result = FindAllStatements(table, (Dictionary<string, object>)dictionary[node], nodes);
            if (((Dictionary<string, object>)dictionary[node]).Count.Equals(0))
            {
                dictionary.Remove(node);
            }
            return result;
        }
        return null;
    }

    private void SetAllStatements(Dictionary<string, object> dictionary, string[] nodes, string fullName, bool noHardMode = false)
    {
        if (nodes.Length.Equals(2))
        {
            // 若非數字則為困難模式
            if (!noHardMode && OptionMenu.Instance.hardMode && !int.TryParse(nodes[1], out int result)) return;
            if (dictionary.ContainsKey(nodes[0]) && dictionary[nodes[0]] is List<string> existingList) existingList.Add(fullName);
            else
            {
                dictionary[nodes[0]] = new List<string>() { fullName };
            }
        } 
        else
        {
            nodes = Shift(nodes, out string node);
            if (dictionary.ContainsKey(node))
            {
                SetAllStatements((Dictionary<string, object>) dictionary[node], nodes, fullName);
            }
            else
            {
                dictionary.Add(node, new Dictionary<string, object>());
                SetAllStatements((Dictionary<string, object>)dictionary[node], nodes, fullName);
            }
        }
    }

    string[] Shift(string[] array, out string firstElement)
    {
        if (array.Length == 0)
        {
            throw new InvalidOperationException("Cannot shift from an empty array.");
        }

        firstElement = array[0];

        string[] newArray = new string[array.Length - 1];
        Array.Copy(array, 1, newArray, 0, array.Length - 1);

        return newArray;
    }

    void IDataPersistence.LoadData(GameData data)
    {
        if (data.recordData.records != null) _pageContent = data.recordData.RecordsToList();
        if (data.recordData.usedSentences != null) _usedSentences = data.recordData.usedSentences;
        if (data.recordData.hintSentences != null) _hintUsedSentences = data.recordData.hintSentences;
    }

    void IDataPersistence.SaveData(ref GameData data, bool isCrazyEnd)
    {
        data.recordData.records = data.recordData.ListToRecords(_pageContent);
        data.recordData.usedSentences = _usedSentences;
        data.recordData.hintSentences = _hintUsedSentences;
    }
}
