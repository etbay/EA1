using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required for TextMeshProUGUI
using System;

public class TimerManager : MonoBehaviour
{
    public static TimerManager instance;
    [SerializeField] TextMeshProUGUI timerText;
    private float currentTime = 0;
    private bool running = true;

    void Awake()
    {
        if (instance == null) {
            instance = this;
        }
    }
    void Update()
    {
        if (running)
        {
            currentTime += Time.deltaTime; // / Timeslow.slowFactor; if you want the timer to not slow just add / Timeslow.slowFactor; in an if check
            UpdateTimerUI(); 
        }
    }

    public void StopTimer()
    {
        running = false;
    }

    public void StartTimer()
    {
        running = true;
    }

    private void UpdateTimerUI()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
        string timerString = string.Format("{0:00}:{1:00}.{2:000}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        // Set the text of the UI element
        timerText.text = timerString; 
    }

    public TimeSpan GetTime()
    {
        return TimeSpan.FromSeconds(currentTime);
    }
}
