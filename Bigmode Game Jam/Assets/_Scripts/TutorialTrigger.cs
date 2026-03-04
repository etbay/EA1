using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private string displayText;
    [SerializeField] private TutorialDisplay canvasDisplay;
    private bool beenDisplayed = false;

    void OnTriggerEnter(Collider other) // Use OnTriggerEnter2D for 2D games
    {
        if (other.tag == "Player" && !beenDisplayed)
        {
            beenDisplayed = true;
            canvasDisplay.Show(displayText);
        }
    }
}
