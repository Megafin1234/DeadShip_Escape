using System;
using UnityEngine;

[Serializable]
public class LootContainerRuntime
{
    public string ContainerId;
    public string DisplayName;

    public ContainerState Container;
    public bool IsOpened;
    public bool IsEmpty;

    public LootContainerRuntime(string displayName, int slotCount)
    {
        ContainerId = Guid.NewGuid().ToString();
        DisplayName = displayName;

        Container = new ContainerState(ContainerType.LootContainer);

        for (int i = 0; i < slotCount; i++)
        {
            Container.Slots.Add(new SlotState(i, SlotKind.Loot, PositionIndex.None));
        }

        IsOpened = false;
        IsEmpty = true;
    }

    public void RefreshEmptyState()
    {
        IsEmpty = true;

        for (int i = 0; i < Container.Slots.Count; i++)
        {
            if (!Container.Slots[i].IsEmpty)
            {
                IsEmpty = false;
                return;
            }
        }
    }
}