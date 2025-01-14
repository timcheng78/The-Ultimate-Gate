using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditLight : MonoBehaviour
{
    [SerializeField] private Light _spotLight;
    private MeshRenderer _render;
    private Material[] _materials;

    void Awake()
    {
        TryGetComponent<MeshRenderer>(out _render);
        _materials = _render.materials;
    }

    public bool IsLight()
    {
        return _spotLight.enabled;
    }

    public void TurnOnSpot()
    {
        _materials[^1].EnableKeyword("_EMISSION");
        _spotLight.enabled = true;
    }

    public void TurnOffSpot()
    {
        _materials[^1].DisableKeyword("_EMISSION");
        _spotLight.enabled = false;
    }
}
