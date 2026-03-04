using UnityEngine;

public class Killable: MonoBehaviour
{
    [SerializeField] private int health;

    private void Awake()
    {
        
    }

    private void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<TestPlayer>(out TestPlayer player))
        {
            player.AddSlick(3f);
            Destroy(this.gameObject);
        }
    }
}
