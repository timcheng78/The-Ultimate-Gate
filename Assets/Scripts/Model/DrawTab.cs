using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawTab : MonoBehaviour
{
    [SerializeField] private Vector2 openPosition;
    [SerializeField] private Vector2 closedPosition;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve elasticCurve; // �ϥΰʵe���u�ӽվ�u®�ĪG

    [Header("pen detail")]
    [SerializeField] private RectTransform[] pens;
    [SerializeField] private float duration = 1f;
    private RectTransform drawPad;
    private RectTransform tabIcon;
    private Vector2 selectPosition = new Vector2(-29, 53);
    private Vector2 unSelectPosition = new Vector2(29, -53);

    // Start is called before the first frame update
    void Start()
    {
        transform.Find("Draw Pad").TryGetComponent<RectTransform>(out drawPad);
        drawPad.Find("Tab Icon").TryGetComponent<RectTransform>(out tabIcon);
    }

    private void Update()
    {
        if (!Enviroment.Instance.IsStartPlay || Enviroment.Instance.IsPause)
        {
            drawPad.gameObject.SetActive(false);
        }
        else
        {
            drawPad.gameObject.SetActive(true);
        }
        
    }

    public void ChangePen(int newType)
    {
        PlayerDraw.Instance.PaintType = newType;
    }

    public void ClearDrawPage()
    {
        PlayerDraw.Instance.ClearDrawPage();
    }

    public void ChangePen(int originType, int newType)
    {
        if (originType.Equals(newType)) return;
        if (!originType.Equals(5))
        {
            RectTransform originPen = pens[originType];
            StartCoroutine(MoveRectTransform(originPen, false));
        }
        if (newType < 5)
        {
            RectTransform selectedPen = pens[newType];
            StartCoroutine(MoveRectTransform(selectedPen, true));
        }
    }

    IEnumerator MoveRectTransform(RectTransform targetRect, bool isSelected)
    {
        // �O����l��m
        Vector2 initialOffsetMin = targetRect.offsetMin;
        Vector2 initialOffsetMax = targetRect.offsetMax;

        // �p��ؼЦ�m
        Vector2 targetOffsetMin = initialOffsetMin + (isSelected ? selectPosition : unSelectPosition);
        Vector2 targetOffsetMax = initialOffsetMax + (isSelected ? selectPosition : unSelectPosition);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // �u�ʴ��Ȩ�ؼжZ��
            targetRect.offsetMin = Vector2.Lerp(initialOffsetMin, targetOffsetMin, t);
            targetRect.offsetMax = Vector2.Lerp(initialOffsetMax, targetOffsetMax, t);

            yield return null; // ���ݤU�@�V
        }

        // �T�O��F�ؼЦ�m
        targetRect.offsetMin = targetOffsetMin;
        targetRect.offsetMax = targetOffsetMax;
    }

    public void ToggleDrawPad(bool status)
    {
        // �ھڪ��A�]�m�ؼЦ�m
        Vector2 targetPosition = status ? openPosition : closedPosition;

        // �}�l��{�H���Ʀa���ܦ�m
        StartCoroutine(AnimateDrawPad(targetPosition, status));
    }

    IEnumerator AnimateDrawPad(Vector2 targetPosition, bool status)
    {
        Vector2 startPosition = drawPad.anchoredPosition; // �����e��m
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            // �p����ʵe���u���i��
            float t = elapsedTime / animationDuration;
            float easedProgress = status ? elasticCurve.Evaluate(t) : t;

            // �ϥδ��ȩM���ƮĪG
            drawPad.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, easedProgress);

            elapsedTime += Time.deltaTime;
            yield return null; // ���ݤU�@�V
        }

        // �T�O�̲׳]�m���ؼЦ�m
        drawPad.anchoredPosition = targetPosition;
    }
}
