using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDropItem : MonoBehaviour, IInteractive, IUnlockScreenObject, IDataPersistence
{
    [SerializeField] public string _location;
    [SerializeField] public string _itemColor;
    [SerializeField] public string _puzzleName;
    [SerializeField] private bool _canDrag = false;
    [SerializeField] private bool _canDrop = false;
    [SerializeField] private AudioClip _dragSoundClip;
    [SerializeField] private AudioClip _errorSoundClip;
    [SerializeField] private string _id;

    [ContextMenu("Generate guid for id")]
    private void GenerateGuid()
    {
        _id = System.Guid.NewGuid().ToString();
    }

    private Transform _parentTransform;
    private Vector3 _originPosition;
    private Quaternion _originQuaternion;
    private Renderer _renderer;
    private Material[] _materials;
    private bool _dragging = false;
    public DragDropItem _book;
    public DragDropItem _container;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materials = _renderer.materials;
        _parentTransform = transform.parent;
        _originPosition = transform.localPosition;
        _originQuaternion = transform.rotation;
    }

    public void HoverIn()
    {
        if (!LightManagement.Instance.CheckLightExist(_location)) return;
        HoverInItem();
    }

    public void HoverOut()
    {
        HoverOutItem();
    }

    public void Interact()
    {
        if (!LightManagement.Instance.CheckLightExist(_location)) return;
        if (CanDrag())
        {
            // Drag Item
            if (PlayerAttributes.Instance._activingItem) return;
            Drag();
        }
        else if (CanDrop())
        {
            // Drop Item
            if (!PlayerAttributes.Instance._activingItem) return;
            Drop();
        }
        else
        {
            // Others
        }
    }

    public void Cancel()
    {
        if (CanDrag())
        {
            // setting book
            _dragging = false;
            if (_container)
            {
                _container._book = null;
                _container = null;
            }
            // setting camera
            CameraManagement.Instance.objectCamera.gameObject.SetActive(false);
            PlayerAttributes.Instance._activingItem = null;
            // play sound
            SoundManagement.Instance.PlaySoundFXClip(_dragSoundClip, transform, 1f);
            // hide hint
            DialogManagement.Instance.cancelDialog.SetActive(false);
        }
    }

    public bool CanDrag()
    {
        return _canDrag;
    }

    public bool CanDrop()
    {
        return _canDrop;
    }

    private void HoverInItem()
    {
        if (CanDrag())
        {
            // Drag Item
            if (PlayerAttributes.Instance._activingItem) return;
            DialogManagement.Instance.interactDialog.SetActive(true);
            _materials[^1].SetFloat("_IsActive", 1);
        }
        else if (CanDrop())
        {
            // Drop Item
            // if dragItem
            if (!PlayerAttributes.Instance._activingItem) return;
            DialogManagement.Instance.interactDialog.SetActive(true);
            _renderer.enabled = true;
        }
        else
        {
            // Others
        }
    }

    private void HoverOutItem()
    {
        DialogManagement.Instance.interactDialog.SetActive(false);
        if (CanDrag())
        {
            // Drag Item
            _materials[^1].SetFloat("_IsActive", 0);
        }
        else if (CanDrop())
        {
            // Drop Item
            _renderer.enabled = false;
        }
        else
        {
            // Others
        }
    }

    private void Drag()
    {
        // status change
        _dragging = true;
        // setting object rotation
        transform.localRotation = Quaternion.Euler(new (0, 0, 180));
        // setting camera
        CameraManagement.Instance.objectCamera.gameObject.SetActive(true);
        PlayerAttributes.Instance._activingItem = gameObject;
        // setting container if had
        if (_container) _container._book = null;
        // play sound
        SoundManagement.Instance.PlaySoundFXClip(_dragSoundClip, transform, 1f);
        // hide hint
        DialogManagement.Instance.cancelDialog.SetActive(true);
        // Add Drag Sentences
        SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, _itemColor, "t" });
    }

    private void Drop()
    {
        // setting book
        GameObject dragBook = PlayerAttributes.Instance._activingItem;
        _book = dragBook.GetComponent<DragDropItem>();
        _book._dragging = false;
        _book._container = gameObject.GetComponent<DragDropItem>();
        // setting camera
        CameraManagement.Instance.objectCamera.gameObject.SetActive(false);
        PlayerAttributes.Instance._activingItem = null;
        // play sound
        SoundManagement.Instance.PlaySoundFXClip(_dragSoundClip, transform, 1f);
        // puzzle pre checking
        GameObject puzzle = PuzzleManagement.Instance.GetPuzzleObject(_location, _puzzleName);
        OrganPuzzle organ = puzzle.GetComponent<OrganPuzzle>();
        int count = 0;
        foreach (GameObject obj in organ.Organs) {
            DragDropItem item = obj.GetComponent<DragDropItem>();
            if (item._book != null) count++;
        }
        if (count.Equals(4)) PuzzleManagement.Instance.PreCheckAnswer(StartCheck);
        // hide hint
        DialogManagement.Instance.cancelDialog.SetActive(false);
        // Add Drag Sentences
        SubtitleManagement.Instance.AddSentencesToShow("Interact Statement Table", new string[] { _location, "book_drop", "t" });
    }

    private void Update()
    {
        if (CanDrop()) return;

        if (_dragging)
        {
            ApplyBookDrag();
        } 
        else
        {
            if (_container)
            {
                transform.parent = PuzzleManagement.Instance.GetPuzzleObject(_location, _puzzleName).transform;
                transform.position = _container.transform.position;
                transform.localRotation = Quaternion.Euler(new(0, 0, 0));
            }
            else
            {
                transform.parent = _parentTransform;
                transform.localPosition = _originPosition;
                transform.rotation = _originQuaternion;
            }
        }
        
    }

    private void ApplyBookDrag()
    {
        // get window size
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // object in screen middle
        Vector3 centerPosition = new(screenWidth / 1.2f, screenHeight / 3.2f, .9f);
        transform.position = CameraManagement.Instance.objectCamera.ScreenToWorldPoint(centerPosition);
    }

    private void StartCheck()
    {
        bool isSolve = SharedUtils.StartCheckAnswer(_location, _puzzleName);
        if (!isSolve) SoundManagement.Instance.PlaySoundFXClip(_errorSoundClip, transform, 1f);
    }

    void IDataPersistence.LoadData(GameData data)
    {
        string targetId = "";
        data.books.TryGetValue(_id, out targetId);
        if (string.IsNullOrEmpty(targetId)) return;
        if (CanDrag())
        {
            OrganPuzzle organ = PuzzleManagement.Instance.GetPuzzleObject(_location, _puzzleName).GetComponent<OrganPuzzle>();
            foreach (GameObject obj in organ.Organs)
            {
                DragDropItem dropItem = obj.GetComponent<DragDropItem>();
                if (dropItem._id.Equals(targetId))
                {
                    _container = dropItem;
                    dropItem._book = this;
                    break;
                }
            }
        }
    }

    void IDataPersistence.SaveData(ref GameData data, bool isCrazyEnd)
    {
        if (data.books.ContainsKey(_id))
        {
            data.books.Remove(_id);
        }
        if (CanDrag())
        {
            if (_container)
            {
                DragDropItem contianerItem = _container.GetComponent<DragDropItem>();
                data.books.Add(_id, contianerItem._id);
            }
            else
            {
                data.books.Add(_id, "");
            }
        }
    }
}
