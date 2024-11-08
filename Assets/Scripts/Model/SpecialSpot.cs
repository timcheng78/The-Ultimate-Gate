using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialSpot : MonoBehaviour
{
    private Light _light;
    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!LightManagement.Instance.CheckLightExist("bath_room_1") && !PlayerAttributes.Instance._isTriggerFlashlight) _light.intensity = 10;
        else _light.intensity = 0.1f;
    }
}
