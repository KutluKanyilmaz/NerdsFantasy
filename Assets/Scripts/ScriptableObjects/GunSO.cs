using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewGunSO", menuName = "ScriptableObjects/GunSO")]
public class GunSO : ScriptableObject
{
    [ReadOnly]
    [ValidateInput("IsNameUnique", "This Name is already in use by another Gun!")]
    public string Name;
    
    [ReadOnly]
    public int Level;
    
    [Tooltip("Damage per bullet")]
    public float Damage = 1;
    
    [Tooltip("Measured in bullets per second")]
    public float FireRate = 1;
    
    [Tooltip(("Speed?"))]
    public float projectileSpeed = 10;
    
    [Tooltip("Distance?")]
    public float projectileMaxDistance;
    
    [Tooltip("Amount of shots that can be fired before having to reload")]
    public int AmmoCapacity;
    
    [Tooltip("Seconds required to reload the weapon")]
    public float ReloadSpeed;
    
    [Tooltip("Percentage?")]
    public float CriticalChancePercentage;
    
    [Tooltip("The bullet's damage will be multiplied by this amount when it critically strikes")]
    public float CriticalDamageMultiplier;
    
    [Tooltip("The amount of credits required to acquire this weapon")]
    public int CreditCost;
    
    public int Penetration;
    public int Ricochet;

    public GunSO NextLevelGunSO;

#if UNITY_EDITOR
    void OnValidate()
    {
        // 1. Sync the Name to the Asset's filename safely
        if (Name != name)
        {
            Name = name;
            // Tell Unity to save the name change (delayed to avoid AssetDatabase warnings)
            EditorApplication.delayCall += () => { if (this != null) EditorUtility.SetDirty(this); };
        }

        // 2. Calculate the level dynamically for ALL guns
        EditorApplication.delayCall += () => 
        {
            if (this == null) return;
            RecalculateAllGunLevels();
        };
    }

    void RecalculateAllGunLevels()
    {
        if (Application.isPlaying) return;

        string[] guids = AssetDatabase.FindAssets("t:EquipmentIndexSO");
        if (guids.Length == 0) return;

        var index = AssetDatabase.LoadAssetAtPath<EquipmentIndexSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        if (index == null) return;

        // Iterate through EVERY gun in the index, not just 'this' one
        foreach (var gun in index.Guns)
        {
            if (gun == null) continue;

            int newLevel = GetLevelDepth(gun, index.Guns, new List<GunSO>());

            if (gun.Level != newLevel)
            {
                gun.Level = newLevel;
                EditorUtility.SetDirty(gun);
            }
        }
    }
    
    // Recursively traces backwards up the tree to find how deep this gun is
    int GetLevelDepth(GunSO target, List<GunSO> allGuns, List<GunSO> visited)
    {
        // Failsafe: Prevent infinite loops if you accidentally make a circular upgrade path (Gun A -> Gun B -> Gun A)
        if (visited.Contains(target)) return 1; 
        visited.Add(target);

        // Find any gun in the index that has THIS gun as its "NextLevelGunSO"
        var predecessors = allGuns.Where(g => g != null && g.NextLevelGunSO == target).ToList();
        
        // Base case: If nothing points to this gun, it is a base Level 1 gun.
        if (predecessors.Count == 0) return 1;

        // Find the maximum depth among all predecessors and add 1
        int maxDepth = 0;
        foreach (var p in predecessors)
        {
            int pDepth = GetLevelDepth(p, allGuns, new List<GunSO>(visited));
            if (pDepth > maxDepth) maxDepth = pDepth;
        }

        return maxDepth + 1;
    }
    
    bool IsNameUnique(string nameToCheck)
    {
        if (string.IsNullOrEmpty(nameToCheck)) return true; 
        
        // Find the Index in the project
        string[] guids = AssetDatabase.FindAssets("t:EquipmentIndexSO");
        if (guids.Length == 0) return true; // Ignore if no index exists yet
        
        var index = AssetDatabase.LoadAssetAtPath<EquipmentIndexSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        if (index == null) return true;

        // Check how many times this name appears
        int occurrences = 0;
        foreach (var gun in index.Guns)
        {
            if (gun != null && gun.Name == nameToCheck) occurrences++;
        }
        
        // It will find itself, so occurrences should be exactly 1
        return occurrences <= 1; 
    }

    void OnEnable()
    {
        // Only run auto-add logic in the Editor, not in the built game
        if (Application.isPlaying) return;
        
        // DelayCall prevents Unity errors related to modifying other assets during Awake/OnEnable
        EditorApplication.delayCall += () => {
            if (this == null) return; // Failsafe in case it was destroyed immediately

            string[] guids = AssetDatabase.FindAssets("t:EquipmentIndexSO");
            if (guids.Length > 0)
            {
                var index = AssetDatabase.LoadAssetAtPath<EquipmentIndexSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
                if (index != null && !index.Guns.Contains(this))
                {
                    // Clean up any missing references from deleted assets before adding
                    index.Guns.RemoveAll(item => item == null); 
                    index.Guns.Add(this);
                    
                    // Tell Unity the index has changed so it saves the new list
                    EditorUtility.SetDirty(index);
                }
            }
        };
    }
#endif
}