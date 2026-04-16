using System.Collections.Generic;

[System.Serializable]
public class ContainerState
{
    public ContainerType ContainerType;

    public List<SlotState> Slots = new();

    public ContainerState(ContainerType type)
    {
        ContainerType = type;
    }

    public SlotState GetSlot(int index)
    {
        return Slots[index];
    }

    public SlotState FindFirstEmptySlot(SlotKind kind, PositionIndex position = PositionIndex.None)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            SlotState slot = Slots[i];

            if (slot.SlotKind != kind)
                continue;

            if (position != PositionIndex.None && slot.PositionIndex != position)
                continue;

            if (slot.IsUnlocked && slot.IsEmpty)
                return slot;
        }

        return null;
    }

    public List<SlotState> FindAllSlots(SlotKind slotKind)
    {
        List<SlotState> result = new();

        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].SlotKind == slotKind)
                result.Add(Slots[i]);
        }

        return result;
    }
}