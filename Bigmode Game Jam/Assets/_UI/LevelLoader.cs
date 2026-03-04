using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI rank;
    [SerializeField] private TextMeshProUGUI levelName;
    [SerializeField] private TextMeshProUGUI time;
    [SerializeField] private TextMeshProUGUI bestEnemiesTime;
    [SerializeField] private LevelData levelData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LevelSaveData saveData = ScriptableObject.CreateInstance<LevelSaveData>();
        saveData.sceneName = levelData.sceneName;

        LevelDataSaveUtility.Load(saveData);

        if (saveData.unlocked) 
        {
            SetAllActive();
            levelName.text = levelData.levelName;
            if (saveData.completed)
            {
                rank.text = saveData.playerRank.ToString();
                time.text = string.Format("{0:00}:{1:00}.{2:000}",
                    saveData.playerTime.Minutes,
                    saveData.playerTime.Seconds,
                    saveData.playerTime.Milliseconds);
            }
            else
            {
                rank.text = "--";
                time.text = "--";
            }

            if (saveData.playerTimeKilledEnemies != TimeSpan.Zero)
            {
                bestEnemiesTime.text = string.Format("{0:00}:{1:00}.{2:000}",
                    saveData.playerTimeKilledEnemies.Minutes,
                    saveData.playerTimeKilledEnemies.Seconds,
                    saveData.playerTimeKilledEnemies.Milliseconds);
            }
            else
            {
                bestEnemiesTime.text = "--";
            }
        }
        else
        {
            SetAllInactive();
        }
        button.onClick.AddListener(PlayLevel);

        ScriptableObject.Destroy(saveData);
    }
    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
    private void PlayLevel()
    {
        AudioManager.instance.PlayPersistentSoundClip(AudioManager.instance.Play, 1f, false, false);
        SceneManager.LoadScene(levelData.sceneName);
    }
    private void SetAllInactive()
    {
        button.gameObject.SetActive(false);
        rank.gameObject.SetActive(false);
        time.gameObject.SetActive(false);
        bestEnemiesTime.gameObject.SetActive(false);
    }
    private void SetAllActive()
    {
        button.gameObject.SetActive(true);
        rank.gameObject.SetActive(true);
        time.gameObject.SetActive(true);
        bestEnemiesTime.gameObject.SetActive(true);
    }
}
