using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookRoomController : AbstractRoomController
{
    [Header("Level 3")]
    [SerializeField] private Renderer[] _walls;
    [SerializeField] private Material _opacity;
    [Header("Level 4")]
    [SerializeField] private GameObject[] _needShow;
    [SerializeField] private GameObject _silverKey;
    /// <summary>
    /// ­ì©l§÷½è
    /// </summary>
    private List<Material[]> _originWallMaterials;
    private bool _alreadyExecute = false;
    public static BookRoomController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        _originWallMaterials = new List<Material[]>();
        foreach (Renderer renderer in _walls)
        {
            _originWallMaterials.Add(renderer.materials);
        }
    }

    private void Update()
    {
        CheckLivingLevel();
    }

    private void CheckLivingLevel()
    {
        switch (Enviroment.Instance.Level)
        {
            case 3:
            case 4:
                for (int i = 0, len = _walls.Length; i < len; ++i)
                {
                    Renderer renderer = _walls[i];
                    if (_normal)
                    {
                        renderer.materials = _originWallMaterials[i];
                    }
                    else
                    {
                        Material[] tempMaterials = renderer.materials;
                        tempMaterials[^1] = _opacity;
                        renderer.materials = tempMaterials;
                    }
                }
                if (Enviroment.Instance.Level.Equals(4))
                {
                    foreach (GameObject gameObject in _needShow)
                    {
                        if (_normal)
                        {
                            gameObject.SetActive(false);
                        }
                        else
                        {
                            gameObject.SetActive(true);
                        }
                    }
                }
                if (!_alreadyExecute && CheckSilverKeyActive())
                {
                    SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { "book_room", "light_switch_mutation_4", "t" });
                    SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_TRIGGER_BOOK_ROOM_LOCK_COUNT_SEC, Timer.Instance.GetNowSec());
                    _silverKey.SetActive(true);
                    if (!Enviroment.Instance.IsDebug) Camera.main.GetComponent<CameraAttributes>().StartStorm();
                }
                break;
            case 5:
                foreach (GameObject gameObject in _needShow)
                {
                    gameObject.SetActive(true);
                }
                break;
        }
    }

    public bool CheckSilverKeyActive()
    {
        if (!Enviroment.Instance.Level.Equals(4)) return false;
        if (_normal) return false;
        // check all position
        bool kitchen = LightManagement.Instance.CheckLightExist("kitchen");
        bool livingRoom = LightManagement.Instance.CheckLightExist("living_room");
        bool bedRoom = LightManagement.Instance.CheckLightExist("bed_room");
        bool bathRoom = LightManagement.Instance.CheckLightExist("bath_room_1");
        bool bookRoom = LightManagement.Instance.CheckLightExist("book_room");
        if (kitchen && livingRoom && bedRoom && bathRoom && bookRoom)
        {
            _alreadyExecute = true;
            return true;
        }
        return false;
    }
}
