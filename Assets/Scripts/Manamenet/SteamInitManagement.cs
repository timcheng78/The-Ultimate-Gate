using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamInitManagement : MonoBehaviour
{
    // 成就區
    public static readonly string ACHIEVEMENT_ALMOST = "ALMOST";
    public static readonly string ACHIEVEMENT_BEST_POSITION = "BEST_POSITION";
    public static readonly string ACHIEVEMENT_FORESHADOWING = "FORESHADOWING";
    public static readonly string ACHIEVEMENT_HIDE_AND_SEEK = "HIDE_AND_SEEK";
    public static readonly string ACHIEVEMENT_INCOMPLETE_SUMMONING = "INCOMPLETE_SUMMONING";
    public static readonly string ACHIEVEMENT_LIGHT_WITHOUT_BRILLIANCE = "LIGHT_WITHOUT_BRILLIANCE";
    public static readonly string ACHIEVEMENT_OTHER_ROOM = "OTHER_ROOM";
    public static readonly string ACHIEVEMENT_SCENT_OF_DECAY = "SCENT_OF_DECAY";
    public static readonly string ACHIEVEMENT_TRUTHLY_ANSWER = "TRUTHLY_ANSWER";
    public static readonly string ACHIEVEMENT_NORMAL_END = "NORMAL_END";
    public static readonly string ACHIEVEMENT_TRUE_ENDING = "TRUE_ENDING";
    public static readonly string ACHIEVEMENT_CRAZY_ENDING = "CRAZY_ENDING";
    //統計區
    public static readonly string STAT_DEMO_BOX_COUNT_SEC = "DEMO_BOX_COUNT_SEC";
    public static readonly string STAT_DEMO_RED_COUNT_SEC = "DEMO_RED_COUNT_SEC";
    public static readonly string STAT_DEMO_BLUE_COUNT_SEC = "DEMO_BLUE_COUNT_SEC";
    public static readonly string STAT_DEMO_GREEN_COUNT_SEC = "DEMO_GREEN_COUNT_SEC";
    public static readonly string STAT_BOOK_ROOM_COUNT_SEC = "BOOK_ROOM_COUNT_SEC";
    public static readonly string STAT_BOOK_ROOM_SHIEF_COUNT_SEC = "BOOK_ROOM_SHIEF_COUNT_SEC";
    public static readonly string STAT_KITCHEN_ELECTRICAL_BOX_COUNT_SEC = "KITCHEN_ELECTRICAL_BOX_COUNT_SEC";
    public static readonly string STAT_KITCHEN_INDUCTION_COUNT_SEC = "KITCHEN_INDUCTION_COUNT_SEC";
    public static readonly string STAT_BED_ROOM_AIR_PUZZLE_COUNT_SEC = "BED_ROOM_AIR_PUZZLE_COUNT_SEC";
    public static readonly string STAT_BED_ROOM_INSURANCE_COUNT_SEC = "BED_ROOM_INSURANCE_COUNT_SEC";
    public static readonly string STAT_RED_KEY_COLLECT_COUNT_SEC = "RED_KEY_COLLECT_COUNT_SEC";
    public static readonly string STAT_BATH_ROOM_1_CABINET_COUNT_SEC = "BATH_ROOM_1_CABINET_COUNT_SEC";
    public static readonly string STAT_BATH_ROOM_1_PUZZLE_COUNT_SEC = "BATH_ROOM_1_PUZZLE_COUNT_SEC";
    public static readonly string STAT_BATH_ROOM_1_HOLE_INTERACT_COUNT_SEC = "BATH_ROOM_1_HOLE_INTERACT_COUNT_SEC";
    public static readonly string STAT_BLUE_KEY_COLLECT_COUNT_SEC = "BLUE_KEY_COLLECT_COUNT_SEC";
    public static readonly string STAT_KITCHEN_BUTTON_COUNT_SEC = "KITCHEN_BUTTON_COUNT_SEC";
    public static readonly string STAT_LIVING_ROOM_DISPLAY_DOOR_COUNT_SEC = "LIVING_ROOM_DISPLAY_DOOR_COUNT_SEC";
    public static readonly string STAT_LIVING_ROOM_TO_BATH_ROOM_2_COUNT_SEC = "LIVING_ROOM_TO_BATH_ROOM_2_COUNT_SEC";
    public static readonly string STAT_GREEN_KEY_COLLECT_COUNT_SEC = "GREEN_KEY_COLLECT_COUNT_SEC";
    public static readonly string STAT_NORMAL_END_COUNT_SEC = "NORMAL_END_COUNT_SEC";
    public static readonly string STAT_TRIGGER_BOOK_ROOM_LOCK_COUNT_SEC = "TRIGGER_BOOK_ROOM_LOCK_COUNT_SEC";
    public static readonly string STAT_SILVER_KEY_COLLECT_COUNT_SEC = "SILVER_KEY_COLLECT_COUNT_SEC";
    public static readonly string STAT_BATH_ROOM_2_FINAL_PUZZLE_COUNT_SEC = "BATH_ROOM_2_FINAL_PUZZLE_COUNT_SEC";
    public static readonly string STAT_CRAZY_END_COUNT_SEC = "CRAZY_END_COUNT_SEC";
    public static readonly string STAT_TRUTH_END_COUNT_SEC = "TRUTH_END_COUNT_SEC";

    [SerializeField] private bool ResetStatsOnGameStart = false;

    [SerializeField] private bool AlsoResetAchievements = false;

    public static SteamInitManagement Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one SteamInitManagement in the scene.");
        }
        Instance = this;
    }

    private void Start()
    {
        if (!SteamManager.Initialized) return;
        if (ResetStatsOnGameStart) Steamworks.SteamUserStats.ResetAllStats(AlsoResetAchievements);
    }

    public void SettingAchievement(string name)
    {
        if (!SteamManager.Initialized) return;

        Steamworks.SteamUserStats.GetAchievement(name, out bool achievementCompleted);

        if (achievementCompleted) return;

        SteamUserStats.SetAchievement(name);
        SteamUserStats.StoreStats();
    }

    public void UpdateStat(string name, int sec)
    {
        if (!SteamManager.Initialized) return;

        SteamUserStats.SetStat(name, sec);
        SteamUserStats.StoreStats();
    }

    public bool GetAchievementStatus(string name)
    {
        if (!SteamManager.Initialized) return false;

        Steamworks.SteamUserStats.GetAchievement(name, out bool achievementCompleted);

        return achievementCompleted;
    }
}
