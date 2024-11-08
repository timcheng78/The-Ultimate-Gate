using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPuzzle : MonoBehaviour
{
    [SerializeField] private string _type;
    [SerializeField] private int _sortNumber;
    [SerializeField] private bool _trigger = false;
    [SerializeField] private LightPuzzleController _lightPuzzleController;

    private Renderer _renderer;
    private Material[] _materials;
    private bool _startTimer = false;
    private float _timer_f = 0f;
    private int _timer_i = 0;
    private int _index = 0;

    public bool Trigger { get => _trigger; set => _trigger = value; }

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;

    }

    // Update is called once per frame
    void Update()
    {
        if (LockManagement.Instance.IsLocked("kitchen", "drawer_door")) return;
        if (_trigger) _materials[^1].EnableKeyword("_EMISSION");
        else _materials[^1].DisableKeyword("_EMISSION");
        if (_startTimer)
        {
            _timer_f += Time.deltaTime;
            _timer_i = (int)_timer_f;
        }
        ApplyCheckAround();
        ApplyTrigger();
    }

    private void ApplyCheckAround()
    {
        // 計算攝像機的視錐體平面
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        // 測試物體的包圍盒是否在視錐體平面內
        bool isVisible = GeometryUtility.TestPlanesAABB(planes, _renderer.bounds);

        if (isVisible)
        {
            if (!PlayerAttributes.Instance._isTriggerFlashlight) return;
            _startTimer = true;
        }
        else
        {
            if (!PlayerAttributes.Instance._isTriggerFlashlight) return;
            _startTimer = false;
            _timer_f = 0f;
            _timer_i = 0;
        }
    }

    private void ApplyTrigger()
    {
        if (_timer_i >= 1) _trigger = true;
        if (_trigger && _index.Equals(0)) _lightPuzzleController.SetIndex(_sortNumber);
        if (_trigger)
        {
            _timer_f = 0f;
            _timer_i = 0;
            _index = 1;
        }
        else _index = 0;
    }

}
