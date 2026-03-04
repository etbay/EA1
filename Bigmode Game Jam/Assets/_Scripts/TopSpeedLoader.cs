using System;
using TMPro;
using UnityEngine;

public class TopSpeedLoader : MonoBehaviour
{
    private TextMeshProUGUI speedDisplay;

    private void Awake()
    {
        speedDisplay = GetComponent<TextMeshProUGUI>();
        float topSpeed = PlayerPrefs.GetFloat("Top_Speed", 0.0f);
        topSpeed = (float)Math.Truncate(topSpeed * 100) / 100;
        if (topSpeed.CompareTo(0.0f) == 1)
        {
            speedDisplay.text = topSpeed.ToString() + " m/s";
        }
        else
        {
            speedDisplay.text = "--";
        }
    }
}
