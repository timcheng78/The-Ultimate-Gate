using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private MeshCollider _collider;
    private SwitchItem _handle;

    private void Awake()
    {
        _collider = GetComponent<MeshCollider>();
        _handle = transform.GetChild(0).GetComponent<SwitchItem>();
    }

    public void Open(string location)
    {
        if (_collider) StartCoroutine(ToggleCollider());
        AnimationManagement.Instance.Play(location + "_door" , "open");
    }

    public void Close(string location)
    {
        if (_collider) StartCoroutine(ToggleCollider());
        AnimationManagement.Instance.Play(location + "_door", "close");
    }

    public void SetHandleStatus(bool status)
    {
        if (_handle != null) _handle._status = status;
    }

    IEnumerator ToggleCollider()
    {
        _collider.enabled = false;
        yield return new WaitForSecondsRealtime(1);
        _collider.enabled = true;
    }
}
