using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPuzzleController : MonoBehaviour
{
    private int[] _sortNumber = {-1, -1, -1};

    private int _indexNumber = 0;
    private GameObject[] _gameObjects;
    public int[] SortNumber { get => _sortNumber; set => _sortNumber = value; }

    private void Awake()
    {
        _gameObjects = GetComponent<OrganPuzzle>().Organs;
    }

    public void SetIndex(int index)
    {
        // setting number
        _sortNumber[index] = _indexNumber;
        if (++_indexNumber < 3) return;
        // all setting
        PuzzleManagement.Instance.PreCheckAnswer(StartCheck);
    }

    private void StartCheck()
    {
        bool isSolve = SharedUtils.StartCheckAnswer("living_room", "display_door");
        if (!isSolve)
        {
            _indexNumber = 0;
            foreach(GameObject gameObject in _gameObjects)
            {
                LightSensorItem lightPuzzle = gameObject.GetComponent<LightSensorItem>();
                lightPuzzle.LightDown();
            }
        }
    }
}
