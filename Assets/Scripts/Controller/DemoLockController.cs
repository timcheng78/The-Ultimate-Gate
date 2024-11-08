using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoLockController : MonoBehaviour
{
    [SerializeField] private GameObject[] _colorButtons;
    private string _selectedColor;
    private List<GameObject> _numberObjects = new();
    

    public string SelectColor { get => _selectedColor; set => _selectedColor = value; }
    public List<GameObject> NumberObjects { get => _numberObjects; set => _numberObjects = value; }


    private void Update()
    {
        if (string.IsNullOrEmpty(_selectedColor)) return;
        ApplyColorButton();
    }

    private void ApplyColorButton()
    {
        foreach(GameObject gameObject in _colorButtons)
        {
            PasswordPanelItem item = gameObject.GetComponent<PasswordPanelItem>();
            DemoButton demoButton = gameObject.GetComponent<DemoButton>();
            if (!item._buttonValue.Equals(_selectedColor))
            {
                demoButton.AlreadyPlay = true;
                demoButton.Status = false;
            }
        }
    }

    public bool CheckAllSolve()
    {
        bool result = true;
        foreach (GameObject gameObject in _colorButtons)
        {
            DemoButton demoButton = gameObject.GetComponent<DemoButton>();
            if (!PuzzleManagement.Instance.IsSolvePuzzle("demo", demoButton.btnColor))
            {
                result = false;
                break;
            }
        }
        return result;
    }

    public void Open()
    {
        GetComponent<OrganPuzzle>().Open();
    }
}
