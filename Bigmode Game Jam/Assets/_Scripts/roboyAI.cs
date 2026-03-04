using UnityEngine;

public class RoboyAI : MonoBehaviour
{
    Vector3 initialPos;
    Vector3 yPos;
    public Transform objectTransform;
    public Vector3 direction;
    public float amplitude = 1f;
    bool disrupted = false; // so you can push them and they wont teleport back to their initial location

    void Start()
    {
        initialPos = objectTransform.position;
    }

    void FixedUpdate()
    {
        if (!disrupted)
            Move();
    }

    void Move()
    {
        yPos = objectTransform.position.y * Vector3.up;
        objectTransform.position = (Mathf.Sin(Time.fixedTime) * amplitude * direction) + (initialPos.x * Vector3.right) + (initialPos.z * Vector3.right) + yPos;
    }
}
