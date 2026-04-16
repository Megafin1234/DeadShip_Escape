using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;

    [Header("Attack")]
    public float attackRange = 10f;
    public float fireRate = 1.5f;

    public int burstCount = 3;
    public float burstShotInterval = 0.12f;
    public float burstCooldown = 0.8f;

    private int shotsFiredInBurst = 0;
    private float fireTimer = 0f;

    [Header("Ammo")]
    public int magazineSize = 10;
    public int currentAmmo;
    public float reloadTime = 2f;
    private bool isReloading = false;
    private float reloadTimer = 0f;

    [Header("Accuracy")]
    public float spreadAngle = 5f;

    [Header("Damage")]
    [SerializeField] private int bulletDamage = 1;

    private Transform player;

    void Start()
    {
        currentAmmo = magazineSize;

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        UpdateFireTimer();
        UpdateReload();
        TryShoot();
    }

    void TryShoot()
    {
        if (player == null) return;
        if (isReloading) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange) return;

        if (!HasLineOfSight()) return;

        if (currentAmmo <= 0)
        {
            StartReload();
            return;
        }

        if (fireTimer > 0f) return;

        Shoot();

        shotsFiredInBurst++;

        if (currentAmmo <= 0)
        {
            StartReload();
            shotsFiredInBurst = 0;
            return;
        }

        if (shotsFiredInBurst >= burstCount)
        {
            shotsFiredInBurst = 0;
            fireTimer = burstCooldown;
        }
        else
        {
            fireTimer = burstShotInterval;
        }
    }

    bool HasLineOfSight()
    {
        if (firePoint == null || player == null) return false;

        Vector3 origin = firePoint.position;
        Vector3 target = player.position + Vector3.up * 1f;

        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    void Shoot()
    {
        if (firePoint == null || enemyBulletPrefab == null || player == null)
            return;

        Vector3 direction = (player.position - firePoint.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            float randomYaw = Random.Range(-spreadAngle, spreadAngle);
            Quaternion spreadRotation = Quaternion.Euler(0f, randomYaw, 0f);
            Vector3 finalDirection = spreadRotation * direction;

            firePoint.rotation = Quaternion.LookRotation(finalDirection);
        }

        GameObject bulletObj = Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);

        EnemyBullet bullet = bulletObj.GetComponent<EnemyBullet>();
        if (bullet != null)
        {
            // 핵심: 발사자를 owner로 전달
            bullet.Initialize(bulletDamage, transform);
        }

        currentAmmo--;
    }

    void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTime;
    }

    void UpdateReload()
    {
        if (!isReloading) return;

        reloadTimer -= Time.deltaTime;

        if (reloadTimer <= 0f)
        {
            isReloading = false;
            currentAmmo = magazineSize;
        }
    }

    void UpdateFireTimer()
    {
        if (fireTimer > 0f)
        {
            fireTimer -= Time.deltaTime;
        }
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public float GetAttackRange()
    {
        return attackRange;
    }

    public bool CanSeePlayer()
    {
        return HasLineOfSight();
    }
}