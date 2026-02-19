using System;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    float currentHealth;
    
    public Action<float> OnHealthChange;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChange?.Invoke(currentHealth / maxHealth);
    }

    public void ResetHealth() {
        currentHealth = maxHealth;
        OnHealthChange?.Invoke(currentHealth / maxHealth);
    }

    // This is the function the Hitboxes will call
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        //Debug.Log($"{gameObject.name} took {amount} damage. HP: {currentHealth}");
        OnHealthChange.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            PoolManager.Instance.Release(this);
        }
    }
}
