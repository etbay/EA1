using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    [SerializeField] ResultsScreen results;
    [SerializeField] GameObject crosshair;
    [SerializeField] TimerManager timer;
    [SerializeField] SpeedTracker speedTracker;
    [SerializeField] PauseScreen pauseScreen;
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        results.gameObject.SetActive(false);
        crosshair.SetActive(true);
        timer.gameObject.SetActive(true);
        speedTracker.gameObject.SetActive(true);
        pauseScreen.Disable();
    }
    public void UpdateSpeedDisplay(float speed)
    {
        speedTracker.DisplaySpeed(speed);
    }
    public void EndScript(TimeSpan time, int kills, int totalEnemies, float speed, Ranking.Rank plRank)
    {
        results.gameObject.SetActive(true);
        results.DisplayResults(time, kills, totalEnemies, speed, plRank);
        crosshair.SetActive(false);
        timer.gameObject.SetActive(false);
        speedTracker.gameObject.SetActive(false);
    }
    public void PauseScript()
    {
        pauseScreen.Enable();
    }
    public void ClosePauseScript()
    {
        pauseScreen.Disable();
    }
}