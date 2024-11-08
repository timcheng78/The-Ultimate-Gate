using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageSensor : MonoBehaviour
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
        if (!Enviroment.Instance.BedRoomEmission) return;

        ApplyEmission();
    }

    private void ApplyEmission()
    {
        _materials[0].EnableKeyword("_EMISSION");
    }

}
