using System.Collections;
using TMPro;
using UnityEngine;

public class HordeSpawner : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    public float spawnRadius = 18f;
    public float minSpawnDistance = 10f;
    public float spawnHeightOffset = 0.05f;
    public int maxSpawnAttempts = 30;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;
    public float groundRayHeight = 10f;
    public float obstacleCheckRadius = 0.8f;

    [Header("Wave Settings")]
    public int currentWave = 1;
    public int baseEnemyCount = 6;
    public int enemyIncreasePerWave = 3;

    [Header("Timing")]
    public float timeBetweenWaves = 4f;
    public float spawnDelay = 0.25f;

    [Header("Difficulty Scaling")]
    public float enemyHealthIncreasePerWave = 1f;
    public float enemySpeedIncreasePerWave = 0.15f;

    [Header("UI")]
    public TMP_Text waveText;
    public TMP_Text enemyCountText;

    [Header("Wave Text Fade")]
    public CanvasGroup waveTextGroup;
    public float waveFadeInTime = 0.35f;
    public float waveShowTime = 1.2f;
    public float waveFadeOutTime = 0.45f;

    [Header("Debug")]
    public bool showDebug = true;

    private int enemiesAlive;
    private bool spawningWave;
    private Coroutine waveTextRoutine;

    private void Awake()
    {
        SetupWaveText();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        StartCoroutine(WaveLoop());
    }

    private void SetupWaveText()
    {
        if (waveText == null) return;

        if (waveTextGroup == null)
        {
            waveTextGroup = waveText.GetComponent<CanvasGroup>();

            if (waveTextGroup == null)
                waveTextGroup = waveText.gameObject.AddComponent<CanvasGroup>();
        }

        waveTextGroup.alpha = 0f;
        waveTextGroup.interactable = false;
        waveTextGroup.blocksRaycasts = false;
        waveText.gameObject.SetActive(false);
    }

    private IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            yield return StartCoroutine(StartWave());

            while (enemiesAlive > 0)
            {
                UpdateUI();
                yield return null;
            }

            if (showDebug)
                Debug.Log("Wave " + currentWave + " completed!");

            currentWave++;
            UpdateUI();

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private IEnumerator StartWave()
    {
        if (spawningWave) yield break;

        spawningWave = true;

        int enemyCount = baseEnemyCount + ((currentWave - 1) * enemyIncreasePerWave);

        if (showDebug)
            Debug.Log("Wave " + currentWave + " Started | Enemies: " + enemyCount);

        ShowWaveText("Wave " + currentWave);

        UpdateUI();

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }

        spawningWave = false;
    }

    private void ShowWaveText(string message)
    {
        if (waveText == null || waveTextGroup == null) return;

        if (waveTextRoutine != null)
            StopCoroutine(waveTextRoutine);

        waveTextRoutine = StartCoroutine(WaveTextFadeRoutine(message));
    }

    private IEnumerator WaveTextFadeRoutine(string message)
    {
        waveText.text = message;
        waveText.gameObject.SetActive(true);

        waveTextGroup.alpha = 0f;
        waveTextGroup.interactable = false;
        waveTextGroup.blocksRaycasts = false;

        float timer = 0f;

        // Fade in
        while (timer < waveFadeInTime)
        {
            timer += Time.deltaTime;
            float t = timer / waveFadeInTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            waveTextGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        waveTextGroup.alpha = 1f;

        yield return new WaitForSeconds(waveShowTime);

        timer = 0f;

        // Fade out
        while (timer < waveFadeOutTime)
        {
            timer += Time.deltaTime;
            float t = timer / waveFadeOutTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            waveTextGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        waveTextGroup.alpha = 0f;
        waveText.gameObject.SetActive(false);

        waveTextRoutine = null;
    }

    private void SpawnEnemy()
    {
        if (player == null) return;
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        if (!TryGetValidSpawnPosition(out Vector3 spawnPos))
        {
            if (showDebug)
                Debug.LogWarning("No valid spawn position found.");

            return;
        }

        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPos,
            Quaternion.identity
        );

        enemiesAlive++;

        EnemyChasePlayer chase = enemy.GetComponent<EnemyChasePlayer>();
        if (chase != null)
        {
            chase.player = player;
            chase.moveSpeed += (currentWave - 1) * enemySpeedIncreasePerWave;
        }

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            float scaledHealth = health.maxHealth + ((currentWave - 1) * enemyHealthIncreasePerWave);
            health.SetHealth(scaledHealth);
            health.SetSpawner(this);
        }

        UpdateUI();

        if (showDebug)
            Debug.Log("Enemy spawned at: " + spawnPos);
    }

    private bool TryGetValidSpawnPosition(out Vector3 validPosition)
    {
        validPosition = Vector3.zero;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minSpawnDistance, spawnRadius);

            Vector3 randomPos = player.position + new Vector3(
                randomCircle.x * distance,
                0f,
                randomCircle.y * distance
            );

            Vector3 rayStart = randomPos + Vector3.up * groundRayHeight;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRayHeight * 2f, groundLayer))
            {
                Vector3 groundPos = hit.point + Vector3.up * spawnHeightOffset;

                bool blocked = Physics.CheckSphere(
                    groundPos,
                    obstacleCheckRadius,
                    obstacleLayer
                );

                if (!blocked)
                {
                    validPosition = groundPos;
                    return true;
                }
            }
        }

        return false;
    }

    public void EnemyDied()
    {
        enemiesAlive--;
        enemiesAlive = Mathf.Max(enemiesAlive, 0);

        UpdateUI();

        if (showDebug)
            Debug.Log("Enemy Died | Alive: " + enemiesAlive);
    }

    private void UpdateUI()
    {
        if (enemyCountText != null)
            enemyCountText.text = "Enemies: " + enemiesAlive;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, spawnRadius);
    }
}