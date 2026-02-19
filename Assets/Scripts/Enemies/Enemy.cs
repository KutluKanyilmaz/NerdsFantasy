using System;
using UnityEngine;

namespace Enemies {
    public class Enemy : MonoBehaviour, IPooledObject{
        EnemyPathfinder pathfinder;
        HealthController healthController;

        void Awake() {
            pathfinder = GetComponent<EnemyPathfinder>();
            healthController = GetComponent<HealthController>();
        }

        public void OnSpawnFromPool() {
            pathfinder.SetPlayerAsTarget();
            healthController.ResetHealth();
        }
    }
}