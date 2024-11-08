using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoofObject
{
    [SerializeField] private bool _status = false;
    [SerializeField] private GameObject _roofGameObject;
    [SerializeField, ColorUsage(true, true)] private Color _weirdColor;
    [SerializeField] private Light[] _lights;
    [SerializeField] private ReflectionProbe[] _reflectionProbes;

    public bool Status { get => _status; set => _status = value; }
    public GameObject RoofGameObject { get => _roofGameObject; set => _roofGameObject = value; }
    public Color WeirdColor { get => _weirdColor; set => _weirdColor = value; }
    public Light[] Lights { get => _lights; set => _lights = value; }
    public ReflectionProbe[] ReflectionProbes { get => _reflectionProbes; set => _reflectionProbes = value; }
}
