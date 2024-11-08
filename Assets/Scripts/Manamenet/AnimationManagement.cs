using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimationManagement : MonoBehaviour
{
    [SerializeField] private Animating _animating;
    [SerializeField] private Transform _finalDoor;
    private Dictionary<string, Animator> _animator;

    public static AnimationManagement Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one AnimationManagement in the scene.");
        }
        Instance = this;
        _animator = _animating.ToDictionary();
    }
    
    public void Play(string animationName, string action)
    {
        if (animationName.Equals("bath_room_2_final_puzzle"))
        {
            // 特殊處理
            StartCoroutine(OpenFinalDoor());
        } 
        else
        {
            _animator[animationName].Play($"{animationName}_{action}", -1, 0f);
        }
    }

    public void Stop(string animationName)
    {
        _animator[animationName].StopPlayback();
    }

    public void ToggleAnimating(string animationName, bool isPaly)
    {
        _animator[animationName].Play(animationName);
        if (isPaly) _animator[animationName].speed = 1;
        else _animator[animationName].speed = 0;
    }

    private IEnumerator OpenFinalDoor()
    {
        float duration = 1.0f;
        Quaternion startRotation = _finalDoor.rotation;
        Quaternion endRotation = Quaternion.Euler(_finalDoor.eulerAngles.x, _finalDoor.eulerAngles.y, _finalDoor.eulerAngles.z - 90f);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            _finalDoor.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 保證最終達到目標角度
        _finalDoor.rotation = endRotation;
    }
}