using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class AnimationCamera : MonoBehaviour
{
    [SerializeField] private float _playSpeed = .05f;
    [SerializeField] private float _length = 1;
    [SerializeField] private float _pitch = 1;
    [SerializeField] private bool _needSpeedUp = false;
    [SerializeField] private Vector2 _speedUpVector;
    [SerializeField] private float _speedUpStrangth = 1f;
    [SerializeField] private bool _needLook = false;
    [SerializeField] private float _lookStart;
    [SerializeField] private float _lookEnd;
    [SerializeField] private Animator _lookAnimation;
    [SerializeField] private CreditLight[] _lights;
    [SerializeField] private GameObject _endLight;
    [SerializeField] private GameObject _screen;
    [SerializeField] private GameObject _newsPaper;
    [SerializeField] private PlayableDirector director;

    public int type = 0;
    private bool _isPlaying = false;
    private CinemachineVirtualCamera _camera;
    private CinemachineTrackedDolly _dolly;
    private Action _callback;
    private int index = 0;
    private bool _turnToggle = false;
    private bool _animationPlay = false;
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

    [ContextMenu("PlayTimeline")]
    private void PlayTimeLine()
    {
        director.Play();
    }

    [ContextMenu("PlayCamera")]
    private void PlayAnimation()
    {
        _isPlaying = true;
        if (_needLook && _dolly.m_PathPosition >= _lookStart && _dolly.m_PathPosition < _lookEnd)
        {
            if (!_animationPlay)
            {
                _lookAnimation.speed = .5f;
                _lookAnimation.Play("Normal Credit");
                _animationPlay = true;
            }
            float distance = _dolly.m_PathPosition - _lookStart - index;
            if (index < _lights.Length)
            {
                if (distance < .7f && distance > 0)
                {
                    _lights[index].TurnOnSpot();
                    _turnToggle = true;
                }
                else if (_turnToggle)
                {
                    index++;
                    _turnToggle = false;
                }
            }
            _playSpeed = .2f;
        } else if (_needLook && _dolly.m_PathPosition > _lookEnd && _endLight != null)
        {
            _endLight.SetActive(true);
            if (type == 1)
            {
                // normal end
                Renderer renderer = _screen.GetComponent<Renderer>();
                renderer.materials[^1].SetFloat("_ScanningActive", 1);
                _screen.GetComponent<ScreenColor>().ToggleScreenEmission();
            } 
            else if (type == 2)
            {
                _screen.SetActive(false);
                _newsPaper.SetActive(true);
                _playSpeed = .3f;
            }
        }
        if (_needSpeedUp && _dolly.m_PathPosition >= _speedUpVector.x && _dolly.m_PathPosition < _speedUpVector.y)
        {
            _dolly.m_PathPosition += _speedUpStrangth * Time.deltaTime;
        }
        else
        {
            _dolly.m_PathPosition += _playSpeed * Time.deltaTime;
        }
        if (_dolly.m_PathPosition >= _length)
        {
            _isPlaying = false;
            PlayerMovement.Instance._footStepSoundFX.Pause();
            if (_callback != null) _callback.Invoke();
        }
    }
}
