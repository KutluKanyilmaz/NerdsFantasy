using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {

    public class PlayerAimAndShoot : MonoBehaviour
    {
        [Header("Setup")]
        public Transform firePoint; 
        
        public PooledProjectile projectilePrefab;

        [Header("Settings")]
        public float projectileSpeed = 20f;
        public float fireRate = 0.5f;

        float nextFireTime = 0f;

        void Update()
        {
            HandleAiming();
            HandleShooting();
        }

        void HandleAiming()
        {
            Vector3 targetPosition = MouseWorld.Instance.GetPosition();
            Vector3 direction = targetPosition - transform.position;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
            }
        }

        void HandleShooting()
        {
            // Note: Make sure 'Mouse' is properly imported from the New Input System
            if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        void Shoot()
        {
            if (firePoint == null || projectilePrefab == null) return;

            // UPDATED: Now uses the generic multi-pool system!
            PooledProjectile bullet = PoolManager.Instance.Spawn(projectilePrefab, firePoint.position, firePoint.rotation);
            
            if (bullet.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = firePoint.forward * projectileSpeed;
            }
        }
    }
}
