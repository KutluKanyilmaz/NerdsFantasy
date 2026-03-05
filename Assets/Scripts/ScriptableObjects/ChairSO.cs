using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewChairSO", menuName = "ScriptableObjects/ChairSO")]
public class ChairSO : ScriptableObject
{
    [ReadOnly]
    [ValidateInput("IsNameUnique", "This Name is already in use by another Chair!")]
    public string Name;
    
    [ReadOnly]
    public int Level;
    
    public float MaxTurningSpeed;
    public float MinTurningSpeed;
    
    [Tooltip("X Axis: How far the mouse is outside the aiming range (0.0 to 1.0).\nY Axis: Speed multiplier applied to TurningSpeed (0.0 to 1.0).")]
    public AnimationCurve TurnSpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    
    
    public int HP;
    public float RepairSpeed;
    public float SkillCooldown;
    public int Slots;
    public int CreditCost;
    
    public ChairSO NextLevelChairSO;
    
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

        // 2. Calculate the level dynamically for ALL chairs
        EditorApplication.delayCall += () => 
        {
            if (this == null) return;
            RecalculateAllChairLevels();
        };
    }

    void RecalculateAllChairLevels() {
        if (Application.isPlaying) return;

        string[] guids = AssetDatabase.FindAssets("t:EquipmentIndexSO");
        if (guids.Length == 0) return;

        var index = AssetDatabase.LoadAssetAtPath<EquipmentIndexSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        if (index == null) return;

        // Iterate through EVERY chair in the index, not just 'this' one
        foreach (var chair in index.Chairs)
        {
            if (chair == null) continue;

            int newLevel = GetLevelDepth(chair, index.Chairs, new List<ChairSO>());

            if (chair.Level != newLevel)
            {
                chair.Level = newLevel;
                EditorUtility.SetDirty(chair);
            }
        }
    }
    
    // Recursively traces backwards up the tree to find how deep this chair is
    int GetLevelDepth(ChairSO target, List<ChairSO> allChairs, List<ChairSO> visited)
    {
        // Failsafe: Prevent infinite loops if you accidentally make a circular upgrade path
        if (visited.Contains(target)) return 1; 
        visited.Add(target);

        // Find any chair in the index that has THIS chair as its "NextLevelChairSO"
        var predecessors = allChairs.Where(g => g != null && g.NextLevelChairSO == target).ToList();
        
        // Base case: If nothing points to this chair, it is a base Level 1 chair.
        if (predecessors.Count == 0) return 1;

        // Find the maximum depth among all predecessors and add 1
        int maxDepth = 0;
        foreach (var p in predecessors)
        {
            int pDepth = GetLevelDepth(p, allChairs, new List<ChairSO>(visited));
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
        foreach (var chair in index.Chairs)
        {
            if (chair != null && chair.Name == nameToCheck) occurrences++;
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
                if (index != null && !index.Chairs.Contains(this))
                {
                    // Clean up any missing references from deleted assets before adding
                    index.Chairs.RemoveAll(item => item == null); 
                    index.Chairs.Add(this);
                    
                    // Tell Unity the index has changed so it saves the new list
                    EditorUtility.SetDirty(index);
                }
            }
        };
    }
#endif
}