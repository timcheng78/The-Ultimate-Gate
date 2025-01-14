using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenColor : MonoBehaviour
{
    [SerializeField, ColorUsage(true, true)] private Color[] _colors = new Color[5];
    [SerializeField] private float _transitionDuration = 2.0f; // 每個顏色的漸變時間

    private bool _isStart = false;
    private MeshRenderer _render;
    private Material[] _materials;
    private int currentColorIndex = 0;   // 當前顏色的索引
    private float transitionProgress = 0; // 顏色轉換的進度

    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent<MeshRenderer>(out _render);
        _materials = _render.materials;
    }

    [ContextMenu("StartEmission")]
    public void ToggleScreenEmission()
    {
        _materials[^1].EnableKeyword("_EMISSION");
        _isStart = !_isStart;
        StartCoroutine(StartTransferColor());
    }

    IEnumerator StartTransferColor()
    {
        while (_isStart)
        {
            // 計算當前顏色和下一個顏色之間的插值
            Color nextColor = _colors[(currentColorIndex + 1) % _colors.Length];
            _materials[^1].SetColor("_EmissionColor", Color.Lerp(_colors[currentColorIndex], nextColor, transitionProgress));

            // 更新轉換進度
            transitionProgress += Time.deltaTime * _transitionDuration;

            // 如果進度達到1，切換到下一個顏色
            if (transitionProgress >= 1.0f)
            {
                transitionProgress = 0;
                currentColorIndex = (currentColorIndex + 1) % _colors.Length;
            }
            yield return null;
        }
    }
}
