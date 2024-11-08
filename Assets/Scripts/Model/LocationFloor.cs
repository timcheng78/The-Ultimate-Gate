using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationFloor : MonoBehaviour
{
    [SerializeField] private string _location;
    private void OnTriggerEnter(Collider other)
    {
        PlayerAttributes.Instance._location = _location;
        SubtitleManagement.Instance.Count = 0;
    }

}
