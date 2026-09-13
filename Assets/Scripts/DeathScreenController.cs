using UnityEngine;
using TMPro;

/// <summary>
/// Shows the death screen when PlayerVitals reports death, following the
/// same "panel GameObject toggled via SetActive" pattern MenuController.cs
/// already uses for the pause menu, so it fits the project's existing style.
///
/// Respawn reuses SaveController.LoadGame() to move the player back to their
/// last saved position - no separate respawn-point system needed. Inventory
/// is never touched anywhere in this flow, so items are naturally kept.
/// </summary>
public class DeathScreenController : MonoBehaviour
{
    [Header("UI")]
    public GameObject deathScreenPanel;
    [Tooltip("Shown as: \"You Died! Cause of Death: {cause}\"")]
    public TextMeshProUGUI causeOfDeathText;
    [Tooltip("Wire this button's OnClick to DeathScreenController.Respawn().")]
    public GameObject respawnButton;

    [Header("Respawn")]
    [Tooltip("Used to move the player back to their last saved position on respawn.")]
    public SaveController saveController;

    [Header("Player (disabled on death, re-enabled on respawn)")]
    public MonoBehaviour[] scriptsToDisableOnDeath; // e.g. PlayerMovement, PlayerPunch

    private void Start()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }

        // Subscribed here instead of OnEnable: Unity doesn't guarantee
        // PlayerVitals.Awake() (which sets Instance) runs before this
        // object's OnEnable, so subscribing there could silently miss
        // PlayerVitals.Instance being null. Start() runs after every
        // object's Awake, so Instance is guaranteed to be set by now.
        if (PlayerVitals.Instance != null)
        {
            PlayerVitals.Instance.OnDeath += HandleDeath;
        }
        else
        {
            Debug.LogError("[DeathScreenController] No PlayerVitals found in the scene - death screen will never show.");
        }
    }

    private void OnDisable()
    {
        if (PlayerVitals.Instance != null)
        {
            PlayerVitals.Instance.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath(string cause)
    {
        if (causeOfDeathText != null)
        {
            causeOfDeathText.text = $"You Died!\nCause of Death: {cause}";
        }

        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(true);
        }

        SetPlayerScriptsEnabled(false);
    }

    /// <summary>
    /// Hook this up to the Respawn button's OnClick in the Inspector.
    /// </summary>
    public void Respawn()
    {
        if (saveController != null)
        {
            // Moves the player Transform back to the last saved position -
            // same method used for a normal game load, so respawn location
            // always matches "last saved location" as specified.
            saveController.LoadGame();
        }
        else
        {
            Debug.LogWarning("[DeathScreenController] No SaveController assigned - player will respawn in place.");
        }

        if (PlayerVitals.Instance != null)
        {
            PlayerVitals.Instance.Respawn();
        }

        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }

        SetPlayerScriptsEnabled(true);
    }

    private void SetPlayerScriptsEnabled(bool enabled)
    {
        foreach (MonoBehaviour script in scriptsToDisableOnDeath)
        {
            if (script != null) script.enabled = enabled;
        }
    }
}