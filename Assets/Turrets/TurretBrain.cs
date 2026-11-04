using UnityEngine;

/// <summary>
/// The turret's decision layer. Decides WHEN to aim, fire, and pick a new
/// target. TurretBody handles HOW.
/// </summary>
public class TurretBrain : TurretBody
{
    [Tooltip("Seconds a reload takes.")]
    [SerializeField] float reloadTime = 1.5f;

    [Header("Firing")]
    [Tooltip("Seconds between shots. The turret does NOT enforce this. Read it and pace your own shots.")]
    [SerializeField] float fireInterval = 0.4f;

    Enemy target;
    float nextFireTime;


    void Update()
    {
        target = FindNearestEnemy();

        if (target == null)
        {
            SetFiringIndicator(false);
            return;
        }

        bool aimed = RotateTowards(target.transform, Time.deltaTime);
        if (!aimed)
        {
            SetFiringIndicator(false);
            return;
        }

        SetFiringIndicator(true);
        // FireOnce has no cooldown of its own. Pacing is the caller's job.
        if (Time.time >= nextFireTime)
        {
            FireOnce(target.transform);
            nextFireTime = Time.time + fireInterval;
        }
    }

}
