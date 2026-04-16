using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ContainerPanelView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform slotGridRoot;
    [SerializeField] private ItemSlotView slotPrefab;

    private string currentTitle;
    private ContainerState currentContainer;
    private ItemDatabase itemDatabase;
    private ItemInstanceRepository itemRepository;

    private readonly List<ItemSlotView> slotViews = new();

    public Action<ContainerPanelView, ItemSlotView> OnSlotDoubleClicked;
    public Action<ContainerPanelView, ItemSlotView> OnSlotRightClicked;
    //public Action<ContainerPanelView, ItemSlotView> OnSlotLefttClicked;
    public Action<ContainerPanelView, ItemSlotView> OnSlotBeginDrag;
    public Action<ContainerPanelView, ItemSlotView> OnSlotEndDrag;
    public Action<ContainerPanelView, ItemSlotView> OnSlotDroppedOn;

    public void Bind(
        string title,
        ContainerState container,
        ItemDatabase db,
        ItemInstanceRepository repo)
    {
        currentTitle = title;
        currentContainer = container;
        itemDatabase = db;
        itemRepository = repo;

        RefreshView();
    }

    public void RefreshView()
    {
        if (currentContainer == null || slotGridRoot == null || slotPrefab == null)
            return;

        if (titleText != null)
            titleText.text = currentTitle;

        EnsureSlotCount(currentContainer.Slots.Count);

        for (int i = 0; i < slotViews.Count; i++)
        {
            SlotState slot = currentContainer.Slots[i];

            ItemInstance instance = null;
            ItemDefinitionBase def = null;

            if (!slot.IsEmpty && itemRepository != null)
            {
                instance = itemRepository.Get(slot.ItemInstanceId);
                if (instance != null && itemDatabase != null)
                    def = itemDatabase.GetDefinition(instance.DefinitionId);
            }

            SlotBindData bindData = new SlotBindData
            {
                SlotState = slot,
                SourceSlot = slot,
                SourceContainer = currentContainer,
                ItemInstance = instance,
                ItemDefinition = def,
                IsLocked = !slot.IsUnlocked,
                IsInsuranceProtected = false,
                IsFavoriteMarked = def != null && PlayerSquadBridge.Instance != null &&
                       PlayerSquadBridge.Instance.MarkerRepository.IsFavoriteMarked(def.ItemId),
                IsQuestMarked = def != null && PlayerSquadBridge.Instance != null &&
                    PlayerSquadBridge.Instance.MarkerRepository.IsQuestMarked(def.ItemId)
            };

            slotViews[i].Bind(bindData);
        }
    }

    private void EnsureSlotCount(int count)
    {
        while (slotViews.Count < count)
        {
            ItemSlotView newSlot = Instantiate(slotPrefab, slotGridRoot);

            newSlot.OnDoubleClick += HandleSlotDoubleClick;
            newSlot.OnRightClick += HandleSlotRightClick;
            newSlot.OnBeginDragEvent += HandleSlotBeginDrag;
            newSlot.OnEndDragEvent += HandleSlotEndDrag;

            ItemSlotDropReceiver dropReceiver = newSlot.GetComponent<ItemSlotDropReceiver>();
            if (dropReceiver == null)
                dropReceiver = newSlot.gameObject.AddComponent<ItemSlotDropReceiver>();

            dropReceiver.OnDropped += HandleSlotDropped;

            slotViews.Add(newSlot);
        }
    }

    private void HandleSlotDoubleClick(ItemSlotView slotView)
    {
        OnSlotDoubleClicked?.Invoke(this, slotView);
    }
    private void HandleSlotRightClick(ItemSlotView slotView)
    {
        OnSlotRightClicked?.Invoke(this, slotView);
    }
    private void HandleSlotLeftClick(ItemSlotView slotView)
    {
        //OnSlotLeftClicked?.Invoke(this, slotView);
    }
        private void HandleSlotBeginDrag(ItemSlotView slotView)
    {
        OnSlotBeginDrag?.Invoke(this, slotView);
    }

    private void HandleSlotEndDrag(ItemSlotView slotView)
    {
        OnSlotEndDrag?.Invoke(this, slotView);
    }

    private void HandleSlotDropped(ItemSlotDropReceiver receiver)
    {
        if (receiver == null || receiver.SlotView == null)
            return;

        OnSlotDroppedOn?.Invoke(this, receiver.SlotView);
    }

    public void BindFromSlotList(
        string title,
        List<SlotState> sourceSlots,
        ContainerState sourceContainer,
        ItemDatabase db,
        ItemInstanceRepository repo)
    {
        currentTitle = title;
        currentContainer = null;
        itemDatabase = db;
        itemRepository = repo;

        if (titleText != null)
            titleText.text = title;

        EnsureSlotCount(sourceSlots.Count);

        for (int i = 0; i < slotViews.Count; i++)
        {
            SlotState slot = sourceSlots[i];

            ItemInstance instance = null;
            ItemDefinitionBase def = null;

            if (!slot.IsEmpty && itemRepository != null)
            {
                instance = itemRepository.Get(slot.ItemInstanceId);
                if (instance != null && itemDatabase != null)
                    def = itemDatabase.GetDefinition(instance.DefinitionId);
            }

            SlotBindData bindData = new SlotBindData
            {
                SlotState = slot,
                SourceSlot = slot,
                SourceContainer = sourceContainer,
                ItemInstance = instance,
                ItemDefinition = def,
                IsLocked = !slot.IsUnlocked,
                IsInsuranceProtected = false,
                IsFavoriteMarked = def != null && PlayerSquadBridge.Instance != null &&
                    PlayerSquadBridge.Instance.MarkerRepository.IsFavoriteMarked(def.ItemId),
                IsQuestMarked = def != null && PlayerSquadBridge.Instance != null &&
                    PlayerSquadBridge.Instance.MarkerRepository.IsQuestMarked(def.ItemId)
            };

            slotViews[i].Bind(bindData);
        }
    }
}