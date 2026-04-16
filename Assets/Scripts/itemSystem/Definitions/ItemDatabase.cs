using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item System/Definitions/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemDefinitionBase> itemDefinitions = new();

    private Dictionary<string, ItemDefinitionBase> lookup;

    public IReadOnlyList<ItemDefinitionBase> ItemDefinitions => itemDefinitions;

    public void Initialize()
    {
        lookup = new Dictionary<string, ItemDefinitionBase>();

        Debug.Log($"[ItemDatabase] Initialize 시작 - 등록 대상 수: {itemDefinitions.Count}");

        for (int i = 0; i < itemDefinitions.Count; i++)
        {
            ItemDefinitionBase def = itemDefinitions[i];
            if (def == null)
            {
                Debug.LogWarning($"[ItemDatabase] null itemDefinition at index {i}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(def.ItemId))
            {
                Debug.LogWarning($"[ItemDatabase] 빈 ItemId를 가진 아이템 정의: asset={def.name}, displayName={def.DisplayName}");
                continue;
            }

            if (lookup.ContainsKey(def.ItemId))
            {
                Debug.LogWarning($"[ItemDatabase] 중복 ItemId 발견: key={def.ItemId}, asset={def.name}");
                continue;
            }

            lookup.Add(def.ItemId, def);
            Debug.Log($"[ItemDatabase] Register: key={def.ItemId}, displayName={def.DisplayName}, asset={def.name}");
        }

        Debug.Log($"[ItemDatabase] Initialize 완료 - 최종 등록 수: {lookup.Count}");
    }

    public ItemDefinitionBase GetDefinition(string itemId)
    {
        if (lookup == null)
            Initialize();

        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogWarning("[ItemDatabase] GetDefinition 호출 - itemId가 비어 있음");
            return null;
        }

        lookup.TryGetValue(itemId, out ItemDefinitionBase result);

        if (result == null)
            Debug.LogWarning($"[ItemDatabase] GetDefinition 실패 - key={itemId}");

        return result;
    }
}