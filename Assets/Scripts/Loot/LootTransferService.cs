public class LootTransferService
{
    private readonly AutoMoveService autoMoveService;
    private readonly ItemInstanceRepository itemRepository;

    public LootTransferService(AutoMoveService autoMoveService, ItemInstanceRepository itemRepository)
    {
        this.autoMoveService = autoMoveService;
        this.itemRepository = itemRepository;
    }

    public ItemMoveResult MoveFirstLootItemToSquad(OpenedLootContext context)
    {
        if (context == null || !context.IsOpen)
            return ItemMoveResult.Fail("열린 루팅 컨텍스트가 없습니다.");

        SlotState sourceSlot = FindFirstFilledLootSlot(context.LootContainer);
        if (sourceSlot == null)
            return ItemMoveResult.Fail("루팅 컨테이너가 비어 있습니다.");

        ItemInstance item = itemRepository.Get(sourceSlot.ItemInstanceId);
        if (item == null)
            return ItemMoveResult.Fail("루팅 아이템 인스턴스를 찾을 수 없습니다.");

        ItemMoveResult result = autoMoveService.AutoMoveFromLootToInventory(
            item,
            context.LootContainer.Container,
            sourceSlot,
            context.Squad
        );

        context.LootContainer.RefreshEmptyState();
        return result;
    }

    private SlotState FindFirstFilledLootSlot(LootContainerRuntime lootContainer)
    {
        for (int i = 0; i < lootContainer.Container.Slots.Count; i++)
        {
            SlotState slot = lootContainer.Container.Slots[i];
            if (slot.IsUnlocked && !slot.IsEmpty)
                return slot;
        }

        return null;
    }
}