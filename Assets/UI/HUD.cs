using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages HUD visibility. Has helper methods for displaying crystal health,
/// to be wired via events in a later assignment.
/// </summary>
public class HUD : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] Crystal crystal;
    [SerializeField] WaveDirector director;

    [Header("Widgets")]
    [SerializeField] GameObject root;
    [SerializeField] Text killText;
    [SerializeField] Image crystalHealthFill;

    [Header("Bar colours")]
    [SerializeField] Color healthyColor = new Color(0.35f, 0.85f, 1f);
    [SerializeField] Color lowHealthColor = new Color(1f, 0.25f, 0.2f);
    [SerializeField] Color flashColor = Color.white;
    [SerializeField] float flashDuration = 0.25f;

    Color baseColor;
    Coroutine flashRoutine;

    void Awake()
    {
        baseColor = healthyColor;
        crystalHealthFill.color = baseColor;
    }

    void Update()
    {
        // The HUD has nothing to say before the player presses Start.
        bool shouldShow = director != null && director.IsRunning;
        SetVisible(shouldShow);
        if (!shouldShow) return;
    }

    void SetVisible(bool visible)
    {
        if (root.activeSelf != visible) root.SetActive(visible);
    }

    /// <summary>
    /// Fills the crystal health bar to the given fraction (0 = empty, 1 = full).
    /// The bar is drawn by moving the fill's right anchor, not by Image's
    /// fillAmount. fillAmount needs a sprite to slice, and every built-in UI
    /// sprite has rounded corners; a plain sprite-less Image is a hard-edged
    /// rectangle. Driving the anchor also means the fill keeps its padding
    /// and rescales with the bar at any screen size, with no width maths here.
    /// </summary>
    void ShowCrystalHealth(float fraction)
    {
        RectTransform fillRect = crystalHealthFill.rectTransform;
        Vector2 anchorMax = fillRect.anchorMax;
        anchorMax.x = Mathf.Clamp01(fraction);
        fillRect.anchorMax = anchorMax;
    }

    void FlashBar()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flashDuration);
            crystalHealthFill.color = Color.Lerp(flashColor, baseColor, t);
            yield return null;
        }
        crystalHealthFill.color = baseColor;
        flashRoutine = null;
    }

    void SetLowHealth(bool low)
    {
        Color target = low ? lowHealthColor : healthyColor;
        if (baseColor == target) return;

        baseColor = target;
        if (flashRoutine == null)
            crystalHealthFill.color = baseColor;
    }
}