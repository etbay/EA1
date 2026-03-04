using System.Runtime.CompilerServices;
using UnityEngine;

public class Goop : MonoBehaviour
{
    private BoxCollider boxCollider;
    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<TestPlayer>(out TestPlayer player))
        {
            Debug.Log(player);
            player.BoostSpeed(10, 2f);
        }
    }
}
