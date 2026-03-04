using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System.Collections;
using Unity.VisualScripting;

public class TestPlayer : MonoBehaviour
{
    public float moveSpeed = 1f;
    private float currentSpeed = 5f;
    private static float slickValue = 10f;
    private float jumpVelocity = 10f;

    private CharacterController controller;
    private Vector2 moveInput;
    private float verticalVelocity;
    private float gravity = -9.81f;
    private bool slamHeld = false;

    public static float SlickValue
    {
        get
        {
            return slickValue;
        }
    }

    void Awake()
    {
        slickValue = 5f;
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;
        move *= currentSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -8f;
            slamHeld = false;
        }

        float grav = gravity * (slamHeld ? 10f : 1f);
        verticalVelocity += grav * Time.deltaTime;

        Vector3 velocity = move + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        slickValue -= Time.deltaTime * 0.5f;
        slickValue = Mathf.Max(slickValue, 10f);
        currentSpeed = slickValue;
        Debug.Log(slickValue);
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed && controller.isGrounded)
        {
            verticalVelocity = jumpVelocity;
        }
    }

    void OnAttack(InputValue value)
    {
        slamHeld = value.isPressed;
    }

    public void BoostSpeed(int amount, float length)
    {
        this.currentSpeed += amount;
        StartCoroutine(SpeedBoost(length));
    }

    public void AddSlick(float amount)
    {
        slickValue += amount;
    }

    IEnumerator SpeedBoost(float length)
    {
        yield return new WaitForSeconds(length);
        this.currentSpeed = slickValue;
    }
}
