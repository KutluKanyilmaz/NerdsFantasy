using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlayerData {
    // Currencies & Meta
    public int totalCredits = 0;
    public int totalMoney = 0;
    public float globalDamageMultiplier = 1.0f;
    public int highestWaveReached = 0;

    // Loadout (Storing IDs or Names, not the actual items)
    public string equippedWeaponID;
    public string equippedChairID;
    public string equippedHarvesterID;

    public PlayerData(string weaponID, string chairID, string harvesterID) {
        equippedChairID = chairID;
        equippedHarvesterID = harvesterID;
        equippedWeaponID = weaponID;
    }
}

public class CurrentRunState {
    public int currentScore;
    public int currentWave;
    public int creditsEarnedThisRun;

    public void Reset() {
        currentScore = 0;
        currentWave = 0;
        creditsEarnedThisRun = 0;
    }
}

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    // Our data containers
    public PlayerData currentSaveData;
    public CurrentRunState currentRun = new CurrentRunState();

    // --------------------------------------------------------
    // THE AUTO-BOOT MAGIC
    // --------------------------------------------------------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoBoot() {
        // 1. Check if one already exists (e.g., if you accidentally left one in the scene)
        if (FindAnyObjectByType<GameManager>() != null) {
            return; 
        }

        // 2. Load the prefab from the Resources folder
        GameObject prefab = Resources.Load<GameObject>("GameManager");
        
        if (prefab == null) {
            Debug.LogError("Auto-Boot Failed: Could not find a prefab named 'GameManager' in a 'Resources' folder.");
            return;
        }

        // 3. Instantiate it. The Awake() method below will handle the DontDestroyOnLoad
        Instantiate(prefab);
    }

    // --------------------------------------------------------
    // SINGLETON SETUP
    // --------------------------------------------------------
    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.name = "GameManager (Persistent)";

        // Load our player data right as the game starts
        LoadGame();
    }

    // --------------------------------------------------------
    // DATA LOGISTICS
    // --------------------------------------------------------
    public void LoadGame() {
        // TODO: Implement actual JSON reading here. 
        // For now, we'll just create a fresh save if none exists.
        currentSaveData = new PlayerData(currentSaveData.equippedWeaponID,  currentSaveData.equippedChairID, currentSaveData.equippedHarvesterID);
        Debug.Log("GameManager: Player Data Loaded.");
    }

    public void SaveGame() {
        // TODO: Implement actual JSON writing here.
        Debug.Log("GameManager: Player Data Saved to Disk.");
    }

    // --------------------------------------------------------
    // SCENE & RUN FLOW
    // --------------------------------------------------------
    public void StartGameRun() {
        // Reset the current run stats
        currentRun.Reset();
        
        // Load the game scene
        SceneManager.LoadScene("DevScene");
    }

    public void EndGameRun() {
        // 1. Transfer run currency to total currency
        currentSaveData.totalCredits += currentRun.creditsEarnedThisRun;

        // 2. Check for new high scores/waves
        if (currentRun.currentWave > currentSaveData.highestWaveReached) {
            currentSaveData.highestWaveReached = currentRun.currentWave;
        }

        // 3. Save progress
        SaveGame();

        // 4. Return to menu
        SceneManager.LoadScene("MainMenu");
    }
}