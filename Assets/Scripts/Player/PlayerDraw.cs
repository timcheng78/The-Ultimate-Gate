using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerDraw : MonoBehaviour
{
    [Header("Drawing Settings")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private RawImage drawingArea;
    [SerializeField] private Color brushColor = Color.black;
    [SerializeField] private float brushSize = 5f;

    [Header("Cursor Settings")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D highlightCursor;

    private RenderTexture renderTexture;
    private Material drawingMaterial;
    private bool isDrawing = false;
    private bool canDraw = false;
    private Vector2 lastDrawPosition;
    private PlayerMoveInput _playerMoveInput;
    private int paintType = 0;
    private CanvasGroup canvasGroup;
    private DrawTab drawPad;
    public static PlayerDraw Instance { get; private set; }
    public GameObject DrawCanvas { get => canvas; }
    public int PaintType { get => paintType; set
        {
            drawPad.ChangePen(paintType, value);
            paintType = value;
            if (value < 5)
            {
                TriggerDraw(true);
            }
            else if (value.Equals(5))
            {
                // 游標模式
                TriggerDraw(false);
            }
        }
    }
    public bool CanDraw { get => canDraw; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        _playerMoveInput = new PlayerMoveInput();
        _playerMoveInput.Player.Draw.started += OnDrawStarted;
        _playerMoveInput.Player.Draw.canceled += OnDrawCanceled;
        canvas.TryGetComponent<CanvasGroup>(out canvasGroup);
        canvas.TryGetComponent<DrawTab>(out drawPad);
    }

    void Start()
    {
        ClearDrawPage();
    }

    public void SetCursor(Texture2D cursorTexture, Vector2 hotspot)
    {
        // 設定滑鼠指標樣式，熱點在圖片的中心
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }

    private void OnDrawStarted(InputAction.CallbackContext context)
    {
        isDrawing = true;
        Vector2 position = GetDrawPosition();
        lastDrawPosition = position;
        if (canDraw) DrawPoint(position);
    }

    private void OnDrawCanceled(InputAction.CallbackContext context)
    {
        isDrawing = false;
    }

    private Vector2 GetDrawPosition()
    {
        // 獲取滑鼠/觸控位置
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawingArea.rectTransform,
            screenPosition,
            null,
            out Vector2 localPoint);
        // 轉換到RenderTexture座標系
        Vector2 normalizedPoint = new Vector2(
            (localPoint.x + drawingArea.rectTransform.rect.width * .5f) / drawingArea.rectTransform.rect.width,
            (localPoint.y + drawingArea.rectTransform.rect.height * .5f) / drawingArea.rectTransform.rect.height
        );
        return new Vector2(
            normalizedPoint.x * renderTexture.width,
            normalizedPoint.y * renderTexture.height
        );
    }

    private void DrawPoint(Vector2 position)
    {
        RenderTexture.active = renderTexture;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, renderTexture.width, 0, renderTexture.height);

        Color useColor = Color.black;
        switch (paintType)
        {
            case 0:
                // black paint
                useColor = new Color(0, 0, 0, 1f);
                break;
            case 1:
                // white paint
                useColor = new Color(1, 1, 1, 1f);
                break;
            case 2:
                // red paint
                useColor = new Color(1, 0, 0, 1f);
                break;
            case 3:
                // blue paint
                useColor = new Color(0, 0, 1, 1f);
                break;
            case 4:
                // green paint
                useColor = new Color(0, 1, 0, 1f);
                break;
        }
        Texture2D brushTexture = CreateBrushTexture(brushSize, useColor);

        brushTexture.SetPixel(0, 0, brushColor);
        brushTexture.Apply();

        Graphics.DrawTexture(
            new Rect(position.x - brushSize * 0.5f, position.y - brushSize * 0.5f, brushSize, brushSize),
            brushTexture
        );

        GL.PopMatrix();
        RenderTexture.active = null;

        Destroy(brushTexture);
    }

    private Texture2D CreateBrushTexture(float size, Color color)
    {
        Texture2D texture = new Texture2D((int)size, (int)size, TextureFormat.RGBA32, false);
        float radius = size * 0.5f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);

                if (distance <= radius)
                {
                    // 使用平滑過渡
                    float smoothness = 1 - (distance / radius);
                    smoothness = Mathf.SmoothStep(0, 1, smoothness);
                    Color pixelColor = color;
                    pixelColor.a *= smoothness;
                    texture.SetPixel(x, y, pixelColor);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();
        return texture;
    }

    private void DrawLine(Vector2 startPos, Vector2 endPos)
    {
        float distance = Vector2.Distance(startPos, endPos);
        Vector2 direction = (endPos - startPos).normalized;

        float step = brushSize * 0.5f;
        int steps = Mathf.Max(1, Mathf.CeilToInt(distance / step));

        for (int i = 0; i < steps; i++)
        {
            Vector2 currentPos = Vector2.Lerp(startPos, endPos, (float)i / steps);
            DrawPoint(currentPos);
        }
    }

    private void Update()
    {
        if (!isDrawing || !canDraw) return;

        Vector2 currentPosition = GetDrawPosition();
        DrawLine(lastDrawPosition, currentPosition);
        lastDrawPosition = currentPosition;
    }

    public void ToggleDrawPage(bool status)
    {
        drawPad.ToggleDrawPad(status);
        TriggerDraw(status);
    }

    public void TriggerDraw(bool status)
    {
        canDraw = status;
        canvasGroup.blocksRaycasts = status;
        if (canDraw) SetCursor(highlightCursor, new Vector2(highlightCursor.width / 2, highlightCursor.height / 2));
        else SetCursor(defaultCursor, Vector2.zero);
        PlayerController.Instance.StopRotate = status;
    }

    public void ClearDrawPage()
    {
        // 創建RenderTexture
        renderTexture = new RenderTexture(1920, 1080, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        // 清除RenderTexture為白色背景
        Graphics.SetRenderTarget(renderTexture);
        GL.Clear(true, true, new Color(1, 1, 1, 0.05f));
        Graphics.SetRenderTarget(null);

        // 設置繪圖材質
        drawingMaterial = new Material(Shader.Find("Unlit/Texture"));
        drawingArea.texture = renderTexture;
    }


    #region - Enable / Disable

    private void OnEnable()
    {
        _playerMoveInput.Enable();
    }

    private void OnDisable()
    {
        _playerMoveInput.Disable();
    }

    #endregion
}
