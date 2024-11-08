using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Animating
{
    [SerializeField] private AnimatorObject[] _animatorObjects;

    public Dictionary<string, Animator> ToDictionary()
    {
        Dictionary<string, Animator> newAnimating = new Dictionary<string, Animator>();

        foreach (AnimatorObject animatorObject in _animatorObjects)
        {
            newAnimating.Add(animatorObject.animationName, animatorObject.animator);
        }

        return newAnimating;
    }
}
