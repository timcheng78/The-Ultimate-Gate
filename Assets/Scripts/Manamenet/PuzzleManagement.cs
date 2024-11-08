using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PuzzleManagement : MonoBehaviour, IDataPersistence
{
    [Header("Puzzle Config")]
    [SerializeField] private Puzzle _puzzle;
    [SerializeField] private float _delay = .5f;
    public SerializableDictionary<string, SerializableDictionary<string, PuzzleObject>> _puzzleDictionary;

    private bool _hasChanged = false;  
    private float _timer = 0f;       
    private bool _delayActive = false;
    private Action _callback = null;
    private string[] _achievementAnswer = new string[] { "0", "2", "7" };
    private int[] _achievementAnswerInt = new int[] { 1, 6, 5, 9 };

    public static PuzzleManagement Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one PuzzleManagement in the scene.");
        }
        Instance = this;
        _puzzleDictionary = _puzzle.ToDictionary();
    }

    private void Start()
    {
        StartCoroutine(DelayCheck());
    }

    public void PreCheckAnswer(Action callback)
    {
        _delayActive = true;
        _hasChanged = true;
        _callback = callback;
    }

    private IEnumerator DelayCheck()
    {
        while (true)
        {
            if (_delayActive)
            {
                if (_hasChanged)
                {
                    _timer = 0f;
                    _hasChanged = false;
                }
                else
                {
                    _timer += Time.deltaTime;
                }

                if (_timer >= _delay)
                {
                    _callback();
                    _timer = 0f;
                    _delayActive = false;
                }
            }

            yield return null;
        }
    }

    public bool CheckAnswer(string location, string puzzleName)
    {
        Dictionary<string, PuzzleObject> locationPuzzle = _puzzleDictionary[location];
        PuzzleObject puzzleObject = locationPuzzle[puzzleName];
        OrganPuzzle organPuzzle = puzzleObject.PuzzleObj.GetComponent<OrganPuzzle>();
        int count = 0;
        switch (puzzleObject.PuzzleName)
        {
            case "shief":
                {
                    foreach (GameObject gameObject in organPuzzle.Organs)
                    {
                        DragDropItem dragDropItem = gameObject.GetComponent<DragDropItem>();
                        if (!dragDropItem._book) return false;
                        if (!puzzleObject.Answer[count].Equals(dragDropItem._book._itemColor)) return false;
                        count++;
                    }
                    if (count.Equals(puzzleObject.Answer.Length)) puzzleObject.IsSolve = true;
                    else return false;
                    SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_BOOK_ROOM_SHIEF_COUNT_SEC, Timer.Instance.GetNowSec());
                    break;
                }
            case "electrical_box":
            case "door":
            case "final_puzzle":
                {
                    foreach (GameObject gameObject in organPuzzle.Organs)
                    {
                        TMP_Text mP_Text = gameObject.GetComponent<TMP_Text>();
                        if (!mP_Text.text.Equals(puzzleObject.Answer[count])) return false;
                        count++;
                    }
                    if (count.Equals(puzzleObject.Answer.Length)) puzzleObject.IsSolve = true;
                    else return false;
                    if (puzzleObject.PuzzleName.Equals("electrical_box"))
                    {
                        SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_KITCHEN_ELECTRICAL_BOX_COUNT_SEC, Timer.Instance.GetNowSec());
                    }
                    else if (puzzleObject.PuzzleName.Equals("door"))
                    {
                        SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_LIVING_ROOM_TO_BATH_ROOM_2_COUNT_SEC, Timer.Instance.GetNowSec());
                    }
                    else if (puzzleObject.PuzzleName.Equals("final_puzzle"))
                    {
                        SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_BATH_ROOM_2_FINAL_PUZZLE_COUNT_SEC, Timer.Instance.GetNowSec());
                    }
                    break;
                }
            case "induction":
                {
                    foreach(GameObject gameObject in organPuzzle.Organs)
                    {
                        InductionController inductionController = gameObject.GetComponent<InductionController>();
                        if (!inductionController.SelectedNumber.Equals(int.Parse(puzzleObject.Answer[count]))) return false;
                        count++;
                    }
                    if (count.Equals(puzzleObject.Answer.Length)) puzzleObject.IsSolve = true;
                    else return false;
                    SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_KITCHEN_INDUCTION_COUNT_SEC, Timer.Instance.GetNowSec());
                    break;
                }
            case "insurance":
                {
                    List<InsuranceButton> tempButtons = new();
                    bool isAchievement = false;
                    foreach (GameObject gameObject in organPuzzle.Organs)
                    {
                        InsuranceBox insuranceBox = gameObject.GetComponent<InsuranceBox>();
                        foreach (GameObject _gameObject in insuranceBox.NumberObjects)
                        {
                            InsuranceButton insuanceButton = _gameObject.GetComponent<InsuranceButton>();
                            insuanceButton.AlreadyPlay = true;
                            insuanceButton.Status = false;
                            tempButtons.Add(insuanceButton);
                        }
                        insuranceBox.NumberObjects.Clear();
                    }
                    foreach(InsuranceButton insuranceButton in tempButtons)
                    {
                        if (!insuranceButton.Index.Equals(int.Parse(puzzleObject.Answer[count])))
                        {
                            if (!insuranceButton.Index.Equals(_achievementAnswerInt[count])) return false;
                            isAchievement = true;
                        }
                        count++;
                    }
                    if (isAchievement)
                    {
                        SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_TRUTHLY_ANSWER);
                        return false;
                    }
                    if (count.Equals(puzzleObject.Answer.Length)) puzzleObject.IsSolve = true;
                    else return false;
                    SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_BED_ROOM_INSURANCE_COUNT_SEC, Timer.Instance.GetNowSec());
                    break;
                }
            case "air_puzzle":
                {
                    TMP_Text mP_Text = organPuzzle.Organs[0].GetComponent<TMP_Text>();
                    if (!mP_Text.text.Equals(puzzleObject.Answer[count]))
                    {
                        mP_Text.SetText("");
                        return false;
                    }
                    puzzleObject.IsSolve = true;
                    SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_BED_ROOM_AIR_PUZZLE_COUNT_SEC, Timer.Instance.GetNowSec());
                    break;
                }
            case "cabinet":
                {
                    foreach (GameObject gameObject in organPuzzle.Organs)
                    {
                        TMP_Text mP_Text = gameObject.GetComponent<TMP_Text>();
                        if (!mP_Text.text.Equals(puzzleObject.Answer[count])) return false;
                        count++;
                    }
                    if (count.Equals(puzzleObject.Answer.Length)) puzzleObject.IsSolve = true;
                    else return false;
                    SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_BATH_ROOM_1_CABINET_COUNT_SEC, Timer.Instance.GetNowSec());
                    break;
                }
            case "puzzle":
                {
                    foreach (GameObject gameObject in organPuzzle.Organs)
                    {
                        LevelBlockItem blockItem = gameObject.GetComponent<LevelBlockItem>();
                        if (puzzleObject.Answer[count].Split(",").Length > 1)
                        {
                            string[] anses = puzzleObject.Answer[count].Split(",");
                            if (Array.IndexOf(anses, blockItem.Status.ToString()) == -1) return false;
                        } else if (!blockItem.Status.Equals(int.Parse(puzzleObject.Answer[count]))) return false;
                        count++;
                    }
                    if (count.Equals(puzzleObject.Answer.Length)) puzzleObject.IsSolve = true;
                    else return false;
                    SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_BATH_ROOM_1_PUZZLE_COUNT_SEC, Timer.Instance.GetNowSec());
                    break;
                }
            case "display_door":
                {
                    LightPuzzleController lightPuzzleController = puzzleObject.PuzzleObj.GetComponent<LightPuzzleController>();
                    foreach (int number in lightPuzzleController.SortNumber)
                    {
                        if (!number.Equals(count)) return false;
                        count++;
                    }
                    if (count.Equals(3)) puzzleObject.IsSolve = true;
                    else return false;
                    SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_LIVING_ROOM_DISPLAY_DOOR_COUNT_SEC, Timer.Instance.GetNowSec());
                    break;
                }
            case "red":
            case "blue":
            case "green":
                {
                    DemoLockController demoLockController = puzzleObject.PuzzleObj.GetComponent<DemoLockController>();
                    List<DemoButton> tempButtons = new();
                    foreach (GameObject _gameObject in demoLockController.NumberObjects)
                    {
                        DemoButton demoButton = _gameObject.GetComponent<DemoButton>();
                        demoButton.AlreadyPlay = true;
                        demoButton.Status = false;
                        tempButtons.Add(demoButton);
                    }
                    demoLockController.NumberObjects.Clear();
                    foreach (DemoButton tempButton in tempButtons)
                    {
                        if (!tempButton.Index.Equals(int.Parse(puzzleObject.Answer[count]))) return false;
                        count++;
                    }
                    if (count.Equals(puzzleObject.Answer.Length)) puzzleObject.IsSolve = true;
                    else return false;
                    if (puzzleObject.PuzzleName.Equals("red"))
                    {
                        SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_DEMO_RED_COUNT_SEC, Timer.Instance.GetNowSec());
                    }
                    else if (puzzleObject.PuzzleName.Equals("blue"))
                    {
                        SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_DEMO_BLUE_COUNT_SEC, Timer.Instance.GetNowSec());
                    }
                    else if (puzzleObject.PuzzleName.Equals("green"))
                    {
                        SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_DEMO_GREEN_COUNT_SEC, Timer.Instance.GetNowSec());
                    }
                    break;
                }
            case "box":
                {
                    DemoBoxController demoBoxController = puzzleObject.PuzzleObj.GetComponent<DemoBoxController>();
                    foreach (GameObject gameObject in demoBoxController.Buttons)
                    {
                        PasswordPanelItem item = gameObject.GetComponent<PasswordPanelItem>();
                        if (!item._passwordPanel.text.Equals(puzzleObject.Answer[count])) return false;
                        count++;
                    }
                    if (count.Equals(puzzleObject.Answer.Length)) puzzleObject.IsSolve = true;
                    else return false;
                    SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_DEMO_BOX_COUNT_SEC, Timer.Instance.GetNowSec());
                    break;
                }
        }
        return puzzleObject.IsSolve;
    }

    public void SetPuzzleAnswer(string location, string puzzleName, string[] puzzleAnswer)
    {
        Dictionary<string, PuzzleObject> locationPuzzle = _puzzleDictionary[location];
        PuzzleObject puzzleObject = locationPuzzle[puzzleName];
        puzzleObject.Answer = puzzleAnswer;
    }

    public GameObject GetPuzzleObject(string location, string puzzleName)
    {
        Dictionary<string, PuzzleObject> locationPuzzle = _puzzleDictionary[location];
        PuzzleObject puzzleObject = locationPuzzle[puzzleName];
        return puzzleObject.PuzzleObj;
    }

    public bool IsSolvePuzzle(string location, string puzzleName)
    {
        return _puzzleDictionary[location][puzzleName].IsSolve;
    }

    public void SetPuzzleSolve(string location, string puzzleName, bool status)
    {
        _puzzleDictionary[location][puzzleName].IsSolve = status;
    }

    void IDataPersistence.LoadData(GameData data)
    {
        if (data.puzzleData.puzzles == null) return;
        data.puzzleData.SetPuzzleValue(_puzzleDictionary);
        foreach (KeyValuePair<string, SerializableDictionary<string, PuzzleObject>> map1 in _puzzleDictionary)
        {
            string location = map1.Key;
            int demoColor = 0;
            foreach (KeyValuePair<string, PuzzleObject> map2 in _puzzleDictionary[location])
            {
                string puzzleName = map2.Key;
                if (!_puzzleDictionary[location][puzzleName].IsSolve) continue;
                OrganPuzzle organ = _puzzleDictionary[location][puzzleName].PuzzleObj.GetComponent<OrganPuzzle>();
                if (organ == null) organ = LockManagement.Instance.GetLockObject(location, puzzleName).LockObj.GetComponent<OrganPuzzle>();
                if (organ == null) continue;
                if (puzzleName.Equals("red") || puzzleName.Equals("green") || puzzleName.Equals("blue"))
                {
                    demoColor++;
                    if (demoColor.Equals(3)) organ.Open();
                    continue;
                }
                organ.Open();
            }
        }
    }

    void IDataPersistence.SaveData(ref GameData data, bool isCrazyEnd)
    {
        data.puzzleData.puzzles = data.puzzleData.ObjectToPuzzleData(_puzzleDictionary);
    }
}
