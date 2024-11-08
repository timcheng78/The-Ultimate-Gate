using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public bool selected;
    public bool changeColor;
    [SerializeField] private bool _status;
    [SerializeField] private ModeButton _brother;
    [SerializeField] private GameObject _startButton;
    [SerializeField] private AudioClip _hoverClip;
    [SerializeField] private AudioClip _clickClip;
    private Image _image;
    private Color _originColor;

    private void Awake()
    {
        TryGetComponent<Image>(out _image);
    }

    private void Start()
    {
        if (selected) _image.color = Color.red;
        OptionMenu.Instance.SetHardMode(false);
    }

    private void Update()
    {
        if (!selected && changeColor)
        {
            _image.color = Color.white;
            changeColor = false;
        } 
    }

    private void SelectMode()
    {
        selected = true;
        _brother.selected = false;
        _brother.changeColor = true;
        _image.color = Color.red;
        _originColor = Color.red;
        OptionMenu.Instance.SetHardMode(_status);
        if (!SoundManagement.Instance) return;
        SoundManagement.Instance.PlaySoundFXClip(_clickClip, transform, 1.0f, 0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _originColor = _image.color;
        _image.color = Color.yellow;
        if (!SoundManagement.Instance) return;
        SoundManagement.Instance.PlaySoundFXClip(_hoverClip, transform, 1.0f, 0f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _image.color = _originColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _startButton.SetActive(true);
        SelectMode();
    }
}
