using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour, IDataPersistence
{
    private float elapsedTime = 0.0f; // �ֿn�ɶ�
    public TMP_Text timerText; // �Ψ���ܭp�ɾ��� UI ��r

    public static Timer Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Timer in the scene.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        TryGetComponent<TMP_Text>(out timerText);
    }

    void Update()
    {
        if (Enviroment.Instance && !Enviroment.Instance.IsStartPlay) return;
        elapsedTime += Time.deltaTime;
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        int hours = Mathf.FloorToInt(elapsedTime / 3600);
        int minutes = Mathf.FloorToInt((elapsedTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    public int GetNowSec()
    {
        return Mathf.FloorToInt(elapsedTime);
    }

    public string GetFormatTime()
    {
        return timerText.text;
    }

    void IDataPersistence.LoadData(GameData data)
    {
        elapsedTime = data.timer;
    }

    void IDataPersistence.SaveData(ref GameData data, bool isCrazyEnd)
    {
        data.timer = elapsedTime;
    }
}
