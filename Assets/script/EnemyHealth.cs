using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 3f;
    private float currentHealth;

    [Header("Animation")]
    public Animator animator;
    public string deathAnim = "Death";
    public float deathDestroyDelay = 2f;

    [Header("FX")]
    public GameObject deathEffectPrefab;
    public Transform deathEffectPoint;

    [Header("Explosion")]
    public bool explodeOnDeath = false;
    public ExplosiveEnemy explosiveEnemy;

    private bool isDead;
    private Collider[] allColliders;
    private CharacterController characterController;
    private EnemyChasePlayer enemyChase;

    private HordeSpawner spawner;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (explosiveEnemy == null)
            explosiveEnemy = GetComponent<ExplosiveEnemy>();

        allColliders = GetComponentsInChildren<Collider>();
        characterController = GetComponent<CharacterController>();
        enemyChase = GetComponent<EnemyChasePlayer>();
    }

    public void SetSpawner(HordeSpawner newSpawner)
    {
        spawner = newSpawner;
    }

    public void SetHealth(float newHealth)
    {
        maxHealth = newHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, Vector3 hitPoint)
    {
        if (isDead) return;

        currentHealth -= damage;

        Debug.Log("Enemy Health: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die(hitPoint);
        }
    }

    public void ForceDie(Vector3 hitPoint)
    {
        if (isDead) return;

        currentHealth = 0f;
        Die(hitPoint);
    }

    private void Die(Vector3 hitPoint)
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("Enemy Dead");

        if (spawner != null)
            spawner.EnemyDied();

        if (enemyChase != null)
            enemyChase.enabled = false;

        if (characterController != null)
            characterController.enabled = false;

        foreach (Collider col in allColliders)
        {
            if (col != null)
                col.enabled = false;
        }

        if (explodeOnDeath && explosiveEnemy != null)
        {
            explosiveEnemy.Explode();
            Destroy(gameObject, 0.15f);
            return;
        }

        if (animator != null && !string.IsNullOrEmpty(deathAnim))
        {
            animator.applyRootMotion = false;
            animator.CrossFadeInFixedTime(deathAnim, 0.12f);
        }

        if (deathEffectPrefab != null)
        {
            Vector3 spawnPos = deathEffectPoint != null
                ? deathEffectPoint.position
                : hitPoint;

            GameObject fx = Instantiate(
                deathEffectPrefab,
                spawnPos,
                Quaternion.identity
            );

            Destroy(fx, 3f);
        }

        Destroy(gameObject, deathDestroyDelay);
    }
}