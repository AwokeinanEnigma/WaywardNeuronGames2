#region

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

#endregion

namespace Enigmaware.Player
{
    public class CameraController : MonoBehaviour
    {
        [FormerlySerializedAs("eulerRotation")] [HideInInspector] public Vector3 EulerRotation;

        [FormerlySerializedAs("rotationOffset")] public Vector3 RotationOffset;
        [FormerlySerializedAs("targetRotationOffset")] public Vector3 TargetRotationOffset;
        [FormerlySerializedAs("playerCamera")] public Camera PlayerCamera;
        [FormerlySerializedAs("mouseSensitivity")] public float MouseSensitivity;
        
        [FormerlySerializedAs("lockCursor")] [SerializeField] private bool _lockCursor;
        [FormerlySerializedAs("dynamicCamera")] [SerializeField] private bool _dynamicCamera;
        [FormerlySerializedAs("dynamicFOV")] [SerializeField] private bool _dynamicFOV;
        [FormerlySerializedAs("targetFOV")] [SerializeField] private float _targetFOV;
        
        public float FOVChangeSmoothness;

        [Header("Framing")] 
        public Vector2 FollowPointFraming = new(0f, 0f);

        [Header("Rotation")] public bool InvertX;
        public bool InvertY;
        [Range(-90f, 90f)] public float DefaultVerticalAngle = 20f;
        [Range(-90f, 90f)] public float MinVerticalAngle = -90f;
        [Range(-90f, 90f)] public float MaxVerticalAngle = 90f;

        public float RotationSpeed = 1f;
        public float RotationSharpness = 10000f;
        [FormerlySerializedAs("Followtransform")] public Transform FollowTransform;

        private float _time;

        private float _targetVerticalAngle;
        private Vector2 _input;
        private readonly Vector3 _plane = new(1, 0, 1);

        #region  Properties

        public Vector3 Up
        {
            get => PlayerCamera.transform.up;
            set => PlayerCamera.transform.up = value;
        }

        public Vector3 Position
        {
            get => PlayerCamera.transform.position;
            set => PlayerCamera.transform.position = value;
        }

        public Vector3 PlanarForward => Vector3.Scale(_plane, PlayerCamera.transform.forward).normalized;

        public Vector3 PlanarRight => -Vector3.Cross(PlanarForward, Vector3.up).normalized;

        public Vector3 Forward
        {
            get => PlayerCamera.transform.forward;
            set => PlayerCamera.transform.forward = value;
        }

        public Vector3 Right
        {
            get => PlayerCamera.transform.right;
            set => PlayerCamera.transform.right = value;
        }

        public Quaternion Rotation
        {
            get => PlayerCamera.transform.rotation;
            set => PlayerCamera.transform.rotation = value;
        }

        public Vector3 PlanarDirection { get; set; }

        #endregion

        [Header("Headbobbing")]
        private float _headBobTimer;
        
        public float HeadbobFrequency;
        public float HeadbobAmplitude;
        public bool Headbob;

        #region Distance

        [Header("Distance")] public float DefaultDistance = 6f;
        public float MinDistance = 0;
        public float MaxDistance = 0;
        public float DistanceMovementSharpness = 10f;
        public float FollowingSharpness = 10000f;

        public float TargetDistance;
        private float _currentDistance;
        private Vector3 _currentFollowPosition;

        #endregion

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            
            _targetVerticalAngle = 0f;
            
            _currentDistance = DefaultDistance;
            TargetDistance = _currentDistance;
            
            PlanarDirection = Vector3.forward;
        }

        public void Update()
        {
            // Calculate deltaTime
            float deltaTime = UnityEngine.Time.deltaTime;

            // Get the input vector and invert if required
            Vector3 rotationInput = new(_input.x, _input.y, 0f);
            if (InvertX) rotationInput.x *= -1f;
            if (InvertY) rotationInput.y *= -1f;

            // Calculate planar rotation based on input
            var up = FollowTransform.up;
            Quaternion rotationFromInput = Quaternion.Euler(up * (rotationInput.x * RotationSpeed));
            PlanarDirection = rotationFromInput * PlanarDirection;
            PlanarDirection = Vector3.Cross(up, Vector3.Cross(PlanarDirection, up));
            Quaternion planarRot = Quaternion.LookRotation(PlanarDirection, up);

            // Calculate vertical rotation based on input and clamp to min and max angles
            _targetVerticalAngle -= rotationInput.y * RotationSpeed;
            _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
            Quaternion verticalRot = Quaternion.Euler(_targetVerticalAngle, 0, 0);

            // Smoothly interpolate between the planar and vertical rotations
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, planarRot * verticalRot,
                1f - Mathf.Exp(-RotationSharpness * deltaTime));

            // Update the euler rotation from the target rotation
            EulerRotation = targetRotation.eulerAngles;

            // Smoothly interpolate the field of view if it's changing
            if (!Mathf.Approximately(_targetFOV, PlayerCamera.fieldOfView))
            {
                PlayerCamera.fieldOfView =
                    Mathf.Lerp(PlayerCamera.fieldOfView, _targetFOV, FOVChangeSmoothness * deltaTime);
                PlayerCamera.fieldOfView = Mathf.Round(PlayerCamera.fieldOfView * 100) / 100;
            }

            // Apply the euler rotation to the camera transform
            PlayerCamera.transform.eulerAngles = EulerRotation;

            // Update the target distance based on movement speed and clamp to min and max distances
            TargetDistance = Mathf.Clamp(TargetDistance, MinDistance, MaxDistance);

            // Find the smoothed follow position based on following sharpness
            _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, FollowTransform.position,
                1f - Mathf.Exp(-FollowingSharpness * deltaTime));

            // Smoothly interpolate the current distance based on distance movement sharpness
            _currentDistance = Mathf.Lerp(_currentDistance, TargetDistance,
                1 - Mathf.Exp(-DistanceMovementSharpness * deltaTime));

            // Calculate the target position based on the follow position, target rotation, and current distance
            Vector3 targetPosition = _currentFollowPosition - targetRotation * Vector3.forward * _currentDistance;

            // Add framing adjustments to the target position
            targetPosition += transform.right * FollowPointFraming.x;
            targetPosition += transform.up * FollowPointFraming.y;

            if (_dynamicCamera && Headbob)  
            {
                _headBobTimer += Time.deltaTime;
                targetPosition += FollowTransform.up * (Mathf.Sin(_headBobTimer * HeadbobFrequency) * HeadbobAmplitude);
            }
            else 
            {
                _headBobTimer = 0;
            }
            
            // Apply the target position to the camera transform
            transform.position = targetPosition;

            // Many times I've almost erased this comment
            // Let it be known, that this comment is eternally immortalized in the code
            // You will never be able to erase this comment, not as long as I live, fucker.
            // Finuyuiiiiiiiiiiiiiiiiiiiiiiiiiuuuuuuuuuuukoi8uuuuuuuuuu u uuu uu u u u u u u u u u u u u ud the smoothed camera orbit position
        }

        private void OnValidate()
        {
            DefaultVerticalAngle = Mathf.Clamp(DefaultVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
        }

        public void SetFOV(float newFOV)
        {
            if (!_dynamicFOV) return;
            _targetFOV = newFOV;
        }

        public void Shake(float speed, float strength)
        {
            if (!_dynamicCamera) return;
            _time += UnityEngine.Time.deltaTime * speed;
            EulerRotation.y = 0;
            EulerRotation += new Vector3(Mathf.PerlinNoise(_time, 0) - 0.5f, Mathf.PerlinNoise(0, _time) - 0.5f, 0) *
                             strength * Time.deltaTime * 100;
            PlayerCamera.transform.eulerAngles += new Vector3(0, EulerRotation.y, 0);
        }

        public void Look(InputAction.CallbackContext context)
        {
            if (Cursor.lockState != CursorLockMode.Locked ||_lockCursor)
            {
                _input = Vector2.zero;
                return;
            }

            _input = context.ReadValue<Vector2>();
        }
    }
}

// Path: Assets\DE_SNR\Scripts\PlayerController.cs