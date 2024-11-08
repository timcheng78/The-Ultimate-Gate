using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField] private GameObject _gameModeMenu;
    [SerializeField] private GameObject _otherWayButton;
    public static StartMenu Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Start Menu in the scene.");
        }
        Instance = this;
    }

    private void Start()
    {
        //if (SteamManager.Initialized)
        //{
        //    string name = SteamFriends.GetPersonaName();
        //    Debug.Log(name);
        //}
        if (DataPersistenceManagement.Instance.HasEndFile())
        {
            _otherWayButton.SetActive(true);
        }
    }

    public void OtherWay()
    {
        DataPersistenceManagement.Instance.LoadEndFile();
    }

    public void ContinueGame()
    {
        DataPersistenceManagement.Instance.Continue();
    }

    public void StartGame()
    {
        // DataPersistenceManagement.Instance.NewGame();
        _gameModeMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void SwitchOptionMenu()
    {
        if (OptionMenu.Instance != null)
        {
            OptionMenu.Instance.Show();
            gameObject.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void NewGame()
    {
        DataPersistenceManagement.Instance.NewGame();
    }

    public void BackStartMenu()
    {
        _gameModeMenu.SetActive(false);
        gameObject.SetActive(true);
    }
}
