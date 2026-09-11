using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to any object the player should be able to trigger with a real
/// keyboard press (default: E) while standing nearby - the ruins' statue/
/// altar feature, but reusable for doors, levers, signs, etc. elsewhere.
///
/// Uses a trigger Collider2D to detect the player entering/leaving range
/// (same Player-tag check pattern as WorldPickup.cs) and polls the new
/// Input System each frame, the same way PlayerMovement.cs reads the
/// Keyboard, rather than relying on Unity's older Input Manager.
///
/// Wire up OnInteract() (or listen to the onInteract UnityEvent) to whatever
/// this specific feature should do - play a sound, open a dialogue, reveal
/// lore text, start a puzzle, etc.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class RuinsInteractable : MonoBehaviour
{
    [Header("Range")]
    [Tooltip("How close the player needs to be to interact. Also resizes the trigger collider on Awake.")]
    public float interactRadius = 1.5f;

    [Header("Key")]
    [Tooltip("Which key triggers this interactable.")]
    public Key interactKey = Key.E;

    [Header("Prompt (optional)")]
    [Tooltip("Optional 'Press E' world-space indicator (e.g. a small SpriteRenderer using the pack's Interact.png icon) shown only while the player is in range.")]
    public GameObject promptIndicator;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onInteract;

    [Tooltip("If true, this can only be triggered once (e.g. a one-time lore reveal). Set false for repeatable triggers.")]
    public bool oneShot = false;
    private bool consumed = false;

    private bool playerInRange = false;
    private CircleCollider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = interactRadius;

        if (promptIndicator != null) promptIndicator.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange || consumed) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current[interactKey].wasPressedThisFrame)
        {
            OnInteract();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        playerInRange = true;
        if (!consumed && promptIndicator != null) promptIndicator.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        playerInRange = false;
        if (promptIndicator != null) promptIndicator.SetActive(false);
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.transform.root.CompareTag("Player");
    }

    /// <summary>
    /// Override or extend this (or just hook onInteract in the Inspector)
    /// for what happens when the player presses the key in range.
    /// </summary>
    protected virtual void OnInteract()
    {
        Debug.Log($"[RuinsInteractable] {gameObject.name} triggered by player.");

        onInteract?.Invoke();

        if (oneShot)
        {
            consumed = true;
            if (promptIndicator != null) promptIndicator.SetActive(false);
        }
    }
}
