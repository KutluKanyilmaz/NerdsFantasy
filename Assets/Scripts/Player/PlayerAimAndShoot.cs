using UnityEngine;
using UnityEngine.InputSystem;
using Shapes;
using Sirenix.OdinInspector;

namespace Player 
{
    public enum RotationType {
        Keyboard,
        Mouse
    }
    
    public class PlayerAimAndShoot : MonoBehaviour {
        [Header("References")]
        public Transform humanTransform; 
        public Transform firePoint; 
        public PooledProjectile projectilePrefab;
        
        [Header("Chair Settings")]
        [ShowIf("rotationType", RotationType.Keyboard)] 
        public float keyboardChairRotationSpeed = 15f;
        [ShowIf("rotationType", RotationType.Mouse)] 
        public float mouseChairMinRotationSpeed = 10f;
        [ShowIf("rotationType", RotationType.Mouse)] 
        public float mouseChairMaxRotationSpeed = 270f;
        
        
        public RotationType rotationType = RotationType.Keyboard;

        [Header("Gunner Aiming Settings")]
        public float gunnerRotationSpeed = 120f;
        public float gunnerRotationRange = 60f; 
        



        [Header("Shooting Settings")]
        public float fireRate = 0.5f;
        float nextFireTime = 0f;

        [Header("Projectile Settings")]
        public float projectileDamage = 10f;
        public float projectileSpeed = 20f;
        public float projectileMaxDistance = 20f;
        public LayerMask projectileHitLayers = Physics.DefaultRaycastLayers;

        [Header("Projectile Curve Control")]
        public bool useProjectileSpeedCurve = false;
        [ShowIf("useProjectileSpeedCurve")] public AnimationCurve projectileSpeedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        
        [Header("Aiming Visuals Settings")]
        public Disc playerAimRangeArc;


        void Update() 
        {
            HandleChairRotation();
            HandleAiming();
            HandleShooting();
            UpdateAimVisuals(); 
        }

        void HandleChairRotation()
        {
            if (rotationType == RotationType.Keyboard)
            {
                if (Keyboard.current == null) return;
                float rotationInput = 0f;
                if (Keyboard.current.dKey.isPressed) rotationInput += 1f; 
                if (Keyboard.current.aKey.isPressed) rotationInput -= 1f; 

                if (rotationInput != 0f)
                {
                    transform.Rotate(Vector3.up, rotationInput * keyboardChairRotationSpeed * Time.deltaTime, Space.Self);
                }
            }
            else if (rotationType == RotationType.Mouse)
            {
                if (humanTransform == null) return;
                Vector3 targetPosition = MouseWorld.Instance.GetPosition();
                Vector3 direction = targetPosition - humanTransform.position;

                if (direction != Vector3.zero)
                {
                    Quaternion desiredWorldRot = Quaternion.LookRotation(direction);
                    Quaternion desiredLocalRot = Quaternion.Inverse(transform.rotation) * desiredWorldRot;
                    float localYaw = NormalizeAngle(desiredLocalRot.eulerAngles.y);

                    if (Mathf.Abs(localYaw) > gunnerRotationRange)
                    {
                        float overshoot = Mathf.Abs(localYaw) - gunnerRotationRange;
                        float maxPossibleOvershoot = 180f - gunnerRotationRange;
                        float t = maxPossibleOvershoot > 0 ? Mathf.Clamp01(overshoot / maxPossibleOvershoot) : 0f;
                        float currentTurnSpeed = Mathf.Lerp(mouseChairMinRotationSpeed, mouseChairMaxRotationSpeed, t);
                        float turnDirection = Mathf.Sign(localYaw);
                        transform.Rotate(Vector3.up, turnDirection * currentTurnSpeed * Time.deltaTime, Space.Self);
                    }
                }
            }
        }

        void HandleAiming()
        {
            if (humanTransform == null) return;
            Vector3 targetPosition = MouseWorld.Instance.GetPosition();
            Vector3 direction = targetPosition - humanTransform.position;

            if (direction != Vector3.zero)
            {
                Quaternion desiredWorldRot = Quaternion.LookRotation(direction);
                Quaternion desiredLocalRot = Quaternion.Inverse(transform.rotation) * desiredWorldRot;
                Vector3 localEuler = desiredLocalRot.eulerAngles;
                
                float clampedY = NormalizeAngle(localEuler.y);
                clampedY = Mathf.Clamp(clampedY, -gunnerRotationRange, gunnerRotationRange);
                
                localEuler.y = clampedY;
                Quaternion targetLocalRot = Quaternion.Euler(localEuler);
                humanTransform.localRotation = Quaternion.Slerp(humanTransform.localRotation, targetLocalRot, Time.deltaTime * gunnerRotationSpeed);
            }
        }

        void HandleShooting()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        void Shoot()
        {
            if (firePoint == null || projectilePrefab == null) return;
            PooledProjectile bullet = PoolManager.Instance.Spawn(projectilePrefab, firePoint.position, firePoint.rotation);
            
            bullet.Initialize(
                projectileSpeed, 
                projectileDamage, 
                projectileMaxDistance, 
                projectileHitLayers, 
                useProjectileSpeedCurve, 
                projectileSpeedCurve
            );
        }

// --- Visuals Method ---
        void UpdateAimVisuals()
        {
            if (playerAimRangeArc == null) 
                return;

            float baseAngle = 90f;
            float rightAngleDeg = baseAngle - gunnerRotationRange;
            float leftAngleDeg = baseAngle + gunnerRotationRange;

            float rightRad = rightAngleDeg * Mathf.Deg2Rad;
            float leftRad = leftAngleDeg * Mathf.Deg2Rad;

            playerAimRangeArc.Type = DiscType.Pie; 
            playerAimRangeArc.Radius = projectileMaxDistance;
            playerAimRangeArc.AngRadiansStart = rightRad; 
            playerAimRangeArc.AngRadiansEnd = leftRad;
        }

        float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
    }
}
