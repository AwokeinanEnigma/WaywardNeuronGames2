#region

using System;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Enigmaware.Core;
using Enigmaware.Movement.Gravity;
using Enigmaware.World;

#endregion

namespace Enigmaware.Player
{
    public class PlayerMotor : MonoBehaviour, ICharacterController
    {
        #region Enums

        public enum MotorState
        {
            Default,
            Planet
        }

        public enum MovementType
        {
            // On the ground movement
            Ground,

            // In the air movement!
            Air,

            // Like sliding, but less cool.
            Crouching,

            // Grabbing a ledge
            Mantling,

            // For when the velocity of the character is set manually by an outside component (like a vehicle)
            Deferred
        }

        public enum BonusOrientationMethod
        {
            None,
            TowardsGravity,
            TowardsGroundSlopeAndGravity
        }

        #endregion

        #region Movement Types

        public void ForceMovementType(MovementType type)
        {
            _lastMovementType = _currentMovementType;
            _currentMovementType = type;
        }

        #endregion

        #region Fields

        public Vector3 RootMotion;

        public Vector3 FootPosition
        {
            get
            {
                Vector3 position = transform.position;
                position.y -= Motor.Capsule.height * 0.5f;

                return position;
            }
        }

        #region Components

        [Header("Components")] 
        public KinematicCharacterMotor Motor;
        [SerializeField] 
        private CameraController _cameraController;
        public Transform MeshRoot;

        #endregion

        #region Ground Movement

        private float _cosineMaxSlopeAngle;

        [Header("Planet Movement")] public float PlanetGroundMovementSpeed = 10f;
        public float PlanetMovementSharpness = 15f;

        [Header("Ground Movement")] public float GroundMovementSpeed = 15f;
        public float GroundMovementAcceleration = 200f;
        public float MaxSlopeAngle = 50f;

        [Header("Crouching")] public float CrouchedCapsuleHeight = 1f;
        public float CrouchSpeed;
        public float CrouchDrag;
        public float CrouchAcceleration;
        public float CameraCrouchSpeed;

        private bool _shouldBeCrouching;
        private bool _isCrouching;

        public bool IsGrounded => Motor.GroundingStatus.IsStableOnGround;

        /// <summary>
        ///     Used for the default state for when we're not on a planet
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="vel"></param>
        /// <param name="accel"></param>
        /// <param name="cap"></param>
        /// <returns></returns>
        private Vector3 GroundMove(Vector3 dir, Vector3 vel, float accel, float cap)
        {
            vel = ApplyFriction(vel, _currentDrag);
            return Accelerate(vel, dir, accel, cap); //Accelerate(vel, dir, accel, cap);
        }

        /// <summary>
        ///     Used for when we're on a planet
        /// </summary>
        /// <param name="currentVelocity"></param>
        private void GroundMove(ref Vector3 currentVelocity)
        {
            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(WishDirection, Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized *
                                      WishDirection.magnitude;
            Vector3 targetMovementVelocity = reorientedInput * PlanetGroundMovementSpeed;

            // Smooth movement Velocity

            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                1f - Mathf.Exp(-PlanetMovementSharpness * Time.deltaTime));
        }

        #endregion

        #region Air Movement

        [Header("Air Movement")]
        [Tooltip("This is how fast the character can move in the air. It is not a force, but a maximum speed limit.")]
        public float AirMovementSpeed = 15f;

        public float AirAcceleration = 90f;

        private void AirMove(ref Vector3 currentVelocity)
        {
            float deltaTime = Time.deltaTime;
            if (WishDirection.sqrMagnitude > 0f)
            {
                Vector3 addedVelocity = WishDirection * (AirAcceleration * deltaTime);

                Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

                // Limit air velocity from inputs
                if (currentVelocityOnInputsPlane.magnitude < AirMovementSpeed)
                {
                    // clamp addedVel to make total vel not exceed max vel on inputs plane
                    Vector3 newTotal =
                        Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, AirMovementSpeed);
                    addedVelocity = newTotal - currentVelocityOnInputsPlane;
                }
                else
                {
                    // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                    if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                }

                // Prevent air-climbing sloped walls
                if (Motor.GroundingStatus.FoundAnyGround)
                    if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                    {
                        Vector3 perpenticularObstructionNormal = Vector3
                            .Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal),
                                Motor.CharacterUp).normalized;
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                    }

                // Apply added velocity
                currentVelocity += addedVelocity;
            }
        }

        #endregion

        #region Jump

        [Header("Jumping")] public float JumpHeight;
        private bool _canJump;

        private void Jump()
        {
            if (_jumpInput.Down)
            {
                if (_canJump && !_isCrouching)
                {
                    // Calculate jump direction before ungrounding
                    Vector3 jumpDirection = Motor.CharacterUp;
                    if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
                        jumpDirection = Motor.GroundingStatus.GroundNormal;

                    // Makes the character skip ground probing/snapping on its next update. 
                    // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                    Motor.ForceUnground();
                    _currentDrag = 0;

                    // Add to the return velocity and reset jump state
                    //_velocity.y = 1; 
                    AddVelocity(jumpDirection * JumpHeight - Vector3.Project(_velocity, Motor.CharacterUp));
                }
            }
        }

        #endregion

        #region Mantling

        [Header("Mantling & Ledge Grab")] public LedgeDetectionSettings LedgeDetectionSettings;

        [SerializeField] private bool _mantling;
        private bool _mantleExhaust;
        [SerializeField] private Ledge _ledge;

        public float MantleClimbSpeed;
        public float MantleBoost;
        public float MantleDistance;
        public float EndMantleMove;

        public Interval LedgeLookInterval;

        #endregion

        #region Gravity

        [Header("Gravity")] public Vector3 CurrentGravity = new(0, -30f, 0);
        public Vector3 LastGravity = new(0, -30f, 0);
        public Vector3 Gravity = new(0, -30f, 0);

        private void ApplyGravity()
        {
            float deltaTime = Time.deltaTime;

            LastGravity = CurrentGravity;
            CurrentGravity = Gravity;

            Vector3 position = transform.position;
            position.y -= Motor.Capsule.height * 0.5f;

            Vector3 nearestPlanetGravity = GravityFinder.GetGravity(position);
            if (nearestPlanetGravity != Vector3.zero)
            {
                CurrentGravity = nearestPlanetGravity;
                // this is for spheres
                CurrentBonusOrientationMethod = BonusOrientationMethod.TowardsGroundSlopeAndGravity;
                if (CurrentCharacterState != MotorState.Planet)
                {
                    TransitionToState(MotorState.Planet);
                }
            }
            else
            {
                CurrentGravity = Gravity;
                if (CurrentCharacterState != MotorState.Default)
                {
                    CurrentBonusOrientationMethod = BonusOrientationMethod.TowardsGravity;
                    TransitionToState(MotorState.Default);
                }
            }

            // Still calculate gravity, but return before applying it to avoid the velocity from being affected
            if (!(IsGrounded || _mantling)) 
            {
                _velocity += CurrentGravity * deltaTime;
            }
        }

        #endregion

        #region Velocity Modification Methods

        private Vector3 _internalVelocityAdd = Vector3.zero;

        private void ApplyVelocityAdd()
        {
            _velocity += _internalVelocityAdd;
            _internalVelocityAdd = Vector3.zero;
        }

        private Vector3 _internalVelocitySet = Vector3.zero;

        private void ApplyVelocitySet()
        {
            _velocity = _internalVelocitySet;
            _internalVelocitySet = Vector3.zero;
        }

        public void AddVelocity(Vector3 velocity)
        {
            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                {
                    _internalVelocityAdd += velocity;
                    break;
                }
                case MotorState.Planet:
                {
                    _internalVelocityAdd += velocity;
                    break;
                }
            }
        }

        private void ForceVelocity(Vector3 velocity)
        {
            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                {
                    _velocity = velocity;
                    break;
                }
                case MotorState.Planet:
                {
                    _velocity = velocity;
                    break;
                }
            }
        }

        #endregion

        #region Surface Info

        [Header("Surface info")] public SurfaceInfo DefaultSurface;
        [SerializeField] private SurfaceInfo _currentSurface;
        private Collider _currentSurfaceCollider;

        #endregion

        #region State

        [Header("Exposed Innerworkings")] [SerializeField]
        public MotorState CurrentCharacterState;

        [SerializeField] public MotorState LastCharacterState;

        // { get; private set; }

        public MovementType CurrentMovementType => _currentMovementType;

        [FormerlySerializedAs("currentMovementType")] [SerializeField]
        private MovementType _currentMovementType;

        [SerializeField] private MovementType _lastMovementType;

        #endregion

        #region Velocity

        [SerializeField] private Vector3 _velocity;

        public Vector3 Velocity
        {
            get => _velocity;
            set => ForceVelocity(value);
        }

        #endregion

        #region Misc

        [Header("Misc")] public List<Collider> IgnoredColliders = new();

        public BonusOrientationMethod CurrentBonusOrientationMethod = BonusOrientationMethod.None;
        public float BonusOrientationSharpness = 10f;

        private Vector3 _meshTargetScale = Vector3.one;

        private readonly Collider[] _genericColliderCheck = new Collider[8];

        // sweephits for the beforecharacterupdate code
        private RaycastHit _sweepHit;
        private RaycastHit[] _sweepHits = new RaycastHit[32];

        // normals
        private Vector3 _currentNormal;
        private Vector3 _lateNormal;

        public Vector3 Up => transform.rotation * Vector3.up;
        public Vector3 Down => transform.rotation * Vector3.down;
        public Vector3 Left => transform.rotation * Vector3.left;
        public Vector3 Right => transform.rotation * Vector3.right;

        private bool _isVelocityCancelled;
        private bool _lateVelocityCancel;

        #endregion

        #region Mystery Variables

        private readonly Vector3 _plane = new(1, 0, 1);
        
        [FormerlySerializedAs("adhesion")] 
        public float Adhesion = 0.1f;

        #endregion

        #region Drag

        // if you truly loved me, why'd you train me to fight?
        // if it wasn't in my blood, what do you see?

        private float _currentDrag;

        #endregion

        [FormerlySerializedAs("WallSeekDistance")] public float LedgeSeekDistance;
        public float Inflation;

        #region Input Fields

        /// <summary>
        ///     The actual, curated direction the player wants to move in
        /// </summary>
        public Vector3 WishDirection { get; private set; }

        /// <summary>
        ///     The raw direction the player wants to move in
        /// </summary>
        private Vector2 _rawDirectionalMove;

        // create these first
        private readonly InputEventData _jumpInput = new();
        private readonly InputEventData _crouchInput = new();

        #endregion

        #endregion

        #region Unity Methods

        private bool _originalHeadbobValue;
        
        private void Awake()
        {
            Motor.CharacterController = this;

            _originalHeadbobValue = _cameraController.Headbob;
            
            _cosineMaxSlopeAngle = Mathf.Cos(MaxSlopeAngle * Mathf.Deg2Rad);
            _currentSurface = DefaultSurface;

            // Handle initial state
            TransitionToState(MotorState.Default);
            // Assign the characterController to the motor
        }

        public void Update()
        {
            //Debug.Log(Vector3.Dot(Vector3.Scale(plane, _velocity),
            //   Vector3.Scale(WishDirection, plane).normalized));

            // create wish direction
            WishDirection = GetWishDirection();
            //if (Input.GetKey(KeyCode.C)) velocity += _lookInputVector * Time.deltaTime * 50;
            _canJump = IsGrounded;

            CheckInput();

            if (_isCrouching)
            {
                _meshTargetScale = new Vector3(1, 0.3f, 1);
            }

            MeshRoot.localScale =
                Vector3.Lerp(MeshRoot.localScale, _meshTargetScale, Time.deltaTime * CameraCrouchSpeed);

            // [i had a funny joke but this code is self-explainatory]
            _lastMovementType = _currentMovementType;
            DetermineMovementType();
        }

        public void FixedUpdate()
        {
            Debug.Log(_rawDirectionalMove.magnitude);
            if (_rawDirectionalMove.magnitude <= 0 || !IsGrounded)
            {
                _cameraController.Headbob = false;
            }
            else if (_rawDirectionalMove.magnitude > 0 && IsGrounded)
            {
                _cameraController.Headbob = _originalHeadbobValue;
            }
            
            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                {
                    switch (_currentMovementType)
                    {
                        case MovementType.Ground:
                            _currentDrag = 0.3f * _currentSurface.Drag;

                            //_velocity = GroundMove(WishDirection, _velocity, GroundMovementAcceleration,
                            //    GroundMovementSpeed);
                            GroundMove(ref _velocity);
                            break;
                        case MovementType.Air:
                            //AirMove(ref _velocity);
                            AirMove(ref _velocity);
                            break;
                        case MovementType.Crouching:
                            //_velocity = GroundMove(WishDirection, _velocity, CrouchAcceleration, CrouchSpeed);
                            GroundMove(ref _velocity);
                            _currentDrag = CrouchDrag;
                            break;
                        case MovementType.Mantling:
                        {
                            //_velocity = new Vector3(0, _velocity.y, 0);
                            _velocity.y += MantleClimbSpeed * Time.fixedDeltaTime;
                            
                            double footHeight = Math.Round(base.transform.position.y, 3);
                            double ledgeHeight = Math.Round(_ledge.MidPoint.y, 3);
                            Debug.Log($"Moving up. Foot position: " + footHeight + ", Ledge position: " + ledgeHeight + $", Climb speed { MantleClimbSpeed * Time.fixedDeltaTime}");
                            if (footHeight >ledgeHeight && _ledge.IsValid)
                            {
                                Debug.Log("Cleared.");
                                _velocity.y = 2f;
                                _ledge = default;
                                _mantling = false;
                                _velocity += -_currentNormal * EndMantleMove;

                            }
                            break;
                        }
                        case MovementType.Deferred:
                            // do nothing,
                            // because velocity is being handled by an outside source
                            break;
                    }

                    // apply gravity out of switch statement
                    ApplyGravity();
                    break;
                }
                case MotorState.Planet:
                    switch (_currentMovementType)
                    {
                        case MovementType.Ground:
                            GroundMove(ref _velocity);
                            break;
                        case MovementType.Air:
                            AirMove(ref _velocity);
                            break;
                        case MovementType.Crouching:
                            _velocity = GroundMove(WishDirection, _velocity, CrouchAcceleration, CrouchSpeed);
                            _currentDrag = CrouchDrag * _currentSurface.Drag;
                            ;
                            break;
                        case MovementType.Deferred:
                            // do nothing,
                            // because velocity is being handled by an outside source
                            break;
                    }

                    ApplyGravity();
                    // Take into account additive velocity
                    break;
            }

            if (_internalVelocityAdd.sqrMagnitude > 0f)
            {
                ApplyVelocityAdd();
            }

            if (_internalVelocitySet.sqrMagnitude > 0f)
            {
                ApplyVelocitySet();
            }
        }

        public void LateUpdate()
        {
            _jumpInput.RefreshKeyData();
            _crouchInput.RefreshKeyData();
        }

        private void DetermineMovementType()
        {
            if (_currentMovementType == MovementType.Deferred)
            {
                return;
            }

            if (_mantling)
            {
                _currentMovementType = MovementType.Mantling;
                return;
            }

            if (IsGrounded)
            {
                _currentMovementType = MovementType.Ground;
                if (_isCrouching)
                {
                    _currentMovementType = MovementType.Crouching;
                }
            }
            else
            {
                _currentMovementType = MovementType.Air;
            }
        }

        #endregion

        #region Character States

        /// <summary>
        ///     Handles movement state transitions and enter/exit callbacks
        /// </summary>
        private void TransitionToState(MotorState newState)
        {
            MotorState tmpInitialState = CurrentCharacterState;
            OnStateExit(tmpInitialState, newState);
            LastCharacterState = CurrentCharacterState;
            CurrentCharacterState = newState;
            OnStateEnter(newState, tmpInitialState);
        }

        /// <summary>
        ///     Event when entering a state
        /// </summary>
        private void OnStateEnter(MotorState state, MotorState fromState)
        {
        }

        /// <summary>
        ///     Event when exiting a state
        /// </summary>
        private void OnStateExit(MotorState state, MotorState toState)
        {
        }

        #endregion

        #region Input

        private Vector3 GetWishDirection()
        {
            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                    if (!IsGrounded || _currentNormal.y < _cosineMaxSlopeAngle)
                        return (_rawDirectionalMove.x * _cameraController.PlanarRight +
                                _rawDirectionalMove.y * _cameraController.PlanarForward).normalized;
                    return Vector3
                        .Cross(
                            (_rawDirectionalMove.x * -_cameraController.PlanarForward +
                             _rawDirectionalMove.y * _cameraController.PlanarRight).normalized,
                            _currentNormal).normalized;

                case MotorState.Planet:

                    Vector3 moveInputVector =
                        Vector3.ClampMagnitude(new Vector3(_rawDirectionalMove.x, 0f, _rawDirectionalMove.y), 1f);
                    Vector3 cameraPlanarDirection = Vector3
                        .ProjectOnPlane(_cameraController.Rotation * Vector3.forward, Motor.CharacterUp).normalized;
                    if (cameraPlanarDirection.sqrMagnitude == 0f)
                        cameraPlanarDirection = Vector3
                            .ProjectOnPlane(_cameraController.Rotation * Vector3.up, Motor.CharacterUp).normalized;
                    Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);
                    return cameraPlanarRotation * moveInputVector;
            }

            return Vector3.zero;
        }

        // These methods are called by Unity
        // Don't mess with them unless you know what you're doing!
        public void ReadMovement(InputAction.CallbackContext context)
        {
            // swagilicous
            _rawDirectionalMove = context.ReadValue<Vector2>();
        }

        public void ReadJump(InputAction.CallbackContext context)
        {
            _jumpInput.UpdateKeyState(context);
        }

        public void ReadCrouch(InputAction.CallbackContext context)
        {
            _crouchInput.UpdateKeyState(context);
        }

        private void CheckInput()
        {
            Jump();
            if (_crouchInput.Pressed)
            {
                /*StreamWriter streamWriter = new StreamWriter("wallrun.log");
                //_wallrunInfos.ForEach(x => streamWriter.WriteLine(x));
                streamWriter.Close();*/

                _shouldBeCrouching = true;

                // TODO: Don't do this. This is stupid.
                if (!_isCrouching)
                {
                    _isCrouching = true;
                    Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                }
            }
            else
            {
                _shouldBeCrouching = false;
                _meshTargetScale = Vector3.one;
            }
        }

        #endregion

        #region Physics!

        /// <summary>
        ///     Does source-like acceleration
        /// </summary>
        /// <param name="currentVelocity">The current velocity</param>
        /// <param name="wishDirection">The direction the player wants to move in.</param>
        /// <param name="accelerationRate">The rate of acceleration.</param>
        /// <param name="accelerationLimit">The limit of acceleration.</param>
        /// <returns>Accelerated velocity</returns>
        private Vector3 Accelerate(Vector3 currentVelocity, Vector3 wishDirection, float accelerationRate,
            float accelerationLimit)
        {
            float speed = Vector3.Dot(Vector3.Scale(_plane, currentVelocity),
                Vector3.Scale(wishDirection, _plane).normalized);
            float speedGain = accelerationRate * Time.deltaTime;

            if (speed + speedGain > accelerationLimit)
                speedGain = Mathf.Clamp(accelerationLimit - speed, 0, accelerationLimit);

            return currentVelocity + wishDirection * speedGain;
        }

        /// <summary>
        ///     Does source-like acceleration
        /// </summary>
        /// <param name="currentVelocity">The current velocity</param>
        /// <param name="wishDirection">The direction the player wants to move in.</param>
        /// <param name="accelerationRate">The rate of acceleration.</param>
        /// <returns>Accelerated velocity</returns>
        private Vector3 AccelerateUnlimited(Vector3 currentVelocity, Vector3 wishDirection, float accelerationRate)
        {
            float speedGain = accelerationRate * Time.deltaTime;
            return currentVelocity + wishDirection * speedGain;
        }


        private Vector3 PlanarAccelerate(Vector3 currentVelocity, Vector3 wishDirection, float accelerationRate,
            float accelerationLimit)
        {
            float speed = Vector3.Dot(Vector3.Scale(Motor.GroundingStatus.GroundNormal, currentVelocity),
                Vector3.Scale(wishDirection, Motor.GroundingStatus.GroundNormal).normalized);
            float speedGain = accelerationRate * Time.deltaTime;

            if (speed + speedGain > accelerationLimit)
                speedGain = Mathf.Clamp(accelerationLimit - speed, 0, accelerationLimit);

            return currentVelocity + wishDirection * speedGain;
        }

        /// <summary>
        ///     Applies friction
        /// </summary>
        /// <param name="currentVelocity">The velocity to apply the friction to</param>
        /// <param name="friction">The amount of friction to apply</param>
        /// <returns></returns>
        public Vector3 ApplyFriction(Vector3 currentVelocity, float friction)
        {
            return currentVelocity * (1 / (friction + 1));
        }

        #endregion

        #region General Movement

        /// <summary>
        ///     When the player hits something, this method is called to calculate the velocity the player should have upon impact.
        /// </summary>
        private void CalculateOnHitVelocity(Vector3 currentNormalA)
        {
            bool fall = false;
            if (_lastMovementType == MovementType.Air && currentNormalA.y > 0.706f) fall = true;
            if (!fall)
            {
                float momentum = Vector3.Dot(_currentNormal, _velocity);
                _velocity -= currentNormalA * momentum;
                _velocity -= currentNormalA * Adhesion;
            }
            else
            {
                Vector3 startVel = _velocity;
                float momentum = Vector3.Dot(Motor.CharacterUp, _velocity);
                _velocity -= Motor.CharacterUp * momentum;

                Vector3 dir = Vector3.zero;
                dir = Vector3.Cross(new Vector3(_velocity.z, 0, -_velocity.x).normalized, currentNormalA);
                _velocity = dir * _velocity.magnitude;

                if (Vector3.Dot(_velocity, currentNormalA) > 0.1f)
                {
                    _velocity = startVel;
                    momentum = Vector3.Dot(currentNormalA, _velocity);
                    _velocity -= currentNormalA * momentum;
                }
            }
        }

        #endregion

        #region Built in Kinematic Character Motor methods

        /// <summary>
        ///     (Called by KinematicCharacterMotor during its update cycle)
        ///     This is where you tell your character what its rotation should be right now.
        ///     This is the ONLY place where you should set the character's rotation
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                {
                    if (CurrentBonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
                    {
                        // Rotate from current up to invert gravity
                        Vector3 smoothedGravityDir = Vector3.Slerp(Up, -CurrentGravity.normalized,
                            1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(Up, smoothedGravityDir) * currentRotation;
                    }
                    else if (CurrentBonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
                    {
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {

                            Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp,
                                Motor.GroundingStatus.GroundNormal,
                                1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(Up, smoothedGroundNormal) *
                                              currentRotation;


                            /* Move the position to create a rotation around the bottom hemi center instead of around the pivot
                            Motor.SetTransientPosition(initialCharacterBottomHemiCenter +
                                                       (currentRotation * Vector3.down * Motor.Capsule.radius));*/
                        }
                        else
                        {
                            Vector3 smoothedGravityDir = Vector3.Slerp(Up, -CurrentGravity.normalized,
                                1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            currentRotation = Quaternion.FromToRotation(Up, smoothedGravityDir) *
                                              currentRotation;
                        }
                    }
                    else
                    {
                        Vector3 smoothedGravityDir = Vector3.Slerp(Up, Vector3.up,
                            1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(Up, smoothedGravityDir) * currentRotation;
                    }

                    break;
                }
                case MotorState.Planet:

                    Vector3 currentUpP = currentRotation * Vector3.up;

                    if (CurrentBonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
                    {
                        // Rotate from current up to invert gravity
                        Vector3 smoothedGravityDir = Vector3.Slerp(currentUpP, -CurrentGravity.normalized,
                            1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(currentUpP, smoothedGravityDir) * currentRotation;
                    }
                    else if (CurrentBonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
                    {
                        Quaternion rotation = Quaternion.identity;
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            Vector3 initialCharacterBottomHemiCenter =
                                Motor.TransientPosition + currentUpP * Motor.Capsule.radius;


                            Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp,
                                Motor.GroundingStatus.GroundNormal,
                                1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                            rotation = Quaternion.FromToRotation(currentUpP, smoothedGroundNormal) *
                                       currentRotation;

                            /* Move the position to create a rotation around the bottom hemi center instead of around the pivot
                            Motor.SetTransientPosition(initialCharacterBottomHemiCenter +
                                                       (currentRotation * Vector3.down * Motor.Capsule.radius));*/
                        }
                        else
                        {
                            Vector3 smoothedGravityDir = Vector3.Slerp(currentUpP, -CurrentGravity.normalized,
                                1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));

                            rotation = Quaternion.FromToRotation(currentUpP, smoothedGravityDir) *
                                       currentRotation;
                        }

                        currentRotation = rotation;
                    }
                    else
                    {
                        Vector3 smoothedGravityDir = Vector3.Slerp(currentUpP, Vector3.up,
                            1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
                        currentRotation = Quaternion.FromToRotation(currentUpP, smoothedGravityDir) * currentRotation;
                    }

                    break;
            }
        }


        /// <summary>
        ///     (Called by KinematicCharacterMotor during its update cycle)
        ///     This is where you tell your character what its velocity should be right now.
        ///     This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // Here, we're just going to set the velocity to be the desired velocity we keep in this character motor
            // For two reasons:
            // 1) We want to be able to control the character's velocity in any way we want
            // 2) I am not shoving all of my movement code here. Fuck you.
            currentVelocity = _velocity;

            Vector3 currentUpD = Motor.TransientRotation * Vector3.up;
            DrawVector(Motor.TransientPosition, Motor.CharacterUp, 25, Color.red);
            DrawVector(Motor.TransientPosition, -Motor.CharacterUp, 25, Color.blue);
        }

        #endregion

        #region Character Update Methods

        /// <summary>
        ///     (Called by KinematicCharacterMotor during its update cycle)
        ///     This is called before the character begins its movement update
        ///     Called in Fixed Update
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime)
        {
            if (RootMotion != Vector3.zero)
            {
                Vector3 storage = RootMotion;
                RootMotion = Vector3.zero;
                Motor.MoveCharacter(transform.position + storage);
            }

            _lateVelocityCancel = _isVelocityCancelled;
            _isVelocityCancelled = false;

            // This is used to determine if the character is colliding with anything
            // If they aren't, wallrunning is stopped
            int hitShit = Motor.CharacterCollisionsSweep(Motor.TransientPosition, Motor.TransientRotation,
                _velocity.normalized, LedgeSeekDistance, out _sweepHit, _sweepHits, Inflation);
            DrawVector(Motor.TransientPosition, _velocity, 25, Color.green);

            if (hitShit != 0)
            {


                /*if (results != LedgeDetectionUtil.LedgeDetectionResults.FoundLedge)
                {
                    _ledge = default;
                    //_mantling = false;
                }*/
                

                // i breathe entropy
                /*if (IsGrounded)
                {
                    CalculateOnHitVelocity(_sweepHit.normal);
                }*/
            }
        }

        public void OnDrawGizmos()
        {
            LedgeDetectionUtil.TryFindLedge(new Ray(Motor.TransientPosition,
                    -_sweepHit.normal), LedgeDetectionSettings,
                out Ledge ledge, out LedgeDetectionUtil.LedgeDetectionResults results, true);
        }


        /// <summary>
        ///     (Called by KinematicCharacterMotor during its update cycle)
        ///     This is called after the character has finished its movement update.
        ///     Called in Fixed Update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
            //Debug.Log(Motor.CharacterCollisionsOverlap(base.transform.position, base.transform.rotation, hits)) ;


            switch (CurrentCharacterState)
            {
                case MotorState.Default:
                {
                    // Handle uncrouching
                    if (_isCrouching && !_shouldBeCrouching)
                    {
                        // Do an overlap test with the character's standing height to see if there are any obstructions
                        Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                        if (Motor.CharacterOverlap(
                                Motor.TransientPosition,
                                Motor.TransientRotation,
                                _genericColliderCheck,
                                Motor.CollidableLayers,
                                QueryTriggerInteraction.Ignore) > 0)
                        {
                            // If obstructions, just stick to crouching dimensions
                            Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                            _meshTargetScale = new Vector3(1, 0.3f, 1);
                        }
                        else
                        {
                            // If no obstructions, uncrouch
                            _meshTargetScale = Vector3.one;
                            _isCrouching = false;
                        }
                    }

                    break;
                }
                case MotorState.Planet:
                    // Handle uncrouching
                    if (_isCrouching && !_shouldBeCrouching)
                    {
                        // Do an overlap test with the character's standing height to see if there are any obstructions
                        Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                        if (Motor.CharacterOverlap(
                                Motor.TransientPosition,
                                Motor.TransientRotation,
                                _genericColliderCheck,
                                Motor.CollidableLayers,
                                QueryTriggerInteraction.Ignore) > 0)
                        {
                            // If obstructions, just stick to crouching dimensions
                            Motor.SetCapsuleDimensions(0.5f, CrouchedCapsuleHeight, CrouchedCapsuleHeight * 0.5f);
                            _meshTargetScale = new Vector3(1, 0.3f, 1);
                        }
                        else
                        {
                            // If no obstructions, uncrouch
                            _meshTargetScale = Vector3.one;
                            _isCrouching = false;
                        }
                    }

                    break;
            }
        }

        #endregion

        #region Grounding & Movement Methods

        public void PostGroundingUpdate(float deltaTime)
        {
            // Handle landing and leaving ground
            if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
                OnLanded();
            else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
                OnLeaveStableGround();
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (IgnoredColliders.Count == 0) return true;

            if (IgnoredColliders.Contains(coll)) return false;

            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
        }
        


        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            if (hitCollider != _currentSurfaceCollider)
            {
                SurfaceInfo info = SurfaceInfo.FindSurfaceInfo(hitCollider);
                _currentSurface = info != null ? info : DefaultSurface;
                _currentSurfaceCollider = hitCollider;
            }

            _lateNormal = _currentNormal;
            _currentNormal = hitNormal;

            DrawVector(transform.position, Up, 200, Color.cyan);

            bool cancel = true;
            if (
                !IsGrounded && !_isCrouching || !_shouldBeCrouching)
            {


                bool ledgeFound = LedgeDetectionUtil.TryFindLedge(new Ray(Motor.TransientPosition,
                        -hitNormal), LedgeDetectionSettings,
                    out Ledge ledge, out LedgeDetectionUtil.LedgeDetectionResults results, false);

                bool canClimb = ledge.Start.y - transform.position.y < MantleDistance; // && _cameraController.transform.position.y < ledge.Start.y ;
                bool canSkip = ledge.Start.y - transform.position.y < MantleDistance / 2;
                if (
                    ledgeFound && results == LedgeDetectionUtil.LedgeDetectionResults.FoundLedge
                )
                {
                    // feed you your own entropy
                    if (canClimb && ledge.IsValid && !IsGrounded)
                    {
                        Debug.Log($"Start y: {ledge.Start.y}, transform y: {transform.position.y}, is start y less than transform y: {ledge.Start.y < transform.position.y}");
                        //new GameObject("Ledge").transform.position = _ledge.Start;
                        Debug.Log(_ledge.Start);

                        cancel = false;
                        
                        _ledge = ledge;
                        _velocity = Vector3.zero;
                        _mantling = true;
                        Debug.Log("Entering ledge grab");
                    }
                }
            }

            if (cancel)
            {
                CalculateOnHitVelocity(_currentNormal);
            }
        }


        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            // kanye west
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        protected void OnLanded()
        {
            if (CurrentCharacterState == MotorState.Default)
            {
                CurrentGravity = Gravity;
            }
        }

        protected void OnLeaveStableGround()
        {
            _isVelocityCancelled = false;
        }

        /// <summary> draws Vector gizmo, only for debugging
        ///     <summary>
        public void DrawVector(Vector3 origin, Vector3 vector, float lengthMultiplier, Color color)
        {
            Debug.DrawLine(origin, origin + vector * lengthMultiplier, color);
        }

        //private List<string> _wallrunInfos= new List<string>();

        #endregion
    }
}