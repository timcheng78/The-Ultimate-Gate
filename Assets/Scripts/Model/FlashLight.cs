using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLight : MonoBehaviour
{
    private CinemachinePOV _pov;
    private void Start()
    {
        _pov = CameraManagement.Instance.playerVirtualCamera.GetCinemachineComponent<CinemachinePOV>();
    }

    void Update()
    {
        ApplyRotate();
    }

    private void ApplyRotate()
    {
        if (!_pov) return;
        float horizontalAngle = _pov.m_HorizontalAxis.Value;
        float verticalAngle = _pov.m_VerticalAxis.Value;

        Quaternion horizontalRotation = Quaternion.Euler(0, horizontalAngle, 0);

        Quaternion verticalRotation = Quaternion.Euler(verticalAngle, 0, 0);

        Quaternion combinedRotation = horizontalRotation * verticalRotation;

        transform.rotation = Quaternion.Slerp(transform.rotation, combinedRotation, 10 * Time.fixedDeltaTime); // smooth the rotation
    }
}
