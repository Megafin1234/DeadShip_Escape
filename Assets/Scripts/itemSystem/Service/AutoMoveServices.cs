using System;

public class AutoMoveService
{
    private readonly ItemDatabase itemDatabase;
    private readonly ItemInstanceRepository itemRepository;
    private readonly ItemMoveService moveService;
    private readonly InventoryQueryService queryService;

    public AutoMoveService(
        ItemDatabase itemDatabase,
        ItemInstanceRepository itemRepository,
        ItemMoveService moveService)
    {
        this.itemDatabase = itemDatabase;
        this.itemRepository = itemRepository;
        this.moveService = moveService;
        this.queryService = new InventoryQueryService(itemDatabase);
    }

    public ItemMoveResult AutoMoveFromLootToInventory(
        ItemInstance item,
        ContainerState lootContainer,
        SlotState sourceSlot,
        SquadRuntime squad)
    {
        SlotState stackable = queryService.FindFirstStackableSlot(
            item,
            squad.InventoryContainer,
            itemRepository,
            SlotKind.Inventory);

        if (stackable != null)
        {
            return moveService.MoveItem(
                item,
                lootContainer,
                sourceSlot,
                squad.InventoryContainer,
                stackable,
                squad);
        }

        SlotState empty = squad.InventoryContainer.FindFirstEmptySlot(SlotKind.Inventory);
        if (empty == null)
            return ItemMoveResult.Fail("인벤토리에 빈 칸이 없습니다.");

        return moveService.MoveItem(
            item,
            lootContainer,
            sourceSlot,
            squad.InventoryContainer,
            empty,
            squad);
    }

    public ItemMoveResult AutoEquipFromInventory(
        ItemInstance item,
        SlotState inventorySlot,
        SquadRuntime squad)
    {
        SlotState equipSlot = queryService.FindFirstValidEquipSlot(
            item,
            squad.EquipmentContainer,
            squad);

        if (equipSlot == null)
            return ItemMoveResult.Fail("장착 가능한 빈 슬롯이 없습니다.");

        return moveService.MoveItem(
            item,
            squad.InventoryContainer,
            inventorySlot,
            squad.EquipmentContainer,
            equipSlot,
            squad);
    }

    public ItemMoveResult AutoUnequipToInventory(
        ItemInstance item,
        SlotState equipmentSlot,
        SquadRuntime squad)
    {
        SlotState stackable = queryService.FindFirstStackableSlot(
            item,
            squad.InventoryContainer,
            itemRepository,
            SlotKind.Inventory);

        if (stackable != null)
        {
            return moveService.MoveItem(
                item,
                squad.EquipmentContainer,
                equipmentSlot,
                squad.InventoryContainer,
                stackable,
                squad);
        }

        SlotState empty = squad.InventoryContainer.FindFirstEmptySlot(SlotKind.Inventory);
        if (empty == null)
            return ItemMoveResult.Fail("인벤토리에 빈 칸이 없습니다.");

        return moveService.MoveItem(
            item,
            squad.EquipmentContainer,
            equipmentSlot,
            squad.InventoryContainer,
            empty,
            squad);
    }

    public ItemMoveResult AutoMoveBetweenStashAndInventory(
        ItemInstance item,
        ContainerState sourceContainer,
        SlotState sourceSlot,
        ContainerState targetContainer,
        SquadRuntime squad)
    {
        SlotState stackable = queryService.FindFirstStackableSlot(
            item,
            targetContainer,
            itemRepository,
            SlotKind.Inventory);

        if (stackable != null)
        {
            return moveService.MoveItem(
                item,
                sourceContainer,
                sourceSlot,
                targetContainer,
                stackable,
                squad);
        }

        SlotState empty = targetContainer.FindFirstEmptySlot(SlotKind.Inventory);
        if (empty == null)
            return ItemMoveResult.Fail("대상 인벤토리에 빈 칸이 없습니다.");

        return moveService.MoveItem(
            item,
            sourceContainer,
            sourceSlot,
            targetContainer,
            empty,
            squad);
    }

    public ItemMoveResult AutoMoveFromInventoryToLoot(
        ItemInstance item,
        SlotState inventorySlot,
        SquadRuntime squad,
        ContainerState lootContainer)
    {
        SlotState stackable = queryService.FindFirstStackableSlot(
            item,
            lootContainer,
            itemRepository,
            SlotKind.Loot);

        if (stackable != null)
        {
            return moveService.MoveItem(
                item,
                squad.InventoryContainer,
                inventorySlot,
                lootContainer,
                stackable,
                squad);
        }

        SlotState empty = lootContainer.FindFirstEmptySlot(SlotKind.Loot);
        if (empty == null)
            return ItemMoveResult.Fail("루팅 컨테이너에 빈 칸이 없습니다.");

        return moveService.MoveItem(
            item,
            squad.InventoryContainer,
            inventorySlot,
            lootContainer,
            empty,
            squad);
    }
}