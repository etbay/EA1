using UnityEngine;
using UnityEngine.InputSystem;

public class MousePainter : MonoBehaviour{
    public Camera cam;
    [Space]
    public bool mouseSingleClick;
    [Space]
    public Color paintColor;
    
    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;

    void Update(){

        //bool click;
        //click = mouseSingleClick ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0);
        var mouse = Mouse.current;
        if (mouse == null || cam == null)
        {
            return;
        }

        bool click = mouseSingleClick
            ? mouse.leftButton.wasPressedThisFrame
            : mouse.leftButton.isPressed;
        if (click){
            Vector3 position = mouse.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100.0f)){
                Debug.DrawRay(ray.origin, hit.point - ray.origin, Color.red);
                transform.position = hit.point;
                Paintable p = hit.collider.GetComponent<Paintable>();
                if(p != null){
                    //Debug.Log($"hit.point={hit.point} uv={hit.textureCoord}");
                    PaintManager.instance.paint(p, hit.point, radius, hardness, strength, paintColor);
                }
            }
        }

    }

}
