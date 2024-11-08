using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuCamera : MonoBehaviour
{
    public float rotationSpeed = 10.0f;
    private CinemachineVirtualCamera virtualCamera;
    private Transform target;

    void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            target = virtualCamera.LookAt;
        }
    }

    void Update()
    {
        if (target != null)
        {
            transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
