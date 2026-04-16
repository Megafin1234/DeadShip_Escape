public class ItemDragPayload
{
    public ContainerPanelView SourcePanel { get; private set; }
    public SlotBindData SourceBindData { get; private set; }
    public bool IsSplitDrag { get; private set; }

    public ItemDragPayload(ContainerPanelView sourcePanel, SlotBindData sourceBindData, bool isSplitDrag = false)
    {
        SourcePanel = sourcePanel;
        SourceBindData = sourceBindData;
        IsSplitDrag = isSplitDrag;
    }
}