using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManagement : MonoBehaviour
{
    /// <summary>
    /// 玩家第一人稱相機
    /// </summary>
    public CinemachineVirtualCamera playerVirtualCamera;
    /// <summary>
    /// 上方針孔相機
    /// </summary>
    public CinemachineVirtualCamera topHoleVirtualCamera;
    /// <summary>
    /// 下方針孔相機
    /// </summary>
    public CinemachineVirtualCamera bottomHoleVirtualCamera;
    /// <summary>
    /// 廁所1相機
    /// </summary>
    public CinemachineVirtualCamera bathroom1VirtualCamera;
    /// <summary>
    /// 第一階段動畫相機
    /// </summary>
    public CinemachineVirtualCamera animationVirtualCamera1;
    /// <summary>
    /// 第二階段動畫相機
    /// </summary>
    public CinemachineVirtualCamera animationVirtualCamera2;
    /// <summary>
    /// 試玩使用結束鏡頭
    /// </summary>
    public CinemachineVirtualCamera demoEndVirtualCamera;
    /// <summary>
    /// 物件專用相機
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
