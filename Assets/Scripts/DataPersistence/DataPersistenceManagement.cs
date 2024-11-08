using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Localization.Tables;

public class DataPersistenceManagement : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private bool useVersionName;
    [SerializeField] private string fileName;
    [SerializeField] private string endFileName;
    [SerializeField] private bool useEncryption;

    [Header("Menu")]
    [SerializeField] private GameObject _progressCanvas;
    [SerializeField] private Slider _progress;
    [SerializeField] private TMP_Text _progressText;
    [SerializeField] private TMP_Text _processingText;
    [SerializeField] private ImageSwitch _imageSwitch;
    [SerializeField] private GameObject[] _menuOption;
    [SerializeField] private AudioSource _menuBGM;

    private GameData gameData;
    private PlayerSettingData playerSettingData;
    private List<IDataPersistence> dataPersistencesObjects;
    private FileDataHandler dataHandler;
    private bool hasData = true;
    private bool isNewGame = true;
    private Dictionary<string, StringTable> stringTables = new();

    public static DataPersistenceManagement Instance { get; private set; }
    public bool HasData { get => hasData; set => hasData = value; }
    public bool IsNewGame { get => isNewGame; set => isNewGame = value; }

    public GameData GameDatas { get => gameData; }

    public Dictionary<string, StringTable> StringTables { get => stringTables; set => stringTables = value; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.LogError("Found more than one Data Persistence Management in the scene.");
            Destroy(gameObject);
        }
        if (useVersionName) fileName = Application.version;
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
        LoadGame();
    }

    private void Start()
    {
        this.dataPersistencesObjects = FindAllDataPersistenceObjects();
    }

    private void StartGame()
    {
        if (MainGame.Instance != null) MainGame.Instance.StartGame();
    }

    private void PreloadGame()
    {
        if (MainGame.Instance != null) MainGame.Instance.PreloadGameSetting();
    }

    public bool HasEndFile()
    {
        if (FileDataHandler.CheckFileExist(Application.persistentDataPath, endFileName))
        {
            return true;
        }
        return false;
    }

    public void LoadEndFile()
    {
        if (FileDataHandler.CheckFileExist(Application.persistentDataPath, endFileName))
        {
            this.dataHandler = new FileDataHandler(Application.persistentDataPath, endFileName, useEncryption);
            LoadGame();
            Continue();
        }
    }

    public void SaveEndFile(bool isCrazyEnd = false)
    {
        FileDataHandler endGameHandler = new FileDataHandler(Application.persistentDataPath, endFileName, useEncryption);
        SaveGame(endGameHandler, isCrazyEnd);
    }

    public void NewGame()
    {
        isNewGame = true;
        StartCoroutine(LoadScene(2));
    }

    public void Continue()
    {
        isNewGame = false;
        StartCoroutine(LoadScene(2));
    }

    public void BackToMainMenu()
    {
        StartCoroutine(LoadScene(1));
    }

    public void ReloadDataPersistenceObjects()
    {
        this.dataPersistencesObjects = FindAllDataPersistenceObjects();
    }

    public void LoadGame()
    {
        ReloadDataPersistenceObjects();
        // load any saved data from a file using the data handler
        this.gameData = dataHandler.Load();

        // if no data can be loaded, initalize to a new game
        if (this.gameData == null)
        {
            Debug.Log("No data was found. Initalizing data to defaults.");
            this.gameData = new GameData();
            hasData = false;
            return;
        }
        else
        {
            if (gameData.playerData.position.Equals(Vector3.zero)) hasData = false;
            else hasData = true;
        }
        // push the loaded data to all other scripts that need it
        foreach (IDataPersistence dataPersistence in dataPersistencesObjects)
        {
            dataPersistence.LoadData(gameData);
        }
    }

    public void LoadPlayerSetting()
    {
        this.playerSettingData = dataHandler.LoadPlayerSetting();
        // if no data can be loaded, initalize to a new game
        if (this.playerSettingData == null)
        {
            this.playerSettingData = new PlayerSettingData();
            return;
        }
        OptionMenu.Instance.LoadPlayerSetting(this.playerSettingData);
    }

    public void ResetLoadFile()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
    }

    public void SaveGame(FileDataHandler dataHandler = null, bool isCrazyEnd = false)
    {
        this.dataPersistencesObjects = FindAllDataPersistenceObjects();

        // pass the data to other scripts so they can update it
        foreach (IDataPersistence dataPersistence in dataPersistencesObjects)
        {
            dataPersistence.SaveData(ref gameData, isCrazyEnd);
        }

        // save that data to a file using the data handler
        if (dataHandler == null) this.dataHandler.Save(gameData);
        else dataHandler.Save(gameData);
    }

    public void SavePlayerSetting()
    {
        OptionMenu.Instance.SavePlayerSetting(ref playerSettingData);
        this.dataHandler.SavePlayerSetting(playerSettingData);
    }

    public void DeleteGame()
    {
        this.dataHandler.Delete();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistencesObjects = FindObjectsOfType<MonoBehaviour>().OrderBy(m => m.tag).OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistencesObjects);
    }

    IEnumerator LoadScene(int sceneIndex)
    {
        _imageSwitch.toggleSwitch = true;
        _processingText.text = "";
        int disableProgress = 0;
        int toProgress = 0;

        _progressCanvas.SetActive(true);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex);
        op.allowSceneActivation = false;
        
        //_processingText.text += "Start scene loading...\n";

        while (!op.isDone)
        {
            toProgress = (int)(op.progress * 100);

            while (disableProgress < toProgress)
            {
                ++disableProgress;
                _progress.value = disableProgress / 100.0f;
                _progressText.text = disableProgress.ToString() + "%";
                yield return new WaitForEndOfFrame();
            }

            // Check if the load has finished
            if (op.progress >= 0.9f)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }
        //_processingText.text += "End scene loading...\n";
        //while (op.progress < 0.9f)
        //{
        //    toProgress = (int)(op.progress * 100);
        //    while (disableProgress < toProgress)
        //    {
        //        ++disableProgress;

        //        _progress.value = disableProgress / 100.0f;
        //        _progressText.text = disableProgress.ToString() + "%";
        //        yield return new WaitForEndOfFrame();
        //    }
        //}

        if (sceneIndex.Equals(2)) _menuBGM.Stop();
        if (OptionMenu.Instance != null) OptionMenu.Instance.SetSoundZero();

        //op.allowSceneActivation = true;
        //toProgress = 80;
        //while (!op.isDone)
        //{
        //    _processingText.text += op.isDone;
        //    while (disableProgress < toProgress)
        //    {
        //        ++disableProgress;

        //        _progress.value = disableProgress / 100.0f;
        //        _progressText.text = disableProgress.ToString() + "%";
        //        yield return new WaitForEndOfFrame();
        //    }
        //}
        

        // Main Game ready
        if (sceneIndex.Equals(2))
        {
            //_processingText.text += "Start data processing...\n";
            PreloadGame();
            yield return new WaitForSeconds(5);
            //_processingText.text += "End data processing...\n";
        }
        

        //_processingText.text += "Ready to start...\n";
        toProgress = 100;
        while (disableProgress < toProgress)
        {
            ++disableProgress;

            _progress.value = disableProgress / 100.0f;
            _progressText.text = disableProgress.ToString() + "%";

            yield return new WaitForEndOfFrame();
        }
        if (OptionMenu.Instance != null) OptionMenu.Instance.RevertSound();
        _progressCanvas.SetActive(false);
        _imageSwitch.toggleSwitch = false;
        if (sceneIndex.Equals(1)) _menuBGM.Play();
        if (sceneIndex.Equals(2)) StartGame();
        
    }
}
