using TMPro;
using UnityEngine;

public class CompanionShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private PlayerSquadBridge bridge;

    [Header("Reload UI")]
    [SerializeField] private TMP_Text reloadStateText;

    [Header("Fallback")]
    [SerializeField] private int fallbackDamage = 1;
    [SerializeField] private float fallbackFireInterval = 0.25f;

    [Header("Magazine")]
    [SerializeField] private int magazineSize = 10;
    [SerializeField] private float reloadTime = 2f;

    [Header("Accuracy")]
    [SerializeField] private float spreadAngle = 3f;

    private float fireTimer = 0f;
    private int currentAmmo;
    private bool isReloading = false;
    private float reloadTimer = 0f;

    private void Awake()
    {
        currentAmmo = magazineSize;
        RefreshReloadUI();
    }

    private void Update()
    {
        UpdateFireTimer();
        UpdateReload();
    }

    public void SetBridge(PlayerSquadBridge squadBridge)
    {
        bridge = squadBridge;
    }
    public bool TryFire()
    {
        if (bulletPrefab == null || firePoint == null)
            return false;

        if (isReloading)
            return false;

        if (fireTimer > 0f)
            return false;

        if (currentAmmo <= 0)
        {
            StartReload();
            return false;
        }

        float fireInterval = fallbackFireInterval;
        int damage = fallbackDamage;

        if (bridge != null && bridge.Squad != null)
        {
            if (bridge.Squad.DerivedStats.FireInterval > 0f)
                fireInterval = bridge.Squad.DerivedStats.FireInterval;

            if (bridge.Squad.DerivedStats.Attack > 0)
                damage = bridge.Squad.DerivedStats.Attack;
        }

        Quaternion fireRotation = firePoint.rotation;

        if (spreadAngle > 0f)
        {
            float randomYaw = Random.Range(-spreadAngle, spreadAngle);
            fireRotation = firePoint.rotation * Quaternion.Euler(0f, randomYaw, 0f);
        }

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, fireRotation);

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(damage);
        }

        currentAmmo--;
        fireTimer = fireInterval;

        if (currentAmmo <= 0)
        {
            StartReload();
        }

        return true;
    }

    private void StartReload()
    {
        if (isReloading)
            return;

        isReloading = true;
        reloadTimer = reloadTime;
        RefreshReloadUI();
    }

    private void UpdateReload()
    {
        if (!isReloading)
            return;

        reloadTimer -= Time.deltaTime;

        if (reloadTimer <= 0f)
        {
            isReloading = false;
            currentAmmo = magazineSize;
            RefreshReloadUI();
        }
    }

    private void UpdateFireTimer()
    {
        if (fireTimer > 0f)
            fireTimer -= Time.deltaTime;
    }

    private void RefreshReloadUI()
    {
        if (reloadStateText == null)
            return;

        reloadStateText.gameObject.SetActive(isReloading);

        if (isReloading)
            reloadStateText.text = "재장전 중!";
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetMagazineSize()
    {
        return magazineSize;
    }

    public void ResetRuntimeStateAfterRespawn()
    {
        fireTimer = 0f;
        reloadTimer = 0f;
        isReloading = false;
        currentAmmo = magazineSize;
        RefreshReloadUI();
    }
}