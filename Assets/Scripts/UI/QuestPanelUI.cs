using System.Collections.Generic;
using UnityEngine;

public class QuestPanelUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private QuestManager questManager;

    [Header("UI")]
    [SerializeField] private Transform questListRoot;
    [SerializeField] private QuestEntryUI questEntryPrefab;

    private readonly List<QuestEntryUI> spawnedEntries = new();

    public void Refresh()
    {
        if (questManager == null || questListRoot == null || questEntryPrefab == null)
            return;

        IReadOnlyList<QuestRuntime> quests = questManager.ActiveQuests;
        EnsureEntryCount(quests.Count);

        for (int i = 0; i < spawnedEntries.Count; i++)
        {
            if (i < quests.Count)
            {
                spawnedEntries[i].gameObject.SetActive(true);
                spawnedEntries[i].Bind(quests[i], questManager);
            }
            else
            {
                spawnedEntries[i].gameObject.SetActive(false);
            }
        }
    }

    private void EnsureEntryCount(int requiredCount)
    {
        while (spawnedEntries.Count < requiredCount)
        {
            QuestEntryUI entry = Instantiate(questEntryPrefab, questListRoot);
            spawnedEntries.Add(entry);
        }
    }
}