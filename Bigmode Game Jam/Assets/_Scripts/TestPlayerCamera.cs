using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayerCamera : MonoBehaviour
{
    public float sensitivity = 0.15f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    private PlayerInput playerInput;
    private InputAction lookAction;
    private Camera camera_;

    private float pitch;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        camera_ = GetComponent<Camera>();
        playerInput = GetComponentInParent<PlayerInput>();

        lookAction = playerInput.actions.FindAction("Look", throwIfNotFound: false)
                 ?? playerInput.actions.FindAction("look", throwIfNotFound: false);

        if (lookAction == null)
        {
            Debug.LogError("Look action not found. Expected an action named 'Look' in the PlayerInput actions.");
        }
    }

    void Update()
    {
        if (lookAction == null) return;

        Vector2 delta = lookAction.ReadValue<Vector2>() * sensitivity;

        // Pitch (camera local)
        pitch -= delta.y;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Yaw (rotate parent capsule)
        transform.parent.Rotate(Vector3.up * delta.x);

        float slickRatio = (TestPlayer.SlickValue - 10) / (30 - 10);
        camera_.fieldOfView = 60f + slickRatio * (120 - 60);
    }
}
