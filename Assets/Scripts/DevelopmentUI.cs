using System.Collections;
using Enemies;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DevelopmentUI : MonoBehaviour {
    [SerializeField] TextMeshProUGUI timerTextUGUI;
    [SerializeField] TextMeshProUGUI waveCounterTextUGUI;
    [SerializeField] WaveSpawner waveSpawner;

    void Update() {
        timerTextUGUI.text = $"Timer: {waveSpawner.CurrentWaveTime:F2}";
        waveCounterTextUGUI.text = $"Wave: {waveSpawner.CurrentWaveDisplay}";

        if (Keyboard.current.rKey.wasPressedThisFrame) {
            StartCoroutine(ClearAndResetRoutine());
        }
    }
    
    // This Coroutine handles the sequence of events over multiple frames
    IEnumerator ClearAndResetRoutine() {
        // 1. Clear the enemies first
        ClearCurrentEnemies();

        // 2. Skip a frame. Unity will wait until the next Update cycle to continue.
        yield return null; 

        // 3. Reset the waves on the next frame
        waveSpawner.ResetWaves();
    }

    static void ClearCurrentEnemies() {
        Enemy[] allEnemiesInScene = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var enemy in allEnemiesInScene) {
            PoolManager.Instance.Release(enemy);
        }
    }
}
