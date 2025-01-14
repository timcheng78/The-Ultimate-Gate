using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToiletItem : MonoBehaviour, IInteractive
{
    [SerializeField] private string _location;
    [SerializeField] private Light _spot;

    private Renderer _renderer;
    private Material[] _materials;
    private PlayerMoveInput _playerMoveInput;
    private float _mouseScrollY;
    private bool _isActiving = false;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;

        _playerMoveInput = new PlayerMoveInput();
        _playerMoveInput.Player.Scale.performed += x => { _mouseScrollY = x.ReadValue<float>(); };
    }

    public void Cancel()
    {
        LeaveWatching();
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

    private void HoverInItem()
    {
        _materials[^1].EnableKeyword("_EMISSION");
        DialogManagement.Instance.interactDialog.SetActive(true);
    }

    private void HoverOutItem()
    {
        _materials[^1].DisableKeyword("_EMISSION");
        DialogManagement.Instance.interactDialog.SetActive(false);
    }

    public void Interact()
    {
        if (!CanTrigger()) return;
        Watching();
    }


    // Update is called once per frame
    void Update()
    {
        if (!_isActiving)
        {
            _spot.enabled = false;
            return;
        }
        if (PlayerAttributes.Instance._isTriggerFlashlight) _spot.enabled = true;
        else _spot.enabled = false;
        if (_mouseScrollY < 0)
        {
            if (CameraManagement.Instance.bathroom1VirtualCamera.m_Lens.FieldOfView < 47f) CameraManagement.Instance.bathroom1VirtualCamera.m_Lens.FieldOfView *= 1.1f;
            if (CameraManagement.Instance.bathroom1VirtualCamera.m_Lens.FieldOfView > 47f) CameraManagement.Instance.bathroom1VirtualCamera.m_Lens.FieldOfView = 47f;
        }
        else if (_mouseScrollY > 0)
        {
            if (CameraManagement.Instance.bathroom1VirtualCamera.m_Lens.FieldOfView > 38f) CameraManagement.Instance.bathroom1VirtualCamera.m_Lens.FieldOfView *= 0.9f;
            if (CameraManagement.Instance.bathroom1VirtualCamera.m_Lens.FieldOfView < 38f) CameraManagement.Instance.bathroom1VirtualCamera.m_Lens.FieldOfView = 38f;
        }
    }

    private void Watching()
    {
        TogglePlayerSetting(true);
    }

    private void LeaveWatching()
    {
        TogglePlayerSetting(false);
    }

    private void TogglePlayerSetting(bool status)
    {
        CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        CameraManagement.Instance.TogglePlayerVirtualCamera(!status);
        CameraManagement.Instance.ToggleBathroom1VirtualCamera(status);
        PlayerMovement.Instance.ToggleMove(!status);
        PlayerAttributes.Instance._activingItem = status ? gameObject : null;
        DialogManagement.Instance.ToggleAccurateImage(!status);
        //DialogManagement.Instance.ToggleHalfCanvas(status);
        //DialogManagement.Instance.ToggleInteractHelp1(status);
        cameraAttributes._cursorVisable = status;
        cameraAttributes._cursorLockMode = status ? CursorLockMode.Confined : CursorLockMode.Locked;
        _isActiving = status;
    }

    private bool CanTrigger()
    {
        // 條件一: 沒電
        // 條件二: 玩家是否有拿東西
        // 條件三: 是否開啟手電筒
        // 條件四: 是否有開燈
        // 權重: 2 > 1 > 3 > 4
        if (PlayerAttributes.Instance._activingItem) return false;
        if (!Enviroment.Instance.IsElectrified) return false;
        if (PlayerAttributes.Instance._isTriggerFlashlight) return true;
        if (LightManagement.Instance.CheckLightExist(_location)) return true;
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
