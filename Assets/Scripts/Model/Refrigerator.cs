using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refrigerator : MonoBehaviour
{
    private Renderer _renderer;
    private Material[] _materials;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;
    }

    // Update is called once per frame
    void Update()
    {
        if (Enviroment.Instance.IsElectrified)
        {
            // 沒電就黑掉
            _materials[^1].EnableKeyword("_EMISSION");
        }
        else
        {
            // 有電就亮起來
            _materials[^1].DisableKeyword("_EMISSION");
        }
    }
}
