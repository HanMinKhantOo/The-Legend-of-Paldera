using UnityEngine;

/// <summary>
/// Makes an NPC walk back and forth between two points (or stand
/// still, for a stationary NPC like the village chief - just leave
/// Waypoint B empty / equal to the start position).
///
/// Drives the same Animator parameters as PlayerMovement
/// (MoveX/MoveY/LastMoveX/LastMoveY/isMoving), so it works with the
/// exact same Animator Controller setup/blend tree you already built
/// for the player.
///
/// Setup:
/// 1. Add to your villager NPC GameObject (needs a SpriteRenderer +
///    Animator using the same parameter names as the player).
/// 2. Assign waypointA / waypointB as empty child Transforms marking
///    the two ends of its patrol path, OR leave them unassigned and
///    it will patrol "moveDistance" units left/right of its start
///    position automatically.
/// 3. For a stationary NPC (the chief), either don't add this script
///    at all, or add it with moveSpeed = 0.
/// </summary>
public class NPCWalker : MonoBehaviour
{
    [Header("Patrol Points")]
    [Tooltip("Optional. If left empty, a patrol point is auto-generated moveDistance units to each side of the NPC's starting position.")]
    public Transform waypointA;
    public Transform waypointB;
    [Tooltip("Used only if waypointA/B are left empty - how far left/right of the start position to patrol.")]
    public float moveDistance = 3f;

    [Header("Movement")]
    public float moveSpeed = 1.2f;
    [Tooltip("Seconds to idle at each end of the path before turning around.")]
    public float pauseAtEnds = 1f;

    private Vector2 pointA;
    private Vector2 pointB;
    private Vector2 currentTarget;
    private float pauseTimer;
    private bool paused; // externally paused (dialogue)

    private Animator animator;
    private Rigidbody2D rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        pointA = waypointA != null ? (Vector2)waypointA.position : (Vector2)transform.position - new Vector2(moveDistance, 0f);
        pointB = waypointB != null ? (Vector2)waypointB.position : (Vector2)transform.position + new Vector2(moveDistance, 0f);

        currentTarget = pointB;
    }

    private void Update()
    {
        if (paused || moveSpeed <= 0f)
        {
            SetMoving(false, Vector2.zero);
            return;
        }

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            SetMoving(false, Vector2.zero);
            return;
        }

        Vector2 currentPos = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 toTarget = currentTarget - currentPos;

        if (toTarget.magnitude < 0.05f)
        {
            // Reached the end - flip target and pause briefly.
            currentTarget = currentTarget == pointA ? pointB : pointA;
            pauseTimer = pauseAtEnds;
            SetMoving(false, Vector2.zero);
            return;
        }

        Vector2 dir = toTarget.normalized;
        Vector2 newPos = currentPos + dir * moveSpeed * Time.deltaTime;

        if (rb != null) rb.MovePosition(newPos);
        else transform.position = newPos;

        SetMoving(true, dir);
    }

    private void SetMoving(bool moving, Vector2 dir)
    {
        if (animator == null) return;

        animator.SetBool("isMoving", moving);
        if (moving)
        {
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
            animator.SetFloat("LastMoveX", dir.x);
            animator.SetFloat("LastMoveY", dir.y);
        }
    }

    /// <summary>Called by NPCInteractable when dialogue starts/ends.</summary>
    public void PauseForDialogue(bool shouldPause)
    {
        paused = shouldPause;
        if (shouldPause) SetMoving(false, Vector2.zero);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 a = waypointA != null ? (Vector2)waypointA.position : (Vector2)transform.position - new Vector2(moveDistance, 0f);
        Vector2 b = waypointB != null ? (Vector2)waypointB.position : (Vector2)transform.position + new Vector2(moveDistance, 0f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(a, 0.2f);
        Gizmos.DrawWireSphere(b, 0.2f);
    }
}
