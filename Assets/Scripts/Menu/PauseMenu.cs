using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseBlock;
    public GameObject controlBlock;
    public static PauseMenu Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void TogglePauseMenu()
    {
        transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeSelf);
    }

    public void Resume()
    {
        PlayerController.Instance.OnEscape();
    }

    public void ToggleOptionMenu()
    {
        if (OptionMenu.Instance)
        {
            if (transform.GetChild(0).gameObject.activeSelf)
            {
                OptionMenu.Instance.Show();
                TogglePauseMenu();
            }
            else
            {
                OptionMenu.Instance.canvas.SetActive(false);
                TogglePauseMenu();
            }
        }
    }

    public void ToggleControlBlock(bool status)
    {
        controlBlock.SetActive(status);
        pauseBlock.SetActive(!status);
    }

    public void ReturnToMainMenu()
    {
        if (!DataPersistenceManagement.Instance) return;
        DataPersistenceManagement.Instance.SaveGame();
        DataPersistenceManagement.Instance.BackToMainMenu();
        DataPersistenceManagement.Instance.HasData = true;
        Time.timeScale = 1;
    }
}
