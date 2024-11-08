using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PuzzleObject
{
    [SerializeField] private string _puzzleName;
    [SerializeField] private bool _isSolve = false;
    [SerializeField] private string[] _answer;
    [SerializeField] private GameObject _puzzleObj;

    public string PuzzleName { get => _puzzleName; set => _puzzleName = value; }
    public bool IsSolve { get => _isSolve; set => _isSolve = value; }
    public string[] Answer { get => _answer; set => _answer = value; }
    public GameObject PuzzleObj { get => _puzzleObj; set => _puzzleObj = value; }
}
