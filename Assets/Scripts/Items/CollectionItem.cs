using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class CollectionItem : MonoBehaviour, IInteractive, IUnlockScreenObject
{
    [SerializeField] private string _itemType;
    [SerializeField] private string _location;
    [SerializeField] private bool _canDrag = false;
    [SerializeField] private bool _canDrop = false;
    [SerializeField] private bool _isEmergency = false;
    [SerializeField] private AudioClip _pickUpSoundClip;

    private Renderer _renderer;
    private Material[] _materials;
    private void Awake()
    {
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
        throw new System.NotImplementedException();
    }

    public bool CanDrag()
    {
        return _canDrag;
    }

    public bool CanDrop()
    {
        return _canDrop;
    }

    public void Interact()
    {
        if (!CanTrigger()) return;
        StartCoroutine(PickUp());
        DoAction();
        SetStat();
    }

    private void Update()
    {
        if (PlayerAttributes.Instance._hasNotebook && _itemType.Equals("book")) gameObject.SetActive(false);
        if (PlayerAttributes.Instance._hasFlashlight && _itemType.Equals("flashlight")) gameObject.SetActive(false);
        if (PlayerAttributes.Instance._keys[1] && _location.Equals("bed_room") && _itemType.Equals("key")) gameObject.SetActive(false);
        if (PlayerAttributes.Instance._keys[2] && _location.Equals("bath_room_1") && _itemType.Equals("key")) gameObject.SetActive(false);
        if (PlayerAttributes.Instance._keys[3] && _location.Equals("bath_room_2") && _itemType.Equals("key")) gameObject.SetActive(false);
        if (PlayerAttributes.Instance._keys[4] && _location.Equals("book_room") && _itemType.Equals("key")) gameObject.SetActive(false);
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

    private IEnumerator PickUp()
    {
        if (gameObject.GetComponent<MeshCollider>()) gameObject.GetComponent<MeshCollider>().enabled = false;
        if (gameObject.GetComponent<BoxCollider>()) gameObject.GetComponent<BoxCollider>().enabled = false;
        while(Vector3.Distance(transform.position, Enviroment.Instance.Player.transform.position + new Vector3(-0.2f, 0.3f, 0)) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, Enviroment.Instance.Player.transform.position + new Vector3(-0.2f, 0.3f, 0), (float)(5 * Time.deltaTime));
            transform.Rotate(new Vector3(0, 0, 1));
            yield return null;
        }
        laterSetting();
        // play sound
        SoundManagement.Instance.PlaySoundFXClip(_pickUpSoundClip, transform, 1f);
    }

    private void DoAction()
    {
        switch(_itemType)
        {
            case "flashlight":
                {
                    // flashlight
                    DialogManagement.Instance.AddHelpTextToPlay(new int[] { 4 });
                    var localString = LocalizationSettings.StringDatabase.GetTableEntry("String Table", "HintPageMode2");
                    SubtitleManagement.Instance.SetNotePageText(0, localString.Entry.GetLocalizedString());
                    SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, "flashlight", "t" });
                    break;
                }
            case "book":
                {
                    // note book
                    DialogManagement.Instance.AddHelpTextToPlay(new int[] { 3 });
                    break;
                }
        }
    }

    private void SetStat()
    {
        switch (_location)
        {
            case "bed_room":
                if (_itemType.Equals("key")) SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_RED_KEY_COLLECT_COUNT_SEC, Timer.Instance.GetNowSec());
                break;
            case "bath_room_1":
                if (_itemType.Equals("key")) SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_BLUE_KEY_COLLECT_COUNT_SEC, Timer.Instance.GetNowSec());
                break;
            case "bath_room_2":
                if (_itemType.Equals("key")) SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_GREEN_KEY_COLLECT_COUNT_SEC, Timer.Instance.GetNowSec());
                break;
            case "book_room":
                if (_itemType.Equals("key")) SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_SILVER_KEY_COLLECT_COUNT_SEC, Timer.Instance.GetNowSec());
                break;

        }
    }

    private void laterSetting()
    {
        switch (_itemType)
        {
            case "flashlight":
                {
                    PlayerAttributes.Instance._hasFlashlight = true;
                    break;
                }
            case "key":
                {
                    if (_location.Equals("bed_room")) PlayerAttributes.Instance._keys[1] = true;
                    if (_location.Equals("bath_room_1")) PlayerAttributes.Instance._keys[2] = true;
                    if (_location.Equals("bath_room_2")) PlayerAttributes.Instance._keys[3] = true;
                    if (_location.Equals("book_room"))
                    {
                        Enviroment.Instance.Level = 5;
                        PlayerAttributes.Instance._keys[4] = true;
                        BookRoomController.Instance.Normal = true;
                        BathRoom2Controller.Instance.Normal = false;
                    }
                    SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, "key", "t" });
                    break;
                }
            case "book":
                {
                    PlayerAttributes.Instance._hasNotebook = true;
                    break;
                }
        }
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
        if (LightManagement.Instance.CheckLightExist(_location)) return true;
        if (Enviroment.Instance.IsElectrified) return true;
        return false;
    }
}
