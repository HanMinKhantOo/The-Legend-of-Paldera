using System.Collections;
using UnityEngine;

/// <summary>
/// Minecraft-style hit flash: briefly tints the sprite red (with an
/// optional flicker) whenever the enemy takes damage, then fades back to
/// its normal color. Useful as a stand-in for a proper Hurt animation when
/// no hurt sprite sheet exists yet (like the Boss right now) - or can be
/// layered on top of a real Hurt animation for extra impact on any enemy.
///
/// Purely additive: reads EnemyHealth's existing OnDamaged event, doesn't
/// touch EnemyHealth, EnemyAI, or the Animator at all.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class DamageFlash : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave empty to auto-find the first SpriteRenderer on this object or its children.")]
    public SpriteRenderer targetRenderer;

    [Header("Flash Settings")]
    public Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [Tooltip("How many times it flickers red-then-normal per hit (2 = red, normal, red, normal).")]
    [Range(1, 5)]
    public int flickerCount = 2;
    [Tooltip("Total time (seconds) the whole flicker sequence takes.")]
    public float flashDuration = 0.25f;

    private EnemyHealth health;
    private Color originalColor;
    private Coroutine flashRoutine;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetRenderer != null)
            originalColor = targetRenderer.color;
    }

    private void OnEnable()
    {
        if (health != null) health.OnDamaged += HandleDamaged;
    }

    private void OnDisable()
    {
        if (health != null) health.OnDamaged -= HandleDamaged;
    }

    private void HandleDamaged()
    {
        if (targetRenderer == null) return;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        float segmentTime = flashDuration / (flickerCount * 2f);

        for (int i = 0; i < flickerCount; i++)
        {
            targetRenderer.color = flashColor;
            yield return new WaitForSeconds(segmentTime);

            targetRenderer.color = originalColor;
            yield return new WaitForSeconds(segmentTime);
        }

        targetRenderer.color = originalColor;
        flashRoutine = null;
    }

    private void OnDestroy()
    {
        // Safety: if this gets destroyed mid-flash, don't leave a stray
        // reference dangling (the SpriteRenderer itself will be destroyed
        // along with the GameObject anyway, but this avoids edge cases if
        // targetRenderer somehow points elsewhere).
        if (flashRoutine != null) StopCoroutine(flashRoutine);
    }
}
