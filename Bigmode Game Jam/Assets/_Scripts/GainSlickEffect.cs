using System;
using UnityEngine;

public class GainSlickEffect : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        Player.SlickGained += OnSlickGained;
    }

    private void OnDisable()
    {
        Player.SlickGained -= OnSlickGained;
    }

    private void OnSlickGained()
    {
        ps.Play();
    }
}
