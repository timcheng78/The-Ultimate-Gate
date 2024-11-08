using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    private SerializableDictionary<int, bool> keys = new SerializableDictionary<int, bool>
    {
        { 1, false },
        { 2, false },
        { 3, false },
        { 4, false }
    };
    // defined
    public PlayerData playerData;
    public LockData lockData;
    public PuzzleData puzzleData;
    public RecordData recordData;
    public SerializableDictionary<string, string> books = new();
    public SerializableDictionary<string, bool> switchItems = new();
    public SerializableDictionary<string, bool> lightsData = new();
    public SerializableDictionary<string, InductionData> inductionData = new();
    public SerializableDictionary<string, bool> roomNormalStatus = new();
    public bool is_electrified;
    public bool hard_mode = false;
    public bool is_endgame_file = false;
    public bool trigger_incomplete_summoning = false;
    public int level;
    public int step;
    public bool bedRoomEmission;
    public float timer = 0.0f;
    public GameData()
    {
        playerData = new(false, false, keys, Vector3.zero, new (0.65f, 1f, 0.65f));
        recordData = new();
        lockData = new();
        puzzleData = new();
    }
}
