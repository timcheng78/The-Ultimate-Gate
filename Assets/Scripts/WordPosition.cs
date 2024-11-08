using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordPosition : MonoBehaviour
{
    [SerializeField] private int _left = 1080;
    [SerializeField] private int _top = 1920;

    private RectTransform _rectTransform;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        RandomPosition();
    }

    private void RandomPosition()
    {
        int randomLeft = Random.Range(0, _left);
        int randomTop = Random.Range(0, _top);
        _rectTransform.offsetMin = new(randomLeft, _rectTransform.offsetMin.y);
        _rectTransform.offsetMax = new(_rectTransform.offsetMax.x, -randomTop);
    }
}
