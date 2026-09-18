using System.Collections;
using UnityEngine;

/// <summary>
/// Floating HP bar shown above an enemy while it's in combat. Purely
/// additive - reads EnemyHealth's public events/fields, doesn't modify
/// EnemyHealth or EnemyAI at all.
///
/// Hidden by default. Appears the moment the enemy takes damage, updates
/// to show current HP, and fades out again after `hideDelay` seconds of no
/// further damage. Disappears immediately (permanently) on death.
///
/// Setup (see integration notes): this expects two child SpriteRenderers
/// using a plain white 1x1 sprite - `background` (tinted dark/black,
/// fixed width) and `fill` (tinted per current HP, scaled on X to show
/// the HP ratio). No custom art required.
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The dark background bar behind the fill. Should NOT be resized at runtime.")]
    public SpriteRenderer background;
    [Tooltip("The colored fill bar. Its transform's X scale is what actually shrinks as HP drops.")]
    public SpriteRenderer fill;

    [Header("Behavior")]
    [Tooltip("Seconds after the last hit before the bar fades back out.")]
    public float hideDelay = 3f;
    [Tooltip("Local Y offset above the enemy's own position (in local space, above its pivot).")]
    public Vector3 offset = new Vector3(0f, 0.9f, 0f);

    [Header("Color by HP % remaining")]
    public Color highColor = new Color(0.35f, 0.85f, 0.35f); // green
    public Color midColor = new Color(0.95f, 0.85f, 0.25f);  // yellow
    public Color lowColor = new Color(0.9f, 0.25f, 0.25f);   // red

    private EnemyHealth health;
    private Coroutine hideRoutine;
    private float fullFillWidth;

    private void Awake()
    {
        health = GetComponentInParent<EnemyHealth>();

        if (fill != null)
            fullFillWidth = fill.transform.localScale.x;

        SetVisible(false);
    }

    private void OnEnable()
    {
        if (health == null) return;
        health.OnDamaged += HandleDamaged;
        health.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (health == null) return;
        health.OnDamaged -= HandleDamaged;
        health.OnDeath -= HandleDeath;
    }

    private void LateUpdate()
    {
        // Keep the bar pinned above the enemy and unrotated, even if the
        // enemy's own sprite/root ever rotates or flips.
        transform.position = health.transform.position + offset;
        transform.rotation = Quaternion.identity;
    }

    private void HandleDamaged()
    {
        UpdateFill();
        SetVisible(true);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private void HandleDeath(EnemyHealth _)
    {
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        SetVisible(false);
    }

    private void UpdateFill()
    {
        if (fill == null || health == null) return;

        float ratio = Mathf.Clamp01(health.CurrentHP / Mathf.Max(0.0001f, health.maxHP));

        Vector3 scale = fill.transform.localScale;
        scale.x = fullFillWidth * ratio;
        fill.transform.localScale = scale;

        fill.color = ratio > 0.5f
            ? Color.Lerp(midColor, highColor, (ratio - 0.5f) * 2f)
            : Color.Lerp(lowColor, midColor, ratio * 2f);
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (background != null) background.enabled = visible;
        if (fill != null) fill.enabled = visible;
    }
}