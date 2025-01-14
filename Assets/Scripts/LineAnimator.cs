using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineAnimator : MonoBehaviour
{
    public Animator animator; // �j�w Animator
    public string progressParameter = "Progress"; // Animator �����B�I�ѼƦW��
    private LineRenderer lineRenderer;
    private List<Vector3> points = new();
    private bool isDrawingComplete = false; // �P�_�O�_�w����ø�s

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
        // �q Animator ����i�׭�
        float progress = animator.GetFloat(progressParameter);

        if (progress >= 1.0f)
        {
            isDrawingComplete = true;
            progress = 1.0f; // �T�O�̫᪺�i�פ��W�L 1
        }

        // �p���e���ӵe�����
        int totalPoints = points.Count;
        int segmentsToDraw = Mathf.FloorToInt(progress * (totalPoints - 1));
        float segmentProgress = (progress * (totalPoints - 1)) % 1;

        // ��s LineRenderer ���I��
        lineRenderer.positionCount = segmentsToDraw + 1;
        for (int i = 0; i <= segmentsToDraw; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }

        // ���ȳ]�m��e�q���̫�@���I
        if (segmentsToDraw < totalPoints - 1)
        {
            Vector3 start = points[segmentsToDraw];
            Vector3 end = points[segmentsToDraw + 1];
            Vector3 interpolated = Vector3.Lerp(start, end, segmentProgress);
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, interpolated);
        }
    }
}
