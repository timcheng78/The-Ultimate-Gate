using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManagement : MonoBehaviour
{
    /// <summary>
    /// ���a�Ĥ@�H�٬۾�
    /// </summary>
    public CinemachineVirtualCamera playerVirtualCamera;
    /// <summary>
    /// �W��w�լ۾�
    /// </summary>
    public CinemachineVirtualCamera topHoleVirtualCamera;
    /// <summary>
    /// �U��w�լ۾�
    /// </summary>
    public CinemachineVirtualCamera bottomHoleVirtualCamera;
    /// <summary>
    /// �Z��1�۾�
    /// </summary>
    public CinemachineVirtualCamera bathroom1VirtualCamera;
    /// <summary>
    /// �Ĥ@���q�ʵe�۾�
    /// </summary>
    public CinemachineVirtualCamera animationVirtualCamera1;
    /// <summary>
    /// �ĤG���q�ʵe�۾�
    /// </summary>
    public CinemachineVirtualCamera animationVirtualCamera2;
    /// <summary>
    /// �ժ��ϥε������Y
    /// </summary>
    public CinemachineVirtualCamera demoEndVirtualCamera;
    /// <summary>
    /// ����M�ά۾�
    /// </summary>
    public Camera objectCamera;
    /// <summary>
    /// TODO - REMOVE
    /// </summary>
    public Camera _interactCamera;

    public static CameraManagement Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one CameraManagement in the scene.");
        }
        Instance = this;
    }

    public void TogglePlayerVirtualCamera(bool status)
    {
        playerVirtualCamera.enabled = status;
    }

    public void ToggleTopHoleVirtualCamera(bool status)
    {
        topHoleVirtualCamera.enabled = status;
    }

    public void ToggleBottomHoleVirtualCamera(bool status)
    {
        bottomHoleVirtualCamera.enabled = status;
    }

    public void ToggleBathroom1VirtualCamera(bool status)
    {
        bathroom1VirtualCamera.enabled = status;
    }

    public void ChangeHoleCamera(string position, bool status)
    {
        TogglePlayerVirtualCamera(!status);
        PlayerMovement.Instance.ToggleMove(!status);
        if (position.Equals("top")) ToggleTopHoleVirtualCamera(status);
        else if (position.Equals("bottom")) ToggleBottomHoleVirtualCamera(status);
    }
}
