using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class Photo
{
    public Texture2D texture;
    public Vector2 position;
    public bool isDragging;
    public float rotation;
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Light _flashLight;
    [SerializeField] private AudioClip _flashLightOnSoundClip;
    [SerializeField] private AudioClip _flashLightOffSoundClip;
    [SerializeField] private AutoFlip _book;
    [SerializeField] private float _speed;
    [SerializeField] private float _zoomInFOV = 20f;
    [SerializeField] private float _zoomOutFOV = 60f;
    [SerializeField] private float _zoomSpeed = 10f;

    public bool _isRightClickHold = false;
    private CameraAttributes _cameraAttributes;
    private InteractController _interactController;
    private PlayerAttributes _playerAttributes;
    private PlayerMovement _playerMovement;
    private PlayerMoveInput _playerMoveInput;
    private Vector3 _moveInput;
    private CinemachineTransposer _transposer;
    private CursorLockMode _originMode;
    private bool _originCourse;
    private bool _isObjectMove = false;
    private bool _isWaiting = false;
    private bool _stopRotate = false;
    private bool _screenDraw = false;
    private bool _photoMode = false;
    private GameObject _isDragging;
    private int _countTimes = 0;
    public static PlayerController Instance { get; private set; }
    public bool StopRotate { get => _stopRotate; set => _stopRotate = value; }
    public bool ScreenDraw { get => _screenDraw; }
    public bool PhotoMode { get => _photoMode; }
    public GameObject IsDragging { get => _isDragging; set => _isDragging = value; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        _playerAttributes = GetComponent<PlayerAttributes>();
        _playerMovement = GetComponent<PlayerMovement>();
        _transposer = CameraManagement.Instance.playerVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        _interactController = Camera.main.GetComponent<InteractController>();
        _playerMoveInput = new PlayerMoveInput();

        // event binding
        //_playerMoveInput.Player.Move.canceled += _ => { GetComponent<AudioSource>().Pause(); };
        _playerMoveInput.Player.Crouch.performed += _ => { ReadyToCrouch(); };
        _playerMoveInput.Player.Crouch.canceled += _ => { StartCoroutine(Standup()); };
        _playerMoveInput.Player.SwitchPaint.performed += ctx => SwitchPaintType(ctx);
        // temp
        CursorSwitch(false);
    }
    public void OnEscape(InputValue value = null)
    {
        if (_screenDraw)
        {
            OnTabSwitch();
            return;
        }
        // 加入取消互動
        if (_playerAttributes._activingItem)
        {
            OnCancel();
            return;
        }
        int type = 0;
        if (DataPersistenceManagement.Instance) DataPersistenceManagement.Instance.SaveGame();
        // 關閉選項菜單
        if (Enviroment.Instance.IsPause && OptionMenu.Instance && OptionMenu.Instance.canvas.activeSelf) PauseMenu.Instance.ToggleOptionMenu();
        else if (PauseMenu.Instance.controlBlock.activeSelf) PauseMenu.Instance.ToggleControlBlock(false);
        else
        {
            type = 1;
            PauseMenu.Instance.TogglePauseMenu();
            Enviroment.Instance.IsPause = !Enviroment.Instance.IsPause;
        }
        if (Enviroment.Instance.IsPause && type.Equals(1))
        {
            // add sentences
            if (_countTimes != 0 && _countTimes % 5 == 0) SubtitleManagement.Instance.AddSentencesToShow("Close Statement Table", new string[] { "any", "esc", "t" });
            _countTimes++;
            // start pause
            _originCourse = _cameraAttributes._cursorVisable;
            _originMode = _cameraAttributes._cursorLockMode;
            CursorSwitch(true);
            Time.timeScale = 0;
        }
        else if (!Enviroment.Instance.IsPause && type.Equals(1))
        {
            // cancel pause
            _cameraAttributes._cursorVisable = _originCourse;
            _cameraAttributes._cursorLockMode = _originMode;
            Time.timeScale = 1;
            _cameraAttributes.ApplyMouseSensitive();
        }
    }

    public void OnLook(InputValue value)
    {
        return;
    }

    /// <summary>
    /// 開啟手電筒
    /// </summary>
    /// <param name="value">null</param>
    public void OnFlashLight(InputValue value = null)
    {
        // 開啟條件 1: 必須要有手電筒; 2: 不能正在畫畫; 3: 不能暫停
        if (!_playerAttributes._hasFlashlight || _screenDraw || Enviroment.Instance.IsPause) return;
        _flashLight.enabled = !_flashLight.enabled;
        _playerAttributes._isTriggerFlashlight = !_playerAttributes._isTriggerFlashlight;
        if (_playerAttributes._isTriggerFlashlight) SoundManagement.Instance.PlaySoundFXClip(_flashLightOnSoundClip, transform, 1f);
        else SoundManagement.Instance.PlaySoundFXClip(_flashLightOffSoundClip, transform, 1f);
        SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { "any", "flashlight", "t" });
    }

    private void ReadyToCrouch()
    {
        if (_screenDraw || Enviroment.Instance.IsPause) return;
        StartCoroutine(Crouch());
    }

    private IEnumerator Crouch()
    {
        float lastAxisY;
        bool continueLoop = true;
        _playerAttributes._moveSpeed = 1;
        while (transform.localScale.y > 0.6f && _transposer.m_FollowOffset.y > 0.1 && continueLoop)
        {
            Vector3 subFloatV = new Vector3(0, 0.03f, 0) * Time.deltaTime * 60;
            if ((transform.localScale + subFloatV).y <= 0.6f) transform.localScale = new Vector3(transform.localScale.x, .6f, transform.localScale.z);
            else if (transform.localScale.y > .6f) transform.localScale -= subFloatV;
            if ((_transposer.m_FollowOffset + subFloatV).y <= .1f) _transposer.m_FollowOffset.y = .1f;
            else if (_transposer.m_FollowOffset.y > 0.1f) _transposer.m_FollowOffset -= subFloatV;
            lastAxisY = transform.localScale.y;
            yield return null;
            if (lastAxisY != transform.localScale.y) continueLoop = false;
        }
    }

    private IEnumerator Standup()
    {
        _playerAttributes._moveSpeed = 3;
        while (transform.localScale.y < _playerAttributes._height.y && _transposer.m_FollowOffset.y < 0.6)
        {
            Vector3 subFloatV = new Vector3(0, 0.05f, 0) * _speed * Time.deltaTime;
            if ((transform.localScale + subFloatV).y >= _playerAttributes._height.y) transform.localScale = new Vector3(transform.localScale.x, _playerAttributes._height.y, transform.localScale.z);
            else if (transform.localScale.y < _playerAttributes._height.y) transform.localScale += subFloatV;
            if ((_transposer.m_FollowOffset + subFloatV).y >= .6f) _transposer.m_FollowOffset.y = .6f;
            else if (_transposer.m_FollowOffset.y < 0.6f) _transposer.m_FollowOffset += subFloatV;
            yield return null;
        }
    }

    private void OnViewIn(InputValue value)
    {
        if (!Enviroment.Instance.IsStartPlay || _screenDraw || Enviroment.Instance.IsPause) return;
        if (value.isPressed)
        {
            // zoom in
            _isRightClickHold = true;
        }
        else
        {
            // zoom out
            _isRightClickHold = false;
        }
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        if (Enviroment.Instance.IsPause)
        {
            _moveInput = Vector3.zero;
            _isObjectMove = false;
            return;
        }
        if (_playerAttributes._activingItem && _playerAttributes._activingItem.GetComponent<DragDropItem>() == null)
        {
            if (DialogManagement.Instance.noteBookCanvas.activeSelf)
            {
                _isObjectMove = false;
                _moveInput = Vector3.zero;
                if (input == Vector2.zero) return;
                if (input.x > 0f) _book.FlipRightPage();
                else if (input.x < 0f) _book.FlipLeftPage();
            }
            else if (_playerAttributes._activingItem.GetComponent<BookItem>() != null)
            {
                BookItem item = _playerAttributes._activingItem.GetComponent<BookItem>();
                _isObjectMove = false;
                _moveInput = Vector3.zero;
                if (input == Vector2.zero) return;
                if (input.x > 0f) item._book.FlipRightPage();
                else if (input.x < 0f) item._book.FlipLeftPage();
            }
            else
            {
                _moveInput = new Vector3(input.x * 0.001f, input.y * 0.001f, 0f);
                _isObjectMove = true;
            }
        } 
        else
        {
            _moveInput = new Vector3(input.x, 0f, input.y);
            _isObjectMove = false;
        }
    }

    public void OnInteract(InputValue value)
    {
        if (Enviroment.Instance.IsPause || _screenDraw) return;
        if (!_cameraAttributes._screenLock)
        {
            _interactController.Interact();
        } 
    }

    public void OnNotebook(InputValue value)
    {
        // open book
        if (DialogManagement.Instance._animating || _cameraAttributes._screenLock || !_playerAttributes._hasNotebook || !Enviroment.Instance.IsStartPlay || Enviroment.Instance.IsPause) return;
        ToggleNoteBook(true);
        _book.ControledBook.SettingPage();
    }

    public void OnTabSwitch(InputValue value = null)
    {
        if (!Enviroment.Instance.IsStartPlay || Enviroment.Instance.IsPause) return;
        _screenDraw = !_screenDraw;
        // trigger draw
        if (_playerAttributes._activingItem)
        {
            PlayerDraw.Instance.ToggleDrawPage(_screenDraw);
        }
        else
        {
            ToggleViewScreen(_screenDraw);
            PlayerDraw.Instance.ToggleDrawPage(_screenDraw);
        }
        if (_screenDraw) DialogManagement.Instance.CloseRotateHint();
    }

    public void OnTakePhoto(InputValue value)
    {
        if (!Enviroment.Instance.IsStartPlay || Enviroment.Instance.IsPause || _screenDraw) return;
        if (DialogManagement.Instance.maskCanvas.activeSelf) return;
        StartCoroutine(PlayerPhotoSystem.Instance.TakePhotoWithEffects());
    }

    public void OnDeletePhoto(InputValue value)
    {
        if (!Enviroment.Instance.IsStartPlay || Enviroment.Instance.IsPause) return;
        PlayerPhotoSystem.Instance.DeletePhoto();
    }

    public void OnResetRotation(InputValue value)
    {
        _interactController.ResetRotation();
    }

    public void OnPhotoMode(InputValue value)
    {
        // 拍照模式
        if (!Enviroment.Instance.IsDebug) return;
        _photoMode = !_photoMode;

        Timer.Instance.gameObject.SetActive(!_photoMode);
        DialogManagement.Instance.accurateImage.SetActive(!_photoMode);
        PlayerDraw.Instance.DrawCanvas.SetActive(!_photoMode);
    }

    public void OnCancel(InputValue value = null)
    {
        if (Enviroment.Instance.IsPause) return;
        if (_screenDraw)
        {
            OnTabSwitch();
            return;
        }
        if (DialogManagement.Instance.noteBookCanvas.activeSelf)
        {
            if (!_book.ControledBook.pageDragEnd) return;
            if (DialogManagement.Instance._animating) return;
            if (!_isWaiting) StartCoroutine(WaitAsec());
        } 
        else
        {
            _interactController.Cancel();
            if (_stopRotate)
            {
                // 代表 tab switch 過
                OnTabSwitch();
            }
        }
    }

    private void SwitchPaintType(InputAction.CallbackContext context)
    {
        if (!Enviroment.Instance.IsStartPlay || !_screenDraw || Enviroment.Instance.IsPause) return;
        if (context.control.name.Equals("backquote"))
        {
            PlayerDraw.Instance.PaintType = 5;
        } 
        else
        {
            string keyName = context.control.name;
            if (keyName.IndexOf("numpad") > -1)
            {
                keyName = keyName.Substring(6);
            }
            int type = int.Parse(keyName) - 1;
            if (type < 5) PlayerDraw.Instance.PaintType = type;
            else PlayerDraw.Instance.ClearDrawPage();
        }
    }

    private IEnumerator WaitAsec()
    {
        _isWaiting = true;
        yield return null;
        ToggleNoteBook(false);
        _isWaiting = false;
    }

    private void ToggleNoteBook(bool status)
    {
        ToggleViewScreen(status);
        //CameraManagement.Instance.TogglePlayerVirtualCamera(!status);
        _playerAttributes._activingItem = status ? _book.ControledBook.gameObject : null;
        DialogManagement.Instance.ToggleAccurateImage(!status);
        DialogManagement.Instance.ToggleNoteBookCanvas(status);
        //DialogManagement.Instance.ToggleInteractHelp1(status);
        //DialogManagement.Instance.ToggleInteractHelp3(status);
        //DialogManagement.Instance.ToggleInteractHelp4(status);
    }

    private void ToggleViewScreen(bool status)
    {
        _playerMovement.ToggleMove(!status);
        _cameraAttributes._cursorVisable = status;
        _cameraAttributes._cursorLockMode = status ? CursorLockMode.Confined : CursorLockMode.Locked;
        _cameraAttributes._screenLock = status;
        if (status) Camera.main.GetComponent<CameraAttributes>().ZeroCameraSpeed();
        else StartCoroutine(Camera.main.GetComponent<CameraAttributes>().RestoreCameraSpeed());
    }

    private void Update()
    {
        if (_playerAttributes._activingItem && _playerAttributes._activingItem.GetComponent<InteractItem>() != null)
        {
            if (!_isObjectMove) return;
            // move object
            _playerAttributes._activingItem.transform.localPosition += _moveInput;
        }

        if (_playerMovement == null || !Enviroment.Instance.IsStartPlay) return;
        
        if (!_isObjectMove)
        {
            _playerMovement.SetMoveInput(_moveInput);
            _playerMovement.SetLookDirection(_moveInput);
        }
        else
        {
            _playerMovement.SetMoveInput(Vector3.zero);
            _playerMovement.SetLookDirection(Vector3.zero);
        }

        if (_isRightClickHold && _playerAttributes._activingItem == null)
        {
            CameraManagement.Instance.playerVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(CameraManagement.Instance.playerVirtualCamera.m_Lens.FieldOfView, _zoomInFOV, Time.deltaTime * _zoomSpeed);
        }
        else
        {
            CameraManagement.Instance.playerVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(CameraManagement.Instance.playerVirtualCamera.m_Lens.FieldOfView, _zoomOutFOV, Time.deltaTime * _zoomSpeed);
        }
    }

    private void CursorSwitch(bool bol)
    {
        _cameraAttributes._cursorVisable = bol;
        _cameraAttributes._cursorLockMode = bol ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void SetPlayerStartPosition()
    {

        transform.parent.position = new Vector3(7.39300013f, 0.850000024f, 4.04400015f);
        transform.localPosition = Vector3.zero;
    }

    #region - Enable / Disable

    private void OnEnable()
    {
        _playerMoveInput.Enable();
    }

    private void OnDisable()
    {
        _playerMoveInput.Disable();
    }

    #endregion
}
