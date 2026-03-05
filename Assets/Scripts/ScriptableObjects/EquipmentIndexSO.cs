using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;

[CreateAssetMenu(fileName = "EquipmentIndex", menuName = "ScriptableObjects/Equipment Index")]
public class EquipmentIndexSO : ScriptableObject
{
    // IsReadOnly prevents manual adding in the inspector, forcing the auto-add system to do the work.
    [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, ShowPaging = true, NumberOfItemsPerPage = 10)]
    [ValidateInput("ValidateGunNames", "Duplicate Gun names detected!")]
    public List<GunSO> Guns = new List<GunSO>();

    [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, ShowPaging = true, NumberOfItemsPerPage = 10)]
    [ValidateInput("ValidateChairNames", "Duplicate Chair names detected!")]
    public List<ChairSO> Chairs = new List<ChairSO>();

    [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false, ShowPaging = true, NumberOfItemsPerPage = 10)]
    [ValidateInput("ValidateHarvesterNames", "Duplicate Harvester names detected!")]
    public List<HarvesterSO> Harvesters = new List<HarvesterSO>();

    public ChairSO GetChair(string chairName) {
        ChairSO chairSO = Chairs.FirstOrDefault(c => c.name == chairName);
        if (chairSO == null) {
            Debug.LogError($"Chair {chairName} not found");
            return null;
        }
        return chairSO;
    }
    
    public GunSO GetGun(string gunName) {
        GunSO gunSO = Guns.FirstOrDefault(g => g.name == gunName);
        if (gunSO == null) {
            Debug.LogError($"Gun {gunName} not found");
            return null;
        }
        return gunSO;
    }

    public HarvesterSO GetHarvester(string harvesterName) {
        HarvesterSO harvesterSO  = Harvesters.FirstOrDefault(h => h.name == harvesterName);
        if (harvesterSO == null) {
            Debug.LogError($"Harvester {harvesterName} not found");
            return null;
        }
        return harvesterSO;
    }


#if UNITY_EDITOR
    // Odin Validator methods to check for duplicates across the lists
    bool ValidateGunNames(List<GunSO> list, ref string errorMessage)
    {
        return CheckDuplicates(list.Where(x => x != null).Select(x => x.Name), ref errorMessage);
    }

    bool ValidateChairNames(List<ChairSO> list, ref string errorMessage)
    {
        return CheckDuplicates(list.Where(x => x != null).Select(x => x.Name), ref errorMessage);
    }

    bool ValidateHarvesterNames(List<HarvesterSO> list, ref string errorMessage)
    {
        return CheckDuplicates(list.Where(x => x != null).Select(x => x.Name), ref errorMessage);
    }

    bool CheckDuplicates(IEnumerable<string> names, ref string errorMessage)
    {
        var duplicates = names.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
        {
            errorMessage = "Duplicate names found: " + string.Join(", ", duplicates);
            return false;
        }
        return true;
    }
#endif
}