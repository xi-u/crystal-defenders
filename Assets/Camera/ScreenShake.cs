using System.Collections;
using UnityEngine;

/// Shakes the camera to sell impacts. Lives on the Main Camera and does
/// nothing until Shake() is called. Zero per-frame cost when idle.
///
/// The shake works by adding a random offset to the camera's LOCAL position
/// each frame, then snapping it back to the original position when the shake
/// ends. Because it uses localPosition, it plays nicely with any parent rig
/// (a follow cam, a Cinemachine brain, etc.) without fighting it.
public class ScreenShake : MonoBehaviour
{
    [Header("Defaults")]
    [Tooltip("How strong the shake is at its peak (world units of offset).")]
    [SerializeField] float defaultIntensity = 0.15f;
    [Tooltip("How long the shake lasts (seconds).")]
    [SerializeField] float defaultDuration = 0.2f;

    public static ScreenShake Instance { get; private set; }

    Vector3 originalLocalPos;
    Coroutine shakeRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }

        originalLocalPos = transform.localPosition;
    }

    /// Triggers a shake with the inspector defaults.
    /// Safe to call while a shake is already running.
    public void Shake() => Shake(defaultIntensity, defaultDuration);

    /// Triggers a shake with custom intensity and duration.
    /// Use the overload when different events deserve different strengths:
    /// a crystal hit might want a harder shake than an enemy death, for instance.
    public void Shake(float intensity, float duration)
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // The shake decays linearly: full strength at the start,
            // zero at the end. This feels like an impact that settles,
            // rather than a constant vibration that cuts off abruptly.
            float remaining = 1f - (elapsed / duration);
            float currentIntensity = intensity * remaining;

            // Random point inside a circle keeps the shake on the XY plane
            // (the camera's view plane in a top-down game). A sphere offset
            // would push the camera along Z, which reads as zoom jitter.
            Vector2 offset = Random.insideUnitCircle * currentIntensity;
            transform.localPosition = originalLocalPos + new Vector3(offset.x, offset.y, 0f);

            yield return null;
        }

        // Snap back cleanly. No accumulated drift.
        transform.localPosition = originalLocalPos;
        shakeRoutine = null;
    }
}
