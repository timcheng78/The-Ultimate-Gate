using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public bool hasFlashlight;
    public bool hasNotebook;
    public bool hasBinaryPaper;
    public float horizontalAxisValue;
    public float verticalAxisValue;
    public string location;
    public SerializableDictionary<int, bool> keys;
    public Vector3 position;
    public Vector3 scale;

    public PlayerData(bool hasFlashlight, bool hasNotebook, SerializableDictionary<int, bool> keys, Vector3 position, Vector3 scale)
    {
        this.hasFlashlight = hasFlashlight;
        this.hasNotebook = hasNotebook;
        this.keys = keys;
        this.position = position;
        this.scale = scale;
    }
}
