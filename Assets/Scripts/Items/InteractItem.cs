using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractItem : MonoBehaviour, IInteractive, ILockScreenObject
{
    [SerializeField] private string _location;
    [SerializeField] private bool _canRotate = false;
    [SerializeField] private bool _canControl = false;
    [SerializeField] private bool _isEmergency = false;
    [SerializeField] private Vector3 _interactRotation;
    [SerializeField] private Vector3 _localScaleResize;
    [SerializeField] private GameObject _rotationCameraObject;
    [SerializeField] private Vector2 _rotationLimitX;
    [SerializeField] private Vector2 _rotationLimitY;
    [SerializeField] private Camera _rotationCamera;
    [SerializeField] private GameObject _reflectProbe;
    [SerializeField] private GameObject _numberObject;

    [Header("Plot Detail")]
    [SerializeField] private string _plotName;
    [SerializeField] private bool _isInstant = false;
    //[SerializeField] private int _hintPerSec = 300;
    [SerializeField] private int _plotPerSec = 30;
    [SerializeField] private bool _isMutation = false;

    [Header("Rotate Hint")]
    [SerializeField] private float _idleTime = 30f;

    private float _currentTime = 0f;
    private Renderer _renderer;
    private Material[] _materials;
    private Vector2 _rotation;
    private Transform _originParent;
    private Vector3 _originPosition;
    private Quaternion _originRotation;
    private Vector3 _originScale;
    private PlayerMoveInput _playerMoveInput;
    private float _mouseScrollY;
    private float _timer_f = 0f;
    private int _timer_i = 0;
    private bool _isActiving = false;
    private bool _rotating = false;
    private bool _isIdle = false;

    private void Awake()
    {
        // setting renderer
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;

        // setting origin transform
        _originPosition = transform.position;
        _originRotation = transform.rotation;
        _originScale = transform.localScale;
        _originParent = transform.parent;

        // binding rotatable item
        if (_canRotate)
        {
            _playerMoveInput = new PlayerMoveInput();
            _playerMoveInput.RotatableItem.Press.performed += value => { StartCoroutine(Rotate(value.control.name)); };
            _playerMoveInput.RotatableItem.Press.canceled += _ => { _rotating = false; };
            _playerMoveInput.RotatableItem.Axis.performed += context => { _rotation = context.ReadValue<Vector2>(); };
            _playerMoveInput.Player.Scale.performed += x => { _mouseScrollY = x.ReadValue<float>(); };
        }
    }

    public void HoverIn()
    {
        if (!CanTrigger()) return;
        HoverInItem();
    }

    public void HoverOut()
    {
        HoverOutItem();
    }

    public void Cancel()
    {
        PullBack();
    }

    public bool CanControl()
    {
        return _canControl;
    }

    public bool CanRotate()
    {
        return _canRotate;
    }

    public void Interact()
    {
        if (!CanTrigger()) return;
        if (CanRotate())
        {
            PickUpRotate();
            SetAchievement();
        }
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(_interactRotation);
    }

    private void Update()
    {
        if (!_isActiving || Enviroment.Instance.IsPause) return;
        // 互動中且非暫停開始計時
        _timer_f += Time.deltaTime;
        _timer_i = (int)_timer_f;
        if (_mouseScrollY > 0)
        {
            transform.localScale *= 1.2f;
        }
        else if (_mouseScrollY < 0)
        {
            transform.localScale *= 0.8f;
        }
        if (!_isInstant) CheckingAddSentences();
        CheckNeedShowHint();
    }

    private void HoverInItem()
    {
        _materials[^1].SetFloat("_IsActive", 1);
        DialogManagement.Instance.interactDialog.SetActive(true);
    }

    private void HoverOutItem()
    {
        _materials[^1].SetFloat("_IsActive", 0);
        DialogManagement.Instance.interactDialog.SetActive(false);
    }

    private void CheckingAddSentences()
    {
        // 當秒數達到會隨機出嘲諷語句
        if (_timer_i % _plotPerSec == 0 && _timer_i != 0)
        {
            SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, _plotName, "t" });
            _timer_f += 1f;
        }
    }

    private void CheckNeedShowHint()
    {
        // 檢查是否有輸入，重置計時
        if (Input.anyKey || Input.GetMouseButton(0))
        {
            _currentTime = 0f;
            _isIdle = false;
        }
        else
        {
            // 如果沒有輸入，增加計時
            _currentTime += Time.deltaTime;

            // 檢查是否達到閒置時間
            if (_currentTime >= _idleTime)
            {
                _isIdle = true;
                DialogManagement.Instance.StartRotateHint();
                _currentTime = 0f;
            }
        }
    }

    private void TogglePlayerSetting(bool status)
    {
        if (_reflectProbe)
        {
            _reflectProbe.SetActive(status);
        }

        if (_numberObject)
        {
            _numberObject.SetActive(status);
        }
        if (_rotationCameraObject) _rotationCameraObject.SetActive(status);
        CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        GameObject objectCamera = CameraManagement.Instance.objectCamera.gameObject;
        AudioListener mainAudio = Camera.main.GetComponent<AudioListener>();
        AudioListener subAudio = CameraManagement.Instance._interactCamera.GetComponent<AudioListener>();
        mainAudio.enabled = !status;
        subAudio.enabled = status;
        objectCamera.SetActive(status);
        //CameraManagement.Instance.TogglePlayerVirtualCamera(!status);
        PlayerMovement.Instance.ToggleMove(!status);
        PlayerAttributes.Instance._activingItem = status ? gameObject : null;
        DialogManagement.Instance.ToggleAccurateImage(!status);
        DialogManagement.Instance.ToggleMaskCanvas(status);
        //DialogManagement.Instance.ToggleInteractHelp1(status);
        if (status && Enviroment.Instance.IsFirstInteract)
        {
            DialogManagement.Instance.StartRotateHint();
            Enviroment.Instance.IsFirstInteract = false;
        }
        if (!status) DialogManagement.Instance.CloseRotateHint();
        cameraAttributes._screenLock = status;
        cameraAttributes._cursorVisable = status;
        cameraAttributes._cursorLockMode = status ? CursorLockMode.Confined : CursorLockMode.Locked;
        _isActiving = status;
        if (_isActiving) Camera.main.GetComponent<CameraAttributes>().ZeroCameraSpeed();
        else StartCoroutine(Camera.main.GetComponent<CameraAttributes>().RestoreCameraSpeed());
    }

    private void ApplyObjectPosition()
    {
        // get window size
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // object in screen middle
        Vector3 centerPosition = new(screenWidth / 2f, screenHeight / 2f, .9f);
        transform.position = CameraManagement.Instance.objectCamera.ScreenToWorldPoint(centerPosition);
        if (_interactRotation != Vector3.zero) transform.rotation = Quaternion.Euler(_interactRotation);
        if (_localScaleResize != Vector3.zero) transform.localScale = _localScaleResize;
        transform.parent = null;
    }

    private void PickUpRotate()
    {
        // disabled camera and move
        TogglePlayerSetting(true);

        // apply position
        ApplyObjectPosition();

        if (_isInstant)
        {
            if (_plotName.Equals("knife"))
            {
                SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, _plotName + "_1", "t" });
            } 
            else
            {
                if (_isMutation && Enviroment.Instance.GetControllerByLocation(_location).Normal && Enviroment.Instance.Level.Equals(5))
                {
                    _plotName += "_mutation";
                    _plotName += "_" + Enviroment.Instance.Level;
                }
                SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, _plotName, "t" });
            }
        }   
    }

    private void SetAchievement()
    {
        if (_location.Equals("demo") && _plotName.Equals("easter"))
        {
            SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_OTHER_ROOM);
        }
    }

    private void PullBack()
    {
        TogglePlayerSetting(false);

        transform.parent = _originParent;
        transform.SetPositionAndRotation(_originPosition, _originRotation);
        transform.localScale = _originScale;
    }

    private bool CanTrigger()
    {
        // 條件一: 沒電
        // 條件二: 玩家是否有拿東西
        // 條件三: 是否開啟手電筒
        // 條件四: 是否有開燈
        // 權重: 2 > 3 > 4 > 1
        if (PlayerAttributes.Instance._activingItem) return false;
        if (_isEmergency) return true;
        if (PlayerAttributes.Instance._isTriggerFlashlight) return true;
        if (Enviroment.Instance.IsElectrified && LightManagement.Instance.CheckLightExist(_location)) return true;
        return false;
    }

    private IEnumerator Rotate(string action)
    {
        _rotating = true;
        while (CanRotate() && _isActiving && _rotating && !Enviroment.Instance.IsPause && !PlayerController.Instance.StopRotate)
        {
            // apply rotation
            _rotation *= .5f;
            switch (action)
            {
                case "rightButton":
                    transform.Rotate(CameraManagement.Instance.objectCamera.transform.forward, -_rotation.x, Space.World);
                    if (_rotationCamera) _rotationCamera.transform.Rotate(_rotationCamera.transform.forward, -_rotation.x, Space.World);
                    break;
                case "leftButton":
                    transform.Rotate(Vector3.down, _rotation.x, Space.World);
                    transform.Rotate(CameraManagement.Instance.objectCamera.transform.right, _rotation.y, Space.World);
                    if (_rotationCamera) 
                    {
                        RotateWithLimits(_rotationCamera.transform, _rotationLimitY, _rotationLimitX, _rotation);
                    }
                    
                    break;
            }
            yield return null;
        }
    }

    private void RotateWithLimits(Transform cameraTransform, Vector2 yRange, Vector2 xRange, Vector2 rotationInput)
    {
        // 根據輸入進行旋轉
        cameraTransform.Rotate(Vector3.down, rotationInput.x, Space.World);

        // 將 eulerAngles 轉換為 -180 到 180 範圍的角度
        float yAngle = cameraTransform.eulerAngles.y;
        if (yAngle > 180f) yAngle -= 360f;

        // 檢查並限制 y 軸旋轉，超出範圍時從另一側回來
        if (yAngle < yRange.x)
        {
            yAngle = yRange.y - (yRange.x - yAngle); // 超過下限，從上限回來
        }
        else if (yAngle > yRange.y)
        {
            yAngle = yRange.x + (yAngle - yRange.y); // 超過上限，從下限回來
        }

        // 套用新的 y 軸角度
        cameraTransform.eulerAngles = new Vector3(cameraTransform.eulerAngles.x, yAngle, cameraTransform.eulerAngles.z);

        // 繼續旋轉 x 軸
        cameraTransform.Rotate(cameraTransform.right, rotationInput.y, Space.World);

        // 檢查並限制 x 軸旋轉，超出範圍時從另一側回來
        float xAngle = cameraTransform.eulerAngles.x;
        if (xAngle < 360f + xRange.x && xAngle > 180f) // 適用 Unity 360 度範圍
        {
            xAngle = xRange.y - (360f + xRange.x - xAngle); // 超過下限，從上限回來
        }
        else if (xAngle > xRange.y && xAngle < 180f)
        {
            xAngle = xRange.x + (xAngle - xRange.y); // 超過上限，從下限回來
        }

        // 套用新的 x 軸角度
        cameraTransform.eulerAngles = new Vector3(xAngle, cameraTransform.eulerAngles.y, cameraTransform.eulerAngles.z);
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
