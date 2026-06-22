using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;

    [Header("Result Texts")]
    public TMP_Text enemyKillsText;
    public TMP_Text accuracyText;
    public TMP_Text survivalTimeText;

    [Header("References")]
    public TopDown3DPlayerMovement playerWeapon;

    private int enemyKills;
    private float survivalTimer;
    private bool gameEnded;

    private void Awake()
    {
        Instance = this;

        if (playerWeapon == null)
            playerWeapon = FindAnyObjectByType<TopDown3DPlayerMovement>();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        enemyKills = 0;
        survivalTimer = 0f;
        gameEnded = false;
    }

    private void Update()
    {
        if (gameEnded) return;

        survivalTimer += Time.deltaTime;
    }

    public void AddEnemyKill()
    {
        if (gameEnded) return;

        enemyKills++;
    }

    public void ShowGameOver()
    {
        if (gameEnded) return;

        gameEnded = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        UpdateGameOverUI();

        Time.timeScale = 0f;
    }

    private void UpdateGameOverUI()
    {
        if (enemyKillsText != null)
            enemyKillsText.text = enemyKills.ToString();

        if (accuracyText != null)
        {
            float accuracy = 0f;

            if (playerWeapon != null)
                accuracy = playerWeapon.accuracyPercent;

            accuracyText.text = accuracy.ToString("0") + "%";
        }

        if (survivalTimeText != null)
            survivalTimeText.text = FormatTime(survivalTimer);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;

        SmoothSceneTransition.Instance.FadeToLevel(0);
    }
}