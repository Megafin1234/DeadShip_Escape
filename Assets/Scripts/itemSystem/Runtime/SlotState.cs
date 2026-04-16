using System;

[Serializable]
public class SlotState
{
    public int SlotIndex;

    public SlotKind SlotKind;
    public PositionIndex PositionIndex;

    public string ItemInstanceId;

    public bool IsUnlocked = true;
    public bool IsReserved = false;

    public SlotState(int index, SlotKind kind, PositionIndex position)
    {
        SlotIndex = index;
        SlotKind = kind;
        PositionIndex = position;
        ItemInstanceId = null;
    }

    public bool IsEmpty => string.IsNullOrEmpty(ItemInstanceId);
}