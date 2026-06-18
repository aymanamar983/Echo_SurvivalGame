using UnityEngine;

public class ExplosiveEnemy : MonoBehaviour
{
    [Header("Explosion")]
    public float explosionRadius = 4f;
    public float enemyExplosionDamage = 999f;
    public int playerDamage = 1;

    [Header("Layers")]
    public LayerMask enemyLayer;
    public LayerMask playerLayer;

    [Header("FX")]
    public GameObject explosionEffectPrefab;
    public Transform explosionPoint;
    public float effectDestroyTime = 3f;

    [Header("Chain Explosion")]
    public bool chainExplosion = true;
    public float chainDelay = 0.08f;

    [Header("Debug")]
    public bool showDebug = true;

    private bool hasExploded;

    public void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        Vector3 explosionPos = explosionPoint != null
            ? explosionPoint.position
            : transform.position + Vector3.up * 0.5f;

        SpawnExplosionEffect(explosionPos);
        DamageNearbyEnemies(explosionPos);
        DamagePlayer(explosionPos);

        if (showDebug)
            Debug.Log("Explosion happened: " + gameObject.name);
    }

    private void SpawnExplosionEffect(Vector3 position)
    {
        if (explosionEffectPrefab == null) return;

        GameObject fx = Instantiate(
            explosionEffectPrefab,
            position,
            Quaternion.identity
        );

        Destroy(fx, effectDestroyTime);
    }

    private void DamageNearbyEnemies(Vector3 explosionPos)
    {
        Collider[] enemies = Physics.OverlapSphere(
            explosionPos,
            explosionRadius,
            enemyLayer
        );

        foreach (Collider enemyCol in enemies)
        {
            if (enemyCol == null) continue;

            EnemyHealth enemyHealth = enemyCol.GetComponentInParent<EnemyHealth>();

            if (enemyHealth == null) continue;
            if (enemyHealth.gameObject == gameObject) continue;

            ExplosiveEnemy otherExplosive = enemyHealth.GetComponent<ExplosiveEnemy>();

            if (chainExplosion && otherExplosive != null)
            {
                otherExplosive.Invoke(nameof(otherExplosive.Explode), chainDelay);
                enemyHealth.ForceDie(explosionPos);
            }
            else
            {
                enemyHealth.TakeDamage(enemyExplosionDamage, explosionPos);
            }

            if (showDebug)
                Debug.Log("Explosion damaged enemy: " + enemyHealth.name);
        }
    }

    private void DamagePlayer(Vector3 explosionPos)
    {
        Collider[] players = Physics.OverlapSphere(
            explosionPos,
            explosionRadius,
            playerLayer
        );

        foreach (Collider playerCol in players)
        {
            if (playerCol == null) continue;

            PlayerHealth playerHealth = playerCol.GetComponentInParent<PlayerHealth>();

            if (playerHealth == null) continue;

            playerHealth.TakeDamage(playerDamage);

            if (showDebug)
                Debug.Log("Explosion damaged player");

            return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 pos = explosionPoint != null
            ? explosionPoint.position
            : transform.position + Vector3.up * 0.5f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, explosionRadius);
    }
}