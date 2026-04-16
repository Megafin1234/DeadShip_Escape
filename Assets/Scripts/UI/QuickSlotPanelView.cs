using System;
using System.Collections.Generic;
using UnityEngine;

public class QuickSlotPanelView : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private List<QuickSlotView> slotViews = new();

    public Action<QuickSlotView> OnQuickSlotDroppedOn;
    public Action<QuickSlotView> OnQuickSlotRightClicked;

    private void Awake()
    {
        for (int i = 0; i < slotViews.Count; i++)
        {
            if (slotViews[i] != null)
            {
                slotViews[i].OnDroppedOn += HandleQuickSlotDroppedOn;
                slotViews[i].OnRightClick += HandleQuickSlotRightClicked;
            }
        }
    }

    private void HandleQuickSlotDroppedOn(QuickSlotView slotView)
    {
        OnQuickSlotDroppedOn?.Invoke(slotView);
    }

    private void HandleQuickSlotRightClicked(QuickSlotView slotView)
    {
        OnQuickSlotRightClicked?.Invoke(slotView);
    }

    public void Bind(PlayerSquadBridge bridge)
    {
        if (bridge == null || bridge.Squad == null)
            return;

        var quickSlots = bridge.Squad.QuickSlots;

        for (int i = 0; i < slotViews.Count; i++)
        {
            if (slotViews[i] == null)
                continue;

            slotViews[i].SetSlotIndex(i);

            if (quickSlots == null || i >= quickSlots.Count || quickSlots[i].IsEmpty)
            {
                slotViews[i].Clear();
                continue;
            }

            string instanceId = quickSlots[i].ItemInstanceId;
            ItemInstance itemInstance = bridge.ItemRepository.Get(instanceId);

            if (itemInstance == null)
            {
                slotViews[i].Clear();
                continue;
            }

            ItemDefinitionBase def = bridge.GetItemDefinition(itemInstance.DefinitionId);
            if (def == null)
            {
                slotViews[i].Clear();
                continue;
            }

            slotViews[i].Bind(itemInstance, def);
        }
    }

    public void RefreshView(PlayerSquadBridge bridge)
    {
        Bind(bridge);
    }
}