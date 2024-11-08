using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class SwitchItem : MonoBehaviour, IInteractive, IUnlockScreenObject, IDataPersistence
{
    [Header("item config")]
    public string _itemType;
    public string _location;
    public string _animationName;
    public bool _status = false;
    public bool _isEmergency = false;
    [SerializeField] private GameObject _controller;
    [SerializeField] private AudioClip _openSoundClip;
    [SerializeField] private AudioClip _closeSoundClip;
    [SerializeField] private AudioClip _lockSoundClip;
    [SerializeField] private AudioClip _errorSoundClip;
    [SerializeField] private string _id;
    [SerializeField] private ParticleSystem _particle;

    [Header("Plot Detail")]
    [SerializeField] private string _plotName;
    [SerializeField] private bool _hasNoLight;
    [SerializeField] private bool _hasMutation;

    [ContextMenu("Generate guid for id")]
    private void GenerateGuid()
    {
        _id = System.Guid.NewGuid().ToString();
    }

    private bool _firstTimeInteract = true;
    private Renderer _renderer;
    private Material[] _materials;
    private Dictionary<string, string> _locationControl = new Dictionary<string, string>();

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;
        _locationControl.Add("living_room", "kitchen");
        _locationControl.Add("kitchen", "bed_room");
        _locationControl.Add("bed_room", "bath_room_1");
        _locationControl.Add("bath_room_1", "living_room");
        _locationControl.Add("book_room", "book_room");
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
        StartAnimation();
        DoActionAsync();
        SetAchievement();
    }

    public void Cancel()
    {
        return;
    }

    public bool CanDrag()
    {
        return false;
    }

    public bool CanDrop()
    {
        return false;
    }

    public void Close()
    {
        if (_status)
        {
            StartAnimation();
            DoActionAsync();
        }
    }

    private void Update()
    {
        switch (_itemType)
        {
            case "light_switch":
                // �Ĥ@���q�T�{�O�_���q
                if (Enviroment.Instance.IsElectrified)
                {
                    // ���q�P�_�O���S���}�A���}�O�N�±��A�S�}�O�N�G�_��
                    if (_status) _materials[^2].DisableKeyword("_EMISSION");
                    else _materials[^2].EnableKeyword("_EMISSION");
                }
                else
                {
                    // �S�q�P�_�O�_���ιq�A���ιq�B�S�}�O�N�G�_�ӡA�Ϥ����O�±�
                    if (_isEmergency && !_status) _materials[^2].EnableKeyword("_EMISSION");
                    else _materials[^2].DisableKeyword("_EMISSION");
                }
                break;
        }
    }

    private void HoverInItem()
    {
        _materials[^1].SetFloat("_IsActive", 1);
        if (!_materials[^1].IsKeywordEnabled("_EMISSION")) _materials[^1].EnableKeyword("_EMISSION");
        DialogManagement.Instance.interactDialog.SetActive(true);
    }

    private void HoverOutItem()
    {
        _materials[^1].SetFloat("_IsActive", 0);
        _materials[^1].DisableKeyword("_EMISSION");
        DialogManagement.Instance.interactDialog.SetActive(false);
    }

    private void StartAnimation(bool forceDoIt = false)
    {
        switch (_itemType)
        {
            case "light_switch":
                AnimationManagement.Instance.Play(_location + "_" + _itemType, "open");
                SoundManagement.Instance.PlaySoundFXClip(_openSoundClip, transform, 1f);
                break;
            case "handle":
                // �W��
                if (LockManagement.Instance.IsLocked(_location, "door") && !forceDoIt)
                {
                    AnimationManagement.Instance.Play(_location + "_door", "locked");
                    SoundManagement.Instance.PlaySoundFXClip(_lockSoundClip, transform, 1f);
                }
                // �S�W��
                else
                {
                    // ����ʵe�ö}��
                    if (_status)
                    {
                        LockManagement.Instance.GetLockObject(_location, "door").LockObj.GetComponent<Door>().Close(_location);
                        SoundManagement.Instance.PlaySoundFXClip(_closeSoundClip, transform, 1f);
                    }
                    else
                    {
                        LockManagement.Instance.GetLockObject(_location, "door").LockObj.GetComponent<Door>().Open(_location);
                        SoundManagement.Instance.PlaySoundFXClip(_openSoundClip, transform, 1f);
                        if (_location.Equals("demo"))
                        {
                            HoverOutItem();
                            if (Enviroment.Instance.IsDemo)
                            {
                                MainGame.Instance.DemoEnd();
                            }
                            else
                            {
                                // Continue
                                MainGame.Instance.TutorialEnd();
                            }
                        }
                    }
                }
                break;
            case "drawer_door":
            case "display_door":
                // �W��
                if (LockManagement.Instance.IsLocked(_location, _itemType) && !forceDoIt)
                {
                    // �����ʵe�n�����ܤ�r
                    SoundManagement.Instance.PlaySoundFXClip(_lockSoundClip, transform, 1f);
                }
                // �S�W��
                else
                {
                    
                    // ����ʵe�ö}��
                    if (_status)
                    {
                        AnimationManagement.Instance.Play($"{_location}_{_itemType}{(_animationName.Equals("") ? "" : $"_{_animationName}")}", "close");
                        SoundManagement.Instance.PlaySoundFXClip(_closeSoundClip, transform, 1f);
                    }
                    else
                    {
                        AnimationManagement.Instance.Play($"{_location}_{_itemType}{(_animationName.Equals("") ? "" : $"_{_animationName}")}", "open");
                        SoundManagement.Instance.PlaySoundFXClip(_openSoundClip, transform, 1f);
                    }
                }
                break;
            case "button":
            case "big_button":
                if (!_status) AnimationManagement.Instance.Play($"{_location}_{_itemType}", "open");
                break;
            case "out_door":
                if (LockManagement.Instance.IsLocked(_location, _itemType) && !forceDoIt)
                {
                    // �����ʵe�n�����ܤ�r
                    SoundManagement.Instance.PlaySoundFXClip(_lockSoundClip, transform, 1f);
                }
                break;
            default:
                if (_status)
                {
                    AnimationManagement.Instance.Play($"{_location}_{_itemType}{(_animationName.Equals("") ? "" : $"_{_animationName}")}", "close");
                    if (_closeSoundClip) SoundManagement.Instance.PlaySoundFXClip(_closeSoundClip, transform, 1f);
                    if (_itemType.Equals("refrigerator_door"))
                    {
                        KitchenController.Instance.StopTimer();
                    }
                }
                else
                {
                    AnimationManagement.Instance.Play($"{_location}_{_itemType}{(_animationName.Equals("") ? "" : $"_{_animationName}")}", "open");
                    if (_openSoundClip) SoundManagement.Instance.PlaySoundFXClip(_openSoundClip, transform, 1f);
                    if (_itemType.Equals("refrigerator_door"))
                    {
                        KitchenController.Instance.StartTimer();
                    }
                }
                break;
        }
    }

    private async Task DoActionAsync()
    {
        switch (_itemType)
        {
            case "light_switch":
                // �ѩЯS��W�h
                if (_location.Equals("book_room") && LockManagement.Instance.IsOpen(_location, "door") && !Enviroment.Instance.Level.Equals(4) && !Enviroment.Instance.Level.Equals(5)) return;
                if (!_location.Equals("book_room") && !Enviroment.Instance.IsElectrified) return;
                // �ܲ���S��W�h - ���q�T�G�p�еL�k���O
                if (Enviroment.Instance.Level.Equals(3) && _location.Equals("kitchen") && !KitchenController.Instance.Normal && _status) return;
                // �ܲ���S��W�h - ���q�|�G�u������
                if (Enviroment.Instance.Level.Equals(4))
                {
                    string newLocation = _locationControl[_location];
                    _status = !LightManagement.Instance.CheckLightExist(newLocation);
                    if (_status)
                    {
                        if (!newLocation.Equals("book_room")) Enviroment.Instance.GetControllerByLocation(newLocation).Normal = false;
                        LightManagement.Instance.TurnOnLight(newLocation);
                    }
                    else LightManagement.Instance.TurnOffLight(newLocation);
                    // ���q�|�������� - �����ж��O�����F�B�ѩпO���}
                    if (_location.Equals("book_room") && _status)
                    {
                        // check all position
                        bool kitchen = LightManagement.Instance.CheckLightExist("kitchen");
                        bool livingRoom = LightManagement.Instance.CheckLightExist("living_room");
                        bool bedRoom = LightManagement.Instance.CheckLightExist("bed_room");
                        bool bathRoom = LightManagement.Instance.CheckLightExist("bath_room_1");
                        if (kitchen && livingRoom && bedRoom && bathRoom)
                        {
                            KitchenController.Instance.Normal = true;
                            BedRoomController.Instance.Normal = true;
                            BathRoom1Controller.Instance.Normal = true;
                            LivingRoomController.Instance.Normal = true;
                            BookRoomController.Instance.Normal = false;
                        }
                    }
                    // �۰�����
                    if (newLocation.Equals("book_room") && LockManagement.Instance.IsOpen(newLocation, "door"))
                    {
                        LockManagement.Instance.GetLockObject(newLocation, "door").LockObj.GetComponent<Door>().Close(newLocation);
                        LockManagement.Instance.GetLockObject(newLocation, "door").LockObj.GetComponent<Door>().SetHandleStatus(false);
                        LockManagement.Instance.SetOpened(newLocation, "door", false);
                    }
                }
                else
                {
                    // �}���O���A�վ�
                    _status = !_status;
                    if (_status) LightManagement.Instance.TurnOnLight(_location);
                    else LightManagement.Instance.TurnOffLight(_location);
                }

                if (_location.Equals("bed_room") && !_status)
                {
                    Enviroment.Instance.BedRoomEmission = true;
                }

                // �Y�S�q�ѩж}���O�n����P�W���
                if (_location.Equals("book_room"))
                {
                    if (_status) LockManagement.Instance.Locked(_location, "door");
                    else LockManagement.Instance.Unlock(_location, "door");
                }

                break;
            case "handle":
                if (!LockManagement.Instance.IsLocked(_location, "door"))
                {
                    _status = !_status;
                    if (_status && _particle) _particle.Play();
                    LockManagement.Instance.SetOpened(_location, "door", _status);
                    if (_location.Equals("book_room") && PlayerAttributes.Instance._hasFlashlight)
                    {
                        if (Enviroment.Instance.Level.Equals(5)) Camera.main.GetComponent<CameraAttributes>().StartToCloseStorm();
                        return;
                    }
                }
                break;
            case "button":
            case "big_button":
                _status = true;
                LockManagement.Instance.Unlock(_location, "drawer_door");
                // �i�J���q�| - �u������
                if (Enviroment.Instance.Level.Equals(3))
                {
                    Enviroment.Instance.Level = 4;
                    LivingRoomController.Instance.Normal = false;
                }
                break;
            case "infrared_button":
                _status = true;
                _controller.SetActive(_status);
                KitchenController.Instance.LightButton();
                break;
            case "electrical_switch":
                Enviroment.Instance.IsElectrified = true;
                _status = true;
                gameObject.layer = 13;
                await Task.Delay(1000);
                // �Ĥ@���q�ܲ�
                Enviroment.Instance.Level = 1;
                KitchenController.Instance.Normal = false;
                break;
            case "cabinet_button":
                _status = !_status;
                CabinetController cabinet = _controller.GetComponent<CabinetController>();
                if (_status)
                {
                    _materials[0].EnableKeyword("_EMISSION");
                    _materials[0].SetColor("_EmissionColor", new Color(3, 0, 0));
                    cabinet.ButtonText[int.Parse(_animationName) - 1].SetText((++cabinet.Index).ToString());
                    if (cabinet.Index >= 4)
                    {
                        // puzzle pre checking
                        PuzzleManagement.Instance.PreCheckAnswer(StartCheck);
                    }
                }
                else
                {
                    cabinet.ButtonText[int.Parse(_animationName) - 1].SetText("");
                    cabinet.Index--;
                }
                break;
            case "drawer_door":
            case "display_door":
                {
                    if (!LockManagement.Instance.IsLocked(_location, _itemType))
                    {
                        _status = !_status;
                        LockManagement.Instance.SetOpened(_location, _itemType, _status);
                        if (_itemType.Equals("display_door"))
                        {
                            // play display animation
                            AnimationManagement.Instance.ToggleAnimating($"{_location}_display_cabinet", _status);
                            // play sound
                            AudioSource audio = transform.parent.GetComponent<AudioSource>();
                            if (_status) audio.Play();
                            else audio.Pause();
                            // �L�����]�r��
                            return;
                        }
                    }
                    break;
                }
            case "out_door":
                if (LockManagement.Instance.IsLocked(_location, _itemType)) return;
                if (_firstTimeInteract)
                {
                    _firstTimeInteract = false;
                } 
                else
                {
                    AnimationManagement.Instance.Play($"{_location}_{_itemType}", "open");
                    AnimationManagement.Instance.Play($"{_location}_{_itemType}_2", "open");
                    // end game
                    HoverOutItem();
                    SoundManagement.Instance.PlaySoundFXClip(_openSoundClip, transform, 1f);
                    MainGame.Instance.OutDoorEnd();
                }
                break;
            default:
                _status = !_status;
                break;
        }
        string plotFullName = _plotName;
        if (_hasNoLight && !LightManagement.Instance.CheckLightExist(_location) && Enviroment.Instance.GetControllerByLocation(_location).Normal) plotFullName += "_nolight";
        if (_hasMutation && !Enviroment.Instance.GetControllerByLocation(_location).Normal && !_location.Equals("book_room"))
        {
            plotFullName += "_mutation";
            plotFullName += "_" + Enviroment.Instance.Level;
        }
        // Add Drag Sentences
        bool hasSentences = SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, plotFullName, _status ? "t" : "f" });
        if (!hasSentences) SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { "any", plotFullName, _status ? "t" : "f" });
    }

    private void SetAchievement()
    {
        switch (_location)
        {
            case "bath_room_1":
                if (_itemType.Equals("lid") && _status)
                {
                    SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_BEST_POSITION);
                }
                break;
            case "bath_room_2":
                if (_itemType.Equals("lid") && _status)
                {
                    SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_HIDE_AND_SEEK);
                }
                break;
            case "kitchen":
                if (_itemType.Equals("electrical_light_status") && _status)
                {
                    SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_FORESHADOWING);
                }
                if (_itemType.Equals("refrigerator_door") && _animationName.Equals("top") && _status)
                {
                    SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_SCENT_OF_DECAY);
                }
                if (_itemType.Equals("button") && _status)
                {
                    SteamInitManagement.Instance.UpdateStat(SteamInitManagement.STAT_KITCHEN_BUTTON_COUNT_SEC, Timer.Instance.GetNowSec());
                }
                break;
        }
    }

    private void StartCheck()
    {
        bool isSolve = SharedUtils.StartCheckAnswer(_location, "cabinet");
        CabinetController cabinet = _controller.GetComponent<CabinetController>();
        foreach (GameObject gameObject in cabinet.ButtonObjects)
        {
            SwitchItem item = gameObject.GetComponent<SwitchItem>();
            item.Close();
        }
        if (isSolve) cabinet.NextPuzzle();
        else if (_errorSoundClip != null) SoundManagement.Instance.PlaySoundFXClip(_errorSoundClip, transform, 1f);
    }

    private bool CanTrigger()
    {
        // ����@: �S�q
        // ����G: ���ιq
        // ����T: ���a�O�_�����F��
        // ����|: �O�_�}�Ҥ�q��
        // ����: �O�_���}�O
        // �v��: 3 > 2 > 4 > 5 > 1
        if (PlayerAttributes.Instance._activingItem) return false;
        if (_isEmergency) return true;
        if (PlayerAttributes.Instance._isTriggerFlashlight) return true;
        if (LightManagement.Instance.CheckLightExist(_location)) return true;
        if (Enviroment.Instance.IsElectrified) return true;
        return false;
    }

    void IDataPersistence.LoadData(GameData data)
    {
        if (string.IsNullOrEmpty(_id)) return;
        data.switchItems.TryGetValue(_id, out _status);
        if (_status)
        {
            _status = !_status;
            StartAnimation(true);
            _status = !_status;
            if (_itemType.Equals("infrared_button")) _controller.SetActive(_status);
            if (_itemType.Equals("electrical_switch")) gameObject.layer = 13;
        }
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