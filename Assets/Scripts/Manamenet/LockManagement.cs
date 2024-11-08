using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockManagement : MonoBehaviour, IDataPersistence
{
    [Header("Lock Config")]
    [SerializeField] private Lock _lock;
    [SerializeField] private AudioClip _unlockSoundClip;
    [SerializeField] private AudioClip _lockSoundClip;
    public SerializableDictionary<string, LockObject[]> _lockDictionary;

    public static LockManagement Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one LockManagement in the scene.");
        }
        Instance = this;
        _lockDictionary = _lock.ToDictionary();
    }

    public bool IsLocked(string roomName, string puzzleName)
    {
        LockObject[] lockObjects = _lockDictionary[roomName];

        foreach (LockObject lockObject in lockObjects)
        {
            if (lockObject.PuzzleName.Equals(puzzleName)) return lockObject.IsLocked;
        }

        return true;
    }

    public bool IsOpen(string roomName, string puzzleName)
    {
        LockObject[] lockObjects = _lockDictionary[roomName];

        foreach (LockObject lockObject in lockObjects)
        {
            if (lockObject.PuzzleName.Equals(puzzleName)) return lockObject.IsOpen;
        }

        return false;
    }

    public void Locked(string roomName, string puzzleName)
    {
        LockObject[] lockObjects = _lockDictionary[roomName];

        foreach (LockObject lockObject in lockObjects)
        {
            if (lockObject.PuzzleName.Equals(puzzleName))
            {
                lockObject.IsLocked = true;
                SoundManagement.Instance.PlaySoundFXClip(_lockSoundClip, lockObject.LockObj.transform, 1f);
                return;
            }
        }

    }

    public LockObject Unlock(string roomName, string puzzleName)
    {
        LockObject[] lockObjects = _lockDictionary[roomName];

        foreach(LockObject lockObject in lockObjects)
        {
            if (lockObject.PuzzleName.Equals(puzzleName))
            {
                lockObject.IsLocked = false;
                SoundManagement.Instance.PlaySoundFXClip(_unlockSoundClip, lockObject.LockObj.transform, 1f);
                return lockObject;
            }
        }

        return null;
    }

    public void SetOpened(string roomName, string puzzleName, bool open)
    {
        LockObject[] lockObjects = _lockDictionary[roomName];

        foreach (LockObject lockObject in lockObjects)
        {
            if (lockObject.PuzzleName.Equals(puzzleName))
            {
                lockObject.IsOpen = open;
                return;
            }
        }
    }

    public LockObject GetLockObject(string roomName, string puzzleName)
    {
        LockObject[] lockObjects = _lockDictionary[roomName];

        foreach (LockObject lockObject in lockObjects)
        {
            if (lockObject.PuzzleName.Equals(puzzleName)) return lockObject;
        }

        return null;
    }

    void IDataPersistence.LoadData(GameData data)
    {
        if (data.lockData.locks == null) return;
        data.lockData.SetLocksValue(_lockDictionary);
    }

    void IDataPersistence.SaveData(ref GameData data, bool isCrazyEnd)
    {
        data.lockData.locks = data.lockData.ObjectToLockData(_lockDictionary);
    }
}
