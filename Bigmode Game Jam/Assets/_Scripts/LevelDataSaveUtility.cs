using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSaveData", menuName = "Scriptable Objects/LevelSaveData")]
public class LevelSaveData : ScriptableObject
{
    public string sceneName;
    [NonSerialized] public TimeSpan playerTime;
    public Ranking.Rank playerRank;
    [NonSerialized] public TimeSpan playerTimeKilledEnemies;
    public bool completed;
    public bool unlocked;
}

public static class LevelDataSaveUtility
{
    public static string Key(string sceneName, string field)
    {
        return $"Level_{sceneName}_{field}";
    }

    static bool CompareRankedData(LevelSaveData oldData, LevelSaveData newData)
    {
        // level unlocked
        if (!oldData.unlocked && newData.unlocked)
        {
            return true;
        }

        // new completed run
        if (!oldData.completed && newData.completed)
        {
            return true;
        }

        // if we are comparing two valid times
        if (oldData.completed && newData.completed &&
            oldData.playerTime > TimeSpan.Zero &&
            newData.playerTime > TimeSpan.Zero &&
            newData.playerTime < oldData.playerTime)
        {
            // the new time is shorter then the last
            return true;
        }

        return false;
    }

    private static bool CompareEnemyTimeData(LevelSaveData oldData, LevelSaveData newData)
    {
        if (newData.playerTimeKilledEnemies == TimeSpan.Zero)
            return false;

        if (oldData.playerTimeKilledEnemies == TimeSpan.Zero)
            return true;

        return newData.playerTimeKilledEnemies < oldData.playerTimeKilledEnemies;
    }
    // TO-DO:
    // Avoid:
    // - Overwriting fastest level time with fastest killed enemies time
    // - Saving both if faster than both

    public static void SmartSave(LevelSaveData newData)
    {
        // Create a temporary copy to load old data into
        LevelSaveData oldData = ScriptableObject.CreateInstance<LevelSaveData>();

        // Copy static identifiers so keys match
        oldData.sceneName = newData.sceneName;

        // Load previously saved data
        Load(oldData);

        // Compare old vs new
        if (CompareRankedData(oldData, newData))
        {
            SaveRankedData(newData);
        }

        Debug.Log("comparing data");
        if (CompareEnemyTimeData(oldData, newData))
        {
            SaveEnemyTimeData(newData);
        }

        // Cleanup temp object
        ScriptableObject.Destroy(oldData);
    }

    // ---------- SAVE ----------
    public static void SaveRankedData(LevelSaveData level)
    {
        // TimeSpan -> ticks (long split into 2 ints)
        long ticks = level.playerTime.Ticks;
        PlayerPrefs.SetInt(Key(level.sceneName, "Time_Low"), (int)(ticks & 0xFFFFFFFF));
        PlayerPrefs.SetInt(Key(level.sceneName, "Time_High"), (int)(ticks >> 32));

        PlayerPrefs.SetInt(Key(level.sceneName, "Rank"), (int)level.playerRank);
        PlayerPrefs.SetInt(Key(level.sceneName, "Completed"), level.completed ? 1 : 0);
        PlayerPrefs.SetInt(Key(level.sceneName, "Unlocked"), level.unlocked ? 1 : 0);

        PlayerPrefs.Save();
        Debug.Log("Saved");
        //Debug.Log($"Completion: {level.completed}");
        //Debug.Log($"Ticks: {level.playerTime.Ticks}");
    }

    private static void SaveEnemyTimeData(LevelSaveData level)
    {
        long ticks = level.playerTimeKilledEnemies.Ticks;
        PlayerPrefs.SetInt(Key(level.sceneName, "Time_Low_Enemies"), (int)(ticks & 0xFFFFFFFF));
        PlayerPrefs.SetInt(Key(level.sceneName, "Time_High_Enemies"), (int)(ticks >> 32));

        PlayerPrefs.SetInt(Key(level.sceneName, "Completed"), level.completed ? 1 : 0);
        PlayerPrefs.SetInt(Key(level.sceneName, "Unlocked"), level.unlocked ? 1 : 0);

        PlayerPrefs.Save();
    }

    // ---------- LOAD ----------
    public static void Load(LevelSaveData level)
    {
        // Time
        int low = PlayerPrefs.GetInt(Key(level.sceneName, "Time_Low"), 0);
        int high = PlayerPrefs.GetInt(Key(level.sceneName, "Time_High"), 0);
        long ticks = ((long)high << 32) | (uint)low;
        level.playerTime = new TimeSpan(ticks);

        low = PlayerPrefs.GetInt(Key(level.sceneName, "Time_Low_Enemies"));
        high = PlayerPrefs.GetInt(Key(level.sceneName, "Time_High_Enemies"));
        ticks = ((long)high << 32) | (uint)low;
        level.playerTimeKilledEnemies = new TimeSpan(ticks);

        // Rank
        level.playerRank = (Ranking.Rank)
            PlayerPrefs.GetInt(Key(level.sceneName, "Rank"), (int)Ranking.Rank.D);

        level.completed =
            PlayerPrefs.GetInt(Key(level.sceneName, "Completed"), 0) == 1;

        level.unlocked =
            PlayerPrefs.GetInt(Key(level.sceneName, "Unlocked"), 0) == 1;

        if(level.sceneName.Equals("Tutorial"))
        {
            Debug.Log("Tutorial is unlocked");
            level.unlocked = true;
        }

        Debug.Log("Loaded");
        //Debug.Log($"Completion: {level.completed}");
        //Debug.Log($"Ticks: {level.playerTime.Ticks}");
    }

    // ---------- RESET (optional) ----------
    public static void Clear(LevelSaveData level)
    {
        PlayerPrefs.DeleteKey(Key(level.sceneName, "Time_Low"));
        PlayerPrefs.DeleteKey(Key(level.sceneName, "Time_High"));
        PlayerPrefs.DeleteKey(Key(level.sceneName, "Time_Low_Enemies"));
        PlayerPrefs.DeleteKey(Key(level.sceneName, "Time_High_Enemies"));
        PlayerPrefs.DeleteKey(Key(level.sceneName, "Rank"));
        PlayerPrefs.DeleteKey(Key(level.sceneName, "Completed"));
        PlayerPrefs.DeleteKey(Key(level.sceneName, "Unlocked"));
    }
}
