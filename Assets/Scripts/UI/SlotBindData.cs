public class SlotBindData
{
    public SlotState SlotState;          // UI 표시용
    public SlotState SourceSlot;         // 실제 이동에 사용할 원본 슬롯
    public ContainerState SourceContainer;
    public ItemInstance ItemInstance;
    public ItemDefinitionBase ItemDefinition;

    public bool IsLocked;
    public bool IsInsuranceProtected;

    public bool IsFavoriteMarked;
    public bool IsQuestMarked;
}