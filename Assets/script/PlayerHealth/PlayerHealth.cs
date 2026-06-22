using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Regeneration")]
    public bool canRegenerate = true;
    public float regenStartDelay = 5f;      // wait after damage
    public float regenTickTime = 2f;        // time between each heart restore
    public int regenAmount = 1;

    [Header("Heart UI - Full Images")]
    public Image[] fullHeartImages;

    [Header("Heart UI - Dead/Empty Images")]
    public Image[] deadHeartImages;

    [Header("Damage Screen Image")]
    public Image damageImage;
    public float damageFadeInTime = 0.08f;
    public float damageHoldTime = 0.08f;
    public float damageFadeOutTime = 0.35f;
    public float damageMaxAlpha = 0.65f;

    [Header("Damage FX")]
    public GameObject damageEffectPrefab;
    public Transform damageEffectPoint;

    [Header("Death Animation")]
    public Animator animator;
    public string deathAnim = "Death";
    public float deathAnimTime = 2f;

    [Header("Death FX")]
    public GameObject deathEffectPrefab;
    public Transform deathEffectPoint;

    [Header("Disable On Death")]
    public MonoBehaviour playerMovementScript;
    public Collider[] playerColliders;

    private bool isDead;
    private Coroutine damageImageRoutine;
    private Coroutine regenRoutine;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerColliders == null || playerColliders.Length == 0)
            playerColliders = GetComponentsInChildren<Collider>();

        SetupDamageImage();
        UpdateHeartUI();
    }

    private void SetupDamageImage()
    {
        if (damageImage == null) return;

        Color c = damageImage.color;
        c.a = 0f;
        damageImage.color = c;
        damageImage.gameObject.SetActive(true);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHeartUI();
        PlayDamageEffect();
        PlayDamageImageFlash();

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        RestartRegeneration();
    }

    private void RestartRegeneration()
    {
        if (!canRegenerate) return;
        if (isDead) return;
        if (currentHealth >= maxHealth) return;

        if (regenRoutine != null)
            StopCoroutine(regenRoutine);

        regenRoutine = StartCoroutine(RegenerationRoutine());
    }

    private IEnumerator RegenerationRoutine()
    {
        yield return new WaitForSeconds(regenStartDelay);

        while (!isDead && currentHealth < maxHealth)
        {
            currentHealth += regenAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            UpdateHeartUI();

            Debug.Log("Player Regenerated Health: " + currentHealth);

            yield return new WaitForSeconds(regenTickTime);
        }

        regenRoutine = null;
    }

    private void UpdateHeartUI()
    {
        for (int i = 0; i < maxHealth; i++)
        {
            bool isAliveHeart = i < currentHealth;

            if (fullHeartImages != null && i < fullHeartImages.Length && fullHeartImages[i] != null)
            {
                fullHeartImages[i].gameObject.SetActive(isAliveHeart);
            }

            if (deadHeartImages != null && i < deadHeartImages.Length && deadHeartImages[i] != null)
            {
                deadHeartImages[i].gameObject.SetActive(!isAliveHeart);
            }
        }
    }

    private void PlayDamageImageFlash()
    {
        if (damageImage == null) return;

        if (damageImageRoutine != null)
            StopCoroutine(damageImageRoutine);

        damageImageRoutine = StartCoroutine(DamageImageFlashRoutine());
    }

    private IEnumerator DamageImageFlashRoutine()
    {
        float timer = 0f;

        while (timer < damageFadeInTime)
        {
            timer += Time.deltaTime;
            float t = timer / damageFadeInTime;
            SetDamageImageAlpha(Mathf.Lerp(0f, damageMaxAlpha, t));
            yield return null;
        }

        SetDamageImageAlpha(damageMaxAlpha);

        yield return new WaitForSeconds(damageHoldTime);

        timer = 0f;

        while (timer < damageFadeOutTime)
        {
            timer += Time.deltaTime;
            float t = timer / damageFadeOutTime;
            SetDamageImageAlpha(Mathf.Lerp(damageMaxAlpha, 0f, t));
            yield return null;
        }

        SetDamageImageAlpha(0f);
        damageImageRoutine = null;
    }

    private void SetDamageImageAlpha(float alpha)
    {
        if (damageImage == null) return;

        Color c = damageImage.color;
        c.a = alpha;
        damageImage.color = c;
    }

    private void PlayDamageEffect()
    {
        if (damageEffectPrefab == null) return;

        Vector3 spawnPos = damageEffectPoint != null
            ? damageEffectPoint.position
            : transform.position + Vector3.up * 1f;

        GameObject fx = Instantiate(
            damageEffectPrefab,
            spawnPos,
            Quaternion.identity
        );

        Destroy(fx, 2f);
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        if (regenRoutine != null)
            StopCoroutine(regenRoutine);

        Debug.Log("Player Dead");

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        foreach (Collider col in playerColliders)
        {
            if (col != null)
                col.enabled = false;
        }

        if (animator != null && !string.IsNullOrEmpty(deathAnim))
        {
            animator.applyRootMotion = false;
            animator.CrossFadeInFixedTime(deathAnim, 0.12f);
        }

        PlayDeathEffect();

        Destroy(gameObject, deathAnimTime);

        StartCoroutine(DeathSceneSwitchDelay());
    }

    private void PlayDeathEffect()
    {
        if (deathEffectPrefab == null) return;

        Vector3 spawnPos = deathEffectPoint != null
            ? deathEffectPoint.position
            : transform.position + Vector3.up * 1f;

        GameObject fx = Instantiate(
            deathEffectPrefab,
            spawnPos,
            Quaternion.identity
        );

        Destroy(fx, 3f);
    }

    private IEnumerator DeathSceneSwitchDelay()
    {
        yield return new WaitForSeconds(1f);

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.ShowGameOver();
    }
}