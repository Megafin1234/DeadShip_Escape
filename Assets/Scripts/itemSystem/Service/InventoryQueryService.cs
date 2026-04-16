using UnityEngine;

public class InventoryQueryService  //아이템을 어디로 보내야 하는가
{
    private readonly ItemDatabase itemDatabase;

    public InventoryQueryService(ItemDatabase itemDatabase)
    {
        this.itemDatabase = itemDatabase;
    }

    public SlotState FindFirstValidEquipSlot(ItemInstance item, ContainerState equipmentContainer, SquadRuntime squad)
    {
        if (item == null || equipmentContainer == null || squad == null)
            return null;

        ItemDefinitionBase itemDef = itemDatabase.GetDefinition(item.DefinitionId);
        if (itemDef == null || !itemDef.IsEquippable)
            return null;

        for (int i = 0; i < equipmentContainer.Slots.Count; i++)
        {
            SlotState slot = equipmentContainer.Slots[i];

            if (slot.SlotKind != SlotKind.Equipment)
                continue;

            if (!slot.IsUnlocked || !slot.IsEmpty)
                continue;

            if (ItemSlotRules.CanPlaceItemInSlot(itemDef, slot, squad, itemDatabase))
                return slot;
        }

        return null;
    }

    public SlotState FindFirstStackableSlot(ItemInstance item, ContainerState container, ItemInstanceRepository repo, SlotKind slotKind)
    {
        if (item == null || container == null || repo == null)
            return null;

        ItemDefinitionBase def = itemDatabase.GetDefinition(item.DefinitionId);
        if (def == null || def.MaxStack <= 1)
            return null;

        for (int i = 0; i < container.Slots.Count; i++)
        {
            SlotState slot = container.Slots[i];

            if (slot.SlotKind != slotKind)
                continue;

            if (!slot.IsUnlocked || slot.IsEmpty)
                continue;

            ItemInstance targetItem = repo.Get(slot.ItemInstanceId);
            if (targetItem == null)
                continue;

            if (targetItem.DefinitionId != item.DefinitionId)
                continue;

            if (targetItem.StackCount >= def.MaxStack)
                continue;

            return slot;
        }

        return null;
    }
}