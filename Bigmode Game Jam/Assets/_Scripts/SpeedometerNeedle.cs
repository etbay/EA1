using System;
using UnityEngine;

public class SpeedometerNeedle : MonoBehaviour
{
    [SerializeField] private Color fullColor;
    [SerializeField] private Color halfFullColor;
    [SerializeField] private Color emptyColor;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        Player.SlickChanged += UpdateNeedle;
    }

    private void OnDestroy()
    {
        Player.SlickChanged -= UpdateNeedle;
    }

    private void UpdateNeedle()
    {
        if (Player.SlickValue >= 2.5f)
        {
            meshRenderer.material.color = fullColor;
        }
        else if (Player.SlickValue >= 1.5f)
        {
            meshRenderer.material.color = halfFullColor;
        }
        else
        {
            meshRenderer.material.color = emptyColor;
        }
    }
}
