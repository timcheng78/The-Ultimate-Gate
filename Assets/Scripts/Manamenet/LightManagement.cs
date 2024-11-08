using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LightManagement : MonoBehaviour, IDataPersistence
{
    [Header("Roof Light")]
    [SerializeField, ColorUsage(true, true)] private Color _normalColor;
    [SerializeField] private Roof _roof;
    [SerializeField] private Dictionary<string, RoofObject> _roomRoof;

    [Header("Flicker Light")]
    [SerializeField] private bool _flicking = false;
    [SerializeField] private bool _startFlicker = false;
    [SerializeField] private float _flickerDuration = 0.1f;  // 每次閃爍的持續時間
    [SerializeField] private float _delayBetweenFlickers = 2f;  // 閃爍之間的延遲時間
    [SerializeField] private int _minFlickerCount = 2;  // 最少閃爍次數
    [SerializeField] private int _maxFlickerCount = 3;  // 最多閃爍次數
    private List<Light> _needFlickerLights = new();

    public static LightManagement Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one LightManagement in the scene.");
        }
        Instance = this;
        _roomRoof = _roof.ToDictionary();
        if (!Enviroment.Instance.IsElectrified) TurnOffAllLight();
    }

    private void Update()
    {
        if (Enviroment.Instance.Step.Equals(1)) return;
        CheckLightPower();
        CheckFlickerLight();
    }

    private void CheckLightPower()
    {
        foreach (KeyValuePair<string, RoofObject> kvp in _roomRoof)
        {
            RoofObject roof = kvp.Value;
            if (roof.Status) TurnOnLight(kvp.Key);
            else TurnOffLight(kvp.Key);
        }
    }

    private void CheckFlickerLight()
    {
        if (!BookRoomController.Instance.Normal && _roomRoof["book_room"].Status)
        {
            ProcessLightToList(_roomRoof["book_room"].Lights, true);
        }
        else
        {
            ProcessLightToList(_roomRoof["book_room"].Lights, false);
        }
        if (!KitchenController.Instance.Normal)
        {
            if (Enviroment.Instance.Level.Equals(1))
            {
                ProcessLightToList(KitchenController.Instance.Lights, true);
            }
            else if (Enviroment.Instance.Level.Equals(3) && _roomRoof["kitchen"].Status)
            {
                ProcessLightToList(_roomRoof["kitchen"].Lights, true);
            }
            else
            {
                ProcessLightToList(_roomRoof["kitchen"].Lights, false);
            }
        }
        else
        {
            ProcessLightToList(KitchenController.Instance.Lights, false);
            ProcessLightToList(_roomRoof["kitchen"].Lights, false);
        }
        if (!BedRoomController.Instance.Normal && _roomRoof["bed_room"].Status)
        {
            ProcessLightToList(_roomRoof["bed_room"].Lights, true);
        }
        else
        {
            ProcessLightToList(_roomRoof["bed_room"].Lights, false);
        }
        if (!BathRoom1Controller.Instance.Normal && _roomRoof["bath_room_1"].Status)
        {
            ProcessLightToList(_roomRoof["bath_room_1"].Lights, true);
        }
        else
        {
            ProcessLightToList(_roomRoof["bath_room_1"].Lights, false);
        }
        if (!LivingRoomController.Instance.Normal && _roomRoof["living_room"].Status)
        {
            ProcessLightToList(_roomRoof["living_room"].Lights, true);
        }
        else
        {
            ProcessLightToList(_roomRoof["living_room"].Lights, false);
        }
        if (_needFlickerLights.Count > 0) _startFlicker = true;
        if (!_flicking)
        {
            StartCoroutine(FlickerLight());
        }
    }

    private void ProcessLightToList(Light[] lights, bool status)
    {
        foreach (Light light in lights)
        {
            if (!_needFlickerLights.Contains(light) && status) _needFlickerLights.Add(light);
            else if (_needFlickerLights.Contains(light) && !status) _needFlickerLights.Remove(light);
        }
    }

    public bool CheckLightExist(string roomName)
    {
        if (roomName.Equals("")) return true;
        return _roomRoof[roomName].Status;
    }

    public void TurnOnAllLight()
    {
        foreach (KeyValuePair<string, RoofObject> kvp in _roomRoof)
        {
            TurnOnLight(kvp.Key);
        }
    }

    public void TurnOffAllLight()
    {
        foreach (KeyValuePair<string, RoofObject> kvp in _roomRoof)
        {
            TurnOffLight(kvp.Key);
        }
    }

    public void TurnOnLight(string roomName)
    {
        // setting material emission
        Renderer renderer = _roomRoof[roomName].RoofGameObject.GetComponent<Renderer>();
        renderer.materials[^1].EnableKeyword("_EMISSION");
        // turn on light
        foreach (Light light in _roomRoof[roomName].Lights)
        {
            // 二階段
            if (roomName.Equals("bath_room_1") && BathRoom1Controller.Instance && !BathRoom1Controller.Instance.Normal && Enviroment.Instance.Level.Equals(2))
            {
                light.color = _roomRoof[roomName].WeirdColor;
            }
            // 三階段
            else if (roomName.Equals("kitchen") && KitchenController.Instance && !KitchenController.Instance.Normal && Enviroment.Instance.Level.Equals(3))
            {
                light.color = _roomRoof[roomName].WeirdColor;
            }
            // 四階段
            else if (Enviroment.Instance.Level.Equals(4))
            {
                light.color = _normalColor;
                AbstractRoomController controller = Enviroment.Instance.GetControllerByLocation(roomName);
                if (controller && !controller.Normal)
                {
                    light.color = _roomRoof[roomName].WeirdColor;
                }
            }
            else
            {
                light.color = _normalColor;
            }
            light.enabled = true;
        }
        // turn on reflect
        foreach (ReflectionProbe reflectionProbe in _roomRoof[roomName].ReflectionProbes)
        {
            reflectionProbe.enabled = true;
        }
        // change status
        _roomRoof[roomName].Status = true;

        // 一階段
        if (roomName.Equals("kitchen") && Enviroment.Instance.Level.Equals(1))
        {
            KitchenController.Instance.Normal = true;
        }
    }

    public void TurnOffLight(string roomName)
    {
        // setting material emission
        Renderer renderer = _roomRoof[roomName].RoofGameObject.GetComponent<Renderer>();
        renderer.materials[^1].DisableKeyword("_EMISSION");
        // turn off light
        foreach (Light light in _roomRoof[roomName].Lights)
        {
            light.enabled = false;
        }
        // turn off reflect
        foreach (ReflectionProbe reflectionProbe in _roomRoof[roomName].ReflectionProbes)
        {
            reflectionProbe.enabled = false;
        }
        // change status
        _roomRoof[roomName].Status = false;

        // 二階段
        if (roomName.Equals("bath_room_1") && BathRoom1Controller.Instance)
        {
            BathRoom1Controller.Instance.Normal = true;
        }
    }

    private IEnumerator FlickerLight()
    {
        _flicking = true;
        while (_startFlicker)
        {
            // 隨機決定閃爍次數
            int flickerCount = UnityEngine.Random.Range(_minFlickerCount, _maxFlickerCount + 1);

            for (int i = 0; i < flickerCount; i++)
            {
                // 隨機亮度閃爍
                foreach (Light light in _needFlickerLights)
                {
                    light.enabled = false;
                }

                // 保持閃爍狀態一小段時間
                yield return new WaitForSeconds(_flickerDuration);

                // 閃爍結束，恢復到最大亮度
                foreach (Light light in _needFlickerLights)
                {
                    light.enabled = true;
                }

                // 閃爍間的短暫間隔
                yield return new WaitForSeconds(_flickerDuration);
            }

            // 完成一次閃爍後，等待長時間延遲再開始下一次
            yield return new WaitForSeconds(_delayBetweenFlickers);
        }
        _flicking = false;
    }

    void IDataPersistence.LoadData(GameData data)
    {
        if (data.lightsData.Count == 0) return;
        foreach (KeyValuePair<string, RoofObject> kvp in _roomRoof)
        {
            string roomName = kvp.Key;
            RoofObject roof = kvp.Value;
            roof.Status = data.lightsData[roomName];
        }
    }

    void IDataPersistence.SaveData(ref GameData data, bool isCrazyEnd)
    {
        foreach (KeyValuePair<string, RoofObject> kvp in _roomRoof)
        {
            RoofObject roof = kvp.Value;
            if (data.lightsData.ContainsKey(kvp.Key)) data.lightsData[kvp.Key] = roof.Status;
            else data.lightsData.Add(kvp.Key, roof.Status);
        }
    }
}