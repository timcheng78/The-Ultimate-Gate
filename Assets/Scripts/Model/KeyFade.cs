using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyFade : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f; // ���ܫ���ɶ�
    private Image image;
    void Start()
    {
        TryGetComponent<Image>(out image);
        //StartFade();
    }

    public void StartFade()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = image.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            image.color = color;
            yield return null;
        }

        color.a = 0f; // �T�O�����z��
        image.color = color;
    }
}
