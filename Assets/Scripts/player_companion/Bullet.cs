using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 35f;
    public float lifeTime = 1f;
    public int damage = 1;

    private Transform owner;

    public void Initialize(int newDamage, Transform ownerTransform = null)
    {
        damage = newDamage;
        owner = ownerTransform;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            if (enemy != null)
            {
                enemy.TakeHit();
            }

            // 플레이어가 쏜 총알이면 타겟 공유
            if (owner != null && owner.CompareTag("Player"))
            {
                PlayerCombatContext combatContext = owner.GetComponent<PlayerCombatContext>();
                if (combatContext != null)
                {
                    combatContext.SetTarget(other.transform);
                }
            }

            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}