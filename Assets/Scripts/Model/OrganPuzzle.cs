using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OrganPuzzle : MonoBehaviour, IOrganPuzzleObject
{
    [SerializeField] private GameObject[] _organs;
    [SerializeField] private string _puzzleName;
    [SerializeField] private AudioClip _openSoundClip;
    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private GameObject _child;
    [SerializeField] private Animator _badEnd;
    [SerializeField] private GameObject[] _nextLevelShowObject;
    [SerializeField] private GameObject[] _nextLevelHideObject;

    public string PuzzleName { get => _puzzleName; set => _puzzleName = value; }
    public GameObject[] Organs { get => _organs; set => _organs = value; }

    public void Open()
    {
        // 條件: 最終謎題、廁所二必須正常、並非變異第五階段、沒解過成就
        if (_puzzleName.Equals("bath_room_2_final_puzzle") && 
            BathRoom2Controller.Instance.Normal && 
            !Enviroment.Instance.Level.Equals(5))
        {
            // 不完全的招喚
            _badEnd.Play("BadEnd", -1, 0f);
            SteamInitManagement.Instance.SettingAchievement(SteamInitManagement.ACHIEVEMENT_INCOMPLETE_SUMMONING);
            Enviroment.Instance.IncompleteSummoning = true;
            Done();
            return;
        }
        else if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(_puzzleName)) AnimationManagement.Instance.Play(_puzzleName, "open");
        if (_child) _child.transform.parent = transform;
        Done();
        // play sound
        if (_openSoundClip) SoundManagement.Instance.PlaySoundFXClip(_openSoundClip, transform, 1f);
        // play particle
        if (_particle) _particle.Play();
        if (_puzzleName.Equals("bath_room_2_final_puzzle"))
        {
            // 揭露真結局
            LivingRoomController.Instance.ToggleEndGameObjects();
        }
        if (_nextLevelShowObject != null)
        {
            foreach (GameObject gameObject in _nextLevelShowObject)
            {
                gameObject.SetActive(true);
            }
        }
        if (_nextLevelHideObject != null)
        {
            foreach (GameObject gameObject in _nextLevelHideObject)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void Done()
    {
        CloseAllSwitch(transform);
        SharedUtils.SetLayer(transform, 13);
    }

    

    private void CloseAllSwitch(Transform targetTransform)
    {
        SwitchItem switchItem = targetTransform.gameObject.GetComponent<SwitchItem>();
        if (switchItem)
        {
            switchItem.Close();
        }
        // 設定子物件的層級
        foreach (Transform child in targetTransform)
        {
            CloseAllSwitch(child);
        }
    }
}
