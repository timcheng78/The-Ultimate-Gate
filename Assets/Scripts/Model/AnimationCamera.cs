using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCamera : MonoBehaviour
{
    [SerializeField] private float _playSpeed = .05f;
    [SerializeField] private float _length = 1;
    [SerializeField] private float _pitch = 1;
    private bool _isPlaying = false;
    private CinemachineVirtualCamera _camera;
    private CinemachineTrackedDolly _dolly;
    private Action _callback;
    private void Awake()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
        _dolly = _camera.GetCinemachineComponent<CinemachineTrackedDolly>();
    }
    // Update is called once per frame
    void Update()
    {
        if (_isPlaying) PlayAnimation();
    }

    public void StartToPlay(Action callback)
    {
        PlayerMovement.Instance._footStepSoundFX.pitch = _pitch;
        PlayerMovement.Instance._footStepSoundFX.Play();
        _isPlaying = true;
        _callback = callback;
    }

    private void PlayAnimation()
    {
        _dolly.m_PathPosition += _playSpeed * Time.deltaTime;
        if (_dolly.m_PathPosition >= _length)
        {
            _isPlaying = false;
            PlayerMovement.Instance._footStepSoundFX.Pause();
            _callback.Invoke();
        }
    }
}
