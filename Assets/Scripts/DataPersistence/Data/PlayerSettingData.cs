using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSettingData
{
    [Header("Graphics Settings")]
    public int qualityIndex;
    public int screenResolutionWidth;
    public int screenResolutionHeight;
    public bool fullscreen;
    public bool postProcess;
    public float brightnessValue = -1;

    [Header("Audio Settings")]
    public float masterVolume;
    public float soundFXVolume;
    public float musicVolume;

    [Header("Control Settings")]
    public float mouseSensitivity;

    [Header("Others Settings")]
    public int localeLanguageIndex;

    public PlayerSettingData() {}
}
