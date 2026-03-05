using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "NewHarvesterSO", menuName = "ScriptableObjects/HarvesterSO")]
public class HarvesterSO : ScriptableObject
{
    [ReadOnly]
    [ValidateInput("IsNameUnique", "This Name is already in use by another Harvester!")]
    public string Name;
    
    [ReadOnly]
    public int Level;
    
    public float MoveSpeed;
    public float HarvestingMultiplier;
    public float CollectRadius;
    public float Charge;
    public float EnergyEfficiency;
    public int CreditCost;

    public HarvesterSO NextLevelHarvesterSO;
    
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

        // 2. Calculate the level dynamically for ALL harvester
        EditorApplication.delayCall += () => 
        {
            if (this == null) return;
            RecalculateAllHarvesterLevels();
        };
    }

    void RecalculateAllHarvesterLevels() {
        if (Application.isPlaying) return;

        string[] guids = AssetDatabase.FindAssets("t:EquipmentIndexSO");
        if (guids.Length == 0) return;

        var index = AssetDatabase.LoadAssetAtPath<EquipmentIndexSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        if (index == null) return;

        // Iterate through EVERY harvester in the index, not just 'this' one
        foreach (var harvester in index.Harvesters)
        {
            if (harvester == null) continue;

            int newLevel = GetLevelDepth(harvester, index.Harvesters, new List<HarvesterSO>());

            if (harvester.Level != newLevel)
            {
                harvester.Level = newLevel;
                EditorUtility.SetDirty(harvester);
            }
        }
    }
    
    // Recursively traces backwards up the tree to find how deep this harvester is
    int GetLevelDepth(HarvesterSO target, List<HarvesterSO> allHarvesters, List<HarvesterSO> visited)
    {
        // Failsafe: Prevent infinite loops if you accidentally make a circular upgrade path
        if (visited.Contains(target)) return 1; 
        visited.Add(target);

        // Find any harvester in the index that has THIS harvester as its "NextLevelHarvesterSO"
        var predecessors = allHarvesters.Where(g => g != null && g.NextLevelHarvesterSO == target).ToList();
        // Base case: If nothing points to this harvester, it is a base Level 1 harvester.
        if (predecessors.Count == 0) return 1;

        // Find the maximum depth among all predecessors and add 1
        int maxDepth = 0;
        foreach (var p in predecessors)
        {
            int pDepth = GetLevelDepth(p, allHarvesters, new List<HarvesterSO>(visited));
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
        foreach (var harvester in index.Harvesters)
        {
            if (harvester != null && harvester.Name == nameToCheck) occurrences++;
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
                if (index != null && !index.Harvesters.Contains(this))
                {
                    // Clean up any missing references from deleted assets before adding
                    index.Harvesters.RemoveAll(item => item == null); 
                    index.Harvesters.Add(this);
                    
                    // Tell Unity the index has changed so it saves the new list
                    EditorUtility.SetDirty(index);
                }
            }
        };
    }
#endif
}