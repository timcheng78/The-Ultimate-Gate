using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBlockItem : MonoBehaviour, IInteractive
{
    [SerializeField] private string _location;
    [SerializeField] private string _type;
    [SerializeField] private AudioClip _moveSoundClip;

    private Renderer _renderer;
    private Material[] _materials;
    private int _status = 0;
    private string[] _actions = { "1", "1_2", "1_2_3", "1_2_3_4" };

    public int Status { get => _status; set => _status = value; }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;
    }
    public void Cancel()
    {
        return;
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
        MoveBlock();
        switch (BathRoom1Controller.Instance.CountPuzzleButton())
        {
            case 30:
                SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { "bath_room_1", "button_1", "t" });
                break;
            case 50:
                SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { "bath_room_1", "button_2", "t" });
                break;
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

    private void MoveBlock()
    {
        // animation
        AnimationManagement.Instance.Play($"{_location}_{_type}", _actions[_status]);
        // play sound 
        SoundManagement.Instance.PlaySoundFXClip(_moveSoundClip, transform, 1f);
        _status++;
        if (_status > 3) _status = 0;
        PuzzleManagement.Instance.PreCheckAnswer(StartCheck);
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
        if (LightManagement.Instance.CheckLightExist(_location)) return true;
        if (Enviroment.Instance.IsElectrified) return true;
        return false;
    }

    private void StartCheck()
    {
        bool isSolve = SharedUtils.StartCheckAnswer(_location, "puzzle");
        if (isSolve) LockManagement.Instance.Unlock(_location, "drawer_door");
    }
}
