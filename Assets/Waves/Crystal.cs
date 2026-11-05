using System.Collections;
using UnityEngine;

/// <summary>
/// The objective the enemies attack and the turrets defend.
/// It owns its health and nothing else owns it: damage only ever happens
/// through TakeDamage, so there is exactly one place where the number changes.
///
/// Assignment 3 turns the "health changed" and "crystal destroyed" moments
/// into events. Right now nothing is broadcast. The WaveDirector finds out
/// by asking (polling IsDestroyed every frame), which is the pain that
/// assignment motivates. Notice how the hit flash below is wired the "easy"
/// way for the same reason: the crystal has to remember to trigger its own
/// feedback inside TakeDamage. With an event, anything could listen without
/// TakeDamage knowing it exists.
/// </summary>
public class Crystal : MonoBehaviour
{
    [SerializeField] int maxHealth = 10;

    [Header("Hit feedback")]
    [SerializeField] Color flashColor = Color.red;
    [Tooltip("Seconds the crystal stays tinted after a hit.")]
    [SerializeField] float flashDuration = 0.12f;

    [Header("Feedback")]
    [Tooltip("Who makes the noise. The crystal has to be handed one and has to remember to call it.")]
    // URP's Lit shader stores tint in _BaseColor, not the old _Color.
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    int currentHealth;
    Renderer visual;
    MaterialPropertyBlock propertyBlock;
    Color normalColor;
    Coroutine flashRoutine;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDestroyed => currentHealth <= 0;

    void Awake()
    {
        currentHealth = maxHealth;

        visual = GetComponent<Renderer>();
        // A MaterialPropertyBlock tints this one renderer without creating a
        // material instance, so the shared GreyBox_Crystal.mat asset is never
        // modified, and the tint cannot leak out of play mode into the project.
        propertyBlock = new MaterialPropertyBlock();
        normalColor = visual.sharedMaterial.GetColor(BaseColorId);
    }

    public void TakeDamage(int amount)
    {
        if (IsDestroyed) return;            // already dead: ignore late hits

        currentHealth = Mathf.Max(0, currentHealth - amount);

        AudioPlayer.Instance?.PlayCrystalDamage();

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());

        Debug.Log($"Crystal hit for {amount}. Health: {currentHealth}/{maxHealth}", this);
    }

    IEnumerator FlashRoutine()
    {
        SetTint(flashColor);
        yield return new WaitForSeconds(flashDuration);
        SetTint(normalColor);
        flashRoutine = null;

        // The crystal vanishes on the final hit, but only after the last flash
        // has been seen. Hiding it a frame earlier would swallow the feedback.
        if (IsDestroyed) gameObject.SetActive(false);
    }

    void SetTint(Color color)
    {
        propertyBlock.SetColor(BaseColorId, color);
        visual.SetPropertyBlock(propertyBlock);
    }
}
