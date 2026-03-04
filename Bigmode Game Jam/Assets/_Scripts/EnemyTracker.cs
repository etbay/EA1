using UnityEngine;

public class EnemyTracker : MonoBehaviour
{
    // Simple script to count # enemies in a level at runtime
    void Start()
    {
        LevelManager.instance?.RegisterEnemy();
    }
}
