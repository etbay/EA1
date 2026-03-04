using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class DroneAI : MonoBehaviour
{
    Vector3 initialPos;
    public Transform droneTransform;
    public Vector3 direction;
    public float amplitude = 1f;

    void Start() {
        initialPos = droneTransform.position;
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        Vector3 newPos = new Vector3(0, 0, 0);
        if (direction.x != 0)
        {
            newPos += (initialPos.x * Vector3.right) + Mathf.Sin(Time.fixedTime) * amplitude * direction.x * Vector3.right;
        }
        else
        {
            newPos += droneTransform.position.x * Vector3.right;
        }

        if (direction.y != 0)
        {
            newPos += (initialPos.y * Vector3.up) + Mathf.Sin(Time.fixedTime) * amplitude * direction.y * Vector3.up;
        }
        else
        {
            newPos += droneTransform.position.y * Vector3.up;
        }

        if (direction.z != 0)
        {
            newPos += (initialPos.z * Vector3.forward) + Mathf.Sin(Time.fixedTime) * amplitude * direction.z * Vector3.forward;
        }
        else
        {
            newPos += droneTransform.position.z * Vector3.forward;
        }
        
        droneTransform.position = newPos;
    }
}
