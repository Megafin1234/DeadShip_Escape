using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SquadPanelView : MonoBehaviour
{
    [Header("Top Stats")]
    [SerializeField] private TMP_Text attackValueText;
    [SerializeField] private TMP_Text healthValueText;
    [SerializeField] private TMP_Text detailInfoText;

    [Header("Character Portraits")]
    [SerializeField] private Image position1Portrait;
    [SerializeField] private Image position2Portrait;
    [SerializeField] private Image position3Portrait;

    [Header("Equipment Panels")]
    [SerializeField] private ContainerPanelView equipmentPanelP1;
    [SerializeField] private ContainerPanelView equipmentPanelP2;
    [SerializeField] private ContainerPanelView equipmentPanelP3;

    [Header("Inventory Panels")]
    [SerializeField] private ContainerPanelView inventoryPanelP1;
    [SerializeField] private ContainerPanelView inventoryPanelP2;
    [SerializeField] private ContainerPanelView inventoryPanelP3;

    private SquadRuntime squad;
    private ItemDatabase itemDatabase;
    private ItemInstanceRepository itemRepository;

    public void Bind(SquadRuntime squadRuntime, ItemDatabase db, ItemInstanceRepository repo)
    {
        squad = squadRuntime;
        itemDatabase = db;
        itemRepository = repo;

        RefreshView();
    }

    public void RefreshView()
    {
        if (squad == null)
            return;

        RefreshTopStats();
        RefreshCharacters();
        RefreshEquipmentPanels();
        RefreshInventoryPanels();
    }

    public void RegisterSlotEvents(Action<ContainerPanelView, ItemSlotView> handler)
    {
        if (equipmentPanelP1 != null)
            equipmentPanelP1.OnSlotDoubleClicked += handler;
        if (equipmentPanelP2 != null)
            equipmentPanelP2.OnSlotDoubleClicked += handler;
        if (equipmentPanelP3 != null)
            equipmentPanelP3.OnSlotDoubleClicked += handler;

        if (inventoryPanelP1 != null)
            inventoryPanelP1.OnSlotDoubleClicked += handler;
        if (inventoryPanelP2 != null)
            inventoryPanelP2.OnSlotDoubleClicked += handler;
        if (inventoryPanelP3 != null)
            inventoryPanelP3.OnSlotDoubleClicked += handler;
    }
    public void RegisterRightClickEvents(Action<ContainerPanelView, ItemSlotView> handler)
    {
        if (equipmentPanelP1 != null)
            equipmentPanelP1.OnSlotRightClicked += handler;
        if (equipmentPanelP2 != null)
            equipmentPanelP2.OnSlotRightClicked += handler;
        if (equipmentPanelP3 != null)
            equipmentPanelP3.OnSlotRightClicked += handler;

        if (inventoryPanelP1 != null)
            inventoryPanelP1.OnSlotRightClicked += handler;
        if (inventoryPanelP2 != null)
            inventoryPanelP2.OnSlotRightClicked += handler;
        if (inventoryPanelP3 != null)
            inventoryPanelP3.OnSlotRightClicked += handler;
    }

    public bool IsEquipmentPanel(ContainerPanelView panel)
    {
        return panel == equipmentPanelP1 ||
            panel == equipmentPanelP2 ||
            panel == equipmentPanelP3;
    }

    public bool IsInventoryPanel(ContainerPanelView panel)
    {
        return panel == inventoryPanelP1 ||
            panel == inventoryPanelP2 ||
            panel == inventoryPanelP3;
    }

    private void RefreshTopStats()
    {
        if (attackValueText != null)
            attackValueText.text = squad.DerivedStats.Attack.ToString();

        if (healthValueText != null)
            healthValueText.text = $"{squad.CombatState.CurrentHealth}/{squad.CombatState.MaxHealth}";

        if (detailInfoText != null)
            detailInfoText.text = $"이속 {squad.DerivedStats.MoveSpeed:0.0}   무게 {squad.DerivedStats.CarryWeight:0.0}";
    }

    private void RefreshCharacters()
    {
        BindPortrait(position1Portrait, squad.GetCharacter(PositionIndex.Position1));
        BindPortrait(position2Portrait, squad.GetCharacter(PositionIndex.Position2));
        BindPortrait(position3Portrait, squad.GetCharacter(PositionIndex.Position3));
    }

    private void BindPortrait(Image image, CharacterDefinition character)
    {
        if (image == null)
            return;

        if (character != null && character.Portrait != null)
        {
            image.enabled = true;
            image.sprite = character.Portrait;
        }
        else
        {
            image.enabled = false;
            image.sprite = null;
        }
    }

    private void RefreshEquipmentPanels()
    {
        BindSegment(equipmentPanelP1, "장비 P1", squad.EquipmentContainer, SlotKind.Equipment, PositionIndex.Position1);
        BindSegment(equipmentPanelP2, "장비 P2", squad.EquipmentContainer, SlotKind.Equipment, PositionIndex.Position2);
        BindSegment(equipmentPanelP3, "장비 P3", squad.EquipmentContainer, SlotKind.Equipment, PositionIndex.Position3);
    }

    private void RefreshInventoryPanels()
    {
        BindSegment(inventoryPanelP1, "인벤 P1", squad.InventoryContainer, SlotKind.Inventory, PositionIndex.Position1);
        BindSegment(inventoryPanelP2, "인벤 P2", squad.InventoryContainer, SlotKind.Inventory, PositionIndex.Position2);
        BindSegment(inventoryPanelP3, "인벤 P3", squad.InventoryContainer, SlotKind.Inventory, PositionIndex.Position3);
    }

    private void BindSegment(
        ContainerPanelView panel,
        string title,
        ContainerState sourceContainer,
        SlotKind slotKind,
        PositionIndex position)
    {
        if (panel == null)
            return;

        List<SlotState> filtered = new List<SlotState>();

        for (int i = 0; i < sourceContainer.Slots.Count; i++)
        {
            SlotState slot = sourceContainer.Slots[i];

            if (slot.SlotKind != slotKind)
                continue;

            if (slot.PositionIndex != position)
                continue;

            filtered.Add(slot);
        }

        panel.BindFromSlotList(title, filtered, sourceContainer, itemDatabase, itemRepository);
    }

    public void RegisterDragEvents(
        System.Action<ContainerPanelView, ItemSlotView> beginHandler,
        System.Action<ContainerPanelView, ItemSlotView> endHandler,
        System.Action<ContainerPanelView, ItemSlotView> dropHandler)
    {
        RegisterPanelDragEvents(equipmentPanelP1, beginHandler, endHandler, dropHandler);
        RegisterPanelDragEvents(equipmentPanelP2, beginHandler, endHandler, dropHandler);
        RegisterPanelDragEvents(equipmentPanelP3, beginHandler, endHandler, dropHandler);

        RegisterPanelDragEvents(inventoryPanelP1, beginHandler, endHandler, dropHandler);
        RegisterPanelDragEvents(inventoryPanelP2, beginHandler, endHandler, dropHandler);
        RegisterPanelDragEvents(inventoryPanelP3, beginHandler, endHandler, dropHandler);
    }

    private void RegisterPanelDragEvents(
        ContainerPanelView panel,
        System.Action<ContainerPanelView, ItemSlotView> beginHandler,
        System.Action<ContainerPanelView, ItemSlotView> endHandler,
        System.Action<ContainerPanelView, ItemSlotView> dropHandler)
    {
        if (panel == null)
            return;

        panel.OnSlotBeginDrag += beginHandler;
        panel.OnSlotEndDrag += endHandler;
        panel.OnSlotDroppedOn += dropHandler;
    }
}