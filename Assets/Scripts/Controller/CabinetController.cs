using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CabinetController : MonoBehaviour
{
    [SerializeField] private GameObject[] _topicObjects;
    [SerializeField] private GameObject[] _buttonObjects;
    [SerializeField] private TMP_Text[] _topicText;
    [SerializeField] private TMP_Text[] _buttonText;
    [SerializeField] private Light _nextPuzzleSpot;
    [SerializeField] private Renderer _nextPuzzleSpotRenderer;

    private string[] _answer;
    private string[] _startArray = { "1", "2", "3", "4" };
    private string[] _endArray = { "1", "4", "3", "2" };
    private bool _isClose = false;
    private int _index = 0;

    public int Index { get => _index; set => _index = value; }
    public TMP_Text[] ButtonText { get => _buttonText; set => _buttonText = value; }
    public GameObject[] ButtonObjects { get => _buttonObjects; set => _buttonObjects = value; }
    // Start is called before the first frame update
    void Start()
    {
        RandomTopicAndAnswer();
    }

    private void Update()
    {
        if (_isClose) return;
        DoCheckPass();
    }

    private void RandomTopicAndAnswer()
    {
        ShuffleArray(_startArray, _endArray);
        for (int i = 0; i < 4; ++i)
        {
            _topicText[i].SetText(_startArray[i]);
        }
        PuzzleManagement.Instance.SetPuzzleAnswer("bath_room_1", "cabinet", _endArray);
    }

    private void ShuffleArray(string[] startArray, string[] endArray)
    {
        for (int i = startArray.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1); 
            string temp = startArray[i];
            startArray[i] = startArray[randomIndex];
            switch(i)
            {
                case 1:
                    endArray[3] = startArray[i];
                    break;
                case 2:
                    endArray[2] = startArray[i];
                    break;
                case 3:
                    endArray[1] = startArray[i];
                    break;
            }
            startArray[randomIndex] = temp;
        }
        endArray[0] = startArray[0];
    }

    public void NextPuzzle()
    {
        _nextPuzzleSpot.enabled = true;
        _nextPuzzleSpotRenderer.materials[^1].EnableKeyword("_EMISSION");
        //LightManagement.Instance.TurnOffLight("bath_room_1");
        //if (PlayerAttributes.Instance._isTriggerFlashlight)
        //{
        //    PlayerController.Instance.OnFlashLight();
        //}
        // 改為變異階段二
        if (Enviroment.Instance.Level < 2) Enviroment.Instance.Level = 2;
        KitchenController.Instance.Normal = true;
        BathRoom1Controller.Instance.Normal = false;
        LightManagement.Instance.TurnOffLight("bath_room_1");
    }

    private void DoCheckPass()
    {
        if (PuzzleManagement.Instance.IsSolvePuzzle("bath_room_1", "cabinet"))
        {
            // is solve, close all button and check next level
            if (!_nextPuzzleSpot.enabled)
            {
                NextPuzzle();
            }
            foreach (GameObject gameObject in _buttonObjects)
            {
                gameObject.GetComponent<ShinningButton>().CloseButton();
            }
            _isClose = true;
        }
    }
}
