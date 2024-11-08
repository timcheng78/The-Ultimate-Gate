using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Lock
{
    [SerializeField] private LockDictionary[] locks;

    public SerializableDictionary<string, LockObject[]> ToDictionary()
    {
        SerializableDictionary<string, LockObject[]> newLock = new SerializableDictionary<string, LockObject[]>();

        foreach (LockDictionary dLock in locks)
        {
            newLock.Add(dLock.RoomName, dLock.LockObjects);
        }

        return newLock;
    }
}
