using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Timeslow : MonoBehaviour
{
    [SerializeField] private AudioClip timeSlow;
    [SerializeField] private AudioClip timeResume;
    [SerializeField] private PlayerAttackSystem gun;
    [SerializeField] public static readonly float slowFactor = 0.2f; // 1 is full speed, 0.2 is 1/5 speed

    private AudioSource audioSource = null; 
    //[SerializeField] private AudioClip slowedTimeAmbience;

    public static Timeslow instance;
    public static event Action OnTimeslowToggled;
    public static bool IsSlowed = false;
    private PlayerInputActions _inputActions;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    } 

    void Start()
    {
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();
        IsSlowed = false;
    }

    private void CleanupInput()
    {
        if (_inputActions == null) return;

        _inputActions.Player.Disable();
        _inputActions.Disable();
        _inputActions.Dispose();
        _inputActions = null;
    }

    private void OnDisable() => CleanupInput();
    private void OnDestroy() => CleanupInput();

    // Update is called once per frame
    void Update()
    {
        if (_inputActions == null || !LevelManager.gameRunning) return;
        if (_inputActions.Player.Ability.WasPressedThisFrame() && !IsSlowed && Player.SlickValue > 1.0f)
        {
            if (audioSource != null)
            {
                audioSource.volume = 0f; // stops overlaying time related sfx
            }
            IsSlowed = true;
            ActivateSlowMode();
        }
        else if (((Player.SlickValue <= 1.0f) && IsSlowed) || (Player.SlickValue > 1.0f && _inputActions.Player.Ability.WasPressedThisFrame() && IsSlowed))
        {
            if (audioSource != null)
            {
                audioSource.volume = 0f;
            }
            IsSlowed = false;
            DeactivateSlowMode();
        }
    }

    private void ActivateSlowMode()
    {
        OnTimeslowToggled?.Invoke();
        Time.timeScale = slowFactor;
        audioSource = AudioManager.instance?.PlayOmnicientSoundClip(timeSlow, 1f, false, false);
        AudioManager.instance.TimeAudioStretch(0.6f);
        SlickometerData.CurrentSlickDrainRate = SlickometerData.TimeslowSlickDrainRate;
    }

    public void DeactivateSlowMode()
    {
        OnTimeslowToggled?.Invoke();
        StartCoroutine(gun.FireTrackedTracers());
        Time.timeScale = 1f;
        audioSource = AudioManager.instance?.PlayOmnicientSoundClip(timeResume, 1f, false, false);
        AudioManager.instance.TimeAudioStretch(1f);
        SlickometerData.CurrentSlickDrainRate = SlickometerData.BaseSlickDrainRate;
    }
}
