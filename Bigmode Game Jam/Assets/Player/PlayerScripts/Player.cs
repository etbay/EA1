using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private PlayerAttackSystem playerAttackSystem;
    //[SerializeField] private OilShootingSystem playerOilSystem;
    [Space]
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;
    [SerializeField] private bool useCrouchToggle = true;
    [SerializeField] private PlayerSFXBank sfxBank;

    [SerializeField] private float slickSpeedMultStrength = 1.2f;
    [SerializeField] private float maxSlick = 4f;
    public static event System.Action SlickGained;
    public static event System.Action SlickChanged;
    private static float slickValue = 4f;
    private bool escaped = false;
    private bool slickDrains = true;
    private PlayerInputActions _inputActions;

    public static float SlickValue
    {
        get
        {
            return slickValue;
        }
        set
        {
            SlickChanged?.Invoke();
            float prevVal = slickValue;
            if (value > slickValue)
                SlickGained?.Invoke();
            slickValue = value;
            if (prevVal <= 1 && slickValue > 1)
            {
                AudioManager.instance.PlayOmnicientSoundClip(PlayerCharacter.instance.powerUpSound, 1f, false, false);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        slickValue = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());

        cameraSpring.Initialize();
        cameraLean.Initialize();
    }

    private void CleanupInput()
    {
        if (_inputActions == null) return;

        _inputActions.Player.Disable();
        _inputActions.Disable();
        _inputActions.Dispose();
        _inputActions = null;
    }

    private void OnDisable() => CleanupInput();
    private void OnDestroy() => CleanupInput();

    // Update is called once per frame
    void Update()
    {
        if (!LevelManager.gameRunning) return;
        //Debug.Log(slickValue);
        if (slickDrains)
        {
            float prevVal = slickValue;
            SlickValue -= Time.deltaTime * SlickometerData.CurrentSlickDrainRate;
            slickValue = Mathf.Clamp(slickValue, 1f, maxSlick);
            if (slickValue <= 1f && prevVal > 1f)
            {
                AudioManager.instance.PlayOmnicientSoundClip(sfxBank.PowerDown(), 1f, false, false);
            }
        }
        playerCharacter.isSpeedCapped = slickValue <= 1f;
        playerCharacter.speedBoostMultiplier = slickValue * slickSpeedMultStrength;

        var input = _inputActions.Player;
        var deltaTime = Time.deltaTime;

        #if UNITY_EDITOR
        if (input.SlickometerToggle.WasPressedThisFrame())
        {
            slickDrains = !slickDrains;
        }

        if (input.SlickometerFill.WasPressedThisFrame())
        {
            SlickValue = maxSlick;
        }

        if (input.SlickometerEmpty.WasPressedThisFrame())
        {
            SlickValue = 1f;
        }
        if (LevelManager.gameRunning && escaped)
        {
            escaped = false;
        }
        if (!escaped && input.PauseMenuEditor.WasPressedThisFrame())
        {
            LevelManager.instance.PauseGame();
            escaped = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // If escaped, allow refocus on left mouse click
        else if (escaped && input.PauseMenuEditor.WasPressedThisFrame())
        {
            LevelManager.instance.ResumeGame();
            escaped = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        #endif

        // gets camera input, update rotation
        // Handle Escape key to enter "escaped" state
        if (!escaped && input.PauseMenu.WasPressedThisFrame())
        {
            LevelManager.instance.PauseGame();
            escaped = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // If escaped, allow refocus on left mouse click
        else if (escaped && !LevelManager.gameEnded && input.PauseMenu.WasPressedThisFrame())
        {
            LevelManager.instance.ResumeGame();
            escaped = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Only update camera rotation if game is focused and not escaped
        if (Application.isFocused && !escaped)
        {
            var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
            playerCamera.UpdateRotation(cameraInput);
        }

        //Debug.Log(PlayerCharacter.instance.gameObject.transform.position);
        //get chracterinput and update
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Sprint = input.Sprint.IsPressed(),
            Jump = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch = useCrouchToggle
                ? (input.Crouch.WasPressedThisFrame() ? CrouchInput.Toggle : CrouchInput.None)
                : (input.Crouch.IsPressed() ? CrouchInput.Crouch : CrouchInput.UnCrouch),

            Attack = input.Attack.WasPressedThisFrame(),
            SecondaryFire = input.SecondaryFire.IsPressed()
            //Attack = input.Attack.IsPressed()
        };


        

        if (LevelManager.gameRunning)
        {
            playerCharacter.UpdateInput(characterInput);
            playerCharacter.UpdateBody(deltaTime);
            playerAttackSystem.updateInput(characterInput);
            //playerOilSystem.updateInput(characterInput);
        }
        
        #if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("Teleport");
            var ray = new Ray(playerCamera.transform.position,playerCamera.transform.forward);
            if(Physics.Raycast(ray, out var hit))
            {
                playerCharacter.setPosition(hit.point);
            }
        }
        #endif

        if (input.Restart.WasPressedThisFrame())
        {
            LevelManager.instance.RestartLevel();
        }
    }

    private void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = playerCharacter.GetCameraTarget();
        var state = playerCharacter.GetState();

        //playerCamera.UpdatePosition(cameraTarget);
        playerCamera.UpdatePosition(cameraTarget, state.Grounded, state.Velocity.y);
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        cameraLean.UpdateLean(deltaTime ,state.Stance is Stance.Slide ,state.Acceleration , cameraTarget.up);
    }
}
