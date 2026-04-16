using System;

[Serializable]
public class QuickSlotRuntime
{
    public int SlotIndex;
    public string ItemInstanceId;

    public QuickSlotRuntime(int slotIndex)
    {
        SlotIndex = slotIndex;
        ItemInstanceId = null;
    }

    public bool IsEmpty => string.IsNullOrEmpty(ItemInstanceId);

    public void Clear()
    {
        ItemInstanceId = null;
    }
}