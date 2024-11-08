using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InductionController : MonoBehaviour, IDataPersistence
{
    [SerializeField] private bool _power = false;
    [SerializeField] private GameObject[] _inductionPowers;
    [SerializeField] private GameObject[] _inductionFires;
    [SerializeField] private OrganPuzzle _cooker;
    [SerializeField] private string _id;

    [ContextMenu("Generate guid for id")]
    private void GenerateGuid()
    {
        _id = System.Guid.NewGuid().ToString();
    }

    private Dictionary<int, List<GameObject>> _fireMap = new();
    private int _selectedNumber = -1;

    public int SelectedNumber { get => _selectedNumber; set => _selectedNumber = value; }

    private void Awake()
    {
        SetFireMaps();
        CloseAllFire();
    }

    private void Update()
    {
        //CheckStatus();
        if (CheckPower()) ApplyInductionFires(_selectedNumber);
    }

    public bool CheckPower()
    {
        return _power;
    }

    public void TurnOnPower()
    {
        _power = true;
    }

    public void AddFire()
    {
        if (_selectedNumber.Equals(11)) return;
        _selectedNumber++;
    }

    public void SubFire()
    {
        if (_selectedNumber.Equals(-1)) return;
        _selectedNumber--;
    }

    private void CheckStatus()
    {
        for (int i = 0, len = _inductionPowers.Length; i < len; ++i)
        {
            PanelItem item = _inductionPowers[i].GetComponent<PanelItem>();
            if (item.TriggerPoint)
            {
                SetStatus(i);
                ApplyInductionFires(i);
                item.TriggerPoint = false;
            }
        }
    }

    private void ApplyInductionFires(int index)
    {
        CloseAllFire();
        if (index.Equals(-1)) return;
        foreach (GameObject organ in _fireMap[index])
        {
            Renderer rd = organ.GetComponent<Renderer>();
            Material[] mat = rd.materials;
            mat[^1].EnableKeyword("_EMISSION");
        }
    }

    private void CloseAllFire()
    {
        for (int i = 0, len = _inductionFires.Length; i < len; ++i)
        {
            Renderer rd = _inductionFires[i].GetComponent<Renderer>();
            Material[] mat = rd.materials;
            mat[^1].DisableKeyword("_EMISSION");
        }
    }

    private void SetFireMaps()
    {
        // left setting
        List<GameObject> gameObjects = new();
        for (int i = 0, len = 12; i < len; ++i)
        {
            switch (i)
            {
                case 0:
                    gameObjects.Add(_inductionFires[9]);
                    break;
                case 1:
                    gameObjects.Add(_inductionFires[10]);
                    break;
                case 2:
                    gameObjects.Add(_inductionFires[8]);
                    break;
                case 3:
                    gameObjects.Add(_inductionFires[11]);
                    break;
                case 4:
                    gameObjects.Add(_inductionFires[7]);
                    break;
                case 5:
                    gameObjects.Add(_inductionFires[0]);
                    break;
                case 6:
                    gameObjects.Add(_inductionFires[6]);
                    break;
                case 7:
                    gameObjects.Add(_inductionFires[1]);
                    break;
                case 8:
                    gameObjects.Add(_inductionFires[5]);
                    break;
                case 9:
                    gameObjects.Add(_inductionFires[2]);
                    break;
                case 10:
                    gameObjects.Add(_inductionFires[4]);
                    break;
                case 11:
                    gameObjects.Add(_inductionFires[3]);
                    break;
            }
            _fireMap.Add(i, new List<GameObject>(gameObjects));
        }
    }

    private void SetStatus(int index)
    {
        for (int i = 0; i <= index; ++i)
        {
            PanelItem item = _inductionPowers[i].GetComponent<PanelItem>();
            item.Status = true;
        }
        if (index.Equals(_inductionPowers.Length)) return;
        for (int i = index + 1; i < _inductionPowers.Length; ++i)
        {
            PanelItem item = _inductionPowers[i].GetComponent<PanelItem>();
            item.Status = false;
        }
    }

    void IDataPersistence.LoadData(GameData data)
    {
        if (string.IsNullOrEmpty(_id)) return;
        InductionData inductionData = new();
        data.inductionData.TryGetValue(_id, out inductionData);
        if (inductionData == null) return;
        _power = inductionData.power;
        _selectedNumber = inductionData.selectNumber;
        if (_selectedNumber > 0)
        {
            //SetStatus(_selectedNumber - 1);
            ApplyInductionFires(_selectedNumber - 1);
        }
    }

    void IDataPersistence.SaveData(ref GameData data, bool isCrazyEnd)
    {
        if (string.IsNullOrEmpty(_id)) return;
        if (data.inductionData.ContainsKey(_id))
        {
            data.inductionData.Remove(_id);
        }
        data.inductionData.Add(_id, new (_power, _selectedNumber));
    }
}
