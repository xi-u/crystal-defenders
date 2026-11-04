using UnityEngine;

/// <summary>
/// The aiming state of the turret. In this state, the turret is actively rotating towards a target enemy.
/// </summary>
class TurretAimingState : TurretState
{
    readonly Enemy _target;

    public TurretAimingState(TurretBrain turret, Enemy target) : base(turret)
    {
        _target = target;
    }

    public override void Tick()
    {
        // If the target is no longer valid, transition back to the idle state.
        if (!turret.EnemyInRange(_target))
        {
            turret.ChangeState(new TurretIdleState(turret));
            return;
        }

        // If the turret has finished rotating towards the target, transition to the firing state.
        if (turret.RotateTowards(_target.transform, Time.deltaTime))
        {
            turret.ChangeState(new TurretFiringState(turret, _target));
        }
    }
}
