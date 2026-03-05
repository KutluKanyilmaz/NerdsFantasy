using System;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    float currentHealth;
    
    public Action<float> OnHealthChange;
    public Action OnDeath; // Added this!

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChange?.Invoke(currentHealth / maxHealth);
    }

    public void ResetHealth() 
    {
        currentHealth = maxHealth;
        OnHealthChange?.Invoke(currentHealth / maxHealth);
    }

    // This is the function the Hitboxes will call
    public void TakeDamage(float amount)
    {
        // Prevent multiple deaths if hit by multiple projectiles in the same frame
        if (currentHealth <= 0) return; 

        currentHealth -= amount;
        OnHealthChange?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke(); // Tell the Enemy script to drop the loot!
            PoolManager.Instance.Release(this);
        }
    }
}
