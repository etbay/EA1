using System.Collections;
using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private float trailLength = 0.05f;

    public BulletTracer FireTracer(Vector3 start, Vector3 end, float startWidth, float decay)
    {
        line.enabled = true;
        line.widthMultiplier = startWidth;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        StartCoroutine(BulletDecay(line, start, end, decay));

        return this;
    }
    public BulletTracer FireTracer(Vector3 start, Vector3 end, float startWidth)
    {
        line.enabled = true;
        line.widthMultiplier = startWidth;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        StartCoroutine(BulletDecay(line));
        return this;
    }
    private IEnumerator BulletDecay(LineRenderer line, Vector3 start, Vector3 end, float time)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;

            // Keep a portion of the trail visible behind the bullet
            float startT = Mathf.Max(0, t - trailLength);

            // Move the start position towards the end position, creating a trailing effect
            line.SetPosition(0, Vector3.Lerp(start, end, startT));
            line.SetPosition(1, Vector3.Lerp(start, end, t));

            yield return null;
        }

        line.gameObject.SetActive(false);
    }
    private IEnumerator BulletDecay(LineRenderer line)
    {
        while (Timeslow.IsSlowed)
        {
            yield return null;
        }
        line.gameObject.SetActive(false);
    }
}
