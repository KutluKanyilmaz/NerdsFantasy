using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class TurretController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Transform that the Cinemachine Camera LOOKS at.")]
        public Transform aimTarget; 
        
        [Tooltip("The physical root of the player model.")]
        public Transform characterRoot;

        [Header("Tank Settings (A/D)")]
        public float rotationSpeed = 100f;

        [Header("Turret Settings (Mouse)")]
        public float mouseSensitivity = 1.0f;
        public float aimDistance = 10f;
        public float maxYawAngle = 45f;   // Horizontal constraint
        public float maxPitchAngle = 30f; // Vertical constraint

        // Internal State
        Vector2 _moveInput;
        Vector2 _lookInput;
        float _currentYaw;
        float _currentPitch;

        // Input System Messages (Send Messages workflow)
        public void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();
        public void OnLook(InputValue value) => _lookInput = value.Get<Vector2>();

        void Update()
        {
            HandleChassisRotation();
            HandleTurretAim();
        }

        void HandleChassisRotation()
        {
            // A and D (Horizontal input) rotate the entire character root
            if (Mathf.Abs(_moveInput.x) > 0.01f)
            {
                float turnAmount = _moveInput.x * rotationSpeed * Time.deltaTime;
                characterRoot.Rotate(Vector3.up, turnAmount);
            }
        }

        void HandleTurretAim()
        {
            if (aimTarget == null) return;

            // 1. Accumulate Mouse Input
            _currentYaw += _lookInput.x * mouseSensitivity * 0.1f; // Scale down raw delta
            _currentPitch -= _lookInput.y * mouseSensitivity * 0.1f;

            // 2. Clamp the aiming angles relative to the chassis
            // This prevents the camera from spinning 360 independently
            _currentYaw = Mathf.Clamp(_currentYaw, -maxYawAngle, maxYawAngle);
            _currentPitch = Mathf.Clamp(_currentPitch, -maxPitchAngle, maxPitchAngle);

            // 3. Calculate the local rotation for the "Leash"
            Quaternion localRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);

            // 4. Apply this rotation relative to the Character's forward direction
            // We project the point out forward based on the combined rotation
            Vector3 aimDirection = (characterRoot.rotation * localRotation) * Vector3.forward;
            
            // 5. Position the AimTarget object
            // We place it 'aimDistance' units away from the character head/origin
            aimTarget.position = characterRoot.position + (Vector3.up * 1.5f) + (aimDirection * aimDistance);
        }
    }
}