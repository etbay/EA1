using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TimeslowFilter : MonoBehaviour
{
    [SerializeField] private float lerpTime;
    [SerializeField] private float offAlpha;
    [SerializeField] private float onAlpha;
    [SerializeField] private Image image;

    bool isFilterActive = false;
    void Awake()
    {
        Timeslow.OnTimeslowToggled += ToggleFilter;
        // this.gameObject.SetActive(true);
    }

    void OnDestroy()
    {
        Timeslow.OnTimeslowToggled -= ToggleFilter;
    }

    private void ToggleFilter()
    {
        // this.gameObject.SetActive(!this.gameObject.activeSelf);
        if(!isFilterActive)
        {
            StopAllCoroutines();
            StartCoroutine(LerpToOn());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(LerpToOff());
        }

        isFilterActive = !isFilterActive;
    }

    private IEnumerator LerpToOn()
    {
        float startAlpha = image.color.a;
        float startTime = Time.time;
        while(true)
        {
            image.color = new Color( image.color.r, image.color.g, image.color.b, Mathf.Lerp(startAlpha,onAlpha, (Time.time - startTime) / lerpTime));
            yield return null;
        }
    }

        private IEnumerator LerpToOff()
    {
        float startAlpha = image.color.a;
        float startTime = Time.time;
        while(true)
        {
            image.color = new Color( image.color.r, image.color.g, image.color.b, Mathf.Lerp(startAlpha,offAlpha, (Time.time - startTime) / lerpTime));
            yield return null;
        }
    }
}
