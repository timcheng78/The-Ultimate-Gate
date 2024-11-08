using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractRoomController : MonoBehaviour, IDataPersistence
{
    [SerializeField] protected bool _normal = true;

    public bool Normal { get => _normal; set => _normal = value; }

    public void LoadData(GameData data)
    {
        if (data.roomNormalStatus.ContainsKey(name)) _normal = data.roomNormalStatus[name];
    }

    public void SaveData(ref GameData data, bool isCrazyEnd)
    {
        if (data.roomNormalStatus.ContainsKey(name)) data.roomNormalStatus[name] = _normal;
        else data.roomNormalStatus.Add(name, _normal);
    }
}
