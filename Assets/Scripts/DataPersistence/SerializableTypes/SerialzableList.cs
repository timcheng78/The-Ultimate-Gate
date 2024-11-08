using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public class SerialzableList<T> : List<T>, ISerializationCallbackReceiver
{
    [SerializeField] private List<T> values = new();
    public void OnBeforeSerialize()
    {
        values.Clear();
        foreach (T value in this)
        {
            values.Add(value);
        }
    }


    public void OnAfterDeserialize()
    {
        this.Clear();

        for (int i = 0; i < values.Count; i++)
        {
            this.Add(values[i]);
        }
    }
}
