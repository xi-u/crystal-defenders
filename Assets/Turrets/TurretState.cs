/// <summary>
/// The base class for all turret states
/// </summary>
public abstract class TurretState
{
    protected readonly TurretBrain turret;

    protected TurretState(TurretBrain turret)
    {
        this.turret = turret;
    }

    public virtual void Enter() { }
    public abstract void Tick();
    public virtual void Exit() { } 
}