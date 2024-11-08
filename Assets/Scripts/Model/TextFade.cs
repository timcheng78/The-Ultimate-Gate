using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextFade : MonoBehaviour
{
    private TMP_Text _text;
    // Start is called before the first frame update
    void Start()
    {
        TryGetComponent<TMP_Text>(out _text);
    }

    // Update is called once per frame
    void Update()
    {
        if (_text.color.a.Equals(1)) return;
        Fade();
    }

    private void Fade()
    {
        Color color = _text.color;
        float a = color.a;
        a += .1f;
        color.a = a;
        _text.color = color;
    }
}
