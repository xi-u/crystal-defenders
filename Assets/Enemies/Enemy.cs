using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// A single enemy. Walks toward the Crystal and damages it once it is
/// close enough.
public class Enemy : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] int maxHealth = 30;

    [Header("Movement")]
    [Tooltip("World units per second.")]
    [SerializeField] float moveSpeed = 2.5f;

    [Header("Attack")]
    [Tooltip("Distance from the crystal's centre at which the enemy stops and starts hitting it.")]
    [SerializeField] float attackRange = 2f;
    [Tooltip("Seconds between hits.")]
    [SerializeField] float attackInterval = 1f;
    [SerializeField] int damagePerHit = 1;

    [Header("Hit feedback")]
    [SerializeField] Color flashColor = Color.red;
    [Tooltip("Seconds the enemy stays tinted after being shot.")]
    [SerializeField] float flashDuration = 0.1f;
    [Tooltip("How far a hit shoves the enemy back along its own path.")]
    [SerializeField] float staggerDistance = 0.35f;
    [Tooltip("Seconds an enemy is stunned after a hit: it cannot walk or attack.")]
    [SerializeField] float staggerDuration = 0.2f;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    int currentHealth;
    Crystal target;
    float nextAttackTime;
    float staggeredUntil;

    Renderer visual;
    MaterialPropertyBlock propertyBlock;
    Color normalColor;
    Coroutine flashRoutine;

    public int CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;

    private static readonly List<Enemy> alive = new();
    public static IReadOnlyList<Enemy> Alive => alive;

    /// How many enemies have been killed this run. Static, like the Alive list.
    public static int TotalKilled { get; private set; }

    public static void ResetKills() => TotalKilled = 0;

    private void OnEnable()
    {
        if (!Enemy.alive.Contains(this))
        {
            Enemy.alive.Add(this);
        }
    }

    private void OnDisable()
    {
        Enemy.alive.Remove(this);
    }

    void Awake()
    {
        currentHealth = maxHealth;

        visual = GetComponentInChildren<Renderer>();
        // MaterialPropertyBlock: see Crystal.cs for why.
        propertyBlock = new MaterialPropertyBlock();
        normalColor = visual.sharedMaterial.GetColor(BaseColorId);
    }

    /// Called by the SpawnZone right after Instantiate. The enemy is handed
    /// its target rather than looking one up itself.
    public void Initialise(Crystal crystal)
    {
        target = crystal;
    }

    /// <param name="hitDirection">The direction the round was travelling when it
    /// connected. The enemy is shoved this way, so a side-on hit knocks it
    /// sideways instead of always straight back down its own lane.</param>
    public void TakeDamage(int amount, Vector3 hitDirection)
    {
        if (!IsAlive) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            TotalKilled++;
            Destroy(gameObject);
            return;
        }

        hitDirection.y = 0f;                   // stay flat: this is a top-down sim
        Vector3 shove = hitDirection.sqrMagnitude > 0.0001f
            ? hitDirection.normalized
            : -transform.forward;              // no usable direction: push it back down its lane
        transform.position += shove * staggerDistance;
        staggeredUntil = Time.time + staggerDuration;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    void Update()
    {
        if (target == null || target.IsDestroyed) return;

        if (Time.time < staggeredUntil)
        {
            nextAttackTime = Time.time + attackInterval;
            return;
        }

        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0f;                       // stay flat: this is a top-down sim
        float distance = toTarget.magnitude;

        if (distance > attackRange)
        {
            Vector3 direction = toTarget / distance;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.forward = direction;     // face where it is walking

            nextAttackTime = Time.time + attackInterval;
        }
        else if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackInterval;
            target.TakeDamage(damagePerHit);
        }
    }

    IEnumerator FlashRoutine()
    {
        SetTint(flashColor);
        yield return new WaitForSeconds(flashDuration);
        SetTint(normalColor);
        flashRoutine = null;
    }

    void SetTint(Color color)
    {
        propertyBlock.SetColor(BaseColorId, color);
        visual.SetPropertyBlock(propertyBlock);
    }
}