using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyChasePlayer : MonoBehaviour
{
    private enum EnemyState
    {
        Walk,
        Attack
    }

    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float turnSpeed = 15f;

    [Header("Attack Distance")]
    public float attackDistance = 1.25f;
    public float resumeChaseDistance = 1.45f;

    [Header("Attack Timing")]
    public float attackCooldown = 1.2f;
    public float attackAnimTime = 0.7f;

    [Tooltip("Damage moment inside attack animation.")]
    public float damageDelay = 0.35f;

    [Header("Damage")]
    public int attackDamage = 1;

    [Header("Animation")]
    public Animator animator;
    public string walkAnim = "Walk";
    public string attackAnim = "Attack";

    [Header("Crossfade")]
    public float walkFadeTime = 0.12f;
    public float attackFadeTime = 0.06f;

    [Header("Anti Stuck")]
    public bool useAntiStuck = true;
    public float stuckCheckTime = 1f;
    public float stuckMoveDistance = 0.15f;

    [Header("Debug")]
    public bool showDebug = false;

    private CharacterController controller;
    private EnemyState currentState;

    private float nextAttackTime;
    private float attackTimer;
    private float damageTimer;
    private bool damageGivenThisAttack;

    private PlayerHealth playerHealth;

    private Vector3 lastPosition;
    private float stuckTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        if (animator != null)
            animator.applyRootMotion = false;

        currentState = EnemyState.Walk;
        CrossFadeTo(walkAnim, walkFadeTime);

        lastPosition = transform.position;
    }

    private void Update()
    {
        EnemyLogic();
    }

    private void EnemyLogic()
    {
        if (player == null)
        {
            FindPlayerAgain();
            return;
        }

        if (playerHealth == null)
            playerHealth = player.GetComponent<PlayerHealth>();

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= 0.05f)
            return;

        Vector3 moveDir = direction.normalized;

        RotateToPlayer(moveDir);

        bool attackIsPlaying = attackTimer > 0f;

        if (attackIsPlaying)
        {
            AttackTimerLogic(distance);

            // If player moved away during attack, stop attack and chase again
            if (distance > resumeChaseDistance)
            {
                StopAttackAndChase(moveDir);
            }

            return;
        }

        // Player near: attack
        if (distance <= attackDistance)
        {
            TryAttack(distance);
            return;
        }

        // Player far: chase
        Chase(moveDir);

        if (useAntiStuck)
            AntiStuckCheck(moveDir);

        if (showDebug)
            Debug.Log("Enemy chasing | Distance: " + distance);
    }

    private void Chase(Vector3 moveDir)
    {
        controller.SimpleMove(moveDir * moveSpeed);
        SetState(EnemyState.Walk);
    }

    private void TryAttack(float distance)
    {
        controller.SimpleMove(Vector3.zero);

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            attackTimer = attackAnimTime;
            damageTimer = damageDelay;
            damageGivenThisAttack = false;

            SetState(EnemyState.Attack);

            if (showDebug)
                Debug.Log("Enemy attack started");
        }
        else
        {
            SetState(EnemyState.Walk);
        }
    }

    private void AttackTimerLogic(float distance)
    {
        attackTimer -= Time.deltaTime;
        damageTimer -= Time.deltaTime;

        if (!damageGivenThisAttack && damageTimer <= 0f)
        {
            damageGivenThisAttack = true;

            if (distance <= attackDistance + 0.2f)
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);

                    if (showDebug)
                        Debug.Log("Enemy damaged player: " + attackDamage);
                }
            }
        }

        if (attackTimer <= 0f)
        {
            attackTimer = 0f;
            damageTimer = 0f;
            damageGivenThisAttack = false;
        }
    }

    private void StopAttackAndChase(Vector3 moveDir)
    {
        attackTimer = 0f;
        damageTimer = 0f;
        damageGivenThisAttack = false;

        Chase(moveDir);

        if (showDebug)
            Debug.Log("Player moved away, enemy chasing again");
    }

    private void RotateToPlayer(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    private void SetState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case EnemyState.Walk:
                CrossFadeTo(walkAnim, walkFadeTime);
                break;

            case EnemyState.Attack:
                CrossFadeTo(attackAnim, attackFadeTime);
                break;
        }
    }

    private void CrossFadeTo(string animName, float fadeTime)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(animName)) return;

        animator.CrossFadeInFixedTime(animName, fadeTime, 0, 0f);
    }

    private void FindPlayerAgain()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
        {
            player = p.transform;
            playerHealth = p.GetComponent<PlayerHealth>();
        }
    }

    private void AntiStuckCheck(Vector3 moveDir)
    {
        stuckTimer += Time.deltaTime;

        if (stuckTimer < stuckCheckTime)
            return;

        float movedDistance = Vector3.Distance(transform.position, lastPosition);

        if (movedDistance < stuckMoveDistance)
        {
            // Small side push if enemy is stuck against object
            Vector3 sideDir = Vector3.Cross(Vector3.up, moveDir).normalized;
            controller.Move(sideDir * 0.35f);
        }

        lastPosition = transform.position;
        stuckTimer = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, resumeChaseDistance);
    }
}