using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLevelSystem : MonoBehaviour
{
    [Header("Level")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public int xpIncreasePerLevel = 50;

    [Header("XP Gain")]
    public int xpPerEnemyHit = 10;

    [Header("UI")]
    public TMP_Text currentLevelText;
    public TMP_Text nextLevelText;
    public TMP_Text xpText;
    public Image xpFillImage;

    [Header("Upgrade")]
    public WeaponUpgradeManager upgradeManager;

    private bool choosingUpgrade;

    private void Awake()
    {
        if (upgradeManager == null)
            upgradeManager = FindAnyObjectByType<WeaponUpgradeManager>();

        UpdateUI();
    }

    public void AddEnemyHitXP()
    {
        AddXP(xpPerEnemyHit);
    }

    public void AddXP(int amount)
    {
        if (choosingUpgrade) return;

        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }

        UpdateUI();
    }

    private void LevelUp()
    {
        currentLevel++;
        xpToNextLevel += xpIncreasePerLevel;

        choosingUpgrade = true;

        UpdateUI();

        if (upgradeManager != null)
            upgradeManager.OpenUpgradePanel(this);
    }

    public void UpgradeSelected()
    {
        choosingUpgrade = false;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentLevelText != null)
            currentLevelText.text = currentLevel.ToString();

        if (nextLevelText != null)
            nextLevelText.text = (currentLevel + 1).ToString();

        if (xpText != null)
            xpText.text = currentXP + " / " + xpToNextLevel + " XP";

        if (xpFillImage != null)
            xpFillImage.fillAmount = (float)currentXP / xpToNextLevel;
    }
}