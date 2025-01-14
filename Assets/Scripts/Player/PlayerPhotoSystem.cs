using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class PhotoUI
{
    public GameObject targetObject;
    public RawImage photoImage;             // �Ӥ���RawImage�ե�
    public Image borderImage;               // ��ت�Image�ե�
    public RectTransform rectTransform;     // �Ω󱱨��m�M����
    public List<RectTransform> scaleHandles = new List<RectTransform>();
    public bool isDragging;
    public bool isScaling;
    public int activeScaleHandleIndex = -1;
}
public class PlayerPhotoSystem : MonoBehaviour
{
    [Header("Camera Settings")]
    public CinemachineVirtualCamera virtualCamera;
    public AudioClip _photoClip;
    public Camera mainCamera;
    public int photoResolutionWidth = 1920;
    public int photoResolutionHeight = 1080;

    [Header("UI Settings")]
    public Canvas targetCanvas;        // �n�N�Ӥ��ͦ��b����Canvas�W
    public float photoWidth = 400f;
    public float photoHeight = 300f;
    public float borderWidth = 10f;
    public Color borderColor = Color.white;
    public Texture2D trashPhoto;

    [Header("Photo Position")]
    public Vector2 initialPosition = new Vector2(100f, -100f); // �۹�󥪤W������m
    public float initialRotation = 45f;

    [Header("Flash Effect")]
    public float flashDuration = 0.05f;
    public float flashFadeOutDuration = 0.2f;

    [Header("Scale Handle Settings")]
    public float handleSize = 20f;
    public Color handleColor = new Color(1, 1, 1, 0.8f);
    public float minScale = 0.3f;
    public float maxScale = 3f;

    private List<PhotoUI> photos = new List<PhotoUI>();
    private PhotoUI currentDraggingPhoto;
    private Image flashImage;
    private Vector2 lastMousePosition;

    public static PlayerPhotoSystem Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        if (targetCanvas == null)
        {
            Debug.LogWarning("No target canvas assigned, creating new one...");
            CreateCanvas();
        }
    }
    void Start()
    {
        CreateFlashEffect();
    }

    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("PhotoCanvas");
        targetCanvas = canvasObj.AddComponent<Canvas>();
        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
    }

    void CreateFlashEffect()
    {
        GameObject flashObj = new GameObject("FlashEffect");
        flashObj.transform.SetParent(targetCanvas.transform, false);

        flashImage = flashObj.AddComponent<Image>();
        flashImage.color = new Color(1, 1, 1, 0);

        RectTransform flashRect = flashImage.rectTransform;
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.sizeDelta = Vector2.zero;
        flashRect.localScale = Vector3.one;
    }

    void Update()
    {
        if (PlayerDraw.Instance.CanDraw) return;
        HandlePhotoDragging();
    }

    public IEnumerator TakePhotoWithEffects(bool needPhoto = true)
    {
        // �{���ĪG
        flashImage.color = new Color(1, 1, 1, 0.7f);
        yield return new WaitForSeconds(flashDuration);
        SoundManagement.Instance.PlaySoundFXClip(_photoClip, transform, 1f);
        if (needPhoto) yield return StartCoroutine(CapturePhoto());

        // �H�X�{��
        float elapsed = 0f;
        while (elapsed < flashFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.7f, 0f, elapsed / flashFadeOutDuration);
            flashImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        flashImage.color = new Color(1, 1, 1, 0);
    }

    IEnumerator CapturePhoto()
    {
        yield return new WaitForEndOfFrame();

        RenderTexture rt = new RenderTexture(photoResolutionWidth, photoResolutionHeight, 24);
        mainCamera.targetTexture = rt;
        mainCamera.Render();

        Texture2D photo = new Texture2D(photoResolutionWidth, photoResolutionHeight, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        photo.ReadPixels(new Rect(0, 0, photoResolutionWidth, photoResolutionHeight), 0, 0);
        photo.Apply();

        mainCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        CreatePhotoUI(photo);
    }

    void CreatePhotoUI(Texture2D photoTexture)
    {
        // �ЫطӤ��e��
        GameObject photoContainer = new GameObject("Photo");
        photoContainer.transform.SetParent(targetCanvas.transform, false);

        // �]�mRectTransform
        RectTransform containerRect = photoContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);  // ���I�]�b���W��
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0.5f, 0.5f); // �]�m�����I������b��
        containerRect.sizeDelta = new Vector2(photoWidth + borderWidth * 2, photoHeight + borderWidth * 2);
        containerRect.anchoredPosition = initialPosition;
        containerRect.localRotation = Quaternion.Euler(0, 0, initialRotation);

        // �Ыإզ����
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(containerRect, false);
        Image borderImage = borderObj.AddComponent<Image>();
        borderImage.color = borderColor;
        RectTransform borderRect = borderImage.rectTransform;
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;

        // �ЫطӤ�RawImage
        GameObject photoObj = new GameObject("PhotoImage");
        photoObj.transform.SetParent(containerRect, false);
        RawImage photoImage = photoObj.AddComponent<RawImage>();
        photoImage.texture = photoTexture;
        RectTransform photoRect = photoImage.rectTransform;
        photoRect.anchorMin = Vector2.zero;
        photoRect.anchorMax = Vector2.one;
        photoRect.sizeDelta = new Vector2(-borderWidth * 2, -borderWidth * 2);
        photoRect.anchoredPosition = Vector2.zero;

        // �K�[�즲����
        photoContainer.AddComponent<CanvasGroup>();

        // �Ы�PhotoUI����å[�J�C��
        PhotoUI newPhotoUI = new PhotoUI
        {
            targetObject = photoContainer,
            photoImage = photoImage,
            borderImage = borderImage,
            rectTransform = containerRect,
            isDragging = false
        };

        CreateScaleHandle(containerRect, new Vector2(-1, -1), newPhotoUI); // ���U
        CreateScaleHandle(containerRect, new Vector2(-1, 1), newPhotoUI);  // ���W
        CreateScaleHandle(containerRect, new Vector2(1, -1), newPhotoUI);  // �k�U
        CreateScaleHandle(containerRect, new Vector2(1, 1), newPhotoUI);   // �k�W

        if (photos.Count > 1)
        {
            Destroy(photos[0].targetObject);
            photos.RemoveAt(0);
        }
        photos.Add(newPhotoUI);
    }

    private void CreateScaleHandle(RectTransform containerRect, Vector2 anchorPosition, PhotoUI photoUI)
    {
        GameObject handleObj = new GameObject("ScaleHandle");
        handleObj.transform.SetParent(containerRect, false);

        RawImage handleImage = handleObj.AddComponent<RawImage>();
        handleImage.color = handleColor;
        if (anchorPosition.Equals(new Vector2(1, 1)))
        {
            handleImage.texture = trashPhoto;
        }

        RectTransform handleRect = handleImage.rectTransform;
        if (anchorPosition.Equals(new Vector2(1, 1)))
        {
            handleRect.sizeDelta = new Vector2(150f, 150f); ;
        }
        else
        {
            handleRect.sizeDelta = new Vector2(handleSize, handleSize);
        }

        // �]�m�����I��m
        handleRect.anchorMin = new Vector2((anchorPosition.x + 1) / 2, (anchorPosition.y + 1) / 2);
        handleRect.anchorMax = handleRect.anchorMin;
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;

        photoUI.scaleHandles.Add(handleRect);
    }


    private void HandlePhotoDragging()
    {
        if (PlayerController.Instance.IsDragging && currentDraggingPhoto != null && !PlayerController.Instance.IsDragging.Equals(currentDraggingPhoto.targetObject)) return;
        if (Input.GetMouseButtonDown(0))
        {
            // �ˬd�O�_�I�����Y�񱱨��I
            foreach (PhotoUI photo in photos)
            {
                for (int i = 0; i < photo.scaleHandles.Count; i++)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(
                        photo.scaleHandles[i],
                        Input.mousePosition,
                        null))
                    {
                        if (i.Equals(3))
                        {
                            // delete photo
                            Destroy(photo.targetObject);
                            photos.Remove(photo);
                        }
                        else
                        {
                            photo.isScaling = true;
                            photo.activeScaleHandleIndex = i;
                            currentDraggingPhoto = photo;
                            PlayerController.Instance.IsDragging = photo.targetObject;
                            lastMousePosition = Input.mousePosition;
                        }
                        return;
                    }
                }

                // �N�ƹ���m�ഫ��Canvas�Ŷ��y��
                Vector2 mousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    targetCanvas.GetComponent<RectTransform>(),
                    Input.mousePosition,
                    null,
                    out mousePos
                );

                lastMousePosition = mousePos;

                // �즳���즲�˴�...
                if (RectTransformUtility.RectangleContainsScreenPoint(
                    photo.rectTransform,
                    Input.mousePosition,
                    null))
                {
                    photo.isDragging = true;
                    currentDraggingPhoto = photo;
                    PlayerController.Instance.IsDragging = photo.targetObject;
                    photo.rectTransform.SetAsLastSibling();
                    break;
                }
            }
        }
        else if (Input.GetMouseButton(0) && currentDraggingPhoto != null)
        {
            PlayerController.Instance.StopRotate = true;
            if (currentDraggingPhoto.isScaling)
            {
                HandleScaling();
            }
            else if (currentDraggingPhoto.isDragging)
            {
                Vector2 mousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    targetCanvas.GetComponent<RectTransform>(),
                    Input.mousePosition,
                    null,
                    out mousePos
                );

                Vector2 difference = mousePos - lastMousePosition;
                currentDraggingPhoto.rectTransform.anchoredPosition += difference;
                lastMousePosition = mousePos;
            }
        }
        else if (Input.GetMouseButtonUp(0) && currentDraggingPhoto != null)
        {
            currentDraggingPhoto.isDragging = false;
            currentDraggingPhoto.isScaling = false;
            currentDraggingPhoto.activeScaleHandleIndex = -1;
            currentDraggingPhoto = null;
            PlayerController.Instance.IsDragging = null;
            PlayerController.Instance.StopRotate = false;
        }
    }

    private void HandleScaling()
    {
        Vector2 currentMousePosition = Input.mousePosition;
        Vector2 difference = currentMousePosition - lastMousePosition;

        // ����Ӥ������I���ù��y��
        Vector3[] corners = new Vector3[4];
        currentDraggingPhoto.rectTransform.GetWorldCorners(corners);
        Vector2 centerPoint = (corners[0] + corners[2]) * 0.5f;

        // �p��ƹ��۹�󤤤��I����m
        Vector2 mouseVectorFromCenter = currentMousePosition - centerPoint;
        Vector2 lastMouseVectorFromCenter = lastMousePosition - centerPoint;

        // �p���Y��]�l
        float currentDistance = mouseVectorFromCenter.magnitude;
        float lastDistance = lastMouseVectorFromCenter.magnitude;
        float scaleFactor = currentDistance / lastDistance;

        // �����Y��
        Vector2 currentSize = currentDraggingPhoto.rectTransform.sizeDelta;
        Vector2 newSize = currentSize * scaleFactor;

        // �O�����e��
        float aspectRatio = photoWidth / photoHeight;

        // ����̤p�M�̤j�ؤo
        float targetWidth = Mathf.Clamp(newSize.x, photoWidth * minScale, photoWidth * maxScale);
        float targetHeight = targetWidth / aspectRatio;

        currentDraggingPhoto.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);

        // ��s��m�H�O���즲�I�T�w
        Vector2 newMouseVectorFromCenter = mouseVectorFromCenter.normalized * currentDistance;
        Vector2 positionCorrection = (newMouseVectorFromCenter - mouseVectorFromCenter);
        currentDraggingPhoto.rectTransform.position += (Vector3)positionCorrection;

        lastMousePosition = currentMousePosition;
    }

    public void DeletePhoto()
    {
        if (photos.Count.Equals(0)) return;
        Destroy(photos[0].targetObject);
        photos.RemoveAt(0);
    }

    public void DeleteAllPhoto()
    {
        if (photos.Count.Equals(0)) return;
        foreach (PhotoUI photo in photos)
        {
            Destroy(photos[0].targetObject);
        }
        photos.Clear();
    }

    void OnDestroy()
    {
        // �M�z�귽
        foreach (PhotoUI photo in photos)
        {
            if (photo.photoImage != null && photo.photoImage.texture != null)
            {
                Destroy(photo.photoImage.texture);
            }
        }
    }
}
