using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaperItem : MonoBehaviour, IInteractive, ILockScreenObject
{
    [SerializeField] private string _itemType;
    [SerializeField] private string _location;
    [SerializeField] private bool _canRotate = false;
    [SerializeField] private bool _canControl = false;
    [SerializeField] private bool _canCollection = false;
    [SerializeField] private bool _isEmergency = false;
    [SerializeField] private RawImage _pageImage;
    [SerializeField] private Texture _newTexture; 
    [SerializeField] private Vector3 _interactRotation;
    [SerializeField] private Vector3 _localScaleResize;
    [SerializeField] private AudioClip _pickUpSoundClip;

    [Header("Plot Detail")]
    [SerializeField] private string _plotName;

    private Renderer _renderer;
    private Material[] _materials;
    private Transform _originParent;
    private Vector3 _originPosition;
    private Quaternion _originRotation;
    private Vector3 _originScale;
    private PlayerMoveInput _playerMoveInput;
    private bool _isActiving = false;
    private float _mouseScrollY;
    private bool _closeExecute = false;

    private void Awake()
    {
        // setting renderer
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;

        // setting origin transform
        _originPosition = transform.localPosition;
        _originRotation = transform.rotation;
        _originScale = transform.localScale;
        _originParent = transform.parent;

        _playerMoveInput = new PlayerMoveInput();
        _playerMoveInput.Player.Scale.performed += x => { _mouseScrollY = x.ReadValue<float>(); };
    }
    private void Update()
    {
        if (_closeExecute) return;
        if (PlayerAttributes.Instance._hasBinaryPaper)
        {
            gameObject.SetActive(false);
            _pageImage.texture = _newTexture;
            _closeExecute = true;
        }
        if (!_isActiving || Enviroment.Instance.IsPause) return;
        if (_mouseScrollY > 0)
        {
            transform.localScale *= 1.2f;
        }
        else if (_mouseScrollY < 0)
        {
            transform.localScale *= 0.8f;
        }
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

    public void HoverIn()
    {
        if (!CanTrigger()) return;
        HoverInItem();
    }

    public void HoverOut()
    {
        HoverOutItem();
    }

    public void Interact()
    {
        if (!CanTrigger()) return;
        PickUp();
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

    private void PickUp()
    {
        if (_canCollection && PlayerAttributes.Instance._hasNotebook)
        {
            StartCoroutine(TakeIt());
        }
        else
        {
            // disabled camera and move
            TogglePlayerSetting(true);

            // apply position
            ApplyObjectPosition();

            SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, _plotName, "t" });
        }
    }

    private IEnumerator TakeIt()
    {
        if (gameObject.GetComponent<MeshCollider>()) gameObject.GetComponent<MeshCollider>().enabled = false;
        if (gameObject.GetComponent<BoxCollider>()) gameObject.GetComponent<BoxCollider>().enabled = false;
        while (Vector3.Distance(transform.position, Enviroment.Instance.Player.transform.position + new Vector3(-0.2f, 0.3f, 0)) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, Enviroment.Instance.Player.transform.position + new Vector3(-0.2f, 0.3f, 0), (float)(5 * Time.deltaTime));
            transform.Rotate(new Vector3(0, 0, 1));
            yield return null;
        }
        gameObject.SetActive(false);
        _pageImage.texture = _newTexture;
        PlayerAttributes.Instance._hasBinaryPaper = true;
        // play sound
        SoundManagement.Instance.PlaySoundFXClip(_pickUpSoundClip, transform, 1f);
    }

    private void PullBack()
    {
        TogglePlayerSetting(false);

        transform.parent = _originParent;
        transform.localPosition = _originPosition;
        transform.rotation = _originRotation;
        transform.localScale = _originScale;
    }

    private void TogglePlayerSetting(bool status)
    {
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
        if (LightManagement.Instance.CheckLightExist(_location)) return true;
        if (Enviroment.Instance.IsElectrified) return true;
        return false;
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
