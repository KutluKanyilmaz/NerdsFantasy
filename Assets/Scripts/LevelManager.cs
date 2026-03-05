using UnityEngine;
using System;
using Player;

public class LevelManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] WaveSpawner waveSpawner;
    [SerializeField] ChairController playerChair;
    [SerializeField] WeaponController playerWeapon;
    [SerializeField] HarvesterController playerHarvester;
    
    [Header("Data References")]
    [SerializeField] EquipmentIndexSO equipmentIndex;

    [Header("Run State")]
    int currentLevel = 1;
    int currentXP = 0;
    int xpToNextLevel = 100; // Base requirement

    // Events so your UI (like that top XP bar) can listen without being tightly coupled
    public event Action<int, int> OnXPUpdated;
    public event Action<int> OnLevelUp;
    public event Action<int> OnScoreUpdated;

    public Action OnDeath;

    void Start()
    {
        // 1. The Scene Handshake: Setup the Player
        InitializePlayerLoadout();

        // 2. Apply Meta Upgrades from the Shop
        ApplyMetaUpgrades();

        // 3. Reset state
        Time.timeScale = 1f; // Ensure game isn't paused from a previous run

        // 4. Start the chaos!
        waveSpawner.ResetWaves(); 
    }

    // ==========================================
    // INITIALIZATION
    // ==========================================
    void InitializePlayerLoadout()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("No GameManager found! Playing with default editor loadout.");
            return;
        }

        var saveData = GameManager.Instance.currentSaveData;

        // Fetch the data
        ChairSO chairData = equipmentIndex.GetChair(saveData.equippedChairID);
        GunSO gunData = equipmentIndex.GetGun(saveData.equippedWeaponID);
        HarvesterSO harvesterData = equipmentIndex.GetHarvester(saveData.equippedHarvesterID);
        
        // Ensure the chair has its required weapon dependency for aiming constraints
        if (playerChair != null && playerWeapon != null)
        {
            playerChair.weapon = playerWeapon;
        }

        // Inject the data directly into the existing scene controllers
        if (playerChair != null && chairData != null) {
            playerChair.Initialize(chairData);
        }

        if (playerWeapon != null && gunData != null) {
            playerWeapon.Initialize(gunData);
        }

        if (playerHarvester != null) {
            playerHarvester.Initialize(harvesterData);
        }
        

    }

    void ApplyMetaUpgrades()
    {
        if (GameManager.Instance == null) return;

        var saveData = GameManager.Instance.currentSaveData;
        
        // Find the player's health component and apply the shop upgrades
        // PlayerHealth health = playerAnchor.GetComponentInChildren<PlayerHealth>();
        // health.MaxHealth += saveData.bonusHealth;
        
        // We will let the weapons apply the globalDamageMultiplier themselves when they fire
    }

    // ==========================================
    // GAMEPLAY LOOP (XP & Leveling)
    // ==========================================

    /// <summary>
    /// Harvester Droid calls this when it picks up a crystal
    /// </summary>
    public void AddXP(int amount)
    {
        currentXP += amount;

        if (currentXP >= xpToNextLevel)
        {
            TriggerLevelUp();
        }
        else
        {
            OnXPUpdated?.Invoke(currentXP, xpToNextLevel);
        }
    }

    void TriggerLevelUp()
    {
        currentXP -= xpToNextLevel; // Carry over excess XP
        currentLevel++;
        xpToNextLevel = CalculateNextLevelXP(currentLevel); 

        OnXPUpdated?.Invoke(currentXP, xpToNextLevel);

        // Survivors-like mechanic: Pause the game and show upgrades
        Time.timeScale = 0f;
        OnLevelUp?.Invoke(currentLevel);

        // TODO: Populate levelUpPanel with 3 random temporary upgrade choices
    }

    int CalculateNextLevelXP(int level)
    {
        // Simple exponential or linear growth
        return Mathf.RoundToInt(100 * Mathf.Pow(1.2f, level - 1));
    }

    /// <summary>
    /// Called by the UI buttons on the Level Up screen
    /// </summary>
    public void ChooseUpgradeAndResume(/* UpgradeData choice */)
    {
        // Apply the chosen upgrade to the current run...

        
        Time.timeScale = 1f; // Resume game
    }

    // ==========================================
    // END GAME LOGIC
    // ==========================================

    /// <summary>
    /// Called by the Player's health script when they hit 0 HP
    /// </summary>
    public void OnPlayerDeath()
    {
        Time.timeScale = 0f; // Freeze the game
        OnDeath?.Invoke();
    }

    /// <summary>
    /// Called by a "Return to Menu" button on the GameOver screen
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            // The GameManager handles saving the coins/meta-progression and switching scenes
            GameManager.Instance.EndGameRun(); 
        }
        else
        {
            // Fallback for editor testing without a GameManager
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}