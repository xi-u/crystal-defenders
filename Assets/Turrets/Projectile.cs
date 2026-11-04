using UnityEngine;

/// A single round fired by a turret. It carries the damage: the turret spends a
/// bullet and sends this on its way, and nothing happens to the enemy until this
/// object physically touches it. 
///
/// Two details worth understanding:
///
/// The collider is a TRIGGER: 
/// It ignores everything that is not an Enemy in code.
/// The enemy recoil is scripted in TakeDamage instead of relying on physics.
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 25f;
    [Tooltip("Degrees per second the round may steer toward its target in flight.")]
    [SerializeField] float homingTurnRate = 360f;
    [Tooltip("Safety net: a round never lives longer than this.")]
    [SerializeField] float maxLifetime = 2f;

    Rigidbody body;
    Transform target;
    Vector3 direction;
    int damage;
    float despawnTime;
    bool spent;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    /// Sends the round on its way carrying <paramref name="damage"/>.
    /// <paramref name="fallbackDirection"/> is the heading to use when the target
    /// is already gone. A round whose target died mid-flight keeps travelling
    /// and may legitimately hit whatever it runs into next.
    public void Launch(Transform target, Vector3 fallbackDirection, int damage)
    {
        this.target = target;
        this.damage = damage;

        fallbackDirection.y = 0f;
        direction = fallbackDirection.sqrMagnitude > 0.0001f
            ? fallbackDirection.normalized
            : transform.forward;

        despawnTime = Time.time + maxLifetime;
    }

    // Flight happens in FixedUpdate and through the Rigidbody rather than by
    // writing transform.position: that way the physics engine sweeps the round
    // along its path between steps and can register a contact partway through.
    // Teleporting the transform each frame would let a fast round jump straight
    // over a thin target without ever touching it.
    void FixedUpdate()
    {
        if (Time.time >= despawnTime)
        {
            Destroy(gameObject);
            return;
        }

        if (target != null)
        {
            Vector3 toTarget = target.position - body.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                // Steer rather than snap, so a round cannot turn on the spot and
                // always chase a target it has already flown past.
                direction = Vector3.RotateTowards(
                    direction,
                    toTarget.normalized,
                    homingTurnRate * Mathf.Deg2Rad * Time.fixedDeltaTime,
                    0f);
            }
        }

        body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (spent) return;                                  // one round, one hit

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null) return;                          // scenery: fly on through
        if (!enemy.IsAlive) return;

        spent = true;
        // The round's own heading is the hit direction, so the enemy is shoved
        // the way the bullet was travelling at the moment it connected.
        enemy.TakeDamage(damage, direction);
        Destroy(gameObject);
    }
}
