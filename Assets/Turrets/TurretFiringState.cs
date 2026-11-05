using UnityEngine;

/// <summary>
/// The firing state of the turret. In this state, the turret is actively shooting at a target enemy.
/// </summary>
class TurretFiringState : TurretState
{
    readonly Enemy _target;
    float _nextFireTime;

    public TurretFiringState(TurretBrain turret, Enemy target) : base(turret)
    {
        _target = target;
    }

    /// <summary>
    /// Called when the turret enters the firing state. This method sets the firing indicator to true and initializes the
    /// next fire time.
    /// </summary>
    public override void Enter()
    {
        turret.SetFiringIndicator(true);
        _nextFireTime = Time.time;
    }

    public override void Tick()
    {
        // If the turret's clip is empty, transition to the reloading state.
        if (turret.ClipEmpty)
        {
            turret.ChangeState(new TurretReloadingState(turret));
            return;
        }

        // If the target is no longer in range, transition to the idle state.
        if (!turret.EnemyInRange(_target))
        {
            turret.ChangeState(new TurretIdleState(turret));
            return;
        }

        // Rotate the turret towards the target.
        turret.RotateTowards(_target.transform, Time.deltaTime);

        // If it not time to fire yet, return early.
        if (Time.time < _nextFireTime)
        {
            return;
        }

        // Fire at the target and update the next fire time.
        turret.FireOnce(_target.transform);
        float fireInterval = turret.FireInterval;
        if (turret.IsCrystalLowHealth)
            fireInterval *= 0.75f;

        _nextFireTime = Time.time + fireInterval;
    }

    /// <summary>
    /// Called when the turret exits the firing state. Sets the firing indicator to fals.
    /// </summary>
    public override void Exit()
    {
        turret.SetFiringIndicator(false);
    }
}