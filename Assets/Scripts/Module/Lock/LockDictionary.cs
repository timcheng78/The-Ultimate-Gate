using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LockDictionary
{
    [SerializeField] private string _roomName;
    [SerializeField] private LockObject[] _lockObjects;

    public string RoomName { get => _roomName; set => _roomName = value; }
    public LockObject[] LockObjects { get => _lockObjects; set => _lockObjects = value; }
}
