using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public static class SlickometerData
{
    public static float TimeslowSlickDrainRate { get; } = 3f;
    public static float BaseSlickDrainRate { get; } = 0.2f;
    public static float CurrentSlickDrainRate { get; set; } = BaseSlickDrainRate;
    public static float DestructibleSlickGain { get; } = 1f;
    public static float CappedSpeed { get; } = 40f;
}