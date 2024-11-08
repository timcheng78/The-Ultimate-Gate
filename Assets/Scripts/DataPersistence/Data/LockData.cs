using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LockData
{
    public SerializableDictionary<string, SerializableDictionary<string, SerializableDictionary<string, bool>>> locks;

    public LockData() { }

    public SerializableDictionary<string, SerializableDictionary<string, SerializableDictionary<string, bool>>> ObjectToLockData(SerializableDictionary<string, LockObject[]> dictionary)
    {
        SerializableDictionary<string, SerializableDictionary<string, SerializableDictionary<string, bool>>> result = new();
        foreach (KeyValuePair<string, LockObject[]> pair in dictionary)
        {
            string location = pair.Key;
            result[location] = new();
            foreach (LockObject lockObject in dictionary[location])
            {
                result[location][lockObject.PuzzleName] = new();
                result[location][lockObject.PuzzleName]["isLocked"] = lockObject.IsLocked;
                result[location][lockObject.PuzzleName]["isOpen"] = lockObject.IsOpen;
                
            }
        }
        return result;
    }

    public void SetLocksValue(SerializableDictionary<string, LockObject[]> dictionary)
    {
        foreach (KeyValuePair<string, LockObject[]> pair in dictionary)
        {
            string location = pair.Key;
            foreach (LockObject lockObject in dictionary[location])
            {
                lockObject.IsLocked = locks[location][lockObject.PuzzleName]["isLocked"];
                lockObject.IsOpen = locks[location][lockObject.PuzzleName]["isOpen"];
            }
        }
    }
}
