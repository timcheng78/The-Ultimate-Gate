using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InductionData
{
    public bool power;
    public int selectNumber;

    public InductionData() { }

    public InductionData(bool power, int selectNumber)
    {
        this.power = power;
        this.selectNumber = selectNumber;
    }
}
