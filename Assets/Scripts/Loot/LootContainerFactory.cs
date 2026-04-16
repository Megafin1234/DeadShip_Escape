using System.Collections.Generic;
using UnityEngine;

public class LootContainerFactory
{
    private readonly ItemInstanceRepository itemRepository;

    public LootContainerFactory(ItemInstanceRepository itemRepository)
    {
        this.itemRepository = itemRepository;
    }

    public LootContainerRuntime CreateTestContainer(string displayName)
    {
        LootContainerRuntime loot = new LootContainerRuntime(displayName, 14);

        AddItemToFirstEmptySlot(loot, new ItemInstance("material_steel_plate", 5));
        AddItemToFirstEmptySlot(loot, new ItemInstance("consumable_medkit", 2));
        AddItemToFirstEmptySlot(loot, new ItemInstance("valuable_test_relic", 1));
        AddItemToFirstEmptySlot(loot, new ItemInstance("weapon_test_rifle", 1));
        AddItemToFirstEmptySlot(loot, new ItemInstance("equip_test_armor", 1));
        AddItemToFirstEmptySlot(loot, new ItemInstance("consumable_stim_attack", 1));
        AddItemToFirstEmptySlot(loot, new ItemInstance("consumable_stim_movespeed", 3));

        loot.RefreshEmptyState();
        return loot;
    }

    public LootContainerRuntime CreateRandomContainer(
        string displayName,
        int slotCount,
        IReadOnlyList<ItemDefinitionBase> defs,
        int minSpawnCount,
        int maxSpawnCount)
    {
        LootContainerRuntime loot = new LootContainerRuntime(displayName, slotCount);

        if (defs == null || defs.Count == 0)
        {
            loot.RefreshEmptyState();
            return loot;
        }

        int clampedMin = Mathf.Clamp(minSpawnCount, 0, slotCount);
        int clampedMax = Mathf.Clamp(maxSpawnCount, clampedMin, slotCount);

        int spawnCount = Random.Range(clampedMin, clampedMax + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            ItemDefinitionBase def = GetWeightedRandomDefinition(defs);
            if (def == null)
                continue;

            ItemInstance item = new ItemInstance(def.ItemId, 1);
            AddItemToFirstEmptySlot(loot, item);
        }

        loot.RefreshEmptyState();
        return loot;
    }

    private void AddItemToFirstEmptySlot(LootContainerRuntime loot, ItemInstance item)
    {
        itemRepository.Add(item);

        for (int i = 0; i < loot.Container.Slots.Count; i++)
        {
            SlotState slot = loot.Container.Slots[i];
            if (slot.IsUnlocked && slot.IsEmpty)
            {
                slot.ItemInstanceId = item.InstanceId;
                return;
            }
        }
    }

    private ItemDefinitionBase GetWeightedRandomDefinition(IReadOnlyList<ItemDefinitionBase> defs)
    {
        if (defs == null || defs.Count == 0)
            return null;

        float totalWeight = 0f;

        for (int i = 0; i < defs.Count; i++)
        {
            totalWeight += GetLootWeight(defs[i]);
        }

        if (totalWeight <= 0f)
            return defs[Random.Range(0, defs.Count)];

        float roll = Random.Range(0f, totalWeight);
        float accumulated = 0f;

        for (int i = 0; i < defs.Count; i++)
        {
            accumulated += GetLootWeight(defs[i]);

            if (roll <= accumulated)
                return defs[i];
        }

        return defs[defs.Count - 1];
    }

    private float GetLootWeight(ItemDefinitionBase def)
    {
        if (def == null)
            return 0f;

        switch (def.Rarity)
        {
            case ItemRarity.Common: return 10f;
            case ItemRarity.Uncommon: return 6f;
            case ItemRarity.Rare: return 3f;
            case ItemRarity.Epic: return 1.5f;
            case ItemRarity.Legendary: return 0.5f;
            default: return 1f;
        }
    }
}