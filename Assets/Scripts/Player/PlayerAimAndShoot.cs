using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {

    public class PlayerAimAndShoot : MonoBehaviour
    {
        [Header("Setup")]
        public Transform firePoint; 
        public GameObject projectilePrefab;

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
    
            // Determine the look rotation
            Vector3 direction = targetPosition - transform.position;
    
            // OPTIONAL: Lock aiming to the horizon (prevent character tipping over)

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
        
                // Smooth the rotation so the camera doesn't snap instantly (disorienting)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
            }
        }

        void HandleShooting()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        void Shoot()
        {
            if (firePoint == null || projectilePrefab == null) return;

            PooledProjectile bullet = PoolManager.Instance.SpawnProjectile(firePoint.position, firePoint.rotation);
            
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = firePoint.forward * projectileSpeed;
            }
        }
    }
}
