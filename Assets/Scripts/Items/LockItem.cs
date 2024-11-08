using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockItem : MonoBehaviour, IInteractive, IDataPersistence
{

    [SerializeField] private string _type;
    [SerializeField] private string _id;
    [SerializeField] private bool _status;
    [SerializeField] private bool _isEmergency = false;
    [SerializeField] private AudioClip _lockSoundClip;
    [SerializeField] private AudioClip _unlockSoundClip;
    [ContextMenu("Generate guid for id")]
    private void GenerateGuid()
    {
        _id = System.Guid.NewGuid().ToString();
    }
    private Renderer _renderer;
    private Material[] _materials;
    private BoxCollider _collider;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<BoxCollider>();
        _materials = _renderer.materials;
    }
    public void Cancel()
    {
        throw new System.NotImplementedException();
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
        TryUnlock();
    }

    private void HoverInItem()
    {
        if (_type.Equals("silver")) _materials[^1].EnableKeyword("_EMISSION");
        else _materials[^1].SetFloat("_IsActive", 1);
        DialogManagement.Instance.interactDialog.SetActive(true);
    }

    private void HoverOutItem()
    {
        if (_type.Equals("silver")) _materials[^1].DisableKeyword("_EMISSION");
        else _materials[^1].SetFloat("_IsActive", 0);
        DialogManagement.Instance.interactDialog.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        CheckUnlock();
    }

    private void TryUnlock()
    {
        switch (_type)
        {
            case "red":
                if (PlayerAttributes.Instance._keys[1])
                {
                    // unlock
                    AnimationManagement.Instance.Play("living_room_" + _type + "_lock", "unlock");
                    LockManagement.Instance.Unlock("living_room", _type);
                    _status = true;
                    SoundManagement.Instance.PlaySoundFXClip(_unlockSoundClip, transform, 1f);
                } 
                else
                {
                    SoundManagement.Instance.PlaySoundFXClip(_lockSoundClip, transform, 1f);
                    SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { "living_room", "red_lock_noKey", _status ? "t" : "f" });
                }
                break;
            case "blue":
                if (PlayerAttributes.Instance._keys[2])
                {
                    // unlock
                    AnimationManagement.Instance.Play("living_room_" + _type + "_lock", "unlock");
                    LockManagement.Instance.Unlock("living_room", _type);
                    _status = true;
                    SoundManagement.Instance.PlaySoundFXClip(_unlockSoundClip, transform, 1f);
                }
                else
                {
                    SoundManagement.Instance.PlaySoundFXClip(_lockSoundClip, transform, 1f);
                    SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { "living_room", "blue_lock_noKey", _status ? "t" : "f" });
                }
                break;
            case "green":
                if (PlayerAttributes.Instance._keys[3])
                {
                    // unlock
                    AnimationManagement.Instance.Play("living_room_" + _type + "_lock", "unlock");
                    LockManagement.Instance.Unlock("living_room", _type);
                    _status = true;
                    SoundManagement.Instance.PlaySoundFXClip(_unlockSoundClip, transform, 1f);
                }
                else
                {
                    SoundManagement.Instance.PlaySoundFXClip(_lockSoundClip, transform, 1f);
                    SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { "living_room", "green_lock_noKey", _status ? "t" : "f" });
                }
                break;
            case "silver":
                if (PlayerAttributes.Instance._keys[4])
                {
                    // unlock
                    AnimationManagement.Instance.Play("living_room_" + _type + "_lock", "unlock");
                    LockManagement.Instance.Unlock("living_room", _type);
                    _status = true;
                    SoundManagement.Instance.PlaySoundFXClip(_unlockSoundClip, transform, 1f);
                    StartCoroutine(EndGame());
                }
                else SoundManagement.Instance.PlaySoundFXClip(_lockSoundClip, transform, 1f);
                break;
        }
        CheckingAllUnlock();
    }

    private IEnumerator EndGame()
    {
        PlayerMovement.Instance.ToggleMove(false);
        PlayerAttributes.Instance._activingItem = null;
        DialogManagement.Instance.ToggleAccurateImage(false);
        Camera.main.GetComponent<CameraAttributes>().ZeroCameraSpeed();
        yield return new WaitForSeconds(1.5f);
        // 真結局 結束
        AnimationManagement.Instance.Play("living_room_" + _type + "_door", "open");
        yield return new WaitForSeconds(1.5f);
        MainGame.Instance.ExitDoorEnd();
    }

    private void CheckUnlock()
    {
        if (_status && gameObject.activeSelf && _renderer.enabled)
        {
            StartCoroutine(HideLock());
        }
    }

    private void CheckingAllUnlock()
    {
        string[] colors = new string[] { "red", "blue", "green" };
        foreach (string color in colors)
        {
            if (LockManagement.Instance.IsLocked("living_room", color)) return;
        }
        LockManagement.Instance.Unlock("living_room", "out_door");
    }

    IEnumerator HideLock()
    {
        yield return new WaitForSeconds(1);
        _renderer.enabled = false;
        if (_collider) _collider.enabled = false;
    }

    private bool CanTrigger()
    {
        // 條件一: 沒電
        // 條件二: 緊急用電
        // 條件三: 玩家是否有拿東西
        // 條件四: 是否開啟手電筒
        // 條件五: 是否有開燈
        // 權重: 3 > 2 > 4 > 5 > 1
        if (PlayerAttributes.Instance._activingItem) return false;
        if (_isEmergency) return true;
        if (PlayerAttributes.Instance._isTriggerFlashlight) return true;
        if (LightManagement.Instance.CheckLightExist("living_room")) return true;
        if (Enviroment.Instance.IsElectrified) return true;
        return false;
    }

    void IDataPersistence.LoadData(GameData data)
    {
        if (string.IsNullOrEmpty(_id)) return;
        data.switchItems.TryGetValue(_id, out _status);
    }

    void IDataPersistence.SaveData(ref GameData data, bool isCrazyEnd)
    {
        if (string.IsNullOrEmpty(_id)) return;
        if (data.switchItems.ContainsKey(_id))
        {
            data.switchItems.Remove(_id);
        }
        data.switchItems.Add(_id, _status);
    }
}
