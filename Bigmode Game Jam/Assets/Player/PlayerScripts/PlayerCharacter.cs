using KinematicCharacterController;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

#region Enums
public enum CrouchInput
{
    None, Toggle, Crouch, UnCrouch
}

public enum Stance
{
    Stand, Crouch, Slide
}
#endregion

#region Structs
public struct CharacterState
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 Velocity;
    public Vector3 Acceleration;
}

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Sprint;
    public bool Jump;
    public bool JumpSustain;
    public CrouchInput Crouch;
    public bool Attack;
    public bool SecondaryFire;
}
#endregion

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    public static PlayerCharacter instance;

    #region Serialized Fields - References
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerSFXBank sfxBank;
    #endregion

    #region Serialized Fields - Movement Settings
    [Header("Grounded Movement")]
    public float speedBoostMultiplier = 1f;
    public bool isSpeedCapped = true;
    [SerializeField] private bool slick;
    [SerializeField] private float walkSpeed = 20f;
    //[SerializeField] private float sprintAcceleration = 30f;
    [SerializeField] private float crouchSpeed = 7f;
    [SerializeField] private float walkResponse = 25f;
    [SerializeField] private float crouchResponse = 20f;
    [SerializeField] private float groundedStepHeight = 0.5f;
    [SerializeField] private float mantlStepHeight = 2f;
    [SerializeField] private float checkRadius = 0.3f;
    private bool isOnPaintedSurface = false;

    #endregion

    #region Serialized Fields - Air Movement Settings
    [Header("Air Movement")]
    [SerializeField] private float airSpeed = 15f;
    [SerializeField] private float airAcceleration = 70f;
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float coyoteTime = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float jumpSustainGravity = 0.4f;
    [SerializeField] private float gravity = -90f;
    [SerializeField] private float slamGravity = -90f;
    #endregion

    #region Serialized Fields - Slide Settings
    [Header("Slide Settings")]
    [SerializeField] private float slideStartSpeed = 25f;
    [SerializeField] private float slideEndSpeed = 15f;
    [SerializeField] private float slideFriction = 0.8f;
    [SerializeField] private float slideSteerAccelleration = 5f;
    [SerializeField] private float slideGravity = -90f;
    [Tooltip("How much downward (fall) speed is converted into planar slide speed on landing (0 = none, 1 = full).")]
    [SerializeField] private float fallToSlideRatio = 2f;
    #endregion

    #region Serialized Fields - Height Settings
    [Header("Height Settings")]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchHeightResponse = 15f;
    [Range(0f, 1f)]
    [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] private float crouchCameraTargetHeight = 0.7f;
    #endregion

    #region Private Fields
    private CharacterState _state;
    private CharacterState _lastState;
    private CharacterState _tempState;

    private Quaternion _reqestedRotation;
    private Vector3 _reqestedMovement;
    private bool _reqestedSlam;
    private bool _requestedJump;
    private bool _requestedSustainedJump;
    private bool _requestedCrouch;
    private bool _requestedCrouchInAir;
    private Vector3 _landingImpactVelocity;
    private float _timeSinceUngrounded = 0f; // Set to 0 at default to prevent audio from playing immediately
    private float _timeSinceJumpRequest;
    private bool _ungroundedDueToJump;
    private Collider[] _uncrouchOverLapResults;

    // Floats for timing walking sounds
    private float footstepInterval = 0.6f;
    private float footstepTimer = 0f;
    [SerializeField] private float fovChangeSpeed = 50f;
    [SerializeField] public AudioClip powerUpSound;
    private bool stopped = true;
    private AudioSource slidingAudio;
    private AudioSource airAmbience;
    #endregion

    #region Initialization & Input
    public void Initialize()
    {
        _state.Stance = Stance.Stand;
        _lastState = _state;
        _uncrouchOverLapResults = new Collider[8];
        motor.CharacterController = this;
        instance = this;
    }

    private void Start()
    {
        slidingAudio = AudioManager.instance.GetLoopableAudioSource(sfxBank.SlidingSound(), root.position, 0f, true, false);
        airAmbience = AudioManager.instance.GetLoopableAudioSource(sfxBank.AirAmbience(), root.position, 0f, true, false);
        airAmbience.Play();

    }
    public void PauseSounds()
    {
        slidingAudio.Pause();
        airAmbience.Pause();
    }

    public void ResumeSounds()
    {
        slidingAudio.Play();
        airAmbience.Play();
    }

    public void UpdateInput(CharacterInput input)
    {
        _reqestedRotation = input.Rotation;
        _reqestedMovement = input.Rotation * Vector3.ClampMagnitude(new Vector3(input.Move.x, 0f, input.Move.y), 1f);
        _reqestedSlam = input.Crouch switch
        {
            CrouchInput.Toggle => !_requestedCrouch,
            CrouchInput.Crouch => true,
            CrouchInput.UnCrouch => false,
            _ => _requestedCrouch
        };

        var wasRequestingJump = _requestedJump;
        _requestedJump = _requestedJump || input.Jump;
        if (_requestedJump && !wasRequestingJump) _timeSinceJumpRequest = 0f;

        _requestedSustainedJump = input.JumpSustain;

        var wasRequestingCroutch = _requestedCrouch;
        _requestedCrouch = input.Crouch switch
        {
            CrouchInput.Toggle => !_requestedCrouch,
            CrouchInput.Crouch => true,
            CrouchInput.UnCrouch => false,
            _ => _requestedCrouch
        };

        if (_requestedCrouch && !wasRequestingCroutch) _requestedCrouchInAir = !_state.Grounded;
        else if (!_requestedCrouch && wasRequestingCroutch) _requestedCrouchInAir = false;
    }
    #endregion

    #region Update Body
    public void UpdateBody(float deltaTime)
    {
        var currentHeight = motor.Capsule.height;
        var normalizedHeight = currentHeight / standHeight;
        var cameraTargetHeight = currentHeight * (_state.Stance is Stance.Stand ? standCameraTargetHeight : crouchCameraTargetHeight);

        cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition, new Vector3(0f, cameraTargetHeight, 0f), 1f - Mathf.Exp(-crouchHeightResponse * deltaTime));
        root.localScale = Vector3.Lerp(root.localScale, new Vector3(1f, normalizedHeight, 1f), 1f - Mathf.Exp(-crouchHeightResponse * deltaTime));
    }
    #endregion

    #region Velocity Dispatcher
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        Vector3 horizontalVel = currentVelocity - (Vector3.up * currentVelocity.y);
        updateFOV(horizontalVel.magnitude);
        slidingAudio.pitch = Mathf.Clamp(horizontalVel.magnitude / 80, 1, 1.3f);
        UIManager.instance.UpdateSpeedDisplay(horizontalVel.magnitude);
        LevelManager.instance.TrackSpeed(horizontalVel.magnitude);
        _state.Acceleration = Vector3.zero;
        /*
        if (isSpeedCapped)
        {
            Vector3 planarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            planarVelocity = Vector3.ClampMagnitude(planarVelocity, SlickometerData.CappedSpeed);
            currentVelocity.x = planarVelocity.x;
            currentVelocity.z = planarVelocity.z;
        }
        */

        if (motor.GroundingStatus.IsStableOnGround)
        {
            if (isSpeedCapped)
            {
                Vector3 planarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
                planarVelocity = Vector3.ClampMagnitude(planarVelocity, SlickometerData.CappedSpeed);
                currentVelocity.x = planarVelocity.x;
                currentVelocity.z = planarVelocity.z;
            }
            motor.AllowSteppingWithoutStableGrounding = false;
            motor.MaxStepHeight = groundedStepHeight;
            HandleGroundedMovement(ref currentVelocity, deltaTime);
        }
        else
        {
            motor.AllowSteppingWithoutStableGrounding = true;
            motor.MaxStepHeight = mantlStepHeight;
            HandleAirborneMovement(ref currentVelocity, deltaTime);
        }

        HandleJumping(ref currentVelocity, deltaTime);
    }
    #endregion

    #region FOV Visuals
    private void updateFOV(float velMag)
    {
        // if (playerCamera.fieldOfView >90f && velMag < 40f)
        // {
        //     playerCamera.fieldOfView -= 1f * Time.deltaTime;
        // }
        playerCamera.fieldOfView = Mathf.MoveTowards(playerCamera.fieldOfView, 90f + (Mathf.Clamp((velMag - 40f) / 3, 0f, 30f)), fovChangeSpeed*Time.deltaTime);
    }
    #endregion

    #region Grounded Logic
    private void HandleGroundedMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        if (_state.Grounded)
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
        slick = isOnPaintedSurface ? true : false;
        //Debug.Log(slick);
        if (!_state.Grounded && _timeSinceUngrounded > 0.3f)
        {
            AudioManager.instance.PlayOmnicientSoundClip(sfxBank.LandSound(), 1f, true, true);
        }
        airAmbience.volume = Mathf.Clamp01(currentVelocity.magnitude / 400);
        
        _ungroundedDueToJump = false;
        _timeSinceUngrounded = 0f;

        var groundedMovement = motor.GetDirectionTangentToSurface(_reqestedMovement, motor.GroundingStatus.GroundNormal) * _reqestedMovement.magnitude;

        // Transition check for Sliding
        if (ShouldStartSlide(groundedMovement, currentVelocity))
        {
            StartSlide(ref currentVelocity);
        }

        // Execute specific grounded state
        if (_state.Stance is Stance.Slide)
        {
            UpdateSlideMovement(ref currentVelocity, groundedMovement, deltaTime);
        }
        else
        {
            UpdateStandardMovement(ref currentVelocity, groundedMovement, deltaTime);
        }
    }

    private bool ShouldStartSlide(Vector3 groundedMovement, Vector3 currentVelocity)
    {
        bool moving = groundedMovement.sqrMagnitude > 0f;
        bool crouching = _state.Stance is Stance.Crouch;
        bool stanceTransition = _lastState.Stance is Stance.Stand || !_lastState.Grounded;

        float incomingSpeed = !_lastState.Grounded ? _lastState.Velocity.magnitude : currentVelocity.magnitude;

        return _requestedCrouch && moving && stanceTransition && incomingSpeed > slideEndSpeed;
    }

    private void StartSlide(ref Vector3 currentVelocity)
    {
        _state.Stance = Stance.Slide;

        Vector3 groundNormal = motor.GroundingStatus.GroundNormal;

        Vector3 sourceVelocity = (!_lastState.Grounded && _state.Grounded) ? _landingImpactVelocity
            : (!_lastState.Grounded ? _lastState.Velocity : currentVelocity);

        float downwardSpeed = Mathf.Max(0f, -Vector3.Dot(sourceVelocity, motor.CharacterUp));
        Vector3 planar = Vector3.ProjectOnPlane(sourceVelocity, groundNormal);
        float planarSpeed = planar.magnitude;

        float slideSpeed = Mathf.Max(slideStartSpeed, planarSpeed + (downwardSpeed * fallToSlideRatio));

        Vector3 dir = motor.GetDirectionTangentToSurface(motor.CharacterForward, groundNormal).normalized;

        currentVelocity = dir * slideSpeed;

        _requestedCrouchInAir = false;

        if (!slidingAudio.isPlaying || slidingAudio.volume == 0f) {
            slidingAudio.volume = 1f;
            slidingAudio.Play();
        }
    }

    private void UpdateStandardMovement(ref Vector3 currentVelocity, Vector3 groundedMovement, float deltaTime)
    {
        Vector3 prevVelocity = currentVelocity;
        //bool isSprinting = _state.Stance is Stance.Stand && _reqestedSprint && groundedMovement.sqrMagnitude > 0f;
        if (slidingAudio.isPlaying)
        {
            slidingAudio.Stop();
            AudioManager.instance.PlaySoundClipFromList(sfxBank.WalkSounds(), root.position, 1f, true, true);
        }

        float targetSpeed = _state.Stance is Stance.Stand ? walkSpeed : crouchSpeed;
        float response = _state.Stance is Stance.Stand ? walkResponse : crouchResponse;
        targetSpeed *= speedBoostMultiplier;

        float currentSpeed = currentVelocity.magnitude;
        Vector3 desiredDir = groundedMovement.sqrMagnitude > 0.0001f ? groundedMovement.normalized : Vector3.zero;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed * groundedMovement.magnitude, 1f - Mathf.Exp(-response * deltaTime));
        currentVelocity = desiredDir * currentSpeed;
        _state.Acceleration = (currentVelocity - prevVelocity) / deltaTime;

        footstepInterval = Mathf.Clamp(5/currentSpeed , 0.15f, 1f); // the higher the speed, the faster the footsteps, clamped so its not unreasonable (5/x is the equation)
        if (currentSpeed > 0)
        {
            footstepTimer += deltaTime;
            stopped = false;
        }
        else stopped = true;
        
        if ((footstepTimer >= footstepInterval && !stopped))
        {
            footstepTimer = 0f;
            AudioManager.instance.PlaySoundClipFromList(sfxBank.WalkSounds(), root.position, 0.8f, true, true);
        }
    }

    private void UpdateSlideMovement(ref Vector3 currentVelocity, Vector3 groundedMovement, float deltaTime)
    {
        // Extract planar velocity (parallel to surface)
        Vector3 planarVel = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);

        if (slick)
        {
            // On slick surfaces: no friction, no continuous gravity acceleration
            // The slam boost from StartSlide() is preserved as initial velocity
            // Just maintain current velocity (frictionless slide)
        }
        else
        {
            // Calculate downhill direction to check if we're going downhill
            Vector3 downhill = Vector3.ProjectOnPlane(-motor.CharacterUp, motor.GroundingStatus.GroundNormal);
            bool isGoingDownhill = false;

            if (downhill.sqrMagnitude > 1e-6f)
            {
                downhill.Normalize();
                // Check if current velocity has a component in the downhill direction
                float downhillDot = Vector3.Dot(planarVel.normalized, downhill);
                isGoingDownhill = downhillDot > 0.1f; // Small threshold to account for slight angles
            }

            if (isGoingDownhill)
            {
                // Maintain constant speed when going downhill - no gravity or friction
                // Simply preserve the current velocity magnitude
            }
            else
            {
                // Stronger friction when surface is not slick and not going downhill
                float slopeDecelerationFactor = 1f;
                planarVel -= planarVel * (slideFriction * slopeDecelerationFactor * deltaTime);
            }
        }

        // Reassemble velocity with the modified planar component
        currentVelocity = planarVel + Vector3.Project(currentVelocity, motor.CharacterUp);

        // Handle steering
        if (groundedMovement.sqrMagnitude > 0f)
        {
            float speed = currentVelocity.magnitude;
            Vector3 currentDir = currentVelocity.sqrMagnitude > 0.0001f ? currentVelocity.normalized : groundedMovement.normalized;
            Vector3 desiredDir = groundedMovement.normalized;

            // Smoothly blend direction based on slideSteerAccelleration
            float steerFactor = 1f - Mathf.Exp(-slideSteerAccelleration * deltaTime);
            Vector3 newDir = Vector3.Lerp(currentDir, desiredDir, steerFactor);
            if (newDir.sqrMagnitude > 0.001f) newDir.Normalize();
            else newDir = desiredDir;

            Vector3 steerVelocity = newDir * speed;

            _state.Acceleration = (steerVelocity - currentVelocity) / deltaTime;
            currentVelocity = steerVelocity;
        }

        if (currentVelocity.magnitude < slideEndSpeed) _state.Stance = Stance.Crouch;
    }
    #endregion

    #region Airborne Logic
    private void HandleAirborneMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        if (_state.Grounded && isSpeedCapped)
        {
            Vector3 planarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            planarVelocity = Vector3.ClampMagnitude(planarVelocity, SlickometerData.CappedSpeed);
            currentVelocity.x = planarVelocity.x;
            currentVelocity.z = planarVelocity.z;
        }


        if (slidingAudio.isPlaying)
        {
            slidingAudio.Stop();
        }
        _timeSinceUngrounded += deltaTime;

        if (_reqestedMovement.sqrMagnitude > 0f)
        {
            ApplyAirControl(ref currentVelocity, deltaTime);
        }
        else
        {
            Vector3 planarVel = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
            Vector3 targetPlanar = Vector3.Lerp(planarVel, Vector3.zero, 1f - Mathf.Exp(-5f * deltaTime));
            currentVelocity -= (planarVel - targetPlanar);

        }
        //float effectiveGravity = (_requestedSustainedJump && Vector3.Dot(currentVelocity, motor.CharacterUp) > 0f) ? gravity * jumpSustainGravity : gravity;
        // Ground slam: heavier gravity when sprinting in air
        float effectiveGravity = _reqestedSlam ? slamGravity : gravity;

        // Apply jump sustain gravity reduction if holding jump while ascending
        if (_requestedSustainedJump && Vector3.Dot(currentVelocity, motor.CharacterUp) > 0f)
        {
            effectiveGravity *= jumpSustainGravity;
        }
        currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;

        airAmbience.volume = Mathf.Clamp01(currentVelocity.magnitude / 200);        
    }

    private void ApplyAirControl(ref Vector3 currentVelocity, float deltaTime)
    {
        Vector3 planarMove = Vector3.ProjectOnPlane(_reqestedMovement, motor.CharacterUp) * _reqestedMovement.magnitude;
        Vector3 planarVel = Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp);
        Vector3 moveForce = planarMove * airAcceleration * deltaTime;

        if (planarVel.magnitude < airSpeed)
        {
            currentVelocity += Vector3.ClampMagnitude(planarVel + moveForce, airSpeed) - planarVel;
        }
        else if (Vector3.Dot(planarVel, moveForce) > 0f)
        {
            moveForce = Vector3.ProjectOnPlane(moveForce, planarVel.normalized);
        }

        if (motor.GroundingStatus.FoundAnyGround && Vector3.Dot(moveForce, currentVelocity + moveForce) > 0f)
        {
            Vector3 obstructionNormal = Vector3.Cross(motor.CharacterUp, Vector3.Cross(motor.CharacterUp, motor.GroundingStatus.GroundNormal)).normalized;
            moveForce = Vector3.ProjectOnPlane(moveForce, obstructionNormal);
        }

        currentVelocity += moveForce;
    }
    #endregion

    #region Jump Logic
    private void HandleJumping(ref Vector3 currentVelocity, float deltaTime)
    {
        if (!_requestedJump) return;

        bool canJump = motor.GroundingStatus.IsStableOnGround || (_timeSinceUngrounded < coyoteTime && !_ungroundedDueToJump);

        if (canJump)
        {
            _requestedJump = false;
            _requestedCrouch = false;
            _requestedCrouchInAir = false;
            _ungroundedDueToJump = true;
            motor.ForceUnground(0f);

            float currentVertSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            currentVelocity += motor.CharacterUp * (Mathf.Max(currentVertSpeed, jumpSpeed) - currentVertSpeed);
            //currentVelocity += motor.CharacterForward * (Mathf.Max(currentVertSpeed, jumpSpeed) - currentVertSpeed);
            
            AudioManager.instance.PlayOmnicientSoundClip(sfxBank.JumpSound(), 1f, true, true);
        }
        else
        {
            _timeSinceJumpRequest += deltaTime;
            _requestedJump = _timeSinceJumpRequest < coyoteTime;
        }
    }
    #endregion

    #region Character Motor Callbacks
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        Vector3 forward = Vector3.ProjectOnPlane(_reqestedRotation * Vector3.forward, motor.CharacterUp);
        if (forward != Vector3.zero) currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempState = _state;
        if (_requestedCrouch && _state.Stance is Stance.Stand)
        {
            _state.Stance = Stance.Crouch;
            motor.SetCapsuleDimensions(motor.Capsule.radius, crouchHeight, crouchHeight * 0.5f);
        }
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        if (!_requestedCrouch && _state.Stance is not Stance.Stand)
        {
            motor.SetCapsuleDimensions(motor.Capsule.radius, standHeight, standHeight * 0.5f);
            if (motor.CharacterOverlap(motor.TransientPosition, motor.TransientRotation, _uncrouchOverLapResults, motor.CollidableLayers, QueryTriggerInteraction.Ignore) > 0)
            {
                _requestedCrouch = true;
                motor.SetCapsuleDimensions(motor.Capsule.radius, crouchHeight, crouchHeight * 0.5f);
            }
            else _state.Stance = Stance.Stand;
        }

        // detect landing and cache the last airborne velocity as impact velocity
        bool wasGrounded = _state.Grounded;
        bool isGroundedNow = motor.GroundingStatus.IsStableOnGround;
        if (!wasGrounded && isGroundedNow)
        {
            _landingImpactVelocity = _lastState.Velocity;
        }

        _state.Grounded = isGroundedNow;
        _state.Velocity = motor.Velocity;
        _lastState = _tempState;
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        if (!motor.GroundingStatus.IsStableOnGround && _state.Stance is Stance.Slide) _state.Stance = Stance.Crouch;
    }
    #endregion

    #region Public Accessors & Interface
    public Transform GetCameraTarget() => cameraTarget;
    public CharacterState GetState() => _state;
    public CharacterState GetLastState() => _lastState;

    public void setPosition(Vector3 position, bool killVelocity = true)
    {
        motor.SetPosition(position);
        if (killVelocity) motor.BaseVelocity = Vector3.zero;
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    public bool IsColliderValidForCollisions(Collider coll) => true;
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    public Transform GetRoot() { return root; }
    #endregion

    #region Debug UI
    private void OnGUI()
    {
        // int crosshairSize = 20;
        // int crosshairThickness = 2;
        // Color crosshairColor = Color.white;

        // var centerX = Screen.width / 2f;
        // var centerY = Screen.height / 2f;

        // GUI.color = crosshairColor;
        // GUI.DrawTexture(new Rect(centerX - crosshairSize / 2f, centerY - crosshairThickness / 2f, crosshairSize, crosshairThickness), Texture2D.whiteTexture);
        // GUI.DrawTexture(new Rect(centerX - crosshairThickness / 2f, centerY - crosshairSize / 2f, crosshairThickness, crosshairSize), Texture2D.whiteTexture);
        // GUI.color = Color.white;

        // var speedText = $"Speed: {_state.Velocity.magnitude:F1} u/s\nStance: {_state.Stance}\nGrounded: {_state.Grounded} \nSlick:{slick}";
        // var style = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperCenter };
        // var textSize = style.CalcSize(new GUIContent(speedText));
        // var speedText = $"Speed: {(_state.Velocity - (Vector3.up * _state.Velocity.y)).magnitude:F1} u/s\nStance: {_state.Stance}\nGrounded: {_state.Grounded}";
        // var style = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperCenter };
        // var textSize = style.CalcSize(new GUIContent(speedText));

        // var rect = new Rect(centerX - textSize.x / 2f, centerY + crosshairSize / 2f + 15f, textSize.x + 20f, textSize.y + 10f);
        // GUI.color = new Color(0, 0, 0, 0.5f);
        // GUI.Box(rect, "");

        // style.normal.textColor = Color.black;
        // GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), speedText, style);
        // GUI.color = Color.white;
        // style.normal.textColor = Color.white;
        // GUI.Label(rect, speedText, style);
    }
    #endregion
}