using UnityEngine;

namespace Enemies {
    public class Enemy : MonoBehaviour, IPooledObject
    {
        EnemyPathfinder pathfinder;
        HealthController healthController;

        [Header("Loot Settings")]
        [Tooltip("The XP Crystal prefab to drop on death")]
        public ExperienceOrb experienceOrbPrefab; 

        void Awake() 
        {
            pathfinder = GetComponent<EnemyPathfinder>();
            healthController = GetComponent<HealthController>();

            // Listen for the death event
            if (healthController != null)
            {
                healthController.OnDeath += HandleDeath;
            }
        }

        public void OnSpawnFromPool() 
        {
            pathfinder.SetPlayerAsTarget();
            healthController.ResetHealth();
        }

        void HandleDeath()
        {
            // Spawn the crystal exactly where the enemy died
            if (experienceOrbPrefab != null)
            {
                PoolManager.Instance.Spawn(experienceOrbPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} died but has no XP Crystal Prefab assigned!");
            }
        }

        void OnDestroy()
        {
            // Always clean up event listeners to prevent memory leaks
            if (healthController != null)
            {
                healthController.OnDeath -= HandleDeath;
            }
        }
    }
}