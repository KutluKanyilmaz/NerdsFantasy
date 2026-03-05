using UnityEngine;
using UnityEngine.InputSystem;


namespace Player 
{
    public enum RotationType 
    {
        Keyboard,
        Mouse
    }

    public class ChairController : MonoBehaviour 
    {
        [Header("Active Data")]
        public ChairSO ChairData;
        
        [Header("Dependencies")]
        [Tooltip("Required to calculate mouse rotation overshoot based on gunner constraints.")]
        public WeaponController weapon;

        [Header("Chair Settings")]
        public RotationType rotationType = RotationType.Mouse;
        
        void Update() 
        {
            if (ChairData == null) return;
            
            HandleChairRotation();
        }
        
        public void Initialize(ChairSO data)
        {
            ChairData = data;
        }

        private void HandleChairRotation() {
            if (rotationType == RotationType.Keyboard) {
                if (Keyboard.current == null) return;

                float rotationInput = 0f;
                if (Keyboard.current.dKey.isPressed) rotationInput += 1f;
                if (Keyboard.current.aKey.isPressed) rotationInput -= 1f;

                if (rotationInput != 0f) {
                    // Keyboard just uses the raw MaxTurningSpeed
                    transform.Rotate(Vector3.up, rotationInput * ChairData.MaxTurningSpeed * Time.deltaTime, Space.Self);
                }
            }
            else if (rotationType == RotationType.Mouse) {
                if (weapon == null || weapon.humanTransform == null) return;

                Vector3 targetPosition = MouseWorld.Instance.GetPosition();
                Vector3 direction = targetPosition - weapon.humanTransform.position;

                if (direction != Vector3.zero) {
                    Quaternion desiredWorldRot = Quaternion.LookRotation(direction);
                    Quaternion desiredLocalRot = Quaternion.Inverse(transform.rotation) * desiredWorldRot;

                    float localYaw = weapon.NormalizeAngle(desiredLocalRot.eulerAngles.y);

                    if (Mathf.Abs(localYaw) > weapon.GunnerRotationRange) {
                        // 1. Calculate how far past the deadzone we are (0 to Max Overshoot)
                        float overshoot = Mathf.Abs(localYaw) - weapon.GunnerRotationRange;
                        float maxPossibleOvershoot = 180f - weapon.GunnerRotationRange;

                        // 2. Normalize that to a 0.0 - 1.0 percentage (the X axis of your curve)
                        float t = maxPossibleOvershoot > 0 ? Mathf.Clamp01(overshoot / maxPossibleOvershoot) : 0f;

                        // 3. Evaluate the curve to get the interpolation weight (the Y axis of your curve, ideally 0.0 to 1.0)
                        float curveWeight = ChairData.TurnSpeedCurve.Evaluate(t);

                        // 4. Sample between Min and Max using the curve's weight
                        float currentTurnSpeed = Mathf.Lerp(ChairData.MinTurningSpeed, ChairData.MaxTurningSpeed, curveWeight);
                        float turnDirection = Mathf.Sign(localYaw);

                        transform.Rotate(Vector3.up, turnDirection * currentTurnSpeed * Time.deltaTime, Space.Self);
                    }
                }
            }
        }
    }
}