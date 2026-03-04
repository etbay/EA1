using UnityEngine;
using TMPro;

public class SpeedTracker : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void DisplaySpeed(float speed)
    {
        text.text = $"{speed:F1} m/s";
    }
}
