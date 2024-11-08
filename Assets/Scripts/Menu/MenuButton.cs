using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private bool _buttonDisabled;
    [SerializeField] private Color _originColor;
    [SerializeField] private AudioClip _hoverClip;
    [SerializeField] private AudioClip _clickClip;
    private TMP_Text _childText;
    private Button _button;

    private void Awake()
    {
        _childText = transform.GetChild(0).GetComponent<TMP_Text>();
        _button = GetComponent<Button>();
    }

    private void Start()
    {
        if (_buttonDisabled && DataPersistenceManagement.Instance && DataPersistenceManagement.Instance.HasData) _buttonDisabled = false;
        if (_buttonDisabled)
        {
            _button.interactable = false;
            _childText.color = Color.gray;
        }
        else
        {
            _button.interactable = true;
            _childText.color = _originColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _childText.color = Color.red;
        if (!SoundManagement.Instance) return;
        SoundManagement.Instance.PlaySoundFXClip(_hoverClip, transform, 1.0f, 0f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _childText.color = _originColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _childText.color = Color.red;
        if (!SoundManagement.Instance) return;
        SoundManagement.Instance.PlaySoundFXClip(_clickClip, transform, 1.0f, 0f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _childText.color = _originColor;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!_button.interactable) return;
        _childText.color = Color.red;
        if (!SoundManagement.Instance) return;
        SoundManagement.Instance.PlaySoundFXClip(_hoverClip, transform, 1.0f, 0f);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (!_button.interactable) return;
        _childText.color = _originColor;
    }
}
