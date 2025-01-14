using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenController : AbstractRoomController
{
    [Header("General")]
    [SerializeField] private Light[] _lights;
    [SerializeField] private GameObject _hideButton;

    [Header("Level 1")]
    [SerializeField] private Renderer[] _meats;
    [SerializeField] private GameObject[] _hideObject;
    [SerializeField] private GameObject[] _showObject;
    [SerializeField] private ParticleSystem[] _particles;
    [SerializeField] private Material _meatBlack;

    [Header("Level 3")]
    [SerializeField] private Renderer[] _walls;
    [SerializeField] private ParticleSystem[] _secondParticles;
    [SerializeField] private Material _opacity;

    /// <summary>
    /// 原始材質
    /// </summary>
    private List<Material[]> _originMeatMaterials;
    /// <summary>
    /// 原始材質
    /// </summary>
    private List<Material[]> _originWallMaterials;
    /// <summary>
    /// 刀、湯匙拿起的次數
    /// </summary>
    private int _kinfePickUpCount = 0;
    /// <summary>
    /// 是否開始計時
    /// </summary>
    private bool _startTimer = false;
    /// <summary>
    /// 停止粒子效果撥放
    /// </summary>
    private bool _stopPlayParticles = false;

    private float _timer_f = 0f;
    private int _timer_i = 0;
    public Light[] Lights { get => _lights; }
    public static KitchenController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        _originMeatMaterials = new List<Material[]>();
        _originWallMaterials = new List<Material[]>();
        foreach (Renderer renderer in _meats)
        {
            _originMeatMaterials.Add(renderer.materials);
        }
        foreach (Renderer renderer in _walls)
        {
            _originWallMaterials.Add(renderer.materials);
        }
    }

    private void Update()
    {
        CheckKitchenStatement();
        CheckKitchenLevel();
    }

    private void CheckKitchenStatement()
    {
        if (!_startTimer) return;
        _timer_f += Time.deltaTime;
        _timer_i = (int)_timer_f;
        if (_timer_i.Equals(300))
        {
            SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { "kitchen", "refrigerator_door_top_opening", "t" });
            _timer_f += 1f;
        }
    }

    private void CheckKitchenLevel()
    {
        switch (Enviroment.Instance.Level)
        {
            case 1:
                foreach (Light light in _lights)
                {
                    if (_normal)
                    {
                        light.enabled = false;
                    } 
                    else
                    {
                        light.color = new Color(1, 0, 0);
                        light.enabled = true;
                    }
                }
                for (int i = 0, len = _meats.Length; i < len; ++i)
                {
                    Renderer renderer = _meats[i];
                    if (_normal)
                    {
                        renderer.materials = _originMeatMaterials[i];
                    }
                    else
                    {
                        Material[] newMats = new Material[] { _meatBlack, _meatBlack };
                        renderer.materials = newMats;
                    }
                }
                foreach (GameObject hideObject in _hideObject)
                {
                    if (_normal)
                    {
                        hideObject.SetActive(true);
                    }
                    else
                    {
                        hideObject.SetActive(false);
                    }
                }
                foreach (GameObject showObject in _showObject)
                {
                    if (_normal)
                    {
                        showObject.SetActive(false);
                    }
                    else
                    {
                        showObject.SetActive(true);
                    }
                }
                if (_stopPlayParticles) return;
                foreach (ParticleSystem particle in _particles)
                {
                    if (!_normal && !particle.isPlaying)
                    {
                        particle.Play();
                        _stopPlayParticles = true;
                    }
                }
                break;
            case 2:
                foreach (Light light in _lights)
                {
                    light.enabled = false;
                }
                break;
            case 3:
            case 4:
                for (int i = 0, len = _meats.Length; i < len; ++i)
                {
                    Renderer renderer = _meats[i];
                    if (_normal)
                    {
                        renderer.materials = _originMeatMaterials[i];
                    }
                    else
                    {
                        Material[] newMats = new Material[] { _meatBlack, _meatBlack };
                        renderer.materials = newMats;
                    }
                }
                foreach (GameObject hideObject in _hideObject)
                {
                    if (_normal)
                    {
                        hideObject.SetActive(true);
                    }
                    else
                    {
                        hideObject.SetActive(false);
                    }
                }
                foreach (GameObject showObject in _showObject)
                {
                    if (_normal)
                    {
                        showObject.SetActive(false);
                    }
                    else
                    {
                        showObject.SetActive(true);
                    }
                }
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
                foreach (ParticleSystem particle in _secondParticles)
                {
                    if (!_normal && !particle.isPlaying) particle.Play();
                    else if (_normal) particle.Stop();
                }
                break;
        }
    }

    public void LightButton()
    {
        Renderer renderer = _hideButton.GetComponent<Renderer>();
        renderer.materials[0].EnableKeyword("_EMISSION");
        renderer.materials[0].SetColor("_EmissionColor", new Color(5.0f, 0, 0));
    }

    public void StartTimer()
    {
        _startTimer = true;
    }

    public void StopTimer()
    {
        _startTimer = false;
    }

    public int CountKnifePick()
    {
        return ++_kinfePickUpCount;
    }

}
