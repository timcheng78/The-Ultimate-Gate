using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedRoomController : AbstractRoomController
{
    [Header("Level 3")]
    [SerializeField] private Renderer[] _walls;
    [SerializeField] private Material _opacity;

    [Header("screen")]
    [SerializeField] private ScreenColor _screenRenderer; 
    [SerializeField, ColorUsage(true,true)] private Color[] _colors = new Color[5];
    /// <summary>
    /// ­ì©l§÷½è
    /// </summary>
    private List<Material[]> _originWallMaterials;
    
    public static BedRoomController Instance { get; private set; }

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
        if (!Enviroment.Instance) return;
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
                    _screenRenderer.ToggleScreenEmission();
                }
                break;
        }
    }
}
