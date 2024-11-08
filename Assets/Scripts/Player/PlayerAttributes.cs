using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour, IDataPersistence
{
    [Header("Player Configuration")]
    public float _moveSpeed = 5f;
    public float _turnSpeed = 10f;
    public float _gravity = -20f;
    public SerializableDictionary<int, bool> _keys = new();
    public bool _hasFlashlight = false;
    public bool _hasNotebook = false;
    public bool _hasBinaryPaper = false;
    public bool _isTriggerFlashlight = false;
    public string _location;
    public GameObject _activingItem;
    public Vector3 _height;
    [SerializeField] private AudioSource _soundFXObject;
    [SerializeField] private AudioClip[] _bgmClips;
    private AudioSource _lastSource;

    public static PlayerAttributes Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        _keys.Add(1, false);
        _keys.Add(2, false);
        _keys.Add(3, false);
        _keys.Add(4, false);
    }
    private void Update()
    {
        if (_lastSource && _lastSource.isPlaying) return;
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume)
    {
        // assign a random index
        int rand = Random.Range(0, audioClip.Length);
        // spawn in gameObject
        AudioSource audioSource = Instantiate(_soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign the audioClip
        audioSource.clip = audioClip[rand];

        // assign volume
        audioSource.volume = volume;

        // play sound
        audioSource.Play();
        _lastSource = audioSource;

        // get length of sound FX clips
        float clipLength = audioSource.clip.length;

        // destroy the clip after it is done playing
        Destroy(audioSource.gameObject, clipLength);
    }

    public void SetHeight()
    {
        if (_height == Vector3.zero) return;
        transform.localScale = _height;
    }

    void IDataPersistence.LoadData(GameData data)
    {
        _hasFlashlight = data.playerData.hasFlashlight;
        _hasBinaryPaper = data.playerData.hasBinaryPaper;
        _hasNotebook = data.playerData.hasNotebook;
        _location = data.playerData.location;
        _keys = data.playerData.keys;
        transform.parent.position = data.playerData.position;
        transform.localPosition = Vector3.zero;
        transform.localScale = data.playerData.scale;
        _height = data.playerData.scale;
        SetHeight();
    }

    void IDataPersistence.SaveData(ref GameData data, bool isCrazyEnd)
    {
        data.playerData.hasFlashlight = _hasFlashlight;
        data.playerData.hasBinaryPaper = _hasBinaryPaper;
        data.playerData.hasNotebook = _hasNotebook;
        data.playerData.keys = (SerializableDictionary<int, bool>) _keys;
        data.playerData.position = transform.position;
        data.playerData.location = _location;
        if (isCrazyEnd) data.playerData.position = new Vector3(-8.26000023f, 1.07000019f, 0.479999989f);
        data.playerData.scale = _height;
    }
}
