using UnityEngine;
using Sirenix.OdinInspector;

namespace Player
{
    [RequireComponent(typeof(HarvesterPathfinder))]
    public class HarvesterController : MonoBehaviour
    {
        [Header("References")]
        public LevelManager levelManager; 

        [Header("Active Data")]
        [ReadOnly] public HarvesterSO HarvesterData; 

        HarvesterPathfinder pathfinder;
        ExperienceOrb currentTargetCrystal;

        void Awake()
        {
            pathfinder = GetComponent<HarvesterPathfinder>();
        }

        public void Initialize(HarvesterSO data)
        {
            HarvesterData = data;
            
            // Inject the data-driven move speed straight into the legs
            if (pathfinder != null) 
            {
                pathfinder.SetSpeed(HarvesterData.MoveSpeed);
            }
        }

        void Update() {
            // Don't do anything if data hasn't been injected by the LevelManager yet
            if (HarvesterData == null) return;


            // 1. Need a new target?
            if (currentTargetCrystal == null || currentTargetCrystal.IsBeingCollected)
            {
                FindClosestCrystal();
            }

            // 2. Are we close enough to the target to collect it?
            if (currentTargetCrystal != null)
            {
                float distance = Vector3.Distance(transform.position, currentTargetCrystal.transform.position);
                
                // Read the collection range directly from the SO
                if (distance <= HarvesterData.CollectRadius)
                {
                    CollectCrystal(currentTargetCrystal);
                }
            }
        }

        void FindClosestCrystal()
        {
            ExperienceOrb closest = null;
            float minDistance = float.MaxValue;

            // Iterate over the highly-performant static list instead of using FindObjectsByType
            foreach (ExperienceOrb crystal in ExperienceOrb.ActiveOrbs)
            {
                if (crystal.IsBeingCollected) continue;

                float dist = Vector3.Distance(transform.position, crystal.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = crystal;
                }
            }

            currentTargetCrystal = closest;

            if (currentTargetCrystal != null)
            {
                pathfinder.SetTarget(currentTargetCrystal.transform);
            }
            else
            {
                pathfinder.ClearTarget();
            }
        }

        void CollectCrystal(ExperienceOrb crystal)
        {
            crystal.IsBeingCollected = true;
            
            // Pass the data-driven multiplier into the crystal so it can calculate the final XP
            crystal.Consume(levelManager, HarvesterData.HarvestingMultiplier);
            
            currentTargetCrystal = null; 
        }
    }
}