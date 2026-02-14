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

            transform.LookAt(targetPosition);
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

            GameObject bullet = PoolManager.Instance.Spawn("Projectile", firePoint.position, firePoint.rotation);
            
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = firePoint.forward * projectileSpeed;
            }
        }
    }
}
