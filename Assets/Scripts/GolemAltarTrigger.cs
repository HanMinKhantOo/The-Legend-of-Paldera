using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach alongside RuinsInteractable on the altar object (hook this
/// script's TryActivate() into RuinsInteractable's "On Interact" UnityEvent
/// in the Inspector - no changes to RuinsInteractable needed, it stays the
/// generic "press E in range" trigger it already is).
///
/// Reuses existing systems rather than duplicating them:
/// - InventoryController.GetItemCount / TryRemoveItem for the gem check
///   (already exist - no new inventory-querying code needed).
/// - DialogueUI.Instance.Show(...) for the missing-gems message, same
///   singleton pattern NPCInteractable already uses.
/// - A plain prefab Instantiate for the golem, matching how EnemySpawner
///   spawns enemies; the golem prefab itself should just carry the
///   existing EnemyHealth + EnemyAI components (same as Wolf/Boar), so no
///   bespoke boss-health script is needed either.
///
/// Only the altar's activation VISUAL (sprite swap sequence) is new,
/// since nothing existing does that.
/// </summary>
public class GolemAltarTrigger : MonoBehaviour
{
    [System.Serializable]
    public class RequiredGem
    {
        public ItemType itemType;
        [Tooltip("How this gem is referred to in the missing-gems dialogue line, e.g. \"red\".")]
        public string displayName;
    }

    [Header("Requirements")]
    [Tooltip("The 4 gems needed, in the order they should be listed in dialogue (red, green, blue, yellow).")]
    public List<RequiredGem> requiredGems = new List<RequiredGem>
    {
        new RequiredGem { itemType = ItemType.GemRed, displayName = "red" },
        new RequiredGem { itemType = ItemType.GemGreen, displayName = "green" },
        new RequiredGem { itemType = ItemType.GemBlue, displayName = "blue" },
        new RequiredGem { itemType = ItemType.GemYellow, displayName = "yellow" },
    };

    [Header("Inventory")]
    public InventoryController inventoryController;
    [Tooltip("Whether successfully activating the altar removes the 4 gems from the player's inventory. Off by default - flip this on if you decide the gems should be consumed. Nothing in the project currently establishes either behavior, so this is a deliberate choice left to you.")]
    public bool consumeGemsOnActivation = false;

    [Header("Dialogue (missing gems)")]
    [Tooltip("Shown as the speaker name in the dialogue box - the player's own name, since this is an inner thought, e.g. \"Rowan\".")]
    public string speakerName = "You";

    [Header("Activation Visual")]
    public SpriteRenderer altarRenderer;
    [Tooltip("Played once, in order, on successful activation (the climax + settle-down frames).")]
    public Sprite[] activationSequenceFrames;
    [Tooltip("Seconds each activation frame stays on screen.")]
    public float activationFrameInterval = 0.35f;
    [Tooltip("The 1-2 frames the altar idles between afterward (a steady glow, optionally alternating with a slightly dimmer twin for a soft pulse). Leave just one frame if you don't want a pulsing idle.")]
    public Sprite[] idleLoopFrames;
    [Tooltip("Seconds between each idle-loop frame swap.")]
    public float idlePulseInterval = 1.2f;

    [Header("Golem")]
    public GameObject golemPrefab;
    [Tooltip("Where the golem appears. Leave empty to spawn at this altar's own position.")]
    public Transform golemSpawnPoint;

    private bool activated;

    /// <summary>
    /// Hook this to RuinsInteractable's "On Interact" UnityEvent in the
    /// Inspector. Safe to call repeatedly - does nothing once already
    /// activated, and re-checks gems fresh every time before that.
    /// </summary>
    public void TryActivate()
    {
        if (activated) return;

        if (inventoryController == null)
        {
            Debug.LogError("[GolemAltarTrigger] No InventoryController assigned.");
            return;
        }

        List<string> missing = GetMissingGemNames();

        if (missing.Count > 0)
        {
            ShowMissingGemsDialogue(missing);
            return;
        }

        Activate();
    }

    private List<string> GetMissingGemNames()
    {
        List<string> missing = new List<string>();

        foreach (RequiredGem gem in requiredGems)
        {
            if (inventoryController.GetItemCount(gem.itemType) <= 0)
            {
                missing.Add(gem.displayName);
            }
        }

        return missing;
    }

    private void ShowMissingGemsDialogue(List<string> missing)
    {
        string line = $"I still need {JoinWithAnd(missing)} gems to activate it";

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.Show(speakerName, line);
        }
        else
        {
            Debug.LogWarning("[GolemAltarTrigger] No DialogueUI.Instance found in scene - add the DialogueUI script to your Canvas.");
        }
    }

    /// <summary>"a, b, c and d" - no Oxford comma, matching the examples given.</summary>
    private string JoinWithAnd(List<string> items)
    {
        if (items.Count == 1) return items[0];

        string allButLast = string.Join(", ", items.GetRange(0, items.Count - 1));
        return $"{allButLast} and {items[items.Count - 1]}";
    }

    private void Activate()
    {
        activated = true;

        if (consumeGemsOnActivation)
        {
            foreach (RequiredGem gem in requiredGems)
            {
                inventoryController.TryRemoveItem(gem.itemType, 1);
            }
        }

        StartCoroutine(PlayActivationSequence());
    }

    private IEnumerator PlayActivationSequence()
    {
        if (altarRenderer != null && activationSequenceFrames != null)
        {
            foreach (Sprite frame in activationSequenceFrames)
            {
                altarRenderer.sprite = frame;
                yield return new WaitForSeconds(activationFrameInterval);
            }
        }

        SpawnGolem();

        // Settle into the idle glow loop and stay there for the rest of
        // the scene - the altar stays visibly "activated" even after the
        // golem fight ends, rather than reverting to its dormant look.
        yield return StartCoroutine(IdleLoop());
    }

    private IEnumerator IdleLoop()
    {
        if (altarRenderer == null || idleLoopFrames == null || idleLoopFrames.Length == 0)
            yield break;

        int i = 0;
        while (true)
        {
            altarRenderer.sprite = idleLoopFrames[i % idleLoopFrames.Length];
            i++;
            yield return new WaitForSeconds(idlePulseInterval);
        }
    }

    private void SpawnGolem()
    {
        if (golemPrefab == null)
        {
            Debug.LogWarning("[GolemAltarTrigger] No golemPrefab assigned - altar activated but nothing spawned.");
            return;
        }

        Vector3 spawnPos = golemSpawnPoint != null ? golemSpawnPoint.position : transform.position;
        GameObject golem = Instantiate(golemPrefab, spawnPos, Quaternion.identity);

        // Matches EnemySpawner's own spawn pattern - if the golem prefab
        // carries an EnemyAI (as Wolf/Boar do), give it its home position
        // the same way. Entirely optional: a golem that stands its ground
        // near the altar works fine without this too.
        EnemyAI ai = golem.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.Initialize(spawnPos);
        }
    }
}
