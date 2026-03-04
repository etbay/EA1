using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] Vector3 rotationAxis;
    [SerializeField] float speed;

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(rotationAxis, Time.deltaTime * speed);
    }
}
