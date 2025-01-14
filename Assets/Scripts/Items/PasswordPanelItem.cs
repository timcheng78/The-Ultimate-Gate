using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

public class PasswordPanelItem : MonoBehaviour, IInteractive, ILockScreenObject, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public string _location;
    public bool _canRotate = false;
    public bool _canControl = false;
    public bool _canRepeatSelect = false;
    public string _buttonValue;
    public TMP_Text _passwordPanel;
    public string _puzzleName;
    public GameObject _puzzleController;
    [SerializeField] private bool _isEmergency = false;
    [SerializeField] private AudioClip _clickSoundClip;
    [SerializeField] private AudioClip _openSoundClip;
    [SerializeField] private AudioClip _errorSoundClip;
    [SerializeField] private Vector3 _rotation;
    [SerializeField] private Vector3 _localScaleResize;
    [SerializeField] private MeshRenderer _boxRenderer;

    [Header("Plot Detail")]
    [SerializeField] private string _plotName;
    [SerializeField] private bool _isInstant = false;
    //[SerializeField] private int _hintPerSec = 300;
    [SerializeField] private int _plotPerSec = 30;

    private Transform _parentTransform;
    private Renderer _renderer;
    private Material[] _materials;
    private Vector3 _originPosition;
    private Quaternion _originRotation;
    private Vector3 _originScale;
    private float _timer_f = 0f;
    private int _timer_i = 0;
    private PlayerMoveInput _playerMoveInput;
    private float _mouseScrollY;
    private bool _isActiving = false;

    private void Awake()
    {
        // setting renderer
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;

        // setting origin transform
        _parentTransform = transform.parent;
        _originPosition = transform.localPosition;
        _originRotation = transform.localRotation ;
        _originScale = transform.localScale;

        _playerMoveInput = new PlayerMoveInput();
        if (CanControl())
        {
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
        ClosePanel();
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
        if (CanControl())
        {
            ShowPanel();
        }
    }

    private void Update()
    {
        if (LockManagement.Instance.IsLocked(_location, _puzzleName))
        {
            if (gameObject.GetComponent<BoxCollider>()) gameObject.GetComponent<BoxCollider>().enabled = true;
            if (gameObject.GetComponent<CircleCollider2D>()) gameObject.GetComponent<CircleCollider2D>().enabled = true;
        } 
        else
        {
            if (gameObject.GetComponent<BoxCollider>()) gameObject.GetComponent<BoxCollider>().enabled = false;
        }
        if (!_isActiving || Enviroment.Instance.IsPause) return;
        if (LockManagement.Instance.IsLocked(_location, _puzzleName))
        {
            gameObject.GetComponent<BoxCollider>().enabled = false;

            if (_mouseScrollY > 0)
            {
                transform.localScale *= 1.2f;
            }
            else if (_mouseScrollY < 0)
            {
                transform.localScale *= 0.8f;
            }
        } 
        else
        {
            ClosePanel();
        }
        // 互動中且非暫停開始計時
        _timer_f += Time.deltaTime;
        _timer_i = (int)_timer_f;
        if (!_isInstant) CheckingAddSentences();
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

    private void HoverInItem()
    {
        if (_puzzleName.Equals("box") && _passwordPanel) _materials[^1].EnableKeyword("_EMISSION");
        else _materials[^1].SetFloat("_IsActive", 1);
        if (_boxRenderer) _boxRenderer.materials[^1].SetFloat("_IsActive", 1);
        _materials[^1].SetFloat("_EMISSION", 1);
        if (!PlayerAttributes.Instance._activingItem) DialogManagement.Instance.interactDialog.SetActive(true);
        
    }

    private void HoverOutItem()
    {
        if (_puzzleName.Equals("box") && _passwordPanel) _materials[^1].DisableKeyword("_EMISSION");
        else _materials[^1].SetFloat("_IsActive", 0);
        _materials[^1].SetFloat("_EMISSION", 0);
        if (_boxRenderer) _boxRenderer.materials[^1].SetFloat("_IsActive", 0);
        DialogManagement.Instance.interactDialog.SetActive(false);
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
        CameraManagement.Instance._interactCamera.orthographic = false;
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
        Vector3 centerPosition = new (screenWidth / 2f, screenHeight / 2f, .9f);
        transform.position = CameraManagement.Instance.objectCamera.ScreenToWorldPoint(centerPosition);
        if (_rotation != Vector3.zero) transform.rotation = Quaternion.Euler(_rotation);
        if (_localScaleResize != Vector3.zero) transform.localScale = _localScaleResize;
        else transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 0, 0);
    }

    private void ShowPanel()
    {
        // disabled camera and move
        TogglePlayerSetting(true);

        // apply position
        ApplyObjectPosition();

        if (_isInstant)
        {
            SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, _plotName, "t" });
        }
    }

    private void ClosePanel()
    {
        TogglePlayerSetting(false);

        transform.localPosition = _originPosition;
        transform.localRotation = _originRotation;
        transform.localScale = _originScale;

        // play sound
        if (!LockManagement.Instance.IsLocked(_location, _puzzleName) &&
            (_puzzleName.Equals("electrical_box") || _puzzleName.Equals("insurance"))) 
            SoundManagement.Instance.PlaySoundFXClip(_openSoundClip, transform, 1f);
    }

    private void DisplayNumber()
    {
        // play sound
        SoundManagement.Instance.PlaySoundFXClip(_clickSoundClip, transform, 1f);
        switch (_buttonValue)
        {
            case "1_up":
            case "2_up":
            case "3_up":
            case "4_up":
                {
                    try
                    {
                        int pwd = int.Parse(_passwordPanel.text);
                        if (++pwd > 9) pwd = 0;
                        _passwordPanel.SetText($"{pwd}");
                    } 
                    catch (Exception e)
                    {
                        List<string> topic = Enviroment.Instance.FinalAnswerArray.ToList();
                        int index = topic.IndexOf(_passwordPanel.text);
                        if (++index >= topic.Count) index = 0;
                        _passwordPanel.SetText(Enviroment.Instance.FinalAnswerArray[index]);
                    }
                    break;
                }
            case "1_down":
            case "2_down":
            case "3_down":
            case "4_down":
                {
                    try
                    {
                        int pwd = int.Parse(_passwordPanel.text);
                        if (--pwd < 0) pwd = 9;
                        _passwordPanel.SetText($"{pwd}");
                    }
                    catch (Exception e)
                    {
                        List<string> topic = Enviroment.Instance.FinalAnswerArray.ToList();
                        int index = topic.IndexOf(_passwordPanel.text);
                        if (--index < 0) index = topic.Count - 1;
                        _passwordPanel.SetText(Enviroment.Instance.FinalAnswerArray[index]);
                    }
                    break;
                }
            case "enter":
            case "backspace":
                {
                    GameObject insuranceObject = PuzzleManagement.Instance.GetPuzzleObject(_location, _puzzleName);
                    OrganPuzzle insuranceDoor = insuranceObject.GetComponent<OrganPuzzle>();
                    InsuranceBox insuanceBox = insuranceDoor.Organs[0].GetComponent<InsuranceBox>();
                    AnimationManagement.Instance.Play($"{_location}_{_puzzleName}_button_{_buttonValue}", "open");
                    if (_buttonValue.Equals("backspace") && insuanceBox.NumberObjects.Count > 0)
                    {
                        InsuranceButton insuanceButton = insuanceBox.NumberObjects[insuanceBox.NumberObjects.Count - 1].GetComponent<InsuranceButton>();
                        insuanceButton.AlreadyPlay = true;
                        insuanceButton.Status = false;
                        insuanceBox.NumberObjects.RemoveAt(insuanceBox.NumberObjects.Count - 1);
                        return;
                    }
                    if (_buttonValue.Equals("enter") && insuanceBox.NumberObjects.Count == 4)
                    {
                        bool isSolve = SharedUtils.StartCheckAnswer(_location, _puzzleName);
                        if (!isSolve && _errorSoundClip) SoundManagement.Instance.PlaySoundFXClip(_errorSoundClip, transform, 1f);
                        return;
                    }
                    break;
                }
            default:
                {
                    if (_passwordPanel != null)
                    {
                        if (_passwordPanel.text.Length < 4)
                        {
                            _passwordPanel.SetText(_passwordPanel.text.Trim() + _buttonValue);
                            if (_passwordPanel.text.Length != 4) return;
                        }
                    }
                    else
                    {
                        GameObject insuranceObject = PuzzleManagement.Instance.GetPuzzleObject(_location, _puzzleName);
                        OrganPuzzle insuranceDoor = insuranceObject.GetComponent<OrganPuzzle>();
                        InsuranceBox insuanceBox = insuranceDoor.Organs[0].GetComponent<InsuranceBox>();
                        InsuranceButton insuanceButton = GetComponent<InsuranceButton>();
                        if (insuanceBox.NumberObjects.Count < 4)
                        {
                            insuanceButton.AlreadyPlay = true;
                            insuanceButton.Status = true;
                            insuanceBox.NumberObjects.Add(gameObject);
                        }
                        return;
                    }
                    break;
                }

        }

        // puzzle pre checking
        PuzzleManagement.Instance.PreCheckAnswer(StartCheck);
    }

    private void DemoDisplayNumber()
    {
        DemoLockController demoLockController = _puzzleController.GetComponent<DemoLockController>();
        DemoButton demoButton = GetComponent<DemoButton>();
        switch (_buttonValue)
        {
            case "red":
            case "blue":
            case "green":
                if (PuzzleManagement.Instance.IsSolvePuzzle("demo", _buttonValue)) return;
                demoButton.AlreadyPlay = true;
                demoButton.Status = !demoButton.Status;
                if (demoButton.Status) demoLockController.SelectColor = _buttonValue;
                else demoLockController.SelectColor = null;
                if (_passwordPanel)
                {
                    if (demoButton.Status)
                    {
                        Color color = Color.white;
                        if (_buttonValue.Equals("red")) color = Color.red;
                        else if (_buttonValue.Equals("blue")) color = Color.blue;
                        else if (_buttonValue.Equals("green")) color = Color.green;
                        _passwordPanel.color = color;
                    }
                    else
                    {
                        _passwordPanel.color = Color.white;
                    }
                }
                break;
            case "enter":
            case "backspace":
                if (string.IsNullOrEmpty(demoLockController.SelectColor)) return;
                // play sound
                SoundManagement.Instance.PlaySoundFXClip(_clickSoundClip, transform, 1f);
                AnimationManagement.Instance.Play($"{_location}_button_{_buttonValue}", "open");
                if (_buttonValue.Equals("backspace") && demoLockController.NumberObjects.Count > 0)
                {
                    DemoButton dButton = demoLockController.NumberObjects[demoLockController.NumberObjects.Count - 1].GetComponent<DemoButton>();
                    dButton.AlreadyPlay = true;
                    dButton.Status = false;
                    demoLockController.NumberObjects.RemoveAt(demoLockController.NumberObjects.Count - 1);
                    string[] showTexts = new string[] { "X", "X", "X", "X" };
                    for (int i = 0, len = demoLockController.NumberObjects.Count; i < len; ++i)
                    {
                        PasswordPanelItem item = demoLockController.NumberObjects[i].GetComponent<PasswordPanelItem>();
                        showTexts[i] = item._buttonValue;
                    }
                    _passwordPanel.text = string.Join("", showTexts);
                    return;
                }
                if (_buttonValue.Equals("enter") && demoLockController.NumberObjects.Count == 4)
                {
                    bool isSolve = SharedUtils.StartCheckAnswer(_location, demoLockController.SelectColor);
                    if (isSolve)
                    {
                        // add subtitle
                        SubtitleManagement.Instance.AddSentencesToShow("Resolve Puzzle Statement Table", new string[] { _location, demoLockController.SelectColor, "t" });
                        _passwordPanel.color = Color.white;
                        demoLockController.SelectColor = null;
                        _passwordPanel.text = "XXXX";
                    }
                    else
                    {
                        _passwordPanel.text = "XXXX";
                        SoundManagement.Instance.PlaySoundFXClip(_errorSoundClip, transform, 1f);
                    }
                    if (demoLockController.CheckAllSolve())
                    {
                        LockManagement.Instance.Unlock(_location, "door");
                        demoLockController.Open();
                        demoLockController.GetComponent<PasswordPanelItem>().ClosePanel();
                    }
                    return;
                }
                break;
            default:
                {
                    if (_puzzleName.Equals("box"))
                    {
                        // box
                        _passwordPanel.text = _passwordPanel.text.Equals("0") ? "1" : "0";
                        PuzzleManagement.Instance.PreCheckAnswer(StartCheck);
                    } 
                    else
                    {
                        if (string.IsNullOrEmpty(demoLockController.SelectColor)) return;
                        if (demoLockController.NumberObjects.Count < 4)
                        {
                            if (_canRepeatSelect)
                            {
                                demoButton.AlreadyPlay = true;
                                demoButton.Status = true;
                                demoLockController.NumberObjects.Add(gameObject);
                            }
                            else
                            {
                                if (!demoLockController.NumberObjects.Contains(gameObject))
                                {
                                    demoButton.AlreadyPlay = true;
                                    demoButton.Status = true;
                                    demoLockController.NumberObjects.Add(gameObject);
                                    string[] showTexts = new string[] { "X", "X", "X", "X" };
                                    for (int i = 0, len = demoLockController.NumberObjects.Count; i < len; ++i)
                                    {
                                        PasswordPanelItem item = demoLockController.NumberObjects[i].GetComponent<PasswordPanelItem>();
                                        showTexts[i] = item._buttonValue;
                                    }
                                    _passwordPanel.text = string.Join("", showTexts);
                                }
                            }
                        }
                    }
                    
                    break;
                }
        }
        // play sound
        SoundManagement.Instance.PlaySoundFXClip(_clickSoundClip, transform, 1f);
    }

    private void StartCheck()
    {
        bool isSolve = SharedUtils.StartCheckAnswer(_location, _puzzleName);
        if (isSolve && _puzzleName.Equals("air_puzzle")) LockManagement.Instance.Unlock(_location, "door");
        if (!isSolve && _errorSoundClip) SoundManagement.Instance.PlaySoundFXClip(_errorSoundClip, transform, 1f);
        if (isSolve && _location == "living_room" && _puzzleName == "door")
        {
            _puzzleController.GetComponent<AudioSource>().Pause();
        }
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isActiving) return;
        if (_location.Equals("demo")) DemoDisplayNumber();
        else DisplayNumber();
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isActiving) return;
        HoverOutItem();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isActiving) return;
        HoverInItem();
    }

    #endregion
}
