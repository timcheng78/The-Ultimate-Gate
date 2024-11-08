using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShinningButton : MonoBehaviour
{
    public Color emissionColor = Color.white;
    public float fadeSpeed = .5f;

    private SwitchItem _switchItem;
    private Renderer _renderer;
    private Material[] _materials;
    private float _emissionIntensity;
    private bool _isFadingIn = true;
    private bool _isClose = false;

    private void Start()
    {
        _switchItem = GetComponent<SwitchItem>();
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;

        _materials[0].EnableKeyword("_EMISSION");
        _emissionIntensity = 0f;
        UpdateEmission();
    }

    // Update is called once per frame
    void Update()
    {
        if (_switchItem._status) return;
        if (_isClose) return;
        if (_materials[0] != null)
        {
            ApplyShinning();
            UpdateEmission();
        }
    }

    public void CloseButton()
    {
        _isClose = true;
        gameObject.layer = 13;
        _emissionIntensity = 0f;
        UpdateEmission();
    }

    private void ApplyShinning()
    {
        // �ھ� isFadingIn �P�_�O�H�J�٬O�H�X
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
        // �]�m emission �C��M�j��
        Color finalColor = emissionColor * Mathf.LinearToGammaSpace(_emissionIntensity);
        _materials[0].SetColor("_EmissionColor", finalColor);
    }
}
