using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILockScreenObject
{
    public bool CanRotate();

    public bool CanControl();
}
