using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BookMoving : MonoBehaviour
{
    [SerializeField] private GraphicRaycaster raycaster; // 將 Canvas 上的 GraphicRaycaster 拖到此欄位
    private PointerEventData pointerEventData;
    private Canvas parentCanvas;
    private RectTransform book;
    private EventSystem eventSystem;
    private bool isDragging = false;
    private Vector2 lastMousePosition;
    void Start()
    {
        // 獲取 EventSystem
        eventSystem = EventSystem.current;
        parentCanvas = transform.parent.GetComponent<Canvas>();
    }

    void Update()
    {
        if (PlayerDraw.Instance.CanDraw) return;
        HandleBookDragging();
    }

    private void HandleBookDragging()
    {
        if (PlayerController.Instance.IsDragging && !PlayerController.Instance.IsDragging.Equals(gameObject)) return;
        if (Input.GetMouseButtonDown(0))
        {
            // 將滑鼠位置轉換為Canvas空間座標
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.GetComponent<RectTransform>(),
                Input.mousePosition,
                null,
                out mousePos
            );

            lastMousePosition = mousePos;
            if (RectTransformUtility.RectangleContainsScreenPoint(
                GetComponent<RectTransform>(),
                Input.mousePosition,
                null))
            {
                isDragging = true;
                TryGetComponent<RectTransform>(out book);
                PlayerController.Instance.IsDragging = gameObject;
                if (parentCanvas.sortingOrder.Equals(1)) parentCanvas.sortingOrder = 2;
                book.SetAsLastSibling();
            }
        }
        else if (Input.GetMouseButton(0) && book != null)
        {
            if (isDragging)
            {
                Vector2 mousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.GetComponent<RectTransform>(),
                    Input.mousePosition,
                    null,
                    out mousePos
                );

                Vector2 difference = mousePos - lastMousePosition;
                book.anchoredPosition += difference;
                lastMousePosition = mousePos;
            }
        }
        else if (Input.GetMouseButtonUp(0) && book != null)
        {
            isDragging = false;
            book = null;
            PlayerController.Instance.IsDragging = null;
            PlayerController.Instance.StopRotate = false;
            parentCanvas.sortingOrder = 1;
        }
    }
}
