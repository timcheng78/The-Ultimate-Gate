using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsuranceBox : MonoBehaviour
{
    private List<GameObject> _numberObjects = new();
    

    public List<GameObject> NumberObjects { get => _numberObjects; set => _numberObjects = value; }
}
