using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PuzzleData
{
    public SerializableDictionary<string, SerializableDictionary<string, SerializableDictionary<string, bool>>> puzzles;

    public PuzzleData() { }

    public SerializableDictionary<string, SerializableDictionary<string, SerializableDictionary<string, bool>>> ObjectToPuzzleData
        (SerializableDictionary<string, SerializableDictionary<string, PuzzleObject>> dictionary)
    {
        SerializableDictionary<string, SerializableDictionary<string, SerializableDictionary<string, bool>>> result = new();
        foreach (KeyValuePair<string, SerializableDictionary<string, PuzzleObject>> pair in dictionary)
        {
            string location = pair.Key;
            result[location] = new();
            foreach (KeyValuePair<string, PuzzleObject> secPair in dictionary[location])
            {
                string puzzleName = secPair.Key;
                result[location][puzzleName] = new();
                result[location][puzzleName]["isSolve"] = dictionary[location][puzzleName].IsSolve;
            }
        }
        return result;
    }

    public void SetPuzzleValue(SerializableDictionary<string, SerializableDictionary<string, PuzzleObject>> dictionary)
    {
        foreach (KeyValuePair<string, SerializableDictionary<string, PuzzleObject>> pair in dictionary)
        {
            string location = pair.Key;
            foreach (KeyValuePair<string, PuzzleObject> secPair in dictionary[location])
            {
                string puzzleName = secPair.Key;
                dictionary[location][puzzleName].IsSolve = puzzles[location][puzzleName]["isSolve"];
            }
        }
    }

}
