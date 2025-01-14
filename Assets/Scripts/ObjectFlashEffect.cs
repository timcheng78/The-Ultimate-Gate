using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFlashEffect : MonoBehaviour
{
    // 光源元件
    public Light targetLight;

    // 閃爍參數
    [Header("Flicker Settings")]
    public float flickerDuration = 5f;      // 總閃爍持續時間
    public float flickerSpeed = 2f;         // 閃爍速度
    public Color startColor = Color.white;  // 起始顏色
    public Color endColor = Color.yellow;   // 結束顏色
    public AudioClip _photoClip;

    [Header("Intensity Settings")]
    public float minIntensity = 0.5f;      // 最小亮度
    public float maxIntensity = 2f;        // 最大亮度

    private float elapsedTime = 0f;
    private bool isFlickering = false;

    private void Awake()
    {
        TryGetComponent<Light>(out targetLight);
    }

    // 開始閃爍的方法
    public void StartFlicker()
    {
        if (!isFlickering)
        {
            SoundManagement.Instance.PlaySoundFXClip(_photoClip, PlayerController.Instance.transform, 1f);
            StartCoroutine(FlickerCoroutine());
        }
    }

    // 停止閃爍的方法
    public void StopFlicker()
    {
        isFlickering = false;
        // 重置光源到初始狀態
        targetLight.color = startColor;
        targetLight.intensity = minIntensity;
    }

    // 閃爍協程
    private IEnumerator FlickerCoroutine()
    {
        isFlickering = true;
        elapsedTime = 0f;

        while (elapsedTime < flickerDuration)
        {
            // 使用正弦波創建平滑的閃爍效果
            float t = Mathf.PingPong(elapsedTime * flickerSpeed, 1f);

            // 插值顏色
            targetLight.color = Color.Lerp(startColor, endColor, t);

            // 插值亮度
            targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 閃爍結束後重置
        StopFlicker();
    }

    // 在Inspector中使用按鈕觸發
    [ContextMenu("Trigger Flicker")]
    public void TriggerFlicker()
    {
        StartFlicker();
    }
}
