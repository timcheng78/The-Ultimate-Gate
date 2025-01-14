using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enviroment : MonoBehaviour, IDataPersistence
{
    [Header("enviroment")]

    /// <summary>
    /// �O�_���q
    /// </summary>    
    [SerializeField] private bool _isElectrified = false;
    /// <summary>
    /// ���a
    /// </summary>    
    [SerializeField] private GameObject _player;
    /// <summary>
    /// �O�_�Ȱ�
    /// </summary>
    [SerializeField] private bool _isPause;
    /// <summary>
    /// �O�_���ժ�
    /// </summary>
    [SerializeField] private bool _isDemo;
    /// <summary>
    /// ���ռҦ�
    /// </summary>
    [SerializeField] private bool _isDebug;
    /// <summary>
    /// �O�_�}�l�C��
    /// </summary>
    [SerializeField] private bool _isStartPlay;
    /// <summary>
    /// ���L�о����d
    /// </summary>
    [SerializeField] private bool _skipTutorial;
    /// <summary>
    /// �ܲ����q [0: ���`, 1: ���q�@...]
    /// </summary>
    [SerializeField] private int _level = 0;
    /// <summary>
    /// �׫ǿå��O�O�_�}��
    /// </summary>
    [SerializeField] private bool _bedRoomEmission = false;
    /// <summary>
    /// �̲����D���^���D��
    /// </summary>
    [SerializeField] private string[] _finalAnswerArray;
    /// <summary>
    /// �O�_�Ĥ@������
    /// </summary>
    [SerializeField] private bool _isFirstInteract = true;
    /// <summary>
    /// ���q
    /// </summary>
    [SerializeField] private int _step = 1;
    /// <summary>
    /// �O�_����L�X������
    /// </summary>
    [SerializeField] private bool _isEndGameFile = false;
    /// <summary>
    /// �O�_Ĳ�o�������l��
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
