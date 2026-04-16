public class EquipService
{
    private readonly ItemMoveService moveService;

    public EquipService(ItemMoveService moveService)
    {
        this.moveService = moveService;
    }

    public ItemMoveResult EquipItem(
        ItemInstance item,
        ContainerState inventoryContainer,
        SlotState inventorySlot,
        ContainerState equipmentContainer,
        SlotState targetEquipSlot,
        SquadRuntime squad)
    {
        return moveService.MoveItem(
            item,
            inventoryContainer,
            inventorySlot,
            equipmentContainer,
            targetEquipSlot,
            squad);
    }

    public ItemMoveResult UnequipItem(
        ItemInstance item,
        ContainerState equipmentContainer,
        SlotState equipmentSlot,
        ContainerState inventoryContainer,
        SlotState targetInventorySlot,
        SquadRuntime squad)
    {
        return moveService.MoveItem(
            item,
            equipmentContainer,
            equipmentSlot,
            inventoryContainer,
            targetInventorySlot,
            squad);
    }
}