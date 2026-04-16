using UnityEngine;

public static class ItemSlotRules
{
    public static bool CanPlaceItemInSlot(
        ItemDefinitionBase itemDef,
        SlotState targetSlot,
        SquadRuntime squad,
        ItemDatabase itemDatabase)
    {
        if (itemDef == null || targetSlot == null)
            return false;

        if (!targetSlot.IsUnlocked)
            return false;

        switch (targetSlot.SlotKind)
        {
            case SlotKind.Inventory:
                return CanPlaceInInventory(itemDef);

            case SlotKind.Equipment:
                return CanPlaceInEquipment(itemDef, targetSlot, squad);

            case SlotKind.QuickSlot:
                return CanPlaceInQuickSlot(itemDef);

            case SlotKind.Loot:
                return true;

            case SlotKind.Trade:
                return true;

            default:
                return false;
        }
    }

    private static bool CanPlaceInInventory(ItemDefinitionBase itemDef)
    {
        return itemDef.IsStorable;
    }

    private static bool CanPlaceInQuickSlot(ItemDefinitionBase itemDef)
    {
        return itemDef.IsUsable;
    }

    private static bool CanPlaceInEquipment(
        ItemDefinitionBase itemDef,
        SlotState targetSlot,
        SquadRuntime squad)
    {
        if (!itemDef.IsEquippable)
            return false;

        if (itemDef is not EquipmentDefinition equipmentDef)
            return false;

        PositionIndex position = targetSlot.PositionIndex;
        PositionRuleDefinition rule = squad.GetPositionRule(position);
        if (rule == null)
            return false;

        int localSlotIndex = squad.GetLocalEquipmentSlotIndex(targetSlot);
        if (localSlotIndex < 0 || localSlotIndex >= rule.EquipSlotLayout.Length)
            return false;

        EquipSlotType requiredSlotType = rule.EquipSlotLayout[localSlotIndex];
        return MatchEquipType(requiredSlotType, equipmentDef.EquipmentType);
    }

    private static bool MatchEquipType(EquipSlotType slotType, EquipmentType equipmentType)
    {
        return slotType switch
        {
            EquipSlotType.Weapon => equipmentType == EquipmentType.Weapon,
            EquipSlotType.Helmet => equipmentType == EquipmentType.Helmet,
            EquipSlotType.Armor => equipmentType == EquipmentType.Armor,
            EquipSlotType.Gloves => equipmentType == EquipmentType.Gloves,
            EquipSlotType.Boots => equipmentType == EquipmentType.Boots,
            EquipSlotType.Bag => equipmentType == EquipmentType.Bag,
            EquipSlotType.Special => equipmentType == EquipmentType.Special,
            _ => false
        };
    }
}