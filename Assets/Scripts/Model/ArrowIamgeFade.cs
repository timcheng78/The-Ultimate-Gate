using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowIamgeFade : MonoBehaviour
{
    [SerializeField] private Image[] images; // 需要漸變的圖片
    [SerializeField] private float fadeInDuration = 1f; // 漸變持續時間
    [SerializeField] private float fadeOutDuration = 1f; // 漸變持續時間
    [SerializeField] private int repeatTime = 3;

    void Start()
    {
        StartFade();
    }

    public void StartFade()
    {
        StartCoroutine(FadeImages());
    }

    private IEnumerator FadeImages()
    {
        int count = 0;
        int index = 0;
        while (count < repeatTime)
        {
            Image image = images[index++];
            yield return StartCoroutine(FadeIn(image)); // 漸變進入
            StartCoroutine(FadeOut(image)); // 漸變退出
            if (index.Equals(images.Length)) 
            {
                index = 0;
                count++;
            }
        } 
    }

    private IEnumerator FadeIn(Image image)
    {
        float elapsedTime = 0f;
        Color color = image.color;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeInDuration);
            image.color = color;
            yield return null;
        }

        color.a = 1f; // 確保完全不透明
        image.color = color;
    }

    private IEnumerator FadeOut(Image image)
    {
        float elapsedTime = 0f;
        Color color = image.color;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (elapsedTime / fadeOutDuration));
            image.color = color;
            yield return null;
        }

        color.a = 0f; // 確保完全透明
        image.color = color;
    }
}
