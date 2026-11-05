using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

/// An autonomous defense turret. 
///
/// This file is the turret's VERB layer. It knows HOW to turn, HOW to shoot
/// and HOW to show that it is firing it never decides WHEN.
/// 
/// Contract for the whole class: every public member below is safe to call in
/// any order, on any frame. 
public class TurretBody : MonoBehaviour
{
    [Header("Aiming")]
    [Tooltip("The part that rotates. Leave empty to rotate this whole object.")]
    [SerializeField] Transform _head;
    [Tooltip("Degrees per second.")]
    [SerializeField] float _turnSpeed = 180f;
    [Tooltip("How close the aim must be, in degrees, to count as on target.")]
    [SerializeField] float _aimTolerance = 3f;

    [SerializeField] int _clipSize = 6;
    [SerializeField] int _damage = 10;
    [Tooltip("Visual tracer spawned per shot. Optional. The shot lands with or without it.")]
    [SerializeField] Projectile _projectilePrefab;
    [Tooltip("Where tracers spawn from. Falls back to the head if empty.")]
    [SerializeField] Transform _muzzle;

    [Header("Feedback")]
    [SerializeField] Renderer _headRenderer;
    [SerializeField] Color _firingColor = Color.red;

    [Tooltip("How far the turret can see an enemy.")]
    [SerializeField] float range = 14f;

    // URP's Lit shader stores tint in _BaseColor, not the old _Color.
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    int _ammo;
    Color _idleColor;
    MaterialPropertyBlock _propertyBlock;

    void Awake()
    {
        _ammo = _clipSize;

        if (_head == null) _head = transform;
        if (_muzzle == null) _muzzle = _head;

        // MaterialPropertyBlock: see Crystal.cs for why.
        _propertyBlock = new MaterialPropertyBlock();
        if (_headRenderer != null)
        {
            _idleColor = _headRenderer.sharedMaterial.GetColor(BaseColorId);
        }

        if (_projectilePrefab == null)
        {
            Debug.LogError($"{name}: no projectile prefab assigned. This turret cannot damage anything.", this);
        }
    }

    /// Turns the head one frame's worth toward <paramref name="t"/> and reports
    /// whether it is now on target, within the aim tolerance.
    ///
    /// Contract: yaw only. The head never tilts, so a target at a different
    /// height is still aimable. Returns false immediately when
    /// <paramref name="t"/> is null, which makes "did my target die?" and
    /// "am I aimed?" the same question. Call it every frame while you want to
    /// track; it changes nothing permanent and can be abandoned at any time.
    /// <param name="dt">Delta time to step with. Pass the value your state was
    /// given. The turret never reads Time.deltaTime itself, so a state can be
    /// stepped at any rate you like without this method fighting it.</param>
    /// <returns>True when the remaining angle is within the aim tolerance.</returns>
    public bool RotateTowards(Transform t, float dt)
    {
        if (t == null) return false;

        Vector3 toTarget = t.position - _head.position;
        toTarget.y = 0f;                                    // flatten: yaw only
        if (toTarget.sqrMagnitude < 0.0001f) return true;   // target is on top of us

        Quaternion desired = Quaternion.LookRotation(toTarget, Vector3.up);
        _head.rotation = Quaternion.RotateTowards(_head.rotation, desired, _turnSpeed * dt);

        return Quaternion.Angle(_head.rotation, desired) <= _aimTolerance;
    }

    /// Fires exactly one shot at <paramref name="target"/>: one round spent,
    /// one round sent on its way.
    public void FireOnce(Transform target)
    {
        if (ClipEmpty)
        {
            Debug.LogWarning(
                $"{name}: FireOnce was called with an empty clip, so nothing was fired. " +
                "Turret needs to reload.", this);
            return;
        }

        if (target == null) return;
        if (_projectilePrefab == null) return;   // already reported in Awake; never throw

        _ammo--;
        
        AudioPlayer.Instance?.PlayTurretShot();

        Vector3 aimDirection = target.position - _muzzle.position;
        Projectile round = Instantiate(_projectilePrefab, _muzzle.position, _head.rotation);
        round.Launch(target, aimDirection, _damage);
    }

    public Enemy FindNearestEnemy()
    {
        Enemy nearest = null;
        float nearestSqr = range * range;

        foreach (Enemy enemy in Enemy.Alive)
        {
            float sqr = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqr <= nearestSqr)
            {
                nearestSqr = sqr;
                nearest = enemy;
            }
        }

        return nearest;
    }

    public bool EnemyInRange(Enemy enemy)
    {
        if (!enemy.IsAlive)
        {
            return false;
        }
        
        float nearestSqr = range * range;
        float sqr = (enemy.transform.position - transform.position).sqrMagnitude;

        return sqr <= nearestSqr;

    }

    /// Tints the head to show the turret is firing, or restores its normal
    /// colour. Purely cosmetic: never affects aim, ammo or damage.
    public void SetFiringIndicator(bool on)
    {
        if (_headRenderer == null) return;

        _propertyBlock.SetColor(BaseColorId, on ? _firingColor : _idleColor);
        _headRenderer.SetPropertyBlock(_propertyBlock);
    }

    /// The turret never refills itself: once empty it stays empty until
    /// <see cref="RefillClip"/> is called.
    public bool ClipEmpty => _ammo <= 0;

    /// No reload delay. Callers add their own wait.
    public void RefillClip() => _ammo = _clipSize;

}
