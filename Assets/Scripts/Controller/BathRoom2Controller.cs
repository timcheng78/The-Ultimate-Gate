using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BathRoom2Controller : AbstractRoomController
{
    [Header("Level 4")]
    [SerializeField] private GameObject[] _needShow;
    [SerializeField] private GameObject[] _needHide;

    
    public static BathRoom2Controller Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckBathRoomLevel();
    }

    private void CheckBathRoomLevel()
    {
        switch (Enviroment.Instance.Level)
        {
            case 5:
                foreach (GameObject gameObject in _needShow)
                {
                    if (_normal)
                    {
                        gameObject.SetActive(false);
                    }
                    else
                    {
                        gameObject.SetActive(true);
                    }
                }
                foreach (GameObject gameObject in _needHide)
                {
                    if (_normal)
                    {
                        gameObject.SetActive(true);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                }
                break;
        }
    }
}
