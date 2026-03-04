using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            LevelManager.instance.RestartLevel();
        }
    }
}
