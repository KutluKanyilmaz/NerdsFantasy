using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Player
{
    public class Gun : MonoBehaviour
    {
        [Header("References")]
        public Transform firePoint;
        public PooledProjectile projectilePrefab;
        public LayerMask projectileHitLayers = Physics.DefaultRaycastLayers;
        
        [Header("Projectile Curve Control")]
        public bool useProjectileSpeedCurve = false;
        
        [ShowIf("useProjectileSpeedCurve")] public AnimationCurve projectileSpeedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        
        public event Action<float> OnReloadProgress;

        // Active State
        public GunSO GunData { get; private set; }
        public int CurrentAmmo { get; private set; }
        public bool IsReloading { get; private set; }

        float nextFireTime = 0f;
        float reloadEndTime = 0f;

        public void Initialize(GunSO data)
        {
            GunData = data;
            CurrentAmmo = GunData.AmmoCapacity; // Start with a full magazine
            IsReloading = false;
        }

        void Update()
        {
            if (GunData == null) return;

            // Handle the reload timer
            if (IsReloading)
            {
                // Calculate how far along the reload is (0.0 to 1.0)
                float timeRemaining = reloadEndTime - Time.time;
                float progress = 1f - (timeRemaining / GunData.ReloadSpeed);
                
                // Broadcast the current progress
                OnReloadProgress?.Invoke(progress);

                if (Time.time >= reloadEndTime)
                {
                    FinishReloading();
                }
            }
        }

        public void TryShoot()
        {
            if (GunData == null || IsReloading || Time.time < nextFireTime) return;

            if (CurrentAmmo <= 0)
            {
                StartReload();
                return;
            }

            Shoot();
        }

        void Shoot()
        {
            CurrentAmmo--;
            nextFireTime = Time.time + (1f / GunData.FireRate);

            if (firePoint == null || projectilePrefab == null) return;
            
            PooledProjectile bullet = PoolManager.Instance.Spawn(projectilePrefab, firePoint.position, firePoint.rotation);
            
            bullet.Initialize(
                GunData.projectileSpeed,
                GunData.Damage, 
                GunData.projectileMaxDistance, 
                projectileHitLayers, 
                useProjectileSpeedCurve, 
                projectileSpeedCurve
            );

            // Auto-reload if magazine is empty after this shot
            if (CurrentAmmo <= 0)
            {
                StartReload();
            }
        }

        public void StartReload()
        {
            // Don't reload if already reloading or if the magazine is already full
            if (IsReloading || CurrentAmmo == GunData.AmmoCapacity) return;

            IsReloading = true;
            reloadEndTime = Time.time + GunData.ReloadSpeed;
            
            // Optional: Trigger an OnReloadStarted event here for audio/UI
        }

        void FinishReloading()
        {
            IsReloading = false;
            CurrentAmmo = GunData.AmmoCapacity;
            
            OnReloadProgress?.Invoke(0f);
        }
    }
}