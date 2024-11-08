using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BathRoom1Controller : AbstractRoomController
{
    [Header("Level 2")]
    [SerializeField] private Renderer[] _water;
    [SerializeField] private Light[] _lights;
    [SerializeField] private AudioClip[] _audios;
    [SerializeField] private SwitchItem _switchLight;
    [Header("Level 3")]
    [SerializeField] private Renderer[] _walls;
    [SerializeField] private Material _opacity;
    /// <summary>
    /// 原始材質
    /// </summary>
    private List<Material[]> _originWallMaterials;
    /// <summary>
    /// 光影按鈕點擊次數
    /// </summary>
    private int _puzzleButtonCount = 0;
    /// <summary>
    /// 音效只播一次
    /// </summary>
    private bool _played = false;
    
    public static BathRoom1Controller Instance { get; private set; }

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
        CheckBathRoomLevel();
    }

    private void CheckBathRoomLevel()
    {
        switch (Enviroment.Instance.Level)
        {
            case 2:
                //foreach (Light light in _lights)
                //{
                //    if (!_normal && !LightManagement.Instance.CheckLightExist("bath_room_1"))
                //    {
                //        LightManagement.Instance.TurnOnLight("bath_room_1");
                //        _switchLight._status = true;
                //    }
                //}
                foreach (Renderer renderer in _water)
                {
                    if (_normal)
                    {
                        renderer.materials[^1].SetColor("_Water_Color", new Color(0.4745f, 0.4902f, 1f, 0.902f));
                        renderer.materials[^1].SetColor("_Foam_Color", new Color(0.929f, 0.933f, 0.980f, 0.525f));
                    }
                    else
                    {
                        renderer.materials[^1].SetColor("_Water_Color", new Color(0.76f, 0.60f, 0.28f, 0.902f));
                        renderer.materials[^1].SetColor("_Foam_Color", new Color(0.114f, 0.098f, 0.039f, 0.525f));
                    }
                }
                if (_played) return;
                foreach (AudioClip audio in _audios)
                {
                    SoundManagement.Instance.PlaySoundFXClip(audio, transform, 1f);
                }
                _played = true;
                break;
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
                break;
        }
    }

    public int CountPuzzleButton()
    {
        return ++_puzzleButtonCount;
    }
}
