using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DemoBoxController : MonoBehaviour
{
    [SerializeField] private GameObject[] _buttons;
    [SerializeField] private TMP_Text _answerText;

    int _answer;

    public GameObject[] Buttons { get => _buttons; }

    private void Start()
    {
        RandomAnswer();
    }

    private void RandomAnswer()
    {
        _answer = Random.Range(10, 511);
        _answerText.text = _answer.ToString();
        string binaryText = System.Convert.ToString(_answer, 2);
        string answerString = "";
        int index = binaryText.Length;
        while (answerString.Length < 16)
        {
            if (answerString.Length > 0) answerString += " ";
            if (--index >= 0) answerString += binaryText[index];
            else answerString += "0";
        }
        string[] ansArray = answerString.Split(" ");
        PuzzleManagement.Instance.SetPuzzleAnswer("demo", "box", ansArray);
    }
}
