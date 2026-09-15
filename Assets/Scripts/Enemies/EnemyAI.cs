using UnityEngine;

/// <summary>
/// Shared roaming-enemy behavior for the Magical Wolf and Magical Boar.
/// One script, two prefabs - every difference between the Wolf and the
/// Boar (HP, damage, attack speed, roam range) is just Inspector values on
/// EnemyHealth + this component, per the "adjustable per mob" requirement.
///
/// Animator parameters mirror PlayerMovement/PlayerPunch so the same mental
/// model applies: MoveX, MoveY, isMoving (floats/bool for blend trees) plus
/// Attack / Hurt / Death triggers for one-shot animations.
///
/// Movement uses Rigidbody2D.MovePosition like PlayerMovement, and damage is
/// applied to the player via PlayerVitals.Instance.TakeDamage, the same
/// entry point starvation already uses - no new player-damage system.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyAI : MonoBehaviour
{
    private enum State { Roam, Chase, Attack, Dead }

    [Header("Identity")]
    [Tooltip("Used only in death-cause text / debug logs.")]
    public string mobName = "Magical Wolf";

    [Header("Movement")]
    public float moveSpeed = 2f;
    [Tooltip("How far from the spawn point this mob wanders while idle.")]
    public float roamRadius = 3f;
    [Tooltip("Seconds spent waiting around between each new roam destination.")]
    public float roamPauseMin = 1f;
    public float roamPauseMax = 3f;

    [Header("Detection & Combat Range")]
    public float detectionRange = 4f;
    [Tooltip("If the player gets this far from the spawn point, the mob gives up the chase and returns home.")]
    public float leashRange = 7f;
    public float attackRange = 0.9f;

    [Header("Combat Stats")]
    public float attackDamage = 10f;
    [Tooltip("Attacks per second.")]
    public float attackSpeed = 1f;

    private Rigidbody2D rb;
    private Animator animator;
    private EnemyHealth health;

    private Vector3 spawnPoint;
    private Vector2 roamTarget;
    private float roamWaitTimer;
    private float attackCooldownTimer;
    private State state = State.Roam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        health.OnDeath += HandleDeath;
        health.OnDamaged += HandleDamaged;
    }

    private void OnDisable()
    {
        health.OnDeath -= HandleDeath;
        health.OnDamaged -= HandleDamaged;
    }

    /// <summary>Called by EnemySpawner right after Instantiate/on respawn.</summary>
    public void Initialize(Vector3 spawnPosition)
    {
        spawnPoint = spawnPosition;
        transform.position = spawnPosition;
        state = State.Roam;
        PickNewRoamTarget();
    }

    private void Update()
    {
        if (state == State.Dead) return;

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        Transform player = PlayerVitals.Instance != null ? PlayerVitals.Instance.transform : null;
        float distanceToPlayer = player != null
            ? Vector2.Distance(transform.position, player.position)
            : Mathf.Infinity;
        float playerDistanceFromSpawn = player != null
            ? Vector2.Distance(spawnPoint, player.position)
            : Mathf.Infinity;

        switch (state)
        {
            case State.Roam:
                if (player != null && !PlayerVitals.Instance.IsDead && distanceToPlayer <= detectionRange)
                {
                    state = State.Chase;
                    break;
                }
                TickRoam();
                break;

            case State.Chase:
                if (player == null || PlayerVitals.Instance.IsDead || playerDistanceFromSpawn > leashRange)
                {
                    state = State.Roam;
                    PickNewRoamTarget();
                    break;
                }

                if (distanceToPlayer <= attackRange)
                {
                    state = State.Attack;
                    break;
                }

                MoveToward(player.position);
                break;

            case State.Attack:
                if (player == null || PlayerVitals.Instance.IsDead)
                {
                    state = State.Roam;
                    PickNewRoamTarget();
                    break;
                }

                if (distanceToPlayer > attackRange)
                {
                    state = State.Chase;
                    break;
                }

                SetMoving(false);
                FacePoint(player.position);

                if (attackCooldownTimer <= 0f)
                {
                    attackCooldownTimer = 1f / Mathf.Max(0.01f, attackSpeed);
                    if (animator != null) animator.SetTrigger("Attack");
                    PlayerVitals.Instance.TakeDamage(attackDamage, mobName);
                }
                break;
        }
    }

    private void TickRoam()
    {
        float distToTarget = Vector2.Distance(transform.position, roamTarget);

        if (distToTarget <= 0.1f)
        {
            SetMoving(false);

            roamWaitTimer -= Time.deltaTime;
            if (roamWaitTimer <= 0f)
            {
                PickNewRoamTarget();
            }
            return;
        }

        MoveToward(roamTarget);
    }

    private void PickNewRoamTarget()
    {
        Vector2 offset = Random.insideUnitCircle * roamRadius;
        roamTarget = (Vector2)spawnPoint + offset;
        roamWaitTimer = Random.Range(roamPauseMin, roamPauseMax);
    }

    private void MoveToward(Vector2 target)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;

        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);

        SetMoving(true);
        FaceDirection(direction);
    }

    private void SetMoving(bool moving)
    {
        if (animator != null) animator.SetBool("isMoving", moving);
    }

    private void FaceDirection(Vector2 direction)
    {
        if (animator == null) return;
        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveY", direction.y);
        animator.SetFloat("LastMoveX", direction.x);
        animator.SetFloat("LastMoveY", direction.y);
    }

    private void FacePoint(Vector2 point)
    {
        FaceDirection(((Vector2)point - (Vector2)transform.position).normalized);
    }

    private void HandleDamaged()
    {
        if (animator != null) animator.SetTrigger("Hurt");
    }

    private void HandleDeath(EnemyHealth _)
    {
        state = State.Dead;
        SetMoving(false);
        if (animator != null) animator.SetTrigger("Death");
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? spawnPoint : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, roamRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = new Color(1f, 0.3f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
