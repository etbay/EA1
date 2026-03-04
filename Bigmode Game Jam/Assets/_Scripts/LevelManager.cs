using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public static bool gameRunning = true;
    public static bool gameEnded = false;
    [SerializeField] public LevelData data;
    private int numEnemies;
    private int numKills;
    private float topSpeed;
    Ranking.Rank rank = Ranking.Rank.S;
    float scaleBeforePause = 1f;
    [SerializeField] private AudioClip levelEndSound;
    [SerializeField] private AudioClip slickSound;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        gameRunning = true;
        Time.timeScale = 1f;
        numEnemies = 0;
        gameEnded = false;
    }
    private void Update()
    {
        #if UNITY_EDITOR
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            EndLevel();
        }
        #endif
    }
    public void EndLevel()
    {
        TimeSpan timerData = TimerManager.instance.GetTime();
        TimerManager.instance.StopTimer();
        double playerTime = timerData.TotalSeconds;
        rank = Ranking.GenerateRank(data.requirements, playerTime);
        UIManager.instance.EndScript(timerData, numKills, numEnemies, topSpeed, rank);
        gameRunning = false;
        gameEnded = true;
        PauseGame();
        AudioManager.instance.PlayOmnicientSoundClip(levelEndSound, 1f, false, false);
        if (rank == Ranking.Rank.S)
        {
            StartCoroutine(PlaySlick());
        }

        // set up save data
        LevelSaveData saveData = ScriptableObject.CreateInstance<LevelSaveData>();
        saveData.sceneName = data.sceneName;
        saveData.unlocked = true;
        saveData.completed = true;
        saveData.playerTime = timerData;
        if (numKills >= numEnemies)
        {
            saveData.playerTimeKilledEnemies = timerData;
        }
        saveData.playerRank = rank;
        LevelDataSaveUtility.SmartSave(saveData);
        
        // set the next level to unlocked
        PlayerPrefs.SetInt(LevelDataSaveUtility.Key(data.nextLevel, "Unlocked"), 1);

        // update top speed
        float oldTopSpeed = PlayerPrefs.GetFloat("Top_Speed", 0.0f);
        if (topSpeed > oldTopSpeed)
        {
            PlayerPrefs.SetFloat("Top_Speed", topSpeed);
        }

        ScriptableObject.Destroy(saveData);
    }

    private IEnumerator PlaySlick()
    {
        yield return new WaitForSecondsRealtime(2.7f);
        AudioManager.instance.PlayOmnicientSoundClip(slickSound, 1f, false, false);
    }

    public void NextLevel()
    {
        AudioManager.instance.StopFilterMusic();
        SceneManager.LoadScene(data.nextLevel);
        Debug.Log("Loading scene");
    }
    public void RegisterEnemy()
    {
        numEnemies += 1;
    }
    public void RegisterKill()
    {
        numKills += 1;
    }
    public void RestartLevel()
    {
        AudioManager.instance.StopFilterMusic();
        gameEnded = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Timeslow.instance.DeactivateSlowMode();
    }
    public void TrackSpeed(float speed)
    {
        if (speed > topSpeed)
        {
            topSpeed = speed;
        }
    }
    public void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        scaleBeforePause = Time.timeScale;
        PlayerCharacter.instance.PauseSounds();
        Time.timeScale = 0f;
        gameRunning = false;
        AudioManager.instance.FilterMusic();
        if (!gameEnded)
        {
            UIManager.instance.PauseScript();
        }
    }
    public void ResumeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerCharacter.instance.ResumeSounds();
        Time.timeScale = scaleBeforePause;
        gameRunning = true;
        AudioManager.instance.StopFilterMusic();
        UIManager.instance.ClosePauseScript();
    }
    public void GoToMenu()
    {
        gameRunning = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
        AudioManager.instance.StopFilterMusic();
        SceneManager.LoadScene("Main Menu");
    }
}
