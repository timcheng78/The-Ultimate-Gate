using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractController : MonoBehaviour
{
    private Vector3 _origin;
    private Vector3 _direction;
    private RaycastHit _hit;
    private CameraAttributes _cameraAttributes;
    private IInteractive _lastItem;
    private IInteractive _lastLightItem;
    private GameObject _currentObject;
    private GameObject _previousObject;

    private void Awake()
    {
        _cameraAttributes = GetComponent<CameraAttributes>();
    }

    public void Interact()
    {
        if (_lastItem == null) return;
        _lastItem.Interact();
    }

    public void Cancel()
    {
        if (PlayerAttributes.Instance._activingItem) PlayerAttributes.Instance._activingItem.GetComponent<IInteractive>().Cancel();
    }

    public void ResetRotation()
    {
        if (PlayerAttributes.Instance._activingItem && PlayerAttributes.Instance._activingItem.GetComponent<InteractItem>() != null)
        {
            InteractItem item = PlayerAttributes.Instance._activingItem.GetComponent<InteractItem>();
            item.ResetRotation();
        }
    }

    private void Update()
    {
        if (!Enviroment.Instance.IsStartPlay) return;
        _origin = transform.position;
        _direction = transform.forward;
        if (PlayerController.Instance.PhotoMode) return;
        InteractObjectScan();
        LightSensorScan();
    }

    private void InteractObjectScan()
    {
        if (Physics.Raycast(_origin, _direction, out _hit, _cameraAttributes._interactDistance, _cameraAttributes._interactLayer))
        {
            if (_hit.transform.gameObject.layer.Equals(13) || _hit.transform.gameObject.layer.Equals(17) || _hit.transform.gameObject.layer > 21)
            {
                if (_lastItem != null)
                {
                    _lastItem.HoverOut();
                    _lastItem = null;
                }
            }
            else
            {
                if (_lastItem != null)
                {
                    _lastItem.HoverOut();
                    _lastItem = null;
                }
                if (_hit.transform.TryGetComponent(out IInteractive item))
                {
                    _lastItem = item;
                    item.HoverIn();
                }
            }
        }
        else
        {
            if (_lastItem != null)
            {
                _lastItem.HoverOut();
                _lastItem = null;
            }
        }
    }

    private void LightSensorScan()
    {
        if (Physics.Raycast(_origin, _direction, out _hit, _cameraAttributes._interactDistance, _cameraAttributes._interactLayer))
        {
            GameObject hitObject = _hit.collider.gameObject;
            if (_hit.transform.gameObject.layer.Equals(17))
            {
                // �O�s���e���ؼ�
                _previousObject = _currentObject;

                // ��s��e�ؼ�
                _currentObject = hitObject;

                // ��s�ؼа���K�[�ĪG
                ApplyTargetEffect(_currentObject, true);
            }
            else
            {
                // �p�G�S�����y�������A�����ĪG
                if (_currentObject != null)
                {
                    ApplyTargetEffect(_currentObject, false);
                    _previousObject = _currentObject;
                    _currentObject = null;
                }
            }
        }
        else
        {
            // �p�G�S�����y�������A�����ĪG
            if (_currentObject != null)
            {
                ApplyTargetEffect(_currentObject, false);
                _previousObject = _currentObject;
                _currentObject = null;
            }
        }
    }

    private void ApplyTargetEffect(GameObject item, bool status)
    {
        if (item == null) return;

        item.TryGetComponent(out IInteractive interative);
        if (interative != null)
        {
            if (status) interative.HoverIn();
            else interative.HoverOut();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_origin, _direction * _hit.distance);
    }
}
