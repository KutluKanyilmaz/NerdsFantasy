using UnityEngine;
using System.Collections.Generic;

public class ExperienceOrb : MonoBehaviour, IPooledObject
{
    public static readonly List<ExperienceOrb> ActiveOrbs = new();

    [Header("Settings")]
    public float baseXP = 10f; 

    public bool IsBeingCollected { get; set; }

    // --- Pooling & List Management ---
    public void OnSpawnFromPool()
    {
        IsBeingCollected = false;
        
        // Safely add to the list when the pool pushes it into the scene
        if (!ActiveOrbs.Contains(this))
        {
            ActiveOrbs.Add(this);
        }
    }

    void OnDisable()
    {
        // Safely remove when the pool disables it
        if (ActiveOrbs.Contains(this))
        {
            ActiveOrbs.Remove(this);
        }
    }

    public void Consume(LevelManager levelManager, float harvestingMultiplier)
    {
        if (levelManager != null)
        {
            int finalXP = Mathf.RoundToInt(baseXP * harvestingMultiplier);
            levelManager.AddXP(finalXP);
        }
        
        PoolManager.Instance.Release(this); // This triggers SetActive(false), which triggers OnDisable
    }
}