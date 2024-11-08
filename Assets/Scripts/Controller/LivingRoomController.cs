using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingRoomController : AbstractRoomController
{
    [Header("Level 4")]
    [SerializeField] private Renderer[] _walls;
    [SerializeField] private Material _opacity;
    [Header("Level 5")]
    [SerializeField] private GameObject[] _needShow;
    [SerializeField] private GameObject[] _needHide;
    [Header("Truth Ending")]
    [SerializeField] private GameObject[] _showObjects;
    [SerializeField] private GameObject[] _hideObjects;

    /// <summary>
    /// ­ì©l§÷½è
    /// </summary>
    private List<Material[]> _originWallMaterials;
    

    public static LivingRoomController Instance { get; private set; }

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
            case 5:
                foreach (GameObject gameObject in _needShow)
                {
                    gameObject.SetActive(true);
                }
                foreach (GameObject gameObject in _needHide)
                {
                    gameObject.SetActive(false);
                }
                break;
        }
    }

    public void ToggleEndGameObjects()
    {
        foreach (GameObject gameObject in _showObjects)
        {
            gameObject.SetActive(true);
        }
        foreach (GameObject gameObject in _hideObjects)
        {
            gameObject.SetActive(false);
        }
    }
}
