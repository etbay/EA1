using System.Collections;
using UnityEngine;

public class Respawner : MonoBehaviour
{
    public IEnumerator Respawn(float timeWait, GameObject gameObject, Vector3 pos, Quaternion rotation)
    {
        GameObject copy = Instantiate(gameObject, pos, rotation);
        copy.SetActive(false);
        copy.transform.SetParent(this.transform);
        EnemyTracker tracker = copy.GetComponent<EnemyTracker>();
        if (tracker != null)
        {
            tracker.enabled = false;
        }
        Destroy(gameObject);
        yield return new WaitForSeconds(timeWait);
        copy.SetActive(true);
    }
}
