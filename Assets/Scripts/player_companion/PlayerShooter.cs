using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;

    private float fireTimer = 0f;

    private PlayerAmmo playerAmmo;
    private PlayerController playerController;
    private PlayerSquadBridge bridge;

    [Header("Fallback")]
    [SerializeField] private int fallbackDamage = 1;
    [SerializeField] private float fallbackFireInterval = 0.12f;

    void Start()
    {
        playerAmmo = GetComponent<PlayerAmmo>();
        playerController = GetComponent<PlayerController>();
    }

    public void SetBridge(PlayerSquadBridge squadBridge)
    {
        bridge = squadBridge;
    }

    void Update()
    {
        UpdateFireTimer();
        Shoot();
    }

    void Shoot()
    {
        if (bridge != null && !bridge.CanPlayerMove()) return;
        if (playerController != null && playerController.IsDashing()) return;
        if (playerAmmo == null) return;
        if (!playerAmmo.CanShoot()) return;

        float fireInterval = fallbackFireInterval;
        int damage = fallbackDamage;

        if (bridge != null && bridge.Squad != null)
        {
            if (bridge.Squad.DerivedStats.FireInterval > 0f)
                fireInterval = bridge.Squad.DerivedStats.FireInterval;

            if (bridge.Squad.DerivedStats.Attack > 0)
                damage = bridge.Squad.DerivedStats.Attack;
        }

        if (Input.GetMouseButton(0) && fireTimer <= 0f)
        {
            bool usedAmmo = playerAmmo.TryUseAmmo(1);
            if (!usedAmmo) return;

            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(damage, transform);
            }

            SoundManager.EmitSound(transform.position, 15f);
            fireTimer = fireInterval;
        }
    }

    void UpdateFireTimer()
    {
        if (fireTimer > 0f)
        {
            fireTimer -= Time.deltaTime;
        }
    }

    public void ResetRuntimeStateAfterRespawn()
    {
        fireTimer = 0f;
    }
}