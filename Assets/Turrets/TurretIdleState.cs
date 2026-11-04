/// <summary>
/// The idle state of the turret. In this state, the turret is not actively aiming or firing at any target.
/// </summary>
class TurretIdleState : TurretState
{
    public TurretIdleState(TurretBrain turret) : base(turret) { }

    public override void Tick()
    {
        Enemy target = turret.FindNearestEnemy();
        
        // If no target is found, remain in the idle state.
        if (target == null)
        {
            return;
        }

        // If a target is found, transition to the aiming state.
        turret.ChangeState(new TurretAimingState(turret, target));
    }
}
