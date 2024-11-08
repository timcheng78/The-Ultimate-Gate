using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLightIcon : MonoBehaviour
{
    [SerializeField] private Texture2D _trueIcon;
    [SerializeField] private Texture2D _falseIcon;

    private Renderer _renderer;
    private Material[] _materials;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;
    }

    void Update()
    {
        if (PlayerAttributes.Instance._isTriggerFlashlight) _materials[^1].SetTexture("_Icon", _falseIcon);
        else _materials[^1].SetTexture("_Icon", _trueIcon);
    }
}
