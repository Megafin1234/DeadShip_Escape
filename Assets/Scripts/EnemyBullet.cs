using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 25f;
    public float lifeTime = 3f;
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
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // 플레이어가 공격당했으니 해당 적을 타겟으로 설정
            PlayerCombatContext combatContext = other.GetComponent<PlayerCombatContext>();
            if (combatContext != null && owner != null)
            {
                combatContext.SetTarget(owner);
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