using Cinemachine;
using Coffee.UIExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAttributes : MonoBehaviour
{
    [Header("Camera Configuration")]
    [SerializeField] private float _minShakeAngle = 5; // 最小晃動幅度
    [SerializeField] private float _fadeSpeed = .05f;
    [SerializeField] private float _previousHorizontalAngle;
    [SerializeField] private bool _dontDissolve = false;
    [Header("Storm Setting")]
    [SerializeField] private GameObject[] _stormTexts;
    [SerializeField] private GameObject _stormCanvas;
    [SerializeField] private GameObject _stormEndCanvas;

    public float _mouseSensitive = 1300f;
    public float _interactDistance = 5f;
    public bool _cursorVisable = false;
    public bool _screenLock = false;
    public CursorLockMode _cursorLockMode = CursorLockMode.Confined;
    public LayerMask _interactLayer;

    private CinemachinePOV _pov;
    private int _index = 0;

    private void Start()
    {
        _pov = CameraManagement.Instance.playerVirtualCamera.GetCinemachineComponent<CinemachinePOV>();
        if (_pov != null) _previousHorizontalAngle = _pov.m_HorizontalAxis.Value;
        else Debug.LogError("The virtual camera does not have a POV component.");
    }

    private void Update()
    {
        Cursor.visible = _cursorVisable;
        Cursor.lockState = _cursorLockMode;
        if (OptionMenu.Instance != null) _mouseSensitive = OptionMenu.Instance.mouseSensitivity;
        if (_mouseSensitive < 500) _minShakeAngle = 2;
        else if (_mouseSensitive >= 500 && _mouseSensitive < 1000) _minShakeAngle = 5;
        else if (_mouseSensitive >= 1000) _minShakeAngle = 7;
        ApplyShakeCount();
    }

    public void ApplyMouseSensitive()
    {
        if (_pov.m_VerticalAxis.m_MaxSpeed.Equals(0)) return;
        _pov.m_VerticalAxis.m_MaxSpeed = _mouseSensitive;
        _pov.m_HorizontalAxis.m_MaxSpeed = _mouseSensitive;
    }

    public void ZeroCameraSpeed()
    {
        if (_pov == null) _pov = CameraManagement.Instance.playerVirtualCamera.GetCinemachineComponent<CinemachinePOV>();
        _pov.m_VerticalAxis.m_MaxSpeed = 0;
        _pov.m_HorizontalAxis.m_MaxSpeed = 0;
    }

    public IEnumerator RestoreCameraSpeed()
    {
        yield return new WaitForSeconds(0.1f);
        _pov.m_VerticalAxis.m_MaxSpeed = _mouseSensitive;
        _pov.m_HorizontalAxis.m_MaxSpeed = _mouseSensitive;
    }

    private void ApplyShakeCount()
    {
        float currentHorizontalAngle = _pov.m_HorizontalAxis.Value;
        float angleDifference = currentHorizontalAngle - _previousHorizontalAngle;


        // 檢查晃動幅度是否超過閾值
        if (Mathf.Abs(angleDifference) > _minShakeAngle)
        {
            // 檢查左右晃動
            if ((angleDifference > 0 && _previousHorizontalAngle < 0) || (angleDifference < 0 && _previousHorizontalAngle > 0))
            {
                if (!_dontDissolve && SubtitleManagement.Instance.SentencesGameObject.transform.childCount > 0)
                {
                    foreach (UIDissolve dissolve in SubtitleManagement.Instance.SentencesGameObject.transform.GetComponentsInChildren<UIDissolve>())
                    {
                        if (!dissolve.enabled) continue;
                        dissolve.effectFactor += _fadeSpeed;
                        if (dissolve.effectFactor >= 0.99) dissolve.effectFactor = 1.0f;
                    }
                }
                else if (_dontDissolve)
                {
                    _stormTexts[_index++].SetActive(true);
                    if (_index.Equals(_stormTexts.Length))
                    {
                        StartCoroutine(DeleteStorm());
                        _dontDissolve = false;
                    }
                }
            }
        }

        _previousHorizontalAngle = currentHorizontalAngle;
    }

    private IEnumerator DeleteStorm()
    {
        CanvasGroup group = _stormCanvas.GetComponent<CanvasGroup>();
        _stormEndCanvas.SetActive(true);
        while (group.alpha > 0)
        {
            group.alpha -= .01f;
            yield return new WaitForSeconds(0.01f);
        }
        _stormCanvas.SetActive(false);
    }

    public void StartStorm()
    {
        _stormCanvas.SetActive(true);
        _dontDissolve = true;
    }

    public void StartToCloseStorm()
    {
        StartCoroutine(CloseStorm());
    }

    public IEnumerator CloseStorm()
    {
        CanvasGroup group = _stormEndCanvas.GetComponent<CanvasGroup>();
        while (group.alpha > 0)
        {
            group.alpha -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
        _stormEndCanvas.SetActive(false);
    }
}
