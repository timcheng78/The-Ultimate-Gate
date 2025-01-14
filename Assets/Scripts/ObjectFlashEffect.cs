using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFlashEffect : MonoBehaviour
{
    // ��������
    public Light targetLight;

    // �{�{�Ѽ�
    [Header("Flicker Settings")]
    public float flickerDuration = 5f;      // �`�{�{����ɶ�
    public float flickerSpeed = 2f;         // �{�{�t��
    public Color startColor = Color.white;  // �_�l�C��
    public Color endColor = Color.yellow;   // �����C��
    public AudioClip _photoClip;

    [Header("Intensity Settings")]
    public float minIntensity = 0.5f;      // �̤p�G��
    public float maxIntensity = 2f;        // �̤j�G��

    private float elapsedTime = 0f;
    private bool isFlickering = false;

    private void Awake()
    {
        TryGetComponent<Light>(out targetLight);
    }

    // �}�l�{�{����k
    public void StartFlicker()
    {
        if (!isFlickering)
        {
            SoundManagement.Instance.PlaySoundFXClip(_photoClip, PlayerController.Instance.transform, 1f);
            StartCoroutine(FlickerCoroutine());
        }
    }

    // ����{�{����k
    public void StopFlicker()
    {
        isFlickering = false;
        // ���m�������l���A
        targetLight.color = startColor;
        targetLight.intensity = minIntensity;
    }

    // �{�{��{
    private IEnumerator FlickerCoroutine()
    {
        isFlickering = true;
        elapsedTime = 0f;

        while (elapsedTime < flickerDuration)
        {
            // �ϥΥ����i�Ыإ��ƪ��{�{�ĪG
            float t = Mathf.PingPong(elapsedTime * flickerSpeed, 1f);

            // �����C��
            targetLight.color = Color.Lerp(startColor, endColor, t);

            // ���ȫG��
            targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �{�{�����᭫�m
        StopFlicker();
    }

    // �bInspector���ϥΫ��sĲ�o
    [ContextMenu("Trigger Flicker")]
    public void TriggerFlicker()
    {
        StartFlicker();
    }
}
