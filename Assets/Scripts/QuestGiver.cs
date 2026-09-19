using UnityEngine;

/// <summary>
/// Attach alongside NPCInteractable to turn any NPC into a quest giver that
/// rewards a gem on completion. NPCInteractable automatically detects this
/// component (see its small hook in StartDialogue) and asks it for the
/// dialogue line instead of using its own fixed one - no other change to
/// how the NPC is talked to.
///
/// Two quest types, both reusing systems that already exist:
/// - KillEnemy: listens to EnemyHealth.OnAnyEnemyDeath (a scene-wide static
///   event) and counts kills matching targetEnemyName (EnemyAI.mobName,
///   e.g. "Magical Wolf").
/// - GatherItem: checks InventoryController.GetItemCount/TryRemoveItem
///   directly - no separate tracking needed, since the inventory itself is
///   already the source of truth for "do you have 30 logs".
///
/// The quest auto-starts (no separate accept step) and turns in the moment
/// its condition is met and you talk to the NPC again - talk once to learn
/// what's needed, do it, talk again to collect the gem.
/// </summary>
public class QuestGiver : MonoBehaviour
{
    public enum QuestType { KillEnemy, GatherItem }

    /// <summary>Read-only snapshot for UI - never mutates quest state.</summary>
    public struct Snapshot
    {
        public string title;
        public int current;
        public int required;
        /// <summary>True once current >= required - the objective itself is done, so the UI can show "Complete!".</summary>
        public bool objectiveMet;
        /// <summary>True only once the reward has actually been collected (TurnIn() called) - this is what should tell the UI it's safe to remove the row.</summary>
        public bool turnedIn;
    }

    [Header("Quest Definition")]
    public QuestType questType;
    [Tooltip("Short label shown in the Quest Tracker UI, e.g. \"Wolf Hunt\" or \"Gather Logs\".")]
    public string questTitle = "Quest";

    [Header("Kill Quest Settings")]
    [Tooltip("Must exactly match the target EnemyAI.mobName, e.g. \"Magical Wolf\".")]
    public string targetEnemyName = "Magical Wolf";

    [Header("Gather Quest Settings")]
    public ItemType targetItemType;

    [Header("Shared")]
    [Tooltip("How many kills or items are required.")]
    public int requiredAmount = 10;
    [Tooltip("The gem (or any item) granted on turn-in.")]
    public ItemType rewardItemType;
    public InventoryController inventoryController;

    [Header("Dialogue Lines")]
    [TextArea] public string offerLineTemplate = "I need your help - {progress}. Come back once you have.";
    [TextArea] public string completeLine = "You did it! Here, take this - you've earned it.";
    [TextArea] public string alreadyDoneLine = "Thanks again for your help earlier.";

    private int killCount;
    private bool completed;
    private bool accepted;

    private void OnEnable()
    {
        // Deliberately do NOT register with QuestManager or start listening
        // for kills here - that only happens once the player has actually
        // talked to this NPC (see Accept(), called from GetDialogueLine()).
        // Registering here would show every quest in the scene on the
        // tracker UI from the moment the game starts, before the player
        // has ever heard about them.
    }

    private void OnDisable()
    {
        EnemyHealth.OnAnyEnemyDeath -= HandleAnyEnemyDeath;

        if (QuestManager.Instance != null) QuestManager.Instance.Unregister(this);
    }

    /// <summary>
    /// Called once, the first time the player talks to this NPC. Starts
    /// tracking kills (if relevant) and adds this quest to the tracker UI.
    /// Safe to call repeatedly - only takes effect the first time.
    /// </summary>
    private void Accept()
    {
        if (accepted) return;
        accepted = true;

        if (questType == QuestType.KillEnemy)
        {
            EnemyHealth.OnAnyEnemyDeath += HandleAnyEnemyDeath;
        }

        if (QuestManager.Instance != null) QuestManager.Instance.Register(this);
    }

    private void HandleAnyEnemyDeath(string enemyName)
    {
        if (completed) return;
        if (enemyName != targetEnemyName) return;

        killCount++;
    }

    /// <summary>Called by NPCInteractable each time this NPC's dialogue opens.</summary>
    public string GetDialogueLine()
    {
        Accept();

        if (completed) return alreadyDoneLine;

        bool isDone = questType == QuestType.KillEnemy
            ? killCount >= requiredAmount
            : inventoryController != null && inventoryController.GetItemCount(targetItemType) >= requiredAmount;

        if (isDone)
        {
            TurnIn();
            return completeLine;
        }

        return offerLineTemplate.Replace("{progress}", GetProgressText());
    }

    /// <summary>
    /// Read-only progress snapshot for UI (QuestTrackerUI). Never mutates
    /// state or triggers turn-in - safe to call every frame.
    ///
    /// snapshot.completed is true as soon as the objective amount is met,
    /// even before the player has walked back to the NPC to turn it in -
    /// this is what lets the Quest Tracker panel remove the row right when
    /// the bar hits e.g. 10/10, without waiting on the separate turn-in
    /// step (which still independently gates the actual reward via
    /// GetDialogueLine()/TurnIn()).
    /// </summary>
    public Snapshot GetSnapshot()
    {
        int current = questType == QuestType.KillEnemy
            ? killCount
            : (inventoryController != null ? inventoryController.GetItemCount(targetItemType) : 0);

        current = Mathf.Min(current, requiredAmount);

        return new Snapshot
        {
            title = questTitle,
            current = current,
            required = requiredAmount,
            // Sticky once true: after TurnIn() removes the gathered items
            // from inventory, `current` recalculates back down to 0 on the
            // very next poll - without the `completed ||` here, the row
            // would flash back to "0/10" for a frame right as it's meant to
            // start fading out. `completed` (set once, in TurnIn) keeps
            // this true forever afterward regardless of live inventory count.
            objectiveMet = completed || current >= requiredAmount,
            turnedIn = completed
        };
    }

    private string GetProgressText()
    {
        if (questType == QuestType.KillEnemy)
        {
            return $"kill {requiredAmount} {targetEnemyName}s ({killCount}/{requiredAmount})";
        }
        else
        {
            int have = inventoryController != null ? inventoryController.GetItemCount(targetItemType) : 0;
            return $"bring me {requiredAmount} {targetItemType} ({have}/{requiredAmount})";
        }
    }

    private void TurnIn()
    {
        completed = true;

        if (questType == QuestType.GatherItem && inventoryController != null)
        {
            inventoryController.TryRemoveItem(targetItemType, requiredAmount);
        }

        if (inventoryController != null)
        {
            inventoryController.AddItem(rewardItemType, 1);
        }
    }
}