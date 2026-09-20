using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach alongside RuinsInteractable on the Yggdrasil Tree (hook this
/// script's TryActivate() into RuinsInteractable's "On Interact" UnityEvent
/// in the Inspector - same wiring pattern as GolemAltarTrigger uses for the
/// altar, no changes to RuinsInteractable needed).
///
/// Reuses existing systems rather than duplicating them:
/// - InventoryController.GetItemCount for the gem check (same as
///   GolemAltarTrigger already does for its 4 gems).
/// - DialogueUI.Instance.Show(...) for both the missing-gem line and the
///   farewell speech, same singleton pattern every other NPC/altar uses.
/// - SceneManager.LoadScene(...) for the actual "return home" transition,
///   same call MainMenuController already makes.
///
/// Only the fade-to-black visual is new, since nothing existing does that.
/// </summary>
public class YggdrasilTreeEnding : MonoBehaviour
{
    [Header("Requirement")]
    [Tooltip("The item the player needs to be carrying to trigger the ending.")]
    public ItemType requiredGem = ItemType.GemRainbow;

    [Header("Inventory")]
    public InventoryController inventoryController;
    [Tooltip("Whether triggering the ending removes the gem from the player's inventory. Off by default, same deliberate-choice reasoning GolemAltarTrigger uses for its gems - doesn't matter for an ending, but here if you want it.")]
    public bool consumeGemOnActivation = false;

    [Header("Dialogue")]
    [Tooltip("Shown as the speaker name for both the missing-gem line and the farewell lines - the player's own name, since these are inner thoughts.")]
    public string speakerName = "You";

    [TextArea]
    [Tooltip("Shown if the player interacts without the gem yet.")]
    public string missingGemLine = "This isn't enough. I need something more to open the way home.";

    [Header("Ending Sequence")]
    [TextArea(2, 5)]
    [Tooltip("Farewell lines shown one at a time, in order, before the world fades out.")]
    public string[] endingLines = new string[]
    {
        "The gem pulses in my hand, warm and alive.",
        "The tree remembers me. It's time to go home.",
    };
    [Tooltip("Seconds each ending line stays on screen before advancing to the next.")]
    public float lineDisplayDuration = 3f;

    [Header("Player (disabled during the ending)")]
    [Tooltip("e.g. PlayerMovement, PlayerPunch - same pattern DeathScreenController uses so the player can't wander off or swing mid-ending.")]
    public MonoBehaviour[] scriptsToDisableDuringEnding;

    [Header("Fade Out")]
    [Tooltip("A full-screen black Image on a CanvasGroup, alpha 0 by default, sitting above every other UI. Left empty just skips straight to the scene load.")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 2f;

    [Header("Scene Transition")]
    [Tooltip("The scene to load once the ending finishes - your main menu, a dedicated credits scene, whatever \"back in his original world\" means for you.")]
    public string sceneToLoad = "MainMenu";

    private bool triggered = false;

    /// <summary>
    /// Hook this to RuinsInteractable's "On Interact" UnityEvent in the
    /// Inspector. Safe to call repeatedly - does nothing once already
    /// triggered, and re-checks the gem fresh every time before that.
    /// </summary>
    public void TryActivate()
    {
        if (triggered) return;

        if (inventoryController == null)
        {
            Debug.LogError("[YggdrasilTreeEnding] No InventoryController assigned.");
            return;
        }

        if (inventoryController.GetItemCount(requiredGem) <= 0)
        {
            ShowMissingGemDialogue();
            return;
        }

        triggered = true;

        if (consumeGemOnActivation)
        {
            inventoryController.TryRemoveItem(requiredGem, 1);
        }

        StartCoroutine(PlayEndingSequence());
    }

    private void ShowMissingGemDialogue()
    {
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.Show(speakerName, missingGemLine);
        }
        else
        {
            Debug.LogWarning("[YggdrasilTreeEnding] No DialogueUI.Instance found in scene.");
        }
    }

    private IEnumerator PlayEndingSequence()
    {
        SetPlayerScriptsEnabled(false);

        if (DialogueUI.Instance != null)
        {
            foreach (string line in endingLines)
            {
                DialogueUI.Instance.Show(speakerName, line);
                yield return new WaitForSeconds(lineDisplayDuration);
            }

            DialogueUI.Instance.Close();
        }

        yield return StartCoroutine(FadeToBlack());

        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.blocksRaycasts = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    private void SetPlayerScriptsEnabled(bool enabled)
    {
        foreach (MonoBehaviour script in scriptsToDisableDuringEnding)
        {
            if (script != null) script.enabled = enabled;
        }
    }
}
