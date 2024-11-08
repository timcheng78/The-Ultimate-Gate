using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PuzzleDictionary
{
    [SerializeField] private string _roomName;
    [SerializeField] private PuzzleObject[] _puzzleObjects;

    public string RoomName { get => _roomName; set => _roomName = value; }
    public PuzzleObject[] PuzzleObjects { get => _puzzleObjects; set => _puzzleObjects = value; }

    public SerializableDictionary<string, PuzzleObject> ToDictionary()
    {
        SerializableDictionary<string, PuzzleObject> newPuzzles = new SerializableDictionary<string,PuzzleObject>();

        foreach (PuzzleObject puzzle in _puzzleObjects)
        {
            newPuzzles.Add(puzzle.PuzzleName, puzzle);
        }

        return newPuzzles;
    }
}
