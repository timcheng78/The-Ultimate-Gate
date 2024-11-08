using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsuranceButton : MonoBehaviour
{
    [SerializeField] private bool _status;
    [SerializeField] private int _index;

    private bool _alreadyPlay = false;
    private Renderer _renderer;
    private Material[] _materials;

    public bool Status { get => _status; set => _status = value; }
    public bool AlreadyPlay { get => _alreadyPlay; set => _alreadyPlay = value; }
    public int Index { get => _index; set => _index = value; }

    private void Awake()
    {
        // setting renderer
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;
    }

    // Update is called once per frame
    void Update()
    {
        if (_status && _alreadyPlay) PushButton();
        else if (!_status && _alreadyPlay) PullButton();
    }

    private void PushButton()
    {
        AnimationManagement.Instance.Play($"bed_room_insurance_button_{_index}" , "open");
        _materials[0].EnableKeyword("_EMISSION");
        _alreadyPlay = false;
    }

    private void PullButton()
    {
        AnimationManagement.Instance.Play($"bed_room_insurance_button_{_index}" , "close");
        _materials[0].DisableKeyword("_EMISSION");
        _alreadyPlay = false;
    }
}
