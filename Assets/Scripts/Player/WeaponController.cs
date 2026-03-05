using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Shapes;
using UnityEngine.Serialization;

namespace Player 
{
    public class WeaponController : MonoBehaviour 
    {
        [Header("References")]
        public Transform humanTransform; 
        [FormerlySerializedAs("equippedWeapon")]
        public Gun equippedGun; // Reference to the new Weapon component

        [Header("Gunner Aiming Settings")]
        public float gunnerRotationSpeed = 120f;
        [field: SerializeField] public float GunnerRotationRange { get; private set; } = 60f; 
        
        [Header("Aiming Visuals Settings")]
        public Disc playerAimRangeArc;
        
        public event Action<float> OnReloadProgressChanged;

        public void Initialize(GunSO data)
        {
            if (equippedGun != null)
            {
                // Unsubscribe first to avoid duplicate triggers if Initialize is called twice
                equippedGun.OnReloadProgress -= HandleReloadProgress;
                
                equippedGun.Initialize(data);
                
                // Subscribe to the weapon's event
                equippedGun.OnReloadProgress += HandleReloadProgress;
            }
        }
        
        void HandleReloadProgress(float progress)
        {
            OnReloadProgressChanged?.Invoke(progress);
        }

        void Update() 
        {
            if (equippedGun == null || equippedGun.GunData == null) return;

            HandleAiming();
            HandleShooting();
            HandleManualReload();
            UpdateAimVisuals();
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
                clampedY = Mathf.Clamp(clampedY, -GunnerRotationRange, GunnerRotationRange);
                
                localEuler.y = clampedY;
                Quaternion targetLocalRot = Quaternion.Euler(localEuler);
                humanTransform.localRotation = Quaternion.Slerp(humanTransform.localRotation, targetLocalRot, Time.deltaTime * gunnerRotationSpeed);
            }
        }

        void HandleShooting()
        {
            // You can easily swap this to isPressed if you want fully automatic fire
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                equippedGun.TryShoot();
            }
        }

        void HandleManualReload()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                equippedGun.StartReload();
            }
        }

        void UpdateAimVisuals()
        {
            if (playerAimRangeArc == null) return;

            float baseAngle = 90f;
            float rightAngleDeg = baseAngle - GunnerRotationRange;
            float leftAngleDeg = baseAngle + GunnerRotationRange;

            float rightRad = rightAngleDeg * Mathf.Deg2Rad;
            float leftRad = leftAngleDeg * Mathf.Deg2Rad;

            playerAimRangeArc.Type = DiscType.Pie; 
            playerAimRangeArc.Radius = equippedGun.GunData.projectileMaxDistance;
            playerAimRangeArc.AngRadiansStart = rightRad; 
            playerAimRangeArc.AngRadiansEnd = leftRad;
        }

        public float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        public string GetCurrentWeaponAmmoCounterText() {
            return $"{equippedGun.CurrentAmmo}/{equippedGun.GunData.AmmoCapacity}";
        }
    }
}