using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleController : MonoBehaviour, IInteractive
{
    [SerializeField] private string _position;
    [SerializeField] private GameObject[] _accurates;
    [SerializeField] private CinemachineVirtualCamera _holeCamera;
    [SerializeField] private AudioClip _audio;
    [SerializeField] private GameObject _button;
    private Renderer _renderer;
    private Material[] _materials;
    private PlayerMoveInput _playerMoveInput;
    private float _mouseScrollY;
    private bool _isActiving;
    private bool _alreadyMutation = false;
    private SwitchItem _kitchenButton;

    private void Awake()
    {
        // setting renderer
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;

        _playerMoveInput = new PlayerMoveInput();
        _playerMoveInput.Player.Scale.performed += x => { _mouseScrollY = x.ReadValue<float>(); };

        _button.TryGetComponent<SwitchItem>(out _kitchenButton);
    }

    public void Cancel()
    {
        CloseHole();
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
        OpenHole();
    }

    void Update()
    {
        if (!_isActiving) return;
        if (_mouseScrollY < 0)
        {
            if (_holeCamera.m_Lens.FieldOfView < 33f) _holeCamera.m_Lens.FieldOfView *= 1.2f;
        }
        else if (_mouseScrollY > 0)
        {
            if (_holeCamera.m_Lens.FieldOfView > 10f) _holeCamera.m_Lens.FieldOfView *= 0.8f;
        }
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

    private void OpenHole()
    {
        TogglePlayerSetting(true);

        SoundManagement.Instance.PlaySoundFXClip(_audio, transform, 1f);

        SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { "bath_room_1", "hole_top", "t" });
        if (Enviroment.Instance.Level < 3) Enviroment.Instance.Level = 3;
        if (!_alreadyMutation)
        {
            KitchenController.Instance.Normal = false;
            _alreadyMutation = true;
        }
        // 若隱藏按鈕按過，直接進第四階段
        if (_kitchenButton != null && _kitchenButton._status)
        {
            if (Enviroment.Instance.Level.Equals(3)) Enviroment.Instance.Level = 4;
            LivingRoomController.Instance.Normal = false;
        }
        SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_BATH_ROOM_1_HOLE_INTERACT_COUNT_SEC, Timer.Instance.GetNowSec());
    }

    private void CloseHole()
    {
        TogglePlayerSetting(false);
    }

    private void TogglePlayerSetting(bool status)
    {
        CameraManagement.Instance.ChangeHoleCamera(_position, status);
        PlayerAttributes.Instance._activingItem = status ? gameObject : null;
        DialogManagement.Instance.ToggleAccurateImage(!status);
        //DialogManagement.Instance.ToggleHalfCanvas(status);
        //DialogManagement.Instance.ToggleInteractHelp1(status);
        _isActiving = status;
    }

    private bool CanTrigger()
    {
        // 條件一: 沒電
        // 條件二: 玩家是否有拿東西
        // 條件三: 是否開啟手電筒
        // 條件四: 是否有開燈
        // 權重: 2 > 3 > 4 > 1
        if (PlayerAttributes.Instance._activingItem) return false;
        if (PlayerAttributes.Instance._isTriggerFlashlight) return true;
        if (LightManagement.Instance.CheckLightExist("bath_room_1")) return true;
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
