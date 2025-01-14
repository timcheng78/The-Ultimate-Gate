using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enviroment : MonoBehaviour, IDataPersistence
{
    [Header("enviroment")]

    /// <summary>
    /// 是否有電
    /// </summary>    
    [SerializeField] private bool _isElectrified = false;
    /// <summary>
    /// 玩家
    /// </summary>    
    [SerializeField] private GameObject _player;
    /// <summary>
    /// 是否暫停
    /// </summary>
    [SerializeField] private bool _isPause;
    /// <summary>
    /// 是否為試玩
    /// </summary>
    [SerializeField] private bool _isDemo;
    /// <summary>
    /// 測試模式
    /// </summary>
    [SerializeField] private bool _isDebug;
    /// <summary>
    /// 是否開始遊玩
    /// </summary>
    [SerializeField] private bool _isStartPlay;
    /// <summary>
    /// 略過教學關卡
    /// </summary>
    [SerializeField] private bool _skipTutorial;
    /// <summary>
    /// 變異階段 [0: 正常, 1: 階段一...]
    /// </summary>
    [SerializeField] private int _level = 0;
    /// <summary>
    /// 臥室螢光燈是否開啟
    /// </summary>
    [SerializeField] private bool _bedRoomEmission = false;
    /// <summary>
    /// 最終謎題的英文題目
    /// </summary>
    [SerializeField] private string[] _finalAnswerArray;
    /// <summary>
    /// 是否第一次互動
    /// </summary>
    [SerializeField] private bool _isFirstInteract = true;
    /// <summary>
    /// 階段
    /// </summary>
    [SerializeField] private int _step = 1;
    /// <summary>
    /// 是否為其他出路結局
    /// </summary>
    [SerializeField] private bool _isEndGameFile = false;
    /// <summary>
    /// 是否觸發未完成召喚
    /// </summary>
    [SerializeField] private bool _incompleteSummoning = false;

    public bool IsElectrified { get => _isElectrified; set => _isElectrified = value; }
    public bool IsPause { get => _isPause; set => _isPause = value; }
    public bool IsDemo { get => _isDemo; set => _isDemo = value; }
    public bool IsDebug { get => _isDebug; }
    public bool IsStartPlay { get => _isStartPlay; set => _isStartPlay = value; }
    public bool SkipTutorial { get => _skipTutorial; private set => _skipTutorial = value; }
    public GameObject Player { get => _player; private set => _player = value; }
    public int Level { get => _level; set => _level = value; }
    public bool BedRoomEmission { get => _bedRoomEmission; set => _bedRoomEmission = value; }
    public string[] FinalAnswerArray { get => _finalAnswerArray; }
    public bool IsFirstInteract { get => _isFirstInteract; set => _isFirstInteract = value; }
    public int Step { get => _step; set => _step = value; }
    public bool IsEndGameFile { get => _isEndGameFile; set => _isEndGameFile = value; }
    public bool IncompleteSummoning { get => _incompleteSummoning; set => _incompleteSummoning = value; }

    public static Enviroment Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public AbstractRoomController GetControllerByLocation(string location)
    {
        switch (location)
        {
            case "book_room":
                return BookRoomController.Instance;
            case "kitchen":
                return KitchenController.Instance;
            case "bed_room":
                return BedRoomController.Instance;
            case "bath_room_1":
                return BathRoom1Controller.Instance;
            case "living_room":
                return LivingRoomController.Instance;
            case "bath_room_2":
                return BathRoom2Controller.Instance;
        }
        return null;
    }

    void IDataPersistence.LoadData(GameData data)
    {
        _isElectrified = data.is_electrified;
        _level = data.level;
        _bedRoomEmission = data.bedRoomEmission;
        _step = data.step;
        _incompleteSummoning = data.trigger_incomplete_summoning;
        if (OptionMenu.Instance) OptionMenu.Instance.hardMode = data.hard_mode;
    }

    void IDataPersistence.SaveData(ref GameData data, bool isCrazyEnd)
    {
        data.is_electrified = _isElectrified;
        data.level = _level;
        data.step = _step;
        data.bedRoomEmission = _bedRoomEmission;
        data.is_endgame_file = _isEndGameFile;
        data.trigger_incomplete_summoning = _incompleteSummoning;
        if (OptionMenu.Instance) data.hard_mode = OptionMenu.Instance.hardMode;
    }
}
