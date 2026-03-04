using UnityEngine;

public class EndZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other) // Use OnTriggerEnter2D for 2D games
    {
        if (other.tag == "Player")
        {
            LevelManager.instance.EndLevel();
        }
    }
}
