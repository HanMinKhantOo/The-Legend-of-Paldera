using UnityEngine;

/// <summary>
/// Attach to any NPC (chief, villager, etc). Detects when the player
/// is nearby, shows an optional "Press E" prompt, and on E opens a
/// single-line dialogue via DialogueUI. While talking, the NPC stops
/// moving (if it has an NPCWalker) and faces the player.
///
/// Setup:
/// 1. Add this component alongside a SpriteRenderer + Animator on
///    your NPC GameObject.
/// 2. Assign npcName and dialogueLine in the Inspector.
/// 3. Optionally assign an "interactPrompt" GameObject (e.g. a small
///    floating "E" icon/sprite parented above the NPC's head) - it
///    will be shown/hidden automatically based on player distance.
/// 4. Make sure the Player has the tag "Player" (default in Unity)
///    so this script can find it via FindGameObjectWithTag.
/// </summary>
public class NPCInteractable : MonoBehaviour
{
    [Header("Identity")]
    public string npcName = "Villager";

    [Header("Dialogue")]
    [TextArea]
    public string dialogueLine = "Hello there, traveler.";

    [Header("Interaction")]
    [Tooltip("How close the player needs to be to interact.")]
    public float interactRange = 1.5f;
    [Tooltip("Optional floating icon (e.g. an 'E' prompt sprite) shown when the player is in range. Leave empty if you don't have one yet.")]
    public GameObject interactPrompt;
    public KeyCode interactKey = KeyCode.E;

    private Transform player;
    private NPCWalker walker;
    private Animator animator;
    private QuestGiver questGiver;
    private bool isTalking;

    private void Awake()
    {
        walker = GetComponent<NPCWalker>();
        animator = GetComponent<Animator>();
        questGiver = GetComponent<QuestGiver>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inRange = dist <= interactRange;

        if (interactPrompt != null && !isTalking)
        {
            interactPrompt.SetActive(inRange);
        }

        if (inRange && Input.GetKeyDown(interactKey))
        {
            if (isTalking)
            {
                EndDialogue();
            }
            else
            {
                StartDialogue();
            }
        }

        // If the player walks away mid-conversation, close it automatically.
        if (isTalking && !inRange)
        {
            EndDialogue();
        }
    }

    private void StartDialogue()
    {
        isTalking = true;

        if (interactPrompt != null) interactPrompt.SetActive(false);

        if (walker != null) walker.PauseForDialogue(true);

        FacePlayer();

        if (DialogueUI.Instance != null)
        {
            // A QuestGiver on this same NPC (if any) supplies a dynamic line
            // reflecting quest progress/turn-in instead of the fixed
            // dialogueLine - everything else about how the dialogue opens
            // (range check, facing, pausing movement) stays unchanged.
            string line = questGiver != null ? questGiver.GetDialogueLine() : dialogueLine;
            DialogueUI.Instance.Show(npcName, line);
        }
        else
        {
            Debug.LogWarning("[NPCInteractable] No DialogueUI.Instance found in scene - add the DialogueUI script to your Canvas.");
        }
    }

    private void EndDialogue()
    {
        isTalking = false;

        if (DialogueUI.Instance != null) DialogueUI.Instance.Close();
        if (walker != null) walker.PauseForDialogue(false);
    }

    private void FacePlayer()
    {
        if (player == null || animator == null) return;
        if (animator.runtimeAnimatorController == null) return;

        Vector2 dir = (player.position - transform.position).normalized;

        // Snap to the dominant axis so it maps cleanly onto the 4-way
        // Down/Up/Left/Right Idle states (diagonal-ish angles still pick
        // whichever direction is more prominent).
        Vector2 snapped = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
            ? new Vector2(Mathf.Sign(dir.x), 0f)
            : new Vector2(0f, Mathf.Sign(dir.y));

        // Setting real MoveX/MoveY (not just Last) is what actually drives
        // the direction-aware Idle transitions - isMoving stays false so
        // she doesn't play a walk cycle while standing still and talking.
        SetFloatIfExists("MoveX", snapped.x);
        SetFloatIfExists("MoveY", snapped.y);
        SetFloatIfExists("LastMoveX", snapped.x);
        SetFloatIfExists("LastMoveY", snapped.y);
        SetBoolIfExists("isMoving", false);
    }

    private bool HasParameter(string name)
    {
        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == name) return true;
        }
        return false;
    }

    private void SetFloatIfExists(string name, float value)
    {
        if (HasParameter(name)) animator.SetFloat(name, value);
    }

    private void SetBoolIfExists(string name, bool value)
    {
        if (HasParameter(name)) animator.SetBool(name, value);
    }
}