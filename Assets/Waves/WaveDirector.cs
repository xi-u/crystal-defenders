using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrates the flow of the game: it decides when enemies appear and
/// when the run is over. Nothing else in the scene starts or stops the sim.
///
/// Right now this is deliberately a plain timer. It becomes the WORKING
/// state machine students study for assignment 1, and the singleton
/// subject of assignment 2.
/// </summary>
public class WaveDirector : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] Crystal crystal;
    [SerializeField] SpawnZone[] spawnZones;

    [Header("Spawning")]
    [Tooltip("Seconds before the first enemy appears after pressing Play.")]
    [SerializeField] float firstSpawnDelay = 1f;
    [Tooltip("Seconds between spawns.")]
    [SerializeField] float spawnInterval = 2f;
    [Tooltip("Total enemies to spawn this run. 0 means keep spawning forever.")]
    [SerializeField] int totalEnemies = 0;

    float nextSpawnTime;
    int spawnedCount;
    bool running;
    bool gameOver;
    int previousCrystalHealth = -1;
    bool lowHealthMusicStarted;

    /// <summary>True once the crystal has fallen. The UI polls this.</summary>
    public bool IsGameOver => gameOver;

    /// <summary>True once the run has been started. The HUD polls this to know
    /// whether it should be on screen at all.</summary>
    public bool IsRunning => running;

    /// <summary>
    /// Called by the start button. Nothing spawns until this runs, which is
    /// why the sim can sit harmlessly behind the start screen: the director is
    /// the only thing that can put an enemy in the world.
    /// </summary>
    public void BeginRun()
    {
        if (running || gameOver) return;
        running = true;
        
        AudioPlayer.Instance?.PlayBGM();
        previousCrystalHealth = crystal != null ? crystal.CurrentHealth : -1;
        lowHealthMusicStarted = false;
        
        nextSpawnTime = Time.time + firstSpawnDelay;

        // Enemy.TotalKilled is static, so it lives in the C# domain and not in
        // the scene: a scene reload rebuilds every object but leaves that number
        // exactly where the last run left it. Zero it here, at the one moment
        // that unambiguously means "a new run is starting".
        Enemy.ResetKills();
    }

    void Update()
    {
        if (!running || gameOver) return;

        // The director ASKS the crystal every frame whether it is still alive.
        // That polling is the pain assignment 3 removes by having the crystal
        // announce its own destruction instead.
        if (crystal == null || crystal.IsDestroyed)
        {
            EndGame();
            return;
        }

        bool tookDamageThisFrame = previousCrystalHealth >= 0 && crystal.CurrentHealth < previousCrystalHealth;
        if (tookDamageThisFrame)
            ScreenShake.Instance?.Shake();

        bool lowHealth = crystal.CurrentHealth * 4 < crystal.MaxHealth;
        if (lowHealth && !lowHealthMusicStarted)
        {
            AudioPlayer.Instance?.PlayBGMLowHealth();
            lowHealthMusicStarted = true;
        }

        previousCrystalHealth = crystal.CurrentHealth;

        if (Time.time < nextSpawnTime) return;
        nextSpawnTime = Time.time + spawnInterval;

        if (totalEnemies > 0 && spawnedCount >= totalEnemies) return;
        if (spawnZones == null || spawnZones.Length == 0) return;

        // Alternate zones so both lanes stay busy without needing a second timer.
        SpawnZone zone = spawnZones[spawnedCount % spawnZones.Length];
        if (zone != null)
        {
            zone.Spawn(crystal);
            spawnedCount++;
        }
    }

    void EndGame()
    {
        gameOver = true;
        AudioPlayer.Instance?.StopMusic();
        AudioPlayer.Instance?.PlayGameOver();
        
        // No spawns from here on. Enemies already in the field stop on their
        // own because they check the crystal before moving, so nothing needs
        // to hunt them down and switch them off.
        Debug.Log($"GAME OVER: the crystal fell after {spawnedCount} enemies.", this);
    }
}
