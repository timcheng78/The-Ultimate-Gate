using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class SharedUtils
{
    public static void SetLayer(Transform targetTransform, int layer)
    {
        if (targetTransform.name == "nonamebook1") return;
        if (targetTransform.GetComponent<SwitchItem>() == null) targetTransform.gameObject.layer = layer; // 設定目標物件的層級

        // 設定子物件的層級
        foreach (Transform child in targetTransform)
        {
            SetLayer(child, layer);
        }
    }

    public static bool StartCheckAnswer(string location, string puzzleName, string otherPuzzleName = null)
    {
        // checking puzzle solve
        bool isSolve = PuzzleManagement.Instance.CheckAnswer(location, puzzleName);
        SubtitleManagement.Instance.AddSentencesToShow("Resolve Puzzle Statement Table", new string[] { location, puzzleName, isSolve ? "t" : "f" });
        if (!isSolve) return isSolve;
        if (location.Equals("demo") && !puzzleName.Equals("box")) return isSolve;
        // isSolve then unlock
        LockObject lockObject = LockManagement.Instance.Unlock(location, string.IsNullOrEmpty(otherPuzzleName) ? puzzleName : otherPuzzleName);
        // animation
        OrganPuzzle organPuzzle = lockObject.LockObj.GetComponent<OrganPuzzle>();
        if (organPuzzle == null) organPuzzle = PuzzleManagement.Instance.GetPuzzleObject(location, puzzleName).GetComponent<OrganPuzzle>();
        if (!puzzleName.Equals("display_door")) organPuzzle.Open();
        return isSolve;
    }

    public static void RandomStringArray(List<string> targetArray)
    {
        for (int i = targetArray.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            string temp = targetArray[i];
            targetArray[i] = targetArray[randomIndex];
            targetArray[randomIndex] = temp;
        }
    }

    public static void GameOver(bool realEnd = false, bool isCrazyEnd = false)
    {
        CameraAttributes cameraAttributes = Camera.main.GetComponent<CameraAttributes>();
        CameraManagement.Instance.TogglePlayerVirtualCamera(false);
        PlayerMovement.Instance.ToggleMove(false);
        PlayerAttributes.Instance._activingItem = null;
        DialogManagement.Instance.ToggleAccurateImage(false);
        DialogManagement.Instance.interactDialog.SetActive(false);
        cameraAttributes._screenLock = true;
        Camera.main.GetComponent<CameraAttributes>().ZeroCameraSpeed();
        Enviroment.Instance.IsStartPlay = false;
        PlayerPhotoSystem.Instance.DeleteAllPhoto();
        if (realEnd && DataPersistenceManagement.Instance)
        {
            Enviroment.Instance.IsEndGameFile = realEnd;
            // 若觸發未完成的召喚必須要解掉原本解開的狀態
            if (Enviroment.Instance.IncompleteSummoning && !isCrazyEnd)
            {
                PuzzleManagement.Instance.SetPuzzleSolve("bath_room_2", "final_puzzle", false);
                LockManagement.Instance.Locked("bath_room_2", "final_puzzle");
                LockManagement.Instance.SetOpened("bath_room_2", "final_puzzle", false);
            }
            DataPersistenceManagement.Instance.SaveEndFile(isCrazyEnd);
            DataPersistenceManagement.Instance.DeleteNormalFile();
        }
    }

    public static void SaveNormalEnd()
    {
        if (DataPersistenceManagement.Instance) DataPersistenceManagement.Instance.SaveNormalEndFile();
    }

    public static void SwitchGameScenes(GameObject demoGame, GameObject mainGame)
    {
        if (PlayerAttributes.Instance._location.Equals("demo") || PlayerAttributes.Instance._location.Equals("none"))
        {
            Enviroment.Instance.Step = 1;
            demoGame.SetActive(true);
            mainGame.SetActive(false);
        }
        else
        {
            Enviroment.Instance.Step = 2;
            demoGame.SetActive(false);
            mainGame.SetActive(true);
        }
    }

    public static IEnumerator WaitingForSec(float sec, System.Action action = null)
    {
        yield return new WaitForSeconds(sec);
        if (action != null) action.Invoke();
    }

}
