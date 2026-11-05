using UnityEngine;

/// Plays the game's sound effects and background music.
///
/// Two AudioSources live on the same GameObject:
/// - sfxSource  → PlayOneShot for overlapping one-shots (gunfire, hits)
/// - musicSource → Play on a single looping clip (background track)
///
/// Keeping them separate means stopping or swapping the music track
/// never cuts off an in-flight SFX, and adjusting music volume doesn't
/// fight with per-shot volumes.
///
/// Contract: every method is safe to call at any time, in any order.
/// A missing clip is a warning and a no-op, never an exception.
[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    [Header("Clips: SFX")]
    [SerializeField] AudioClip turretShot;
    [SerializeField] AudioClip enemyHit;
    [SerializeField] AudioClip enemyDied;
    [SerializeField] AudioClip crystalDamage;
    [SerializeField] AudioClip gameOver;

    [Header("Clips: Music")]
    [SerializeField] AudioClip backgroundMusic;
    [SerializeField] AudioClip lowHealthBackgroundMusic;

    [Header("SFX Levels")]
    [Tooltip("The shot fires several times a second, so it wants to sit well under the others.")]
    [Range(0f, 1f)][SerializeField] float turretShotVolume = 0.35f;
    [Tooltip("Plays as often as the shot does. Quiet enough to read as an echo of it, not a second event.")]
    [Range(0f, 1f)][SerializeField] float enemyHitVolume = 0.3f;
    [Tooltip("One per kill, so it can afford to be heard over the chatter of hits.")]
    [Range(0f, 1f)][SerializeField] float enemyDiedVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] float crystalDamageVolume = 0.8f;
    [Range(0f, 1f)][SerializeField] float gameOverVolume = 0.9f;

    [Header("Music Level")]
    [Range(0f, 1f)][SerializeField] float musicVolume = 0.4f;

    AudioSource sfxSource;
    AudioSource musicSource;

    public static AudioPlayer Instance { get; private set; }

    void InitializeSources()
    {
        // GetComponents grabs every AudioSource on this GameObject.
        // The first is the one RequireComponent guarantees; the second
        // is the one we added in the Editor for music.  If someone
        // forgets to add it, AddComponent creates it as a fallback.
        var sources = GetComponents<AudioSource>();
        sfxSource   = sources[0];
        musicSource = sources.Length > 1
            ? sources[1]
            : gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake   = false;
        musicSource.playOnAwake = false;
        musicSource.loop        = true;
    }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        InitializeSources();
    }

    // ── SFX (fire-and-forget, overlapping) ──────────────────────

    public void PlayTurretShot()    => PlaySFX(turretShot,    turretShotVolume,    nameof(turretShot));

    public void PlayEnemyHit()      => PlaySFX(enemyHit,      enemyHitVolume,      nameof(enemyHit));

    public void PlayEnemyDied()     => PlaySFX(enemyDied,     enemyDiedVolume,     nameof(enemyDied));

    public void PlayCrystalDamage() => PlaySFX(crystalDamage, crystalDamageVolume, nameof(crystalDamage));

    public void PlayGameOver()      => PlaySFX(gameOver,      gameOverVolume,      nameof(gameOver));

    public void PlayBGM() => PlayMusic(backgroundMusic);

    public void PlayBGMLowHealth() => PlayMusic(lowHealthBackgroundMusic);

    void PlaySFX(AudioClip clip, float volume, string slot)
    {
        if (clip == null)
        {
            Debug.LogWarning($"{name}: no clip assigned in the '{slot}' slot, so nothing played.", this);
            return;
        }

        sfxSource.PlayOneShot(clip, volume);
    }

    // ── Music (one track at a time, looping) ────────────────────

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        musicSource.clip   = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();
}