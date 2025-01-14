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
    [SerializeField] private Renderer _spotRenderer;
    [SerializeField] private Color[] _colors;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private float _transitionDuration = 2.0f;
    private int currentColorIndex = 0;
    private float transitionProgress = 0;
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
        if (DataPersistenceManagement.Instance.IsNormalEnd())
        {
            StartCoroutine(StartTransferColor(_spotRenderer.materials));
            _particleSystem.Play();
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

    IEnumerator StartTransferColor(Material[] materials)
    {
        while (true)
        {
            // 計算當前顏色和下一個顏色之間的插值
            Color nextColor = _colors[(currentColorIndex + 1) % _colors.Length];
            materials[^1].SetColor("_Color", Color.Lerp(_colors[currentColorIndex], nextColor, transitionProgress));

            // 更新轉換進度
            transitionProgress += Time.deltaTime * _transitionDuration;

            // 如果進度達到1，切換到下一個顏色
            if (transitionProgress >= 1.0f)
            {
                transitionProgress = 0;
                currentColorIndex = (currentColorIndex + 1) % _colors.Length;
            }
            yield return null;
        }
    }
}
