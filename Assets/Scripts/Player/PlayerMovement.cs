using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerAttributes _playerAttributes;
    private GameObject _mainCamera;
    private float _speed = 0f;
    private bool _hasMoveInput;
    private Vector3 _moveInput;
    private Vector3 _lookDirection;
    private Vector3 _velocity;
    private CharacterController _characterController;
    private bool _stopMoving = false;
    public AudioSource _footStepSoundFX;

    public static PlayerMovement Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        _mainCamera = Camera.main.gameObject;
        _characterController = GetComponent<CharacterController>();
        _playerAttributes = GetComponent<PlayerAttributes>();
        _footStepSoundFX = GetComponent<AudioSource>();
    }

    public void SetMoveInput(Vector3 input)
    {
        // check if player press the key or not
        _hasMoveInput = input.magnitude > 0.1f;
        _moveInput = _hasMoveInput ? input : Vector3.zero;
    }

    public void SetLookDirection(Vector3 direction)
    {
        // rotate the player
        _lookDirection = new Vector3(direction.x, 0f, direction.z).normalized;
    }

    public void ToggleMove(bool status)
    {
        _characterController.enabled = status;
        _stopMoving = !status;
        if (_stopMoving && _footStepSoundFX.isPlaying) _footStepSoundFX.Pause();
    }

    private void FixedUpdate()
    {
        if (!Enviroment.Instance.IsStartPlay || _stopMoving) return;
        ApplyGravity();
        ApplyRotation();
        ApplyMovement();
    }

    private void ApplyRotation()
    {
        _speed = 0;
        float targetRotation;

        if (_moveInput.magnitude < 0.1f)
        {
            _moveInput = Vector3.zero;
            return;
        }

        // move character
        if (_moveInput != Vector3.zero)
        {
            _speed = _playerAttributes._moveSpeed;
        }

        targetRotation = Quaternion.LookRotation(_lookDirection).eulerAngles.y + _mainCamera.transform.rotation.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, targetRotation, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, _playerAttributes._turnSpeed * Time.fixedDeltaTime); // smooth the rotation

        _moveInput = rotation * Vector3.forward;

    }

    private void ApplyMovement()
    {
        if (_characterController.enabled) _characterController.Move(_moveInput * _speed * Time.fixedDeltaTime);
        if (_moveInput != Vector3.zero)
        {
            if (!_footStepSoundFX.isPlaying) _footStepSoundFX.Play();
        } 
        else
        {
            if (_footStepSoundFX.isPlaying) _footStepSoundFX.Pause();
        }
    }

    private void ApplyGravity()
    {
        _velocity.y += _playerAttributes._gravity * Time.fixedDeltaTime;
        if (_characterController.enabled) _characterController.Move(_velocity * Time.fixedDeltaTime);
    }
}
