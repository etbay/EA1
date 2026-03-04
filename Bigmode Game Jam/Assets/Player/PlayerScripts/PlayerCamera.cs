using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CameraInput
{
    public Vector2 Look;
}


public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float verticalSmoothTime = 0.1f; // Only smooth Y movement
    private Vector3 _eulerAngles;
    private float _verticalVelocity; // For Y-axis smoothing only
    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
        transform.eulerAngles = _eulerAngles = target.eulerAngles;
        
    }

    public void UpdateRotation(CameraInput input)
    {
        sensitivity = SensitivitySlider.mouseSensitivity;

        _eulerAngles += new Vector3(-input.Look.y, input.Look.x) * sensitivity;

        _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -89f, 89f);

        transform.eulerAngles = _eulerAngles;
    }

    private float _visualOffsetColor; // The current "stored" bump height
    private float _lastTargetY;       // The player's Y position in the previous frame

    public void UpdatePosition(Transform target, bool isGrounded, float playerVerticalVelocity)
    {
        Vector3 targetPosition = target.position;
        float deltaY = targetPosition.y - _lastTargetY;
        if (isGrounded)
        {
            // 1. Detect the "Pop": How much did the player move vertically this frame?
            //float deltaY = targetPosition.y - _lastTargetY;

            // 2. If it was a step (sudden Y change), add it to our visual offset
            // We subtract it so the camera stays at its old height for a moment
            if (deltaY > 0.01f)
            {
                _visualOffsetColor -= deltaY;
            }
        }
        else
        {
            // Mantle detection:
            // If we moved up significantly more than our velocity suggests, it's a snap (mantle).
            float expectedY = playerVerticalVelocity * Time.deltaTime;

            //Threshold (0.1f) filters out normal jump movement/jitter
            if (deltaY > expectedY + 0.1f)
            {
                // Smooth camera up for mantle
                _visualOffsetColor -= deltaY;
            }
        }

        // 3. Decay the offset back to zero over time
        // This is what creates the "smooth glide" after a step
        _visualOffsetColor = Mathf.SmoothDamp(
            _visualOffsetColor,
            0f,
            ref _verticalVelocity,
            verticalSmoothTime
        );

        // 4. Final Position = Perfect Target Position + Visual Buffer
        transform.position = new Vector3(
            targetPosition.x,
            targetPosition.y + _visualOffsetColor,
            targetPosition.z
        );

        _lastTargetY = targetPosition.y;
    }
}
