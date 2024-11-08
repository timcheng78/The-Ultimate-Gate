using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoofDictionary
{
    [SerializeField] private string _roomName;
    [SerializeField] private RoofObject _roofObj;

    public string RoomName { get => _roomName; set => _roomName = value; }
    public RoofObject RoofObj { get => _roofObj; set => _roofObj = value; }
}
