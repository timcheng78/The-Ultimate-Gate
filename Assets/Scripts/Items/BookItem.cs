using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookItem : MonoBehaviour, IInteractive, ILockScreenObject
{
    [SerializeField] private string _location;
    [SerializeField] private bool _canRotate = false;
    [SerializeField] private bool _canControl = false;
    [SerializeField] private bool _isEmergency = false;
    [SerializeField] private GameObject _bookCanvas;
    [SerializeField] private ParticleSystem _particle;

    [SerializeField] private string _plotName;
    public AutoFlip _book;

    private Renderer _renderer;
    private Material[] _materials;
    private bool _isActiving = false;

    void Awake()
    {
        // setting renderer
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;
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
        if (!_book.ControledBook.pageDragEnd) return;
        TogglePlayerSetting(false);
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
        TogglePlayerSetting(true);
    }

    private void Update()
    {
        if (!_isActiving || Enviroment.Instance.IsPause) return;
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

    private void TogglePlayerSetting(bool status)
    {
        CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        DialogManagement.Instance.ToggleAccurateImage(!status);
        //CameraManagement.Instance.TogglePlayerVirtualCamera(!status);
        PlayerMovement.Instance.ToggleMove(!status);
        PlayerAttributes.Instance._activingItem = status ? gameObject: null;
        _bookCanvas.SetActive(status);
        //DialogManagement.Instance.ToggleInteractHelp1(status);
        //DialogManagement.Instance.ToggleInteractHelp3(status);
        cameraAttributes._cursorVisable = status;
        cameraAttributes._cursorLockMode = status ? CursorLockMode.Confined : CursorLockMode.Locked;
        cameraAttributes._screenLock = status;
        gameObject.SetActive(!status);
        if (status) Camera.main.GetComponent<CameraAttributes>().ZeroCameraSpeed();
        else StartCoroutine(Camera.main.GetComponent<CameraAttributes>().RestoreCameraSpeed());
        if (_particle && status) _particle.Play();
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
}
