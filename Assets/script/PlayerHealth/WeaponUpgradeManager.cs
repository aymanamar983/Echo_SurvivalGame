using UnityEngine;

public class WeaponUpgradeManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject upgradePanel;

    [Header("Player Weapon")]
    public TopDown3DPlayerMovement playerWeapon;

    [Header("Close Settings")]
    public bool allowManualCloseButton = false;

    private PlayerLevelSystem currentLevelSystem;
    private bool isPanelOpen;

    private void Awake()
    {
        if (playerWeapon == null)
            playerWeapon = FindAnyObjectByType<TopDown3DPlayerMovement>();

        HidePanelOnly();
    }

    public void OpenUpgradePanel(PlayerLevelSystem levelSystem)
    {
        currentLevelSystem = levelSystem;
        ShowPanel();
    }

    public void OpenUpgradePanel()
    {
        currentLevelSystem = null;
        ShowPanel();
    }

    private void ShowPanel()
    {
        isPanelOpen = true;

        if (upgradePanel != null)
            upgradePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    // Top Row

    public void SelectDamage()
    {
        if (!isPanelOpen) return;

        if (playerWeapon != null)
            playerWeapon.UpgradeDeadeyeRounds();

        ApplyUpgradeAndClose();
    }

    public void SelectFireRate()
    {
        if (!isPanelOpen) return;

        if (playerWeapon != null)
            playerWeapon.UpgradeQuickTrigger();

        ApplyUpgradeAndClose();
    }

    public void SelectReloadSpeed()
    {
        if (!isPanelOpen) return;

        if (playerWeapon != null)
            playerWeapon.UpgradeReloadSpeed();

        ApplyUpgradeAndClose();
    }

    public void SelectCriticalHit()
    {
        if (!isPanelOpen) return;

        if (playerWeapon != null)
            playerWeapon.UpgradeCriticalHit();

        ApplyUpgradeAndClose();
    }

    // Bottom Row

    public void SelectPiercingBullet()
    {
        if (!isPanelOpen) return;

        if (playerWeapon != null)
            playerWeapon.UpgradePiercingBullet();

        ApplyUpgradeAndClose();
    }

    public void SelectBiggerCylinder()
    {
        if (!isPanelOpen) return;

        if (playerWeapon != null)
            playerWeapon.UpgradeBiggerChamber();

        ApplyUpgradeAndClose();
    }

    public void SelectFastHand()
    {
        if (!isPanelOpen) return;

        if (playerWeapon != null)
            playerWeapon.UpgradeFastHand();

        ApplyUpgradeAndClose();
    }

    public void SelectDeadlyAim()
    {
        if (!isPanelOpen) return;

        if (playerWeapon != null)
            playerWeapon.UpgradeDeadlyAim();

        ApplyUpgradeAndClose();
    }

    private void ApplyUpgradeAndClose()
    {
        HidePanelOnly();

        Time.timeScale = 1f;

        if (currentLevelSystem != null)
        {
            currentLevelSystem.UpgradeSelected();
            currentLevelSystem = null;
        }
    }

    // Use this only for X close button if needed
    public void CloseUpgradePanel()
    {
        if (!allowManualCloseButton)
            return;

        HidePanelOnly();

        Time.timeScale = 1f;

        if (currentLevelSystem != null)
        {
            currentLevelSystem.UpgradeSelected();
            currentLevelSystem = null;
        }
    }

    private void HidePanelOnly()
    {
        isPanelOpen = false;

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }
}