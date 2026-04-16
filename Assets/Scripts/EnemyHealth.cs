using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public Transform healthBarFill;

    [Header("Quest")]
    [SerializeField] private string enemyQuestId = "enemy_default";

    [Header("Loot Drop")]
    [SerializeField] private LootInteractable enemyLootPrefab;
    [SerializeField] private string lootDisplayName = "전리품";
    [SerializeField] private int lootSlotCount = 4;
    [SerializeField] private int minDropItemCount = 1;
    [SerializeField] private int maxDropItemCount = 3;
    [SerializeField] private Vector3 lootSpawnOffset = Vector3.zero;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            healthBarFill.localScale = new Vector3(healthPercent, 1f, 1f);
        }
    }

    void Die()
    {
        QuestManager questManager = FindAnyObjectByType<QuestManager>();
        if (questManager != null)
        {
            questManager.PublishEvent(new QuestEvent(QuestEventType.KillEnemy, enemyQuestId, 1));
        }

        SpawnEnemyLoot();

        Destroy(gameObject);
    }

    private void SpawnEnemyLoot()
    {
        if (enemyLootPrefab == null)
            return;

        PlayerSquadBridge bridge = PlayerSquadBridge.Instance;
        if (bridge == null)
        {
            Debug.LogWarning("[EnemyHealth] PlayerSquadBridge.Instance가 없어 전리품을 생성할 수 없습니다.");
            return;
        }

        ItemDatabase itemDatabase = bridge.ItemDatabase;
        if (itemDatabase == null || itemDatabase.ItemDefinitions == null || itemDatabase.ItemDefinitions.Count == 0)
        {
            Debug.LogWarning("[EnemyHealth] ItemDatabase가 없어 전리품을 생성할 수 없습니다.");
            return;
        }

        LootContainerFactory factory = new LootContainerFactory(bridge.ItemRepository);
        LootContainerRuntime runtime = factory.CreateRandomContainer(
            lootDisplayName,
            lootSlotCount,
            itemDatabase.ItemDefinitions,
            minDropItemCount,
            maxDropItemCount
        );

        if (runtime == null || runtime.IsEmpty)
            return;

        Vector3 spawnPos = transform.position + lootSpawnOffset;
        Quaternion spawnRot = Quaternion.identity;

        LootInteractable spawned = Instantiate(enemyLootPrefab, spawnPos, spawnRot);

        spawned.InitializeRuntimeContainer(runtime, lootDisplayName);
        spawned.gameObject.SetActive(true);
    }
}