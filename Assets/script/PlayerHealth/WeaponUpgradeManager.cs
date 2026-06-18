using UnityEngine;

public class WeaponUpgradeManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject upgradePanel;

    [Header("Player Weapon")]
    public TopDown3DPlayerMovement playerWeapon;

    private PlayerLevelSystem currentLevelSystem;

    private void Awake()
    {
        if (playerWeapon == null)
            playerWeapon = FindAnyObjectByType<TopDown3DPlayerMovement>();

        HidePanel();
    }

    public void OpenUpgradePanel(PlayerLevelSystem levelSystem)
    {
        currentLevelSystem = levelSystem;

        if (upgradePanel != null)
            upgradePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void SelectDeadeyeRounds()
    {
        if (playerWeapon != null)
            playerWeapon.UpgradeDeadeyeRounds();

        CloseUpgradePanel();
    }

    public void SelectQuickTrigger()
    {
        if (playerWeapon != null)
            playerWeapon.UpgradeQuickTrigger();

        CloseUpgradePanel();
    }

    public void SelectBiggerChamber()
    {
        if (playerWeapon != null)
            playerWeapon.UpgradeBiggerChamber();

        CloseUpgradePanel();
    }

    private void CloseUpgradePanel()
    {
        HidePanel();

        Time.timeScale = 1f;

        if (currentLevelSystem != null)
        {
            currentLevelSystem.UpgradeSelected();
            currentLevelSystem = null;
        }
    }

    private void HidePanel()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }
}