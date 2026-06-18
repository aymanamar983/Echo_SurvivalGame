using UnityEngine;

public class EnemyHeadBlowOff : MonoBehaviour
{
    [Header("Head")]
    public GameObject headObject;

    [Header("FX")]
    public GameObject bloodEffectPrefab;
    public Transform bloodSpawnPoint;

    [Header("Optional Flying Head")]
    public GameObject flyingHeadPrefab;
    public float flyingHeadForce = 5f;
    public float flyingHeadUpForce = 3f;

    [Header("State")]
    public bool headBlownOff;

    public void BlowOffHead(Vector3 hitPoint, Vector3 hitDirection)
    {
        if (headBlownOff) return;

        headBlownOff = true;

        // Hide original head
        if (headObject != null)
            headObject.SetActive(false);

        // Spawn blood effect
        Vector3 spawnPos = bloodSpawnPoint != null
            ? bloodSpawnPoint.position
            : hitPoint;

        if (bloodEffectPrefab != null)
        {
            GameObject blood = Instantiate(
                bloodEffectPrefab,
                spawnPos,
                Quaternion.LookRotation(-hitDirection)
            );

            Destroy(blood, 3f);
        }

        // Optional: spawn flying head prefab
        if (flyingHeadPrefab != null)
        {
            GameObject head = Instantiate(
                flyingHeadPrefab,
                spawnPos,
                Quaternion.identity
            );

            Rigidbody rb = head.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 forceDir = hitDirection.normalized + Vector3.up * flyingHeadUpForce;
                rb.AddForce(forceDir * flyingHeadForce, ForceMode.Impulse);
            }

            Destroy(head, 5f);
        }
    }
}