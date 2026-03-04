using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Destructible : MonoBehaviour
{
    [SerializeField] private float respawnTimer;
    private Respawner respawner;
    private Slider slider;
    private int order = 0;
    private bool dead = false;
    private bool deathStarted = false;

    private void Awake()
    {
        respawner = GetComponentInParent<Respawner>();
    }

    private void OnEnable()
    {
        respawner = GetComponentInParent<Respawner>();
    }

    public void Kill(int num)
    {
        if (!dead)
        {
            dead = true;
            if (Timeslow.IsSlowed)
            {
                order = num;
            }
        }
    }

    void Update()
    {
        if (dead && !Timeslow.IsSlowed && !deathStarted)
        {
            deathStarted = true;
            StartCoroutine(DeathScript(order));
        }
    }

    private IEnumerator DeathScript(int timeWait)
    {
        yield return new WaitForSeconds(timeWait / 15f); // controls deletion after resuming time
        Player.SlickValue += SlickometerData.DestructibleSlickGain;
        var explosion = PoolManager.instance.GetItemFromPool("Explosives");
        explosion.transform.position = gameObject.transform.position;
        explosion.GetComponent<Explosion>()?.Play();
        if (gameObject.GetComponent<EnemyTracker>() != null)
        {
            LevelManager.instance.RegisterKill();
        }

        if (respawner != null)
        {
            respawner.StartCoroutine(respawner.Respawn(respawnTimer, this.gameObject, this.transform.position, this.transform.rotation));
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private IEnumerator Respawn(float timeWait)
    {
        yield return new WaitForSeconds(timeWait);
    }
}