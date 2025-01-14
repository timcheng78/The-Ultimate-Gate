using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenColor : MonoBehaviour
{
    [SerializeField, ColorUsage(true, true)] private Color[] _colors = new Color[5];
    [SerializeField] private float _transitionDuration = 2.0f; // �C���C�⪺���ܮɶ�

    private bool _isStart = false;
    private MeshRenderer _render;
    private Material[] _materials;
    private int currentColorIndex = 0;   // ��e�C�⪺����
    private float transitionProgress = 0; // �C���ഫ���i��

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
            // �p���e�C��M�U�@���C�⤧��������
            Color nextColor = _colors[(currentColorIndex + 1) % _colors.Length];
            _materials[^1].SetColor("_EmissionColor", Color.Lerp(_colors[currentColorIndex], nextColor, transitionProgress));

            // ��s�ഫ�i��
            transitionProgress += Time.deltaTime * _transitionDuration;

            // �p�G�i�׹F��1�A������U�@���C��
            if (transitionProgress >= 1.0f)
            {
                transitionProgress = 0;
                currentColorIndex = (currentColorIndex + 1) % _colors.Length;
            }
            yield return null;
        }
    }
}
