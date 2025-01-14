using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineAnimator : MonoBehaviour
{
    public Animator animator; // 綁定 Animator
    public string progressParameter = "Progress"; // Animator 中的浮點參數名稱
    private LineRenderer lineRenderer;
    private List<Vector3> points = new();
    private bool isDrawingComplete = false; // 判斷是否已完成繪製

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        for (int i = 0; i < lineRenderer.positionCount; ++i)
        {
            points.Add(lineRenderer.GetPosition(i));
        }
        lineRenderer.SetPosition(0, points[0]);
    }

    [ContextMenu("Drawing")]
    private void PlayAnimator()
    {
        animator.Play("DrawText");
    }

    void Update()
    {
        if (isDrawingComplete) return;
        // 從 Animator 獲取進度值
        float progress = animator.GetFloat(progressParameter);

        if (progress >= 1.0f)
        {
            isDrawingComplete = true;
            progress = 1.0f; // 確保最後的進度不超過 1
        }

        // 計算當前應該畫到哪裡
        int totalPoints = points.Count;
        int segmentsToDraw = Mathf.FloorToInt(progress * (totalPoints - 1));
        float segmentProgress = (progress * (totalPoints - 1)) % 1;

        // 更新 LineRenderer 的點數
        lineRenderer.positionCount = segmentsToDraw + 1;
        for (int i = 0; i <= segmentsToDraw; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }

        // 插值設置當前段的最後一個點
        if (segmentsToDraw < totalPoints - 1)
        {
            Vector3 start = points[segmentsToDraw];
            Vector3 end = points[segmentsToDraw + 1];
            Vector3 interpolated = Vector3.Lerp(start, end, segmentProgress);
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, interpolated);
        }
    }
}
