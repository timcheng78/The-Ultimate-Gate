using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoButton : MonoBehaviour
{
    [SerializeField] private bool _status;
    [SerializeField] private int _index;
    [SerializeField] private string _color;

    public float fadeSpeed = .5f;
    private bool _alreadyPlay = false;
    private float _emissionIntensity = 0f;
    private bool _isFadingIn = true;
    private Renderer _renderer;
    private Material[] _materials;
    private PasswordPanelItem _passwordPanelItem;
    private DemoLockController _demoLockController;

    public string btnColor { get => _color; set => _color = value; }
    public bool Status { get => _status; set => _status = value; }
    public bool AlreadyPlay { get => _alreadyPlay; set => _alreadyPlay = value; }
    public int Index { get => _index; set => _index = value; }

    private void Awake()
    {
        // setting renderer
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;

        _passwordPanelItem = GetComponent<PasswordPanelItem>();
    }

    private void Start()
    {
        _demoLockController = _passwordPanelItem._puzzleController.GetComponent<DemoLockController>();
        if (!string.IsNullOrEmpty(_color)) _materials[0].EnableKeyword("_EMISSION");
    }

    // Update is called once per frame
    void Update()
    {
        if (!string.IsNullOrEmpty(_color) && PuzzleManagement.Instance.IsSolvePuzzle("demo", _color))
        {
            Material material = _materials[0];
            material.color = Color.black;
            _renderer.materials = new Material[1];
            _renderer.materials[0] = material;
            return;
        }
        if (_status && _alreadyPlay) PushButton();
        else if (!_status && _alreadyPlay) PullButton();
        if (string.IsNullOrEmpty(_color)) return;
        if (string.IsNullOrEmpty(_demoLockController.SelectColor))
        {
            if (_materials[0] != null)
            {
                ApplyShinning();
                UpdateEmission();
            }
        } 
        else
        {
            if (!_demoLockController.SelectColor.Equals(_color)) _materials[0].DisableKeyword("_EMISSION");
        }
    }

    private void PushButton()
    {
        if (!string.IsNullOrEmpty(_color)) AnimationManagement.Instance.Play($"demo_{_color}_button", "open");
        else AnimationManagement.Instance.Play($"demo_button_number_{_index}" , "open");
        _materials[0].EnableKeyword("_EMISSION");
        _alreadyPlay = false;
    }

    private void PullButton()
    {
        if (!string.IsNullOrEmpty(_color)) AnimationManagement.Instance.Play($"demo_{_color}_button", "close");
        else AnimationManagement.Instance.Play($"demo_button_number_{_index}" , "close");
        _materials[0].DisableKeyword("_EMISSION");
        _alreadyPlay = false;
    }

    private void ApplyShinning()
    {
        // 根據 isFadingIn 判斷是淡入還是淡出
        if (_isFadingIn)
        {
            _emissionIntensity += fadeSpeed * Time.deltaTime;
            if (_emissionIntensity >= 1f)
            {
                _emissionIntensity = 1f;
                _isFadingIn = false;
            }
        }
        else
        {
            _emissionIntensity -= fadeSpeed * Time.deltaTime;
            if (_emissionIntensity <= 0f)
            {
                _emissionIntensity = 0f;
                _isFadingIn = true;
            }
        }
    }

    private void UpdateEmission()
    {
        // 設置 emission 顏色和強度
        Color finalColor = Color.white * Mathf.LinearToGammaSpace(_emissionIntensity);
        _materials[0].SetColor("_EmissionColor", finalColor);
    }
}
