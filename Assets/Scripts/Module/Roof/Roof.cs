using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Roof
{
    [SerializeField] private RoofDictionary[] roofs;

    public Dictionary<string, RoofObject> ToDictionary()
    {
        Dictionary<string, RoofObject> newRoof = new Dictionary<string, RoofObject>();

        foreach (RoofDictionary roof in roofs)
        {
            newRoof.Add(roof.RoomName, roof.RoofObj);
        }

        return newRoof;
    }
}
