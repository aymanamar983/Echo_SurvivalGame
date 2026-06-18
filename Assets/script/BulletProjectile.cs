using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 moveDirection;
    public float speed = 35f;
    public float lifeTime = 3f;

    [Header("Damage")]
    public float damage = 1f;

    [Header("FX")]
    public GameObject hitEffectPrefab;

    [Header("Tags")]
    public string playerTag = "Player";
    public string enemyTag = "Enemy";
    public string enemyHeadTag = "EnemyHead";

    [Header("Debug")]
    public bool showDebug = false;

    private Rigidbody rb;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (hasHit) return;

        Vector3 dir = moveDirection.normalized;

        if (dir.sqrMagnitude <= 0.001f)
            dir = transform.forward;

        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = dir * speed;
#else
            rb.velocity = dir * speed;
#endif
        }
        else
        {
            transform.position += dir * speed * Time.fixedDeltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Do not hit player
        if (other.CompareTag(playerTag)) return;

        HandleHit(other, transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        Collider other = collision.collider;

        // Do not hit player
        if (other.CompareTag(playerTag)) return;

        Vector3 hitPoint = collision.contacts.Length > 0
            ? collision.contacts[0].point
            : transform.position;

        HandleHit(other, hitPoint);
    }

    private void HandleHit(Collider other, Vector3 hitPoint)
    {
        if (hasHit) return;
        hasHit = true;

        Vector3 hitDir = moveDirection.normalized;

        if (hitDir.sqrMagnitude <= 0.001f)
            hitDir = transform.forward;

        // Head hit
        if (other.CompareTag(enemyHeadTag))
        {
            EnemyHeadBlowOff headBlowOff = other.GetComponentInParent<EnemyHeadBlowOff>();

            if (headBlowOff != null)
            {
                headBlowOff.BlowOffHead(hitPoint, hitDir);
            }

            EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage, hitPoint);
            }

            SpawnHitEffect(hitPoint);

            if (showDebug)
                Debug.Log("Bullet hit enemy HEAD: " + other.name);

            Destroy(gameObject);
            return;
        }

        // Body hit
        EnemyHealth health = other.GetComponentInParent<EnemyHealth>();

        if (health != null || other.CompareTag(enemyTag))
        {
            if (health != null)
            {
                health.TakeDamage(damage, hitPoint);
            }

            SpawnHitEffect(hitPoint);

            if (showDebug)
                Debug.Log("Bullet hit enemy BODY: " + other.name);

            Destroy(gameObject);
            return;
        }

        // Hit wall / ground / object
        SpawnHitEffect(hitPoint);

        if (showDebug)
            Debug.Log("Bullet hit object: " + other.name);

        Destroy(gameObject);
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab == null) return;

        GameObject fx = Instantiate(
            hitEffectPrefab,
            position,
            Quaternion.identity
        );

        Destroy(fx, 2f);
    }
}