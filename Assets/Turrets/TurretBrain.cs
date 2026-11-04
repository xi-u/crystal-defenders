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

    private TurretState _current;

    private void OnEnable()
    {
        ChangeState(new TurretIdleState(this));
    }

    public void ChangeState(TurretState next)
    {
        _current?.Exit();
        _current = next;
        _current.Enter();
    }
    
    void Update() => _current.Tick();

    public float ReloadTime => reloadTime;
    public float FireInterval => fireInterval;
}