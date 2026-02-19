using UnityEngine;
using System.Collections.Generic;
using Enemies;
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
        [HorizontalGroup("Top", Width = 0.7f)] 
        public Enemy enemyPrefab;

        [HorizontalGroup("Top")]
        [LabelWidth(12)]
        [LabelText("x")] 
        [MinValue(1)] 
        public int amount = 1;
        
        [Header("Timing")]
        [MinValue(0)] public float startTime;
        
        [MinValue(0.1f)] 
        public float duration = 5f;
        
        [MinValue(0.01f)] 
        public float spawnInterval = 1f;
        
        public SpawnStrategy strategy;
        
        [Header("Strategy Settings")]
        [ShowIf("@this.strategy != SpawnStrategy.Random")] 
        [Range(0f, 360f)] public float angleStart = 0f;

        [ShowIf("strategy", SpawnStrategy.Section)] 
        [Range(0f, 360f)] public float angleEnd = 90f;
    }

    [System.Serializable]
    public class Wave
    {
        [MinValue(1f)]
        public float waveDuration = 60f;

        [ListDrawerSettings(
            ListElementLabelName = "internalDebugName", 
            ShowIndexLabels = false)]
        [ValidateInput("ValidateImpulsesFit", "One or more impulses exceed the wave duration!")]
        public List<WaveImpulse> impulses;

        bool ValidateImpulsesFit(List<WaveImpulse> list)
        {
            if (list == null) return true;
            foreach (var impulse in list)
            {
                if (impulse.startTime > waveDuration) return false;
                if (impulse.startTime + impulse.duration > waveDuration) return false;
            }
            return true;
        }
    }

    [Title("Configuration")]
    [SceneObjectsOnly] 
    public Transform centerPoint;
    
    [PropertyRange(5, 100)]
    public float spawnRadius = 20f;
    
    [Title("Content")]
    public List<Wave> waves;
    
    // ==========================================
    // STATE & CONTROLS (NEW)
    // ==========================================
    [Title("State & Controls")]
    [ShowInInspector, ReadOnly]
    public int CurrentWaveDisplay => currentWaveIndex + 1; // 1-indexed for easier reading
    
    [ShowInInspector]
    [ProgressBar(0, "CurrentWaveDuration", ColorGetter = "GetProgressBarColor")]
    [LabelText("Wave Progress")]
    float waveTimeTracker;
    
    // Public getter so your UI scripts can read the timer
    public float CurrentWaveTime => waveTimeTracker;
    
    // Helper to tell the progress bar what its maximum value should be
    float CurrentWaveDuration => (currentWaveIndex >= 0 && currentWaveIndex < waves.Count) ? waves[currentWaveIndex].waveDuration : 100f;

    // Changes color of the progress bar when the wave is active
    Color GetProgressBarColor() => isWaveActive ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);

    int currentWaveIndex = -1;
    bool isWaveActive;
    
    // ==========================================

    class ImpulseRunner
    {
        public WaveImpulse data;
        public float nextSpawnTime; 

        public ImpulseRunner(WaveImpulse _data)
        {
            data = _data;
            nextSpawnTime = 0f; 
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
        
        waveTimeTracker += Time.deltaTime;

        if (waveTimeTracker >= waves[currentWaveIndex].waveDuration)
        {
            EndCurrentWave();
            return;
        }

        foreach (var runner in activeRunners)
        {
            ProcessImpulse(runner);
        }
    }

    void ProcessImpulse(ImpulseRunner runner)
    {
        float impulseLocalTime = waveTimeTracker - runner.data.startTime;

        if (impulseLocalTime >= 0 && impulseLocalTime < runner.data.duration)
        {
            if (impulseLocalTime >= runner.nextSpawnTime)
            {
                SpawnBatch(runner.data);
                runner.nextSpawnTime += runner.data.spawnInterval;
            }
        }
    }

    void SpawnBatch(WaveImpulse impulse)
    {
        if(impulse.enemyPrefab == null) return;

        for (int i = 0; i < impulse.amount; i++)
        {
            Vector3 pos = GetSpawnPosition(impulse);
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
                float range = impulse.angleEnd - impulse.angleStart;
                float randomDeg = impulse.angleStart + Random.Range(0f, range); 
                angleRad = randomDeg * Mathf.Deg2Rad;
                break;
            case SpawnStrategy.Point:
                angleRad = impulse.angleStart * Mathf.Deg2Rad;
                break;
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
            currentWaveIndex = 0;
        }

        waveTimeTracker = 0f;
        isWaveActive = true;

        activeRunners.Clear();
        foreach (var impulseData in waves[currentWaveIndex].impulses)
        {
            activeRunners.Add(new ImpulseRunner(impulseData));
        }
    }

    void EndCurrentWave()
    {
        isWaveActive = false;
        StartNextWave();
    }

    // ==========================================
    // EXTERNAL CONTROLS (NEW)
    // ==========================================
    
    /// <summary>
    /// Resets the spawner entirely, taking it back to the first wave.
    /// Can be called from other scripts (e.g., when the player dies and restarts)
    /// </summary>
    [Button(ButtonSizes.Large), GUIColor(1f, 0.4f, 0.4f)]
    public void ResetWaves()
    {
        if (waves == null || waves.Count == 0) return;

        isWaveActive = false;
        activeRunners.Clear();
        
        // Setting to -1 so that StartNextWave increments it to 0
        currentWaveIndex = -1;
        waveTimeTracker = 0f;
        
        StartNextWave();
    }

    // ==========================================
    // EDITOR VALIDATION
    // ==========================================
    void OnValidate() {
        if (waves == null) return;

        for (var index = 0; index < waves.Count; index++) {
            var wave = waves[index];
            if (wave.impulses == null) continue;

            foreach (var impulse in wave.impulses) {
                if (impulse.startTime + impulse.duration > wave.waveDuration) {
                    Debug.LogError($"[Config Error] Impulse in Wave '{index}' exceeds wave duration!");
                }
                if (impulse.spawnInterval <= 0) impulse.spawnInterval = 0.1f;
            }
        }
    }
}