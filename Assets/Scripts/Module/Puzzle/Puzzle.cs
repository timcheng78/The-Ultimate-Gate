using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Puzzle
{
    [SerializeField] private PuzzleDictionary[] puzzles;

    public SerializableDictionary<string, SerializableDictionary<string, PuzzleObject>> ToDictionary()
    {
        SerializableDictionary<string, SerializableDictionary<string, PuzzleObject>> newPuzzles = new SerializableDictionary<string, SerializableDictionary<string, PuzzleObject>>();

        foreach (PuzzleDictionary puzzle in puzzles)
        {
            newPuzzles.Add(puzzle.RoomName, puzzle.ToDictionary());
        }

        return newPuzzles;
    }
}
