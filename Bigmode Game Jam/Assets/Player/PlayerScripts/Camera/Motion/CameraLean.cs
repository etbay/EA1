using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLean : MonoBehaviour
{
    [SerializeField] private float attackDamping = 0.5f;
    [SerializeField] private float decayDamping = 0.3f;
    [SerializeField] private float walkStrength = 0.075f;
    [SerializeField] private float slideStrength = 0.2f;
    [SerializeField] private float strengthResponse = 5f;
    [SerializeField] private float maxLeanAngle = 10f; // degrees - clamp maximum tilt to avoid huge tilts on collisions



    private Vector3 _dampedAcceleration;

    private Vector3 _dampedAccelerationVel;

    private float _smoothStrength;

    public void Initialize()
    {
        _smoothStrength = walkStrength;
    }
    public void ResetLean()
    {
        _dampedAcceleration = Vector3.zero;
        _dampedAccelerationVel = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void UpdateLean(float deltaTime,bool sliding,Vector3 acceleration, Vector3 up)
    {
        if (!LevelManager.gameRunning)
            return;


            var planarAcceleration = Vector3.ProjectOnPlane(acceleration, up);
        var damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude
            ? attackDamping
            : decayDamping;

        _dampedAcceleration = Vector3.SmoothDamp
            (
                current: _dampedAcceleration,
                target: planarAcceleration,
                currentVelocity: ref _dampedAccelerationVel,
                smoothTime: damping,
                maxSpeed: float.PositiveInfinity,
                deltaTime: deltaTime
            );
        //Debug.Log($"Acceleration: {acceleration}, Magnitude: {acceleration.magnitude}");

        //get rotation axis based on the acceleration vector
        // If damped acceleration is near zero, skip computing lean axis to avoid NaNs
        if (_dampedAcceleration.sqrMagnitude < 1e-6f)
        {
            // reset rotation to that of parent
            transform.localRotation = Quaternion.identity;
            return;
        }

        var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up).normalized;
        if (leanAxis.sqrMagnitude < 1e-6f)
        {
            transform.localRotation = Quaternion.identity;
            return;
        }

        ////reset rotation to that of parent
        transform.localRotation = Quaternion.identity;

        ////rotate around the leanaxis
        var targetStrength = sliding
                ? slideStrength
                : walkStrength;

        _smoothStrength = Mathf.Lerp(_smoothStrength, targetStrength, 1f - Mathf.Exp(-strengthResponse * deltaTime));

        // Limit maximum tilt to avoid extreme rotations on sudden large accelerations (e.g. hitting a wall)
        float rawAngle = _dampedAcceleration.magnitude * _smoothStrength;
        float clampedAngle = Mathf.Clamp(rawAngle, 0f, maxLeanAngle);

        transform.rotation = Quaternion.AngleAxis(clampedAngle * -1f, leanAxis) * transform.rotation;


        //Debug.DrawRay(transform.position, acceleration, Color.red);
        //Debug.DrawRay(transform.position, _dampedAcceleration, Color.blue);

    }
}
