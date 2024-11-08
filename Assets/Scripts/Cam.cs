using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour, IInteractive
{
    [Header("camera setting")]
    [SerializeField] private Transform _player;
    [SerializeField] private GameObject _cam;
    [SerializeField] private float fadeSpeed = 0.5f;

    [Header("hint setting")]
    [SerializeField] private string _location;
    [SerializeField] private AudioClip _audio;

    private int _scheduleNumber = 0;
    private bool _isFadingIn = true;
    private float _emissionIntensity = 0f;
    private Renderer _renderer;
    private Material[] _materials;

    private void Awake()
    {
        // setting renderer
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;
    }

    private void Start()
    {
        if (Enviroment.Instance.Player) _player = Enviroment.Instance.Player.transform;
    }
    // Update is called once per frame
    void Update()
    {
        LookAtPlayer();
        Shinning();
    }

    private void LookAtPlayer()
    {
        transform.LookAt(_player.transform);
    }

    private void Shinning()
    {
        Material material = _cam.GetComponent<Renderer>().material;
        material.EnableKeyword("_EMISSION");
        ApplyShinning();
        UpdateEmission();
    }

    private void ApplyShinning()
    {
        // 根據 isFadingIn 判斷是淡入還是淡出
        if (_isFadingIn)
        {
            _emissionIntensity += fadeSpeed * Time.deltaTime;
            if (_emissionIntensity >= 1f)
            {
                _emissionIntensity = 1f;
                _isFadingIn = false;
            }
        }
        else
        {
            _emissionIntensity -= fadeSpeed * Time.deltaTime;
            if (_emissionIntensity <= 0f)
            {
                _emissionIntensity = 0f;
                _isFadingIn = true;
            }
        }
    }

    private void UpdateEmission()
    {
        Material material = _cam.GetComponent<Renderer>().material;
        // 設置 emission 顏色和強度
        Color finalColor = Color.red * Mathf.LinearToGammaSpace(_emissionIntensity);
        material.SetColor("_EmissionColor", finalColor);
    }

    public void HoverIn()
    {
        HoverInItem();
    }

    public void HoverOut()
    {
        HoverOutItem();
    }

    public void Interact()
    {
        SoundManagement.Instance.PlaySoundFXClip(_audio, transform, 1f);
        if (!OptionMenu.Instance.hardMode && !SubtitleManagement.Instance.IsTyping) GetHint();
    }

    public void Cancel()
    {
        return;
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

    private void GetHint()
    {
        CheckNowSchedule();
        SubtitleManagement.Instance.AddSentencesToShow("Hint Table", new string[] { _location, "hint", _scheduleNumber + "" });
    }

    private void CheckNowSchedule()
    {
        switch (_location)
        {
            case "demo":
                SettingDemoRoomNumber();
                break;
            case "book_room":
                SettingBookRoomNumber();
                break;
            case "kitchen":
                SettingKitchenNumber();
                break;
            case "bed_room":
                SettingBedRoomNumber();
                break;
            case "bath_room_1":
                SettingBathRoom1Number();
                break;
            case "living_room":
                SettingLivingRoomNumber();
                break;
        }
    }

    private void SettingDemoRoomNumber()
    {
        switch (GetStatus())
        {
            case 0:
                _scheduleNumber = 0;
                break;
            case 1:
                if (_scheduleNumber < 3) _scheduleNumber++;
                break;
            case 2:
                _scheduleNumber = 3;
                break;
            case 3:
                _scheduleNumber = 4;
                break;
            case 4:
                _scheduleNumber = 5;
                break;
            default:
                _scheduleNumber = 6;
                break;
        }

        int GetStatus()
        {
            int status = -1;
            if (!PuzzleManagement.Instance.IsSolvePuzzle(_location, "red"))
            {
                // blue not solve
                if (!PuzzleManagement.Instance.IsSolvePuzzle(_location, "blue"))
                {
                    // green not solve
                    if (!PuzzleManagement.Instance.IsSolvePuzzle(_location, "green"))
                    {
                        status = 1;
                    }
                    // green solve
                    else
                    {
                        status = 2;
                    }
                }
                // blue solve
                else
                {
                    status = 2;
                }
            }
            else
            {
                // blue not solve
                if (!PuzzleManagement.Instance.IsSolvePuzzle(_location, "blue"))
                {
                    //  green not solve
                    if (!PuzzleManagement.Instance.IsSolvePuzzle(_location, "green"))
                    {
                        status = 3;
                    }
                    // green solve
                    else
                    {
                        status = 3;
                    }
                }
                // blue solve
                else
                {
                    //  green not solve
                    if (!PuzzleManagement.Instance.IsSolvePuzzle(_location, "green"))
                    {
                        status = 4;
                    }
                    // green solve
                    else
                    {
                        status = -1;
                    }
                }
            }
            return status;
        }
    }

    private void SettingBookRoomNumber()
    {
        if (!PuzzleManagement.Instance.IsSolvePuzzle("book_room", "shief")) _scheduleNumber = 1;
        else _scheduleNumber = 2;
    }

    private void SettingKitchenNumber()
    {
        if (!PuzzleManagement.Instance.IsSolvePuzzle("kitchen", "electrical_box")) _scheduleNumber = 1;
        else if (!PuzzleManagement.Instance.IsSolvePuzzle("kitchen", "induction")) _scheduleNumber = 2;
        else _scheduleNumber = 3;
    }

    private void SettingBedRoomNumber()
    {
        if (!PuzzleManagement.Instance.IsSolvePuzzle("bed_room", "air_puzzle")) _scheduleNumber = 1;
        else if (!PuzzleManagement.Instance.IsSolvePuzzle("bed_room", "insurance")) _scheduleNumber = 2;
        else _scheduleNumber = 3;
    }

    private void SettingBathRoom1Number()
    {
        if (!PuzzleManagement.Instance.IsSolvePuzzle("bath_room_1", "cabinet")) _scheduleNumber = 1;
        else if (!PuzzleManagement.Instance.IsSolvePuzzle("bath_room_1", "puzzle")) _scheduleNumber = 2;
        else _scheduleNumber = 3;
    }

    private void SettingLivingRoomNumber()
    {
        if (!PuzzleManagement.Instance.IsSolvePuzzle("living_room", "display_door")) _scheduleNumber = 1;
        else if (!PuzzleManagement.Instance.IsSolvePuzzle("living_room", "door")) _scheduleNumber = 2;
        else _scheduleNumber = 3;
    }
    
}
