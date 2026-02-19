using UnityEngine;
using System.Collections.Generic;
using Shapes;
using Sirenix.OdinInspector;


public class WaveSpawner : MonoBehaviour
{
    public enum SpawnStrategy { Random, Section, Point }

    [System.Serializable]
    public class WaveImpulse
    {
        [HideInInspector] 
        public string internalDebugName => enemyPrefab != null ? enemyPrefab.name : "Empty Impulse";

        [Required, AssetsOnly]
        [HorizontalGroup("Top", Width = 0.7f)] // Puts Prefab and Amount on one line
        public GameObject enemyPrefab;

        [HorizontalGroup("Top")]
        [LabelWidth(12)]
        [LabelText("x")] // Tiny label for the amount
        [MinValue(1)] 
        public int amount = 1;
        
        [Header("Timing")]
        [MinValue(0)] public float startTime;
        
        [MinValue(0.1f)] // Prevents 0 or negative duration
        public float duration = 5f;
        
        [MinValue(0.01f)] // Prevents the infinite loop/crash scenario
        public float spawnInterval = 1f;
        
        public SpawnStrategy strategy;
        
        [Header("Strategy Settings")]
        [ShowIf("@this.strategy != SpawnStrategy.Random")] // Only show if relevant
        [Range(0f, 360f)] public float angleStart = 0f;

        [ShowIf("strategy", SpawnStrategy.Section)] // Another way to write the toggle
        [Range(0f, 360f)] public float angleEnd = 90f;
        
    }

    [System.Serializable]
    public class Wave
    {
        [MinValue(1f)]
        public float waveDuration = 60f;

        [ListDrawerSettings(
            ListElementLabelName = "internalDebugName", // Odin uses this property as the header!
            ShowIndexLabels = false)]
        [ValidateInput("ValidateImpulsesFit", "One or more impulses exceed the wave duration!")]
        public List<WaveImpulse> impulses;

        // Validation logic for the List
        bool ValidateImpulsesFit(List<WaveImpulse> list)
        {
            if (list == null) return true;
            foreach (var impulse in list)
            {
                if (impulse.startTime > waveDuration)
                    return false;
                if (impulse.startTime + impulse.duration > waveDuration)
                    return false;
            }
            return true;
        }
    }

    [Title("Configuration")]
    [SceneObjectsOnly] // Only allow transforms in the scene
    public Transform centerPoint;
    
    [PropertyRange(5, 100)]
    public float spawnRadius = 20f;
    
    [Title("Content")]
    public List<Wave> waves;
    
    // State
    int currentWaveIndex = -1;
    float waveTimeTracker;
    bool isWaveActive;
    
    // Tracks the timing state of each impulse in the current wave
    class ImpulseRunner
    {
        public WaveImpulse data;
        public float nextSpawnTime; // Relative to the impulse's start (0 = start of impulse)

        public ImpulseRunner(WaveImpulse _data)
        {
            data = _data;
            nextSpawnTime = 0f; // Ensure we spawn immediately when impulse starts
        }
    }
    
    List<ImpulseRunner> activeRunners = new List<ImpulseRunner>();

    void Start()
    {
        if (centerPoint == null) centerPoint = transform;
        StartNextWave();
    }

    void Update()
    {
        if (!isWaveActive) return;
        
        // 1. Advance Wave Timer
        waveTimeTracker += Time.deltaTime;

        // 2. Check for Wave End
        if (waveTimeTracker >= waves[currentWaveIndex].waveDuration)
        {
            EndCurrentWave();
            return;
        }

        // 3. Process Concurrent Impulses
        foreach (var runner in activeRunners)
        {
            ProcessImpulse(runner);
        }
    }

    void ProcessImpulse(ImpulseRunner runner)
    {
        // Calculate the "local time" for this specific impulse
        // Example: If Wave is at 25s, and Impulse starts at 20s, local time is 5s.
        float impulseLocalTime = waveTimeTracker - runner.data.startTime;

        // Check if this impulse is currently active (Started AND not finished)
        if (impulseLocalTime >= 0 && impulseLocalTime < runner.data.duration)
        {
            // Check if it's time to spawn a batch
            if (impulseLocalTime >= runner.nextSpawnTime)
            {
                SpawnBatch(runner.data);
                
                // Schedule next spawn
                runner.nextSpawnTime += runner.data.spawnInterval;
            }
        }
    }

    void SpawnBatch(WaveImpulse impulse)
    {
        // Safety check
        if(impulse.enemyPrefab == null) return;

        for (int i = 0; i < impulse.amount; i++)
        {
            Vector3 pos = GetSpawnPosition(impulse);
            
            // UPDATED: Pass the specific prefab from the impulse config
            PoolManager.Instance.Spawn(impulse.enemyPrefab, pos, Quaternion.identity);
        }
    }

    Vector3 GetSpawnPosition(WaveImpulse impulse)
    {
        float angleRad = 0f;

        switch (impulse.strategy)
        {
            case SpawnStrategy.Random:
                angleRad = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                break;

            case SpawnStrategy.Section:
                // Random angle between Start and End
                float range = impulse.angleEnd - impulse.angleStart;
                // Handle wrapping (e.g. 350 to 10 degrees) if needed, 
                // but simple Range works for standard input.
                float randomDeg = impulse.angleStart + Random.Range(0f, range); 
                angleRad = randomDeg * Mathf.Deg2Rad;
                break;

            case SpawnStrategy.Point:
                angleRad = impulse.angleStart * Mathf.Deg2Rad;
                break;
                
            // Case "OppositePlayer" reserved for later
        }

        return new Vector3(
            centerPoint.position.x + spawnRadius * Mathf.Cos(angleRad),
            centerPoint.position.y,
            centerPoint.position.z + spawnRadius * Mathf.Sin(angleRad)
        );
    }

    void StartNextWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex >= waves.Count)
        {
            //Debug.Log("All Waves Complete. Looping for debug.");
            currentWaveIndex = 0;
        }

        //Debug.Log($"Starting Wave: {currentWaveIndex}");
        
        // Reset timers
        waveTimeTracker = 0f;
        isWaveActive = true;

        // Initialize Runners for the new wave
        activeRunners.Clear();
        foreach (var impulseData in waves[currentWaveIndex].impulses)
        {
            activeRunners.Add(new ImpulseRunner(impulseData));
        }
    }

    void EndCurrentWave()
    {
        isWaveActive = false;
        // Automatically start next one for now
        StartNextWave();
    }

    // ==========================================
    // EDITOR VALIDATION
    // ==========================================
    void OnValidate() {
        // This runs whenever you change values in the Inspector
        if (waves == null) return;

        for (var index = 0; index < waves.Count; index++) {
            var wave = waves[index];
            if (wave.impulses == null) continue;

            foreach (var impulse in wave.impulses) {
                // Check 1: Does the impulse end after the wave ends?
                if (impulse.startTime + impulse.duration > wave.waveDuration) {
                    Debug.LogError($"[Config Error] Impulse in Wave '{index}' exceeds wave duration!");
                }

                // Check 2: Zero interval check to prevent infinite loops/crashes
                if (impulse.spawnInterval <= 0) impulse.spawnInterval = 0.1f;
            }
        }
    }
}