using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSensorItem : MonoBehaviour, IInteractive
{
    [SerializeField] private string _location;
    [SerializeField] private bool _needLight = true;
    [SerializeField] private float _waitingSecond = 3.0f;
    [SerializeField] private GameObject[] _lightObjects;
    [SerializeField] private int _sortNumber;
    [SerializeField] private LightPuzzleController _controller;
    [SerializeField] private float _lightSpeed = 1.0f;

    [Header("Plot Detail")]
    [SerializeField] private string _plotName;

    private bool _startTimer = false;
    private bool _isLight = false;
    private float _currentIntensity = 0.0f;
    private float _timer_f = 0f;
    private int _timer_i = 0;
    private List<Material[]> _materials = new();
    private Color _baseEmissionColor;

    private void Start()
    {
        if (_lightObjects.Length.Equals(0)) return;
        foreach (GameObject gameObject in _lightObjects)
        {
            _materials.Add(gameObject.GetComponent<Renderer>().materials);
            _baseEmissionColor = _materials[0][^1].color;
        }
    }

    public void HoverIn()
    {
        if (_needLight) StartSensor();
        else
        {
            if (_plotName.Equals("map"))
            {
                // 地圖特殊規則
                if (PlayerAttributes.Instance._isTriggerFlashlight)
                {
                    if (LightManagement.Instance.CheckLightExist(_location))
                    {
                        // 有開燈手電筒照到
                        SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, _plotName + "_flashlight", "t" });
                    }
                    else
                    {
                        // 沒開燈手電筒照到
                        SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, _plotName + "_nolight_flashlight", "t" });
                    }

                }
            } 
            else
            {
                SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, _plotName, "t" });
                if (_plotName.Equals("light") && _location.Equals("book_room")) SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_LIGHT_WITHOUT_BRILLIANCE);
            }
        }
    }

    public void HoverOut()
    {
        if (_needLight) CloseSensor();
    }

    public void Interact()
    {
        return;
    }

    public void Cancel()
    {
        return;
    }

    public bool IsLight()
    {
        return _isLight;
    }

    private void StartSensor()
    {
        if (!PlayerAttributes.Instance._isTriggerFlashlight)
        {
            ResetTimes();
            return;
        }
        _startTimer = true;
    }
    
    private void CloseSensor()
    {
        ResetTimes();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_needLight) return;
        StartTimes();
        if (_timer_i >= _waitingSecond && !_isLight) LightUp();
    }

    private void StartTimes()
    {
        if (_startTimer)
        {
            _timer_f += Time.deltaTime;
            _timer_i = (int)_timer_f;
            _currentIntensity += _lightSpeed * Time.deltaTime;
        }
        else
        {
            _currentIntensity -= _lightSpeed * Time.deltaTime;
            if (_currentIntensity < 0.0f) _currentIntensity = 0.0f;
        }
        SetEmissionIntensity(_currentIntensity);
    }

    private void ResetTimes()
    {
        _timer_f = 0f;
        _timer_i = 0;
        _startTimer = false;
    }

    private void LightUp()
    {
        SetEmissionIntensity(500.0f);
        if (_controller) _controller.SetIndex(_sortNumber);
         _isLight = true;
        _needLight = false;
    }

    public void LightDown()
    {
        SetEmissionIntensity(0.0f);
        _currentIntensity = 0.0f;
        CloseSensor();
         _isLight = false;
        _needLight = true;
    }

    private void SetEmissionIntensity(float intensity)
    {
        Color emissionColor = _baseEmissionColor * Mathf.LinearToGammaSpace(intensity);
        foreach (Material[] materials in _materials)
        {
            materials[^1].SetColor("_EmissionColor", emissionColor);
        }
    }
}
