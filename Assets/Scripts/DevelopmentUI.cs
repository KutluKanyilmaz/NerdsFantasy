using System.Collections;
using Enemies;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class DevelopmentUI : MonoBehaviour {
    [SerializeField] TextMeshProUGUI timerTextUGUI;
    [SerializeField] TextMeshProUGUI waveCounterTextUGUI;
    [SerializeField] TextMeshProUGUI playerAimShootVTextUGUI;
    [SerializeField] WaveSpawner waveSpawner;
    [SerializeField] PlayerAimAndShoot playerAimAndShoot;

    public int projectileDamageStep = 10;
    public float fireRateStep = 0.1f;
    public float gunnerRotationRangeStep = 5f;
    public float gunnerRotationSpeedStep = 5f;
    public float keyboardChairRotationSpeedStep = 10f;
    public float mouseChairRotationSpeedStep = 5f;
    
    bool num1Pressed;
    bool num2Pressed;
    bool num3Pressed;
    bool num4Pressed;
    bool num5Pressed;
    bool num6Pressed;
    bool num7Pressed;

    void Update() {
        // 1. Handle Inputs First (so the UI reflects the current frame's state)
        HandleNumpadKeysPressed();

        if (Keyboard.current.rKey.wasPressedThisFrame) {
            StartCoroutine(ClearAndResetRoutine());
        }

        // 2. Update Basic Text
        timerTextUGUI.text = $"Timer: {waveSpawner.CurrentWaveTime:F2}";
        waveCounterTextUGUI.text = $"Wave: {waveSpawner.CurrentWaveDisplay}";

        // 3. Build the Dev Menu Text
        StringBuilder sb = new StringBuilder("<b>Player Values:</b>");

        sb.Append(FormatLine(1, "Damage", playerAimAndShoot.projectileDamage, num1Pressed));
        sb.Append(FormatLine(2, "FireRate", playerAimAndShoot.fireRate, num2Pressed));
        sb.Append(FormatLine(3, "GunnerRotationRange", playerAimAndShoot.gunnerRotationRange, num3Pressed));
        sb.Append(FormatLine(4, "GunnerRotationSpeed", playerAimAndShoot.gunnerRotationSpeed, num4Pressed));
        sb.Append(FormatLine(5, "KeyboardRotationSpeed", playerAimAndShoot.keyboardChairRotationSpeed, num5Pressed));
        sb.Append(FormatLine(6, "MouseMinRotationSpeed", playerAimAndShoot.mouseChairMinRotationSpeed, num6Pressed));
        sb.Append(FormatLine(7, "MouseMaxRotationSpeed", playerAimAndShoot.mouseChairMaxRotationSpeed, num7Pressed));

        playerAimShootVTextUGUI.text = sb.ToString();
    }

    void HandleNumpadKeysPressed() {
        var kb = Keyboard.current;

        UpdateIntStat(kb.numpad1Key, 
            () => (int)playerAimAndShoot.projectileDamage, 
            v => playerAimAndShoot.projectileDamage = v, 
            projectileDamageStep, out num1Pressed);

        UpdateFloatStat(kb.numpad2Key, 
            () => playerAimAndShoot.fireRate, 
            v => playerAimAndShoot.fireRate = v, 
            fireRateStep, out num2Pressed);

        UpdateFloatStat(kb.numpad3Key, 
            () => playerAimAndShoot.gunnerRotationRange, 
            v => playerAimAndShoot.gunnerRotationRange = v, 
            gunnerRotationRangeStep, out num3Pressed);

        UpdateFloatStat(kb.numpad4Key, 
            () => playerAimAndShoot.gunnerRotationSpeed, 
            v => playerAimAndShoot.gunnerRotationSpeed = v, 
            gunnerRotationSpeedStep, out num4Pressed);

        UpdateFloatStat(kb.numpad5Key, 
            () => playerAimAndShoot.keyboardChairRotationSpeed, 
            v => playerAimAndShoot.keyboardChairRotationSpeed = v, 
            keyboardChairRotationSpeedStep, out num5Pressed);

        UpdateFloatStat(kb.numpad6Key, 
            () => playerAimAndShoot.mouseChairMinRotationSpeed, 
            v => playerAimAndShoot.mouseChairMinRotationSpeed = v, 
            mouseChairRotationSpeedStep, out num6Pressed);

        UpdateFloatStat(kb.numpad7Key, 
            () => playerAimAndShoot.mouseChairMaxRotationSpeed, 
            v => playerAimAndShoot.mouseChairMaxRotationSpeed = v, 
            mouseChairRotationSpeedStep, out num7Pressed);
    }

    // --- Helper Methods ---

    private string FormatLine(int index, string label, object value, bool isPressed) {
        string content = $"({index}) {label}: {value}";
        return isPressed ? $"\n<u>{content}</u>" : $"\n{content}";
    }

    private void UpdateIntStat(KeyControl key, Func<int> getter, Action<int> setter, int step, out bool isPressed) {
        isPressed = key.isPressed;
        if (isPressed) {
            if (Keyboard.current.numpadPlusKey.wasPressedThisFrame) setter(getter() + step);
            else if (Keyboard.current.numpadMinusKey.wasPressedThisFrame) setter(getter() - step);
        }
    }

    private void UpdateFloatStat(KeyControl key, Func<float> getter, Action<float> setter, float step, out bool isPressed) {
        isPressed = key.isPressed;
        if (isPressed) {
            if (Keyboard.current.numpadPlusKey.wasPressedThisFrame) setter(getter() + step);
            else if (Keyboard.current.numpadMinusKey.wasPressedThisFrame) setter(getter() - step);
        }
    }

    // --- Coroutines & Resets ---

    IEnumerator ClearAndResetRoutine() {
        ClearCurrentEnemies();
        yield return null; 
        waveSpawner.ResetWaves();
    }

    static void ClearCurrentEnemies() {
        Enemy[] allEnemiesInScene = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var enemy in allEnemiesInScene) {
            PoolManager.Instance.Release(enemy);
        }
    }
}