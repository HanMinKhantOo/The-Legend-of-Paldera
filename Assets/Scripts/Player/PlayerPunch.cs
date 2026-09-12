using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPunch : MonoBehaviour
{
    private Animator animator;

    [Header("Punch Hit Detection")]
    [SerializeField] private float hitDistance = 0.65f;
    [SerializeField] private float hitRadius = 0.35f;
    [SerializeField] private int damagePerPunch = 1;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            animator.SetTrigger("Punch");
        }
    }

    // Called by an Animation Event at the actual impact frame.
    public void PerformPunchHit()
    {
        Vector2 direction = GetPunchDirection();

        Vector2 hitPoint =
            (Vector2)transform.position +
            direction * hitDistance;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(hitPoint, hitRadius);

        foreach (Collider2D hit in hits)
        {
            TreeHealth tree = hit.GetComponent<TreeHealth>();

            if (tree == null)
                tree = hit.GetComponentInParent<TreeHealth>();

            if (tree != null)
            {
                tree.TakeHit(damagePerPunch);

                // Only hit one tree per punch.
                break;
            }
        }
    }

    private Vector2 GetPunchDirection()
    {
        float moveX = animator.GetFloat("MoveX");
        float moveY = animator.GetFloat("MoveY");

        Vector2 direction = new Vector2(moveX, moveY);

        // If standing still, use the last facing direction.
        if (direction.sqrMagnitude < 0.01f)
        {
            float lastMoveX = animator.GetFloat("LastMoveX");
            float lastMoveY = animator.GetFloat("LastMoveY");

            direction = new Vector2(lastMoveX, lastMoveY);
        }

        // Safety fallback.
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Vector2.down;
        }

        // Force the hit direction to one cardinal direction.
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            direction = direction.x > 0
                ? Vector2.right
                : Vector2.left;
        }
        else
        {
            direction = direction.y > 0
                ? Vector2.up
                : Vector2.down;
        }

        return direction;
    }
}