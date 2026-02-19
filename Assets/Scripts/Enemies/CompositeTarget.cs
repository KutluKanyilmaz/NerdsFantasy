namespace Enemies {
    using UnityEngine;

    public class CompositeTarget : MonoBehaviour
    {
        [Header("Setup")]
        public HealthController mainHealth;
        public float damageMultiplier = 1.0f;

        Transform _enemyRoot;
        Quaternion _initialRotationRelativeToEnemy;

        void Start()
        {
            // Find the actual Enemy component above us, not the scene root
            Enemy enemy = GetComponentInParent<Enemy>();
            if (enemy != null) {
                _enemyRoot = enemy.transform;
                
                // Calculate the offset: How was I rotated relative to the enemy base in T-Pose?
                _initialRotationRelativeToEnemy = Quaternion.Inverse(_enemyRoot.rotation) * transform.rotation;
            }
        }

        void LateUpdate()
        {
            if (_enemyRoot == null) return;

            // Stay locked to the Enemy's rotation, ignoring the bone's animation
            transform.rotation = _enemyRoot.rotation * _initialRotationRelativeToEnemy;
        }

        public void ReceiveHit(float baseDamage)
        {
            if (mainHealth != null)
            {
                mainHealth.TakeDamage(baseDamage * damageMultiplier);
            }
        }
    }
}