using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LockObject
{
    [SerializeField] private string _puzzleName;
    [SerializeField] private bool _isLocked = true;
    [SerializeField] private bool _isOpen = false;
    [SerializeField] private GameObject _lockObj;

    public string PuzzleName { get => _puzzleName; set => _puzzleName = value; }
    public bool IsLocked { get => _isLocked; set => _isLocked = value; }
    public bool IsOpen { get => _isOpen; set => _isOpen = value; }
    public GameObject LockObj { get => _lockObj; set => _lockObj = value; }
}
