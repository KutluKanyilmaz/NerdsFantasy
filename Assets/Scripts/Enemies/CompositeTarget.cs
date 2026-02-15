using UnityEngine;


namespace Enemies {

    public class CompositeTarget : MonoBehaviour
    {
        [Header("Setup")]
        [Tooltip("Drag the main parent's HealthController here.")]
        public HealthController mainHealth;

        [Header("Damage Settings")]
        [Tooltip("Multiplier for damage taken here (e.g., 2.0 for head, 0.5 for armor).")]
        public float damageMultiplier = 1.0f;

        // This function is called by the projectile
        public void ReceiveHit(float baseDamage)
        {
            if (mainHealth != null)
            {
                float finalDamage = baseDamage * damageMultiplier;
                mainHealth.TakeDamage(finalDamage);
            }
        }
    }
}