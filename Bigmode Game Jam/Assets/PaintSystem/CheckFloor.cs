using UnityEngine;
using KinematicCharacterController;

public class CheckFloor : MonoBehaviour
{
    [SerializeField] private float checkRadius = 0.3f;

    private KinematicCharacterMotor motor;
    private bool isOnPaintedSurface = false;

    public bool IsOnPaintedSurface => isOnPaintedSurface;

    void Start()
    {
        motor = GetComponent<KinematicCharacterMotor>();
        if (motor == null)
        {
            Debug.LogError("CheckFloor requires KinematicCharacterMotor component!");
        }
    }

    void Update()
    {
        if (motor == null)
        {
            isOnPaintedSurface = false;
            return;
        }

        
        if (motor.GroundingStatus.IsStableOnGround)
        {
            Collider groundCollider = motor.GroundingStatus.GroundCollider;
            Vector3 groundPoint = motor.GroundingStatus.GroundPoint;

            if (groundCollider != null && PaintTracker.Instance != null)
            {
                
                isOnPaintedSurface = PaintTracker.Instance.IsPainted(
                    groundCollider,
                    groundPoint,
                    checkRadius
                );
            }
            else
            {
                isOnPaintedSurface = false;
            }
        }
        else
        {
            isOnPaintedSurface = false;
        }

        //Debug.Log(IsOnPaintedSurface);
    }
}