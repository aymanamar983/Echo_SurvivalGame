using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TopDown3DPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float inputSmoothTime = 0.08f;
    public float gravity = -20f;

    [Header("Mouse Aim")]
    public Camera mainCamera;
    public Transform model;
    public LayerMask groundLayer;
    public float turnSpeed = 18f;

    [Header("Auto Shooting Only")]
    public bool useAutoShooting = true;
    public LayerMask enemyLayer;
    public float autoShootRadius = 18f;

    [Tooltip("0.85 = enemy must be mostly in front of gun aim. Lower = easier, higher = stricter.")]
    public float aimShootDot = 0.85f;

    [Tooltip("Enemy aim check height. Use 1 for chest height.")]
    public float enemyCheckHeight = 1f;

    [Header("Movement Lock")]
    public bool stopMovementWhileShooting = false;
    public bool stopMovementWhileReloading = true;

    [Header("Animation")]
    public Animator animator;
    public float animFadeTime = 0.12f;

    public string idleAnim = "Idle";

    public string forwardAnim = "Handgun@Walk01 - Forward [RM]";
    public string backwardAnim = "Handgun@Walk01 - Backward [RM]";
    public string leftAnim = "Handgun@Walk01 - Left [RM]";
    public string rightAnim = "Handgun@Walk01 - Right [RM]";

    public string forwardLeftAnim = "Handgun@Walk01 - ForwardLeft [RM]";
    public string forwardRightAnim = "Handgun@Walk01 - ForwardRight [RM]";
    public string backwardLeftAnim = "Handgun@Walk01 - BackwardLeft [RM]";
    public string backwardRightAnim = "Handgun@Walk01 - BackwardRight [RM]";

    [Header("Shooting")]
    public int shootMouseButton = 0;
    public string shootAnim = "Handgun@Shoot";
    public float shootAnimTime = 0.18f;
    public float fireRate = 0.25f;

    [Header("Reload")]
    public int maxBullets = 6;
    public float reloadTime = 1.5f;
    public string reloadAnim = "Handgun@Reload";
    public bool useReloadAnimation = true;

    [Header("Reload UI")]
    public CanvasGroup gunUI;
    public CanvasGroup reloadUI;
    public float uiFadeTime = 0.25f;
    public bool useUIScaleAnimation = true;
    public Vector3 normalUIScale = Vector3.one;
    public Vector3 reloadPopScale = new Vector3(1.12f, 1.12f, 1.12f);

    [Header("Ammo UI")]
    public TMP_Text ammoText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootClip;
    public AudioClip reloadClip;
    public float shootVolume = 1f;
    public float reloadVolume = 1f;

    [Header("Gun Setup")]
    public Transform muzzlePoint;
    public GameObject projectilePrefab;
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;

    [Header("Projectile Settings")]
    public float projectileSpeed = 35f;
    public float projectileLifeTime = 3f;
    public float projectileDamage = 1f;

    [Header("Debug")]
    public bool showDebug = true;

    private CharacterController controller;
    private Vector3 currentMove;
    private Vector3 moveVelocity;
    private float verticalVelocity;
    private string currentAnim;

    private bool isShooting;
    private float shootTimer;
    private float nextFireTime;

    private int currentBullets;
    private bool isReloading;
    private float reloadTimer;

    private Coroutine uiRoutine;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (model == null && animator != null)
            model = animator.transform;

        if (animator != null)
            animator.applyRootMotion = false;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        currentBullets = maxBullets;

        SetupUIStart();
        UpdateAmmoUI();
    }

    private void OnEnable()
    {
        currentBullets = maxBullets;
        isReloading = false;
        isShooting = false;
        reloadTimer = 0f;
        shootTimer = 0f;
        nextFireTime = 0f;

        SetupUIStart();
        UpdateAmmoUI();
    }

    private void PlayShootSound()
    {
        if (audioSource != null && shootClip != null)
            audioSource.PlayOneShot(shootClip, shootVolume);
    }

    private void PlayReloadSound()
    {
        if (audioSource != null && reloadClip != null)
            audioSource.PlayOneShot(reloadClip, reloadVolume);
    }

    private void Update()
    {
        AimToMouse();
        HandleReload();
        HandleShooting();
        MovePlayer();
    }
    private void UpdateAmmoUI()
    {
        if (ammoText == null) return;

        ammoText.text = currentBullets + " / " + maxBullets;
    }

    private void SetupUIStart()
    {
        if (gunUI != null)
        {
            gunUI.alpha = 1f;
            gunUI.interactable = true;
            gunUI.blocksRaycasts = true;
            gunUI.gameObject.SetActive(true);
            gunUI.transform.localScale = normalUIScale;
        }

        if (reloadUI != null)
        {
            reloadUI.alpha = 0f;
            reloadUI.interactable = false;
            reloadUI.blocksRaycasts = false;
            reloadUI.gameObject.SetActive(false);
            reloadUI.transform.localScale = normalUIScale;
        }
    }

    private void HandleShooting()
    {
        if (isReloading)
            return;

        bool wantsToShoot;

        if (useAutoShooting)
            wantsToShoot = IsEnemyInAimDirection();
        else
            wantsToShoot = Input.GetMouseButton(shootMouseButton);

        if (wantsToShoot && Time.time >= nextFireTime)
        {
            if (currentBullets <= 0)
            {
                StartReload();
                return;
            }

            nextFireTime = Time.time + fireRate;
            currentBullets--;
            UpdateAmmoUI();

            isShooting = true;
            shootTimer = shootAnimTime;

            PlayAnim(shootAnim);
            SpawnMuzzleFlash();
            PlayShootSound();
            ShootProjectile();

            if (showDebug)
                Debug.Log("Auto Shoot | Ammo: " + currentBullets + " / " + maxBullets);

            if (currentBullets <= 0)
            {
                StartReload();
            }
        }

        if (isShooting)
        {
            shootTimer -= Time.deltaTime;

            if (shootTimer <= 0f)
            {
                isShooting = false;
                currentAnim = "";
            }
        }
    }

    private bool IsEnemyInAimDirection()
    {
        if (muzzlePoint == null) return false;

        Collider[] enemies = Physics.OverlapSphere(
            transform.position,
            autoShootRadius,
            enemyLayer
        );

        Vector3 aimDir = muzzlePoint.forward;
        aimDir.y = 0f;

        if (aimDir.sqrMagnitude <= 0.01f)
            return false;

        aimDir.Normalize();

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null) continue;

            EnemyHealth enemyHealth = enemies[i].GetComponentInParent<EnemyHealth>();
            if (enemyHealth == null) continue;

            Vector3 enemyPos = enemyHealth.transform.position + Vector3.up * enemyCheckHeight;
            Vector3 dirToEnemy = enemyPos - muzzlePoint.position;
            dirToEnemy.y = 0f;

            if (dirToEnemy.sqrMagnitude <= 0.01f) continue;

            dirToEnemy.Normalize();

            float aimDot = Vector3.Dot(aimDir, dirToEnemy);

            if (aimDot >= aimShootDot)
            {
                if (showDebug)
                    Debug.Log("Enemy in aim direction. Dot: " + aimDot);

                return true;
            }
        }

        return false;
    }

    private void StartReload()
    {
        if (isReloading) return;

        isReloading = true;
        reloadTimer = reloadTime;
        isShooting = false;
        currentAnim = "";

        if (useReloadAnimation)
            PlayAnim(reloadAnim);

        ShowReloadUI();
        PlayReloadSound();

        if (showDebug)
            Debug.Log("Reload Started...");
    }

    private void HandleReload()
    {
        if (!isReloading) return;

        reloadTimer -= Time.deltaTime;

        if (reloadTimer <= 0f)
        {
            isReloading = false;
            currentBullets = maxBullets;
            UpdateAmmoUI();
            currentAnim = "";

            ShowGunUI();

            if (showDebug)
                Debug.Log("Reload Complete | Ammo: " + currentBullets + " / " + maxBullets);
        }
    }

    private void ShowReloadUI()
    {
        if (uiRoutine != null)
            StopCoroutine(uiRoutine);

        uiRoutine = StartCoroutine(SwitchUI(gunUI, reloadUI));
    }

    private void ShowGunUI()
    {
        if (uiRoutine != null)
            StopCoroutine(uiRoutine);

        uiRoutine = StartCoroutine(SwitchUI(reloadUI, gunUI));
    }

    private IEnumerator SwitchUI(CanvasGroup hideUI, CanvasGroup showUI)
    {
        if (showUI != null)
        {
            showUI.gameObject.SetActive(true);
            showUI.interactable = false;
            showUI.blocksRaycasts = false;
        }

        float timer = 0f;

        Vector3 showStartScale = useUIScaleAnimation ? reloadPopScale : normalUIScale;
        Vector3 showEndScale = normalUIScale;

        if (showUI != null)
            showUI.transform.localScale = showStartScale;

        while (timer < uiFadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / uiFadeTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            if (hideUI != null)
                hideUI.alpha = Mathf.Lerp(1f, 0f, t);

            if (showUI != null)
            {
                showUI.alpha = Mathf.Lerp(0f, 1f, t);

                if (useUIScaleAnimation)
                    showUI.transform.localScale = Vector3.Lerp(showStartScale, showEndScale, t);
            }

            yield return null;
        }

        if (hideUI != null)
        {
            hideUI.alpha = 0f;
            hideUI.interactable = false;
            hideUI.blocksRaycasts = false;
            hideUI.gameObject.SetActive(false);
        }

        if (showUI != null)
        {
            showUI.alpha = 1f;
            showUI.interactable = true;
            showUI.blocksRaycasts = true;
            showUI.transform.localScale = normalUIScale;
        }

        uiRoutine = null;
    }

    private void ShootProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab missing!");
            return;
        }

        if (muzzlePoint == null)
        {
            Debug.LogError("Muzzle Point missing!");
            return;
        }

        Vector3 shootDirection = muzzlePoint.forward;
        shootDirection.y = 0f;
        shootDirection.Normalize();

        Vector3 spawnPos = muzzlePoint.position + shootDirection * 0.35f;
        Quaternion spawnRot = Quaternion.LookRotation(shootDirection);

        GameObject bulletObj = Instantiate(projectilePrefab, spawnPos, spawnRot);
        bulletObj.SetActive(true);

        Collider bulletCollider = bulletObj.GetComponent<Collider>();
        Collider[] playerColliders = GetComponentsInChildren<Collider>();

        if (bulletCollider != null)
        {
            foreach (Collider playerCol in playerColliders)
            {
                if (playerCol != null)
                    Physics.IgnoreCollision(bulletCollider, playerCol, true);
            }
        }

        BulletProjectile bullet = bulletObj.GetComponent<BulletProjectile>();

        if (bullet == null)
            bullet = bulletObj.AddComponent<BulletProjectile>();

        bullet.speed = projectileSpeed;
        bullet.lifeTime = projectileLifeTime;
        bullet.damage = projectileDamage;
        bullet.hitEffectPrefab = hitEffectPrefab;
        bullet.moveDirection = shootDirection;

        Rigidbody rb = bulletObj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = shootDirection * projectileSpeed;
#else
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.velocity = shootDirection * projectileSpeed;
#endif
        }

        Destroy(bulletObj, projectileLifeTime + 0.2f);

        if (showDebug)
            Debug.Log("Bullet Spawned: " + bulletObj.name);
    }

    private void SpawnMuzzleFlash()
    {
        if (muzzleFlashPrefab == null || muzzlePoint == null) return;

        GameObject flash = Instantiate(
            muzzleFlashPrefab,
            muzzlePoint.position,
            muzzlePoint.rotation
        );

        Destroy(flash, 2f);
    }

    private void MovePlayer()
    {
        if ((stopMovementWhileShooting && isShooting) ||
            (stopMovementWhileReloading && isReloading))
        {
            currentMove = Vector3.zero;
            moveVelocity = Vector3.zero;

            if (controller.isGrounded)
                verticalVelocity = -1f;
            else
                verticalVelocity += gravity * Time.deltaTime;

            Vector3 gravityMove = new Vector3(0f, verticalVelocity, 0f);
            controller.Move(gravityMove * Time.deltaTime);

            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 targetMove = new Vector3(h, 0f, v).normalized;

        currentMove = Vector3.SmoothDamp(
            currentMove,
            targetMove,
            ref moveVelocity,
            inputSmoothTime
        );

        if (controller.isGrounded)
            verticalVelocity = -1f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = currentMove * moveSpeed;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);

        if (currentMove.magnitude > 0.15f)
        {
            string animName = GetDirectionalAnim(currentMove);
            PlayAnim(animName);
        }
        else
        {
            PlayAnim(idleAnim);
        }
    }

    private void AimToMouse()
    {
        if (mainCamera == null || model == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 300f, groundLayer))
        {
            Vector3 dir = hit.point - model.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.01f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir);

            model.rotation = Quaternion.Slerp(
                model.rotation,
                targetRot,
                turnSpeed * Time.deltaTime
            );
        }
    }

    private string GetDirectionalAnim(Vector3 worldMoveDir)
    {
        Vector3 localDir = model.InverseTransformDirection(worldMoveDir);
        localDir.y = 0f;
        localDir.Normalize();

        float angle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

        if (angle >= -22.5f && angle < 22.5f) return forwardAnim;
        if (angle >= 22.5f && angle < 67.5f) return forwardRightAnim;
        if (angle >= 67.5f && angle < 112.5f) return rightAnim;
        if (angle >= 112.5f && angle < 157.5f) return backwardRightAnim;

        if (angle >= 157.5f || angle < -157.5f) return backwardAnim;
        if (angle >= -157.5f && angle < -112.5f) return backwardLeftAnim;
        if (angle >= -112.5f && angle < -67.5f) return leftAnim;
        if (angle >= -67.5f && angle < -22.5f) return forwardLeftAnim;

        return idleAnim;
    }

    private void PlayAnim(string animName)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(animName)) return;
        if (currentAnim == animName) return;

        animator.CrossFadeInFixedTime(animName, animFadeTime);
        currentAnim = animName;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, autoShootRadius);
    }

    // ===== WEAPON UPGRADE FUNCTIONS =====

    public void UpgradeDeadeyeRounds()
    {
        projectileDamage += 1f;
        Debug.Log("Deadeye Rounds Applied: Damage = " + projectileDamage);
    }

    public void UpgradeQuickTrigger()
    {
        fireRate *= 0.85f; // 15% faster
        fireRate = Mathf.Max(0.05f, fireRate);

        Debug.Log("Quick Trigger Applied: Fire Rate = " + fireRate);
    }

    public void UpgradeBiggerChamber()
    {
        maxBullets += 2;
        currentBullets += 2;

        UpdateAmmoUI();

        Debug.Log("Bigger Chamber Applied: Max Bullets = " + maxBullets);
    }
}