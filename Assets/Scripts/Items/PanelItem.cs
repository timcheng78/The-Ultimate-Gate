using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelItem : MonoBehaviour, IInteractive
{
    [SerializeField] private string _location;
    [SerializeField] private string _puzzleName;
    [SerializeField] private string _type;
    [SerializeField] private bool _status = false;
    [SerializeField] private int _action;
    [SerializeField] private InductionController _inductionController;
    [SerializeField] private AudioClip _clickSoundClip;

    public float _maxEmissionIntensity = 5f;
    public float _minEmissionIntensity = 0f;
    public float _changeSpeed = 1f;

    private Renderer _renderer;
    private Material[] _materials;
    private bool _triggerPoint = false;
    private float _currentIntensity = 0f;
    private bool _isIncreasing = true;

    public string Type { get => _type; set => _type = value; }
    public bool Status { get => _status; set => _status = value; }
    public bool TriggerPoint { get => _triggerPoint; set => _triggerPoint = value; }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;
    }

    public void Cancel()
    {
        return;
    }

    public void HoverIn()
    {
        // 基本條件 + 非開關類必須要開啟才能操作 + 開關類開啟後就不能操作
        if (!CanTrigger() || 
            (!_inductionController.CheckPower() && !_action.Equals(0)) ||
            (_inductionController.CheckPower() && _action.Equals(0))) return;
        HoverInItem();
    }

    public void HoverOut()
    {
        HoverOutItem();
    }

    public void Interact()
    {
        // 基本條件 + 非開關類必須要開啟才能操作 + 開關類開啟後就不能操作
        if (!CanTrigger() ||
            (!_inductionController.CheckPower() && !_action.Equals(0)) ||
            (_inductionController.CheckPower() && _action.Equals(0))) return;
        TouchItem();
    }

    private void Update()
    {
        //if (_status) _materials[^1].color = new Color(1, 0, 0, 1);
        if (_inductionController.CheckPower())
        {
            _materials[^1].EnableKeyword("_EMISSION");
            _materials[^1].SetColor("_EmissionColor", new Color(1, 1, 1, 1));
        }
        else if (!_action.Equals(0)) _materials[^1].color = new Color32(69, 69, 69, 255);
        else if (_action.Equals(0))
        {
            // power
            if (Enviroment.Instance.IsElectrified)
            {
                _materials[^1].EnableKeyword("_EMISSION");
                if (_isIncreasing)
                {
                    _currentIntensity += _changeSpeed * Time.deltaTime;
                    if (_currentIntensity >= _maxEmissionIntensity)
                    {
                        _currentIntensity = _maxEmissionIntensity;
                        _isIncreasing = false;
                    }
                }
                else
                {
                    _currentIntensity -= _changeSpeed * Time.deltaTime;
                    if (_currentIntensity <= _minEmissionIntensity)
                    {
                        _currentIntensity = _minEmissionIntensity;
                        _isIncreasing = true;
                    }
                }
                SetEmissionIntensity(_materials[^1], _currentIntensity);
            }
        }
    }

    private void SetEmissionIntensity(Material material, float intensity)
    {
        Color emissionColor = Color.white * Mathf.LinearToGammaSpace(intensity);
        material.SetColor("_EmissionColor", emissionColor);
    }

    private void HoverInItem()
    {
        _materials[^1].EnableKeyword("_EMISSION");
        DialogManagement.Instance.interactDialog.SetActive(true);
    }

    private void HoverOutItem()
    {
        _materials[^1].DisableKeyword("_EMISSION");
        DialogManagement.Instance.interactDialog.SetActive(false);
    }

    private void TouchItem()
    {
        // play sound
        SoundManagement.Instance.PlaySoundFXClip(_clickSoundClip, transform, 1f);
        switch (_action)
        {
            case -1:
                _inductionController.SubFire();
                PuzzleManagement.Instance.PreCheckAnswer(StartCheck);
                break;
            case 0:
                _inductionController.TurnOnPower();
                _status = true;
                break;
            case 1:
                _inductionController.AddFire();
                PuzzleManagement.Instance.PreCheckAnswer(StartCheck);
                break;
        }
    }

    private bool CanTrigger()
    {
        // 條件一: 沒電
        // 條件二: 玩家是否有拿東西
        // 條件三: 是否開啟手電筒
        // 條件四: 是否有開燈
        // 權重: 2 > 1 > 3 > 4
        if (PlayerAttributes.Instance._activingItem) return false;
        if (!Enviroment.Instance.IsElectrified) return false;
        if (PlayerAttributes.Instance._isTriggerFlashlight) return true;
        if (LightManagement.Instance.CheckLightExist(_location)) return true;
        return false;
    }

    private void StartCheck()
    {
        SharedUtils.StartCheckAnswer(_location, _puzzleName, "door");
    }
}
