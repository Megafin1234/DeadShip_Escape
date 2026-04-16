public class ItemMoveService
{
    private readonly ItemDatabase itemDatabase;
    private readonly ItemInstanceRepository itemRepository;
    private readonly StackService stackService;

    public ItemMoveService(ItemDatabase itemDatabase, ItemInstanceRepository itemRepository)
    {
        this.itemDatabase = itemDatabase;
        this.itemRepository = itemRepository;
        this.stackService = new StackService(itemDatabase);
    }

    public ItemMoveResult MoveItem(
        ItemInstance item,
        ContainerState sourceContainer,
        SlotState sourceSlot,
        ContainerState targetContainer,
        SlotState targetSlot,
        SquadRuntime squad)
    {
        if (item == null)
            return ItemMoveResult.Fail("아이템이 없습니다.");

        if (sourceContainer == null || sourceSlot == null || targetContainer == null || targetSlot == null)
            return ItemMoveResult.Fail("컨테이너 또는 슬롯이 유효하지 않습니다.");

        if (sourceSlot.IsEmpty)
            return ItemMoveResult.Fail("원본 슬롯이 비어 있습니다.");

        if (sourceSlot.ItemInstanceId != item.InstanceId)
            return ItemMoveResult.Fail("원본 슬롯의 아이템과 전달된 아이템이 일치하지 않습니다.");

        ItemDefinitionBase itemDef = itemDatabase.GetDefinition(item.DefinitionId);
        if (itemDef == null)
            return ItemMoveResult.Fail("아이템 정의를 찾을 수 없습니다.");

        if (!ItemSlotRules.CanPlaceItemInSlot(itemDef, targetSlot, squad, itemDatabase))
            return ItemMoveResult.Fail("대상 슬롯에 이 아이템을 넣을 수 없습니다.");

        // 1) 빈 슬롯 이동
        if (targetSlot.IsEmpty)
        {
            targetSlot.ItemInstanceId = item.InstanceId;
            sourceSlot.ItemInstanceId = null;
            return ItemMoveResult.Ok("이동 성공");
        }

        ItemInstance targetItem = itemRepository.Get(targetSlot.ItemInstanceId);

        // 2) 스택 병합
        if (targetItem != null && stackService.CanStack(item, targetItem))
        {
            int merged = stackService.MergeInto(item, targetItem);

            if (item.StackCount <= 0)
            {
                sourceSlot.ItemInstanceId = null;
                itemRepository.Remove(item.InstanceId);
            }

            return merged > 0
                ? ItemMoveResult.Ok("스택 병합 성공")
                : ItemMoveResult.Fail("스택 병합 불가");
        }

        // 3) 자리 교체
        if (targetItem == null)
            return ItemMoveResult.Fail("대상 아이템을 찾을 수 없습니다.");

        ItemDefinitionBase targetDef = itemDatabase.GetDefinition(targetItem.DefinitionId);
        if (targetDef == null)
            return ItemMoveResult.Fail("대상 아이템 정의를 찾을 수 없습니다.");

        // source 아이템이 target 슬롯에 들어갈 수 있는지는 위에서 이미 확인
        // 이제 target 아이템이 source 슬롯에 들어갈 수 있는지도 확인
        if (!ItemSlotRules.CanPlaceItemInSlot(targetDef, sourceSlot, squad, itemDatabase))
            return ItemMoveResult.Fail("자리 교체 불가");

        string sourceId = sourceSlot.ItemInstanceId;
        string targetId = targetSlot.ItemInstanceId;

        sourceSlot.ItemInstanceId = targetId;
        targetSlot.ItemInstanceId = sourceId;

        return ItemMoveResult.Ok("자리 교체 성공");
    }
}