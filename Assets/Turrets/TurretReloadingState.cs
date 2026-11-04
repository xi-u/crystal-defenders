using UnityEngine;

/// <summary>
/// The reloading state of the turret. In this state, the turret is actively reloading its clip and cannot fire at any target.
/// </summary>
class TurretReloadingState : TurretState
{
    float _reloadTimer;

    public TurretReloadingState(TurretBrain turret) : base(turret) { }

    /// <summary>
    /// Called when the turret enters the reloading state. This method initializes the reload timer to the turret's reload time.
    /// </summary>
    public override void Enter()
    {
        _reloadTimer = turret.ReloadTime;
    }

    public override void Tick()
    {
        // Decrease the reload timer by the time elapsed since the last frame
        _reloadTimer -= Time.deltaTime;
        
        if (_reloadTimer > 0f)
        {
            return;
        }

        // Once the reload timer reaches zero, refill the turret's clip and transition back to the idle state.
        turret.RefillClip();
        turret.ChangeState(new TurretIdleState(turret));
    }
}
