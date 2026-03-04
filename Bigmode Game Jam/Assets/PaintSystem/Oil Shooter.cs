using UnityEngine;
using UnityEngine.InputSystem;

public class OilShooter : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [Space]
    [SerializeField] private bool mouseSingleClick;
    [Space]
    [SerializeField] private Color paintColor;

    [SerializeField] private float radius = 1;
    [SerializeField] private float strength = 1;
    [SerializeField] private float hardness = 1;

    void Update()
    {


        var mouse = Mouse.current;
        if (mouse == null || cam == null)
        {
            return;
        }

        bool click = mouseSingleClick
            ? mouse.leftButton.wasPressedThisFrame
            : mouse.leftButton.isPressed;
        if (click)
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                Debug.DrawRay(ray.origin, hit.point - ray.origin, Color.red);
                transform.position = hit.point;
                
                Paintable p = hit.collider.GetComponent<Paintable>();
                if (p != null)
                {
                    //Debug.Log($"hit.point={hit.point} uv={hit.textureCoord}");
                    PaintManager.instance.paint(p, hit.point, radius, hardness, strength, paintColor);
                }
            }
        }

    }
}
