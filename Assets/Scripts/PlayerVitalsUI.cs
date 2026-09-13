using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the HP/Hunger HUD shown in the reference mockup:
/// - White circular player portrait (Assets/Characters/PlayerPortrait.png).
/// - Red HP bar with "current/max" text, gray showing missing HP.
/// - Yellow Hunger bar with "current/max" text, gray showing missing Hunger.
///
/// Each bar is built from two stacked Image(Filled) components sharing the
/// same rect: a gray "empty" background Image that's always fully filled,
/// and a colored "value" Image on top whose fillAmount = current/max. This
/// way the "missing" portion always reads as gray without any extra math -
/// the gray layer is just always there, only ever covered up.
///
/// Values are smoothed toward PlayerVitals' real numbers each frame (rather
/// than snapping instantly) so gradual hunger/HP drain reads as a smooth
/// animation instead of a stepped/ticking bar.
/// </summary>
public class PlayerVitalsUI : MonoBehaviour
{
    [Header("Bars (Image type = Filled, Horizontal)")]
    [Tooltip("The red fill Image for HP. Its background/gray counterpart should be a sibling Image behind it, already fully filled.")]
    public Image hpFillImage;
    [Tooltip("The yellow fill Image for Hunger. Its background/gray counterpart should be a sibling Image behind it, already fully filled.")]
    public Image hungerFillImage;

    [Header("Text")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI hungerText;

    [Header("Smoothing")]
    [Tooltip("How quickly the displayed bar catches up to the real value. Higher = snappier.")]
    public float smoothSpeed = 4f;

    private float displayedHP;
    private float displayedHunger;

    private void OnEnable()
    {
        if (PlayerVitals.Instance != null)
        {
            displayedHP = PlayerVitals.Instance.CurrentHP;
            displayedHunger = PlayerVitals.Instance.CurrentHunger;
            RefreshImmediate();
        }
    }

    private void Update()
    {
        PlayerVitals vitals = PlayerVitals.Instance;
        if (vitals == null) return;

        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
        displayedHP = Mathf.Lerp(displayedHP, vitals.CurrentHP, t);
        displayedHunger = Mathf.Lerp(displayedHunger, vitals.CurrentHunger, t);

        // Snap once close enough so the numeric text settles on a whole
        // number instead of hovering forever (e.g. "74.998").
        if (Mathf.Abs(displayedHP - vitals.CurrentHP) < 0.05f) displayedHP = vitals.CurrentHP;
        if (Mathf.Abs(displayedHunger - vitals.CurrentHunger) < 0.05f) displayedHunger = vitals.CurrentHunger;

        ApplyToUI(vitals.maxHP, vitals.maxHunger);
    }

    private void RefreshImmediate()
    {
        ApplyToUI(PlayerVitals.Instance.maxHP, PlayerVitals.Instance.maxHunger);
    }

    private void ApplyToUI(float maxHP, float maxHunger)
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = maxHP > 0f ? displayedHP / maxHP : 0f;
        }
        if (hungerFillImage != null)
        {
            hungerFillImage.fillAmount = maxHunger > 0f ? displayedHunger / maxHunger : 0f;
        }

        if (hpText != null)
        {
            hpText.text = $"{Mathf.RoundToInt(displayedHP)}/{Mathf.RoundToInt(maxHP)}";
        }
        if (hungerText != null)
        {
            hungerText.text = $"{Mathf.RoundToInt(displayedHunger)}/{Mathf.RoundToInt(maxHunger)}";
        }
    }
}
