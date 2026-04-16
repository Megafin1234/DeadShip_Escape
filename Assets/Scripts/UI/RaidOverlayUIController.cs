using UnityEngine;

public class RaidOverlayUIController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject overlayRoot;

    [Header("Left Panel")]
    [SerializeField] private SquadPanelView squadPanelView;

    [Header("Right Context Panels")]
    [SerializeField] private GameObject lootPanelRoot;
    [SerializeField] private ContainerPanelView lootPanelView;

    [SerializeField] private GameObject mapPanelRoot;
    [SerializeField] private GameObject stashPanelRoot;
    [SerializeField] private ContainerPanelView stashPanelView;

    [SerializeField] private GameObject questPanelRoot;
    [SerializeField] private QuestPanelUI questPanelUI;
    [SerializeField] private GameObject characterDexPanelRoot;
    [SerializeField] private GameObject itemDexPanelRoot;
    [SerializeField] private GameObject settingsPanelRoot;

    [Header("Menu Bar")]
    [SerializeField] private GameObject menuBarRoot;
    [SerializeField] private GameObject mapButtonObject;
    [SerializeField] private GameObject stashButtonObject;

    [Header("Context Menu")]
    [SerializeField] private ItemContextMenuUI itemContextMenuUI;

    [Header("Dropped Item")]
    [SerializeField] private DroppedItemSpawner droppedItemSpawner;
    [SerializeField] private Transform playerTransform;

    [Header("Drag")]
    [SerializeField] private ItemDragVisual itemDragVisual;
    [Header("Stack Divide")]
    [SerializeField] private SplitStackPopupUI splitStackPopupUI;

    [Header("Loss Protection Highlights")]
    [SerializeField] private LossProtectionPanelHighlight inventoryP1Highlight;
    [SerializeField] private LossProtectionPanelHighlight inventoryP2Highlight;
    [SerializeField] private LossProtectionPanelHighlight inventoryP3Highlight;

    [SerializeField] private LossProtectionPanelHighlight equipmentP1Highlight;
    [SerializeField] private LossProtectionPanelHighlight equipmentP2Highlight;
    [SerializeField] private LossProtectionPanelHighlight equipmentP3Highlight;

    [Header("Dependencies")]
    [SerializeField] private PlayerSquadBridge bridge;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private SessionStateController sessionStateController;
    [SerializeField] private QuickSlotPanelView quickSlotPanelView;

    private OverlayContextType currentContext = OverlayContextType.None;
    private ItemDragPayload currentDragPayload;

    public bool IsAnyPanelOpen => overlayRoot != null && overlayRoot.activeSelf;
    public OverlayContextType CurrentContext => currentContext;
    private bool dropHandledThisDrag = false;

    private void Start()
    {
        CloseOverlayImmediate();

        if (lootPanelView != null)
        {
            lootPanelView.OnSlotDoubleClicked += OnSlotDoubleClicked;
            lootPanelView.OnSlotRightClicked += OnSlotRightClicked;
            lootPanelView.OnSlotBeginDrag += OnSlotBeginDrag;
            lootPanelView.OnSlotEndDrag += OnSlotEndDrag;
            lootPanelView.OnSlotDroppedOn += OnSlotDroppedOn;
        }

        if (stashPanelView != null)
        {
            stashPanelView.OnSlotDoubleClicked += OnSlotDoubleClicked;
            stashPanelView.OnSlotRightClicked += OnSlotRightClicked;
            stashPanelView.OnSlotBeginDrag += OnSlotBeginDrag;
            stashPanelView.OnSlotEndDrag += OnSlotEndDrag;
            stashPanelView.OnSlotDroppedOn += OnSlotDroppedOn;
        }

        if (squadPanelView != null)
        {
            squadPanelView.RegisterSlotEvents(OnSlotDoubleClicked);
            squadPanelView.RegisterRightClickEvents(OnSlotRightClicked);
            squadPanelView.RegisterDragEvents(OnSlotBeginDrag, OnSlotEndDrag, OnSlotDroppedOn);
        }

        if (itemContextMenuUI != null)
        {
            itemContextMenuUI.OnActionSelected += OnItemContextActionSelected;
        }
        if (quickSlotPanelView != null)
        {
            quickSlotPanelView.OnQuickSlotDroppedOn += OnQuickSlotDroppedOn;
            quickSlotPanelView.OnQuickSlotRightClicked += OnQuickSlotRightClicked;
        }
        if (bridge != null)
        {
            bridge.OnSquadVisualStateChanged += HandleSquadVisualStateChanged;
            HandleSquadVisualStateChanged(); // 시작 시 1회 즉시 반영
        }
    }

    private void Update()
    {
            if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("[RaidOverlayUIController] TAB 입력 감지");
        }
        HandleToggleInput();
    }

    private void HandleToggleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (IsAnyPanelOpen)
            {
                CloseOverlay();
            }
            else
            {
                OpenDefaultOverlay();
            }
        }

        if (IsAnyPanelOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseOverlay();
        }
    }

    public void OpenDefaultOverlay()
    {
        if (bridge == null)
            return;

        if (overlayRoot != null)
            overlayRoot.SetActive(true);

        if (sessionStateController != null && sessionStateController.IsInRaid)
        {
            ShowMapContext();
        }
        else
        {
            ShowStashContext();
        }
    }

    public void OpenLootOverlay(LootContainerRuntime lootContainer)
    {
        if (bridge == null || lootContainer == null)
            return;

        overlayRoot.SetActive(true);
        bridge.OpenLootContainer(lootContainer);

        RefreshLeftSquadPanel();

        if (lootPanelView != null)
        {
            lootPanelView.Bind(
                lootContainer.DisplayName,
                lootContainer.Container,
                itemDatabase,
                bridge.ItemRepository
            );
        }

        ShowContext(OverlayContextType.Loot);
    }

    public void ShowStashContext()
    {
        if (bridge == null)
            return;

        if (overlayRoot != null)
            overlayRoot.SetActive(true);

        RefreshLeftSquadPanel();

        if (stashPanelView != null)
        {
            stashPanelView.Bind(
                "창고",
                bridge.GetOrCreateStashContainer(),
                itemDatabase,
                bridge.ItemRepository
            );
        }

        ShowContext(OverlayContextType.Stash);
    }

    public void ShowMapContext()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(true);

        RefreshLeftSquadPanel();
        ShowContext(OverlayContextType.Map);
    }

    public void ShowQuestContext()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(true);

        RefreshLeftSquadPanel();

        if (questPanelUI != null)
            questPanelUI.Refresh();

        ShowContext(OverlayContextType.Quest);
    }

    public void ShowCharacterDexContext()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(true);

        RefreshLeftSquadPanel();
        ShowContext(OverlayContextType.CharacterDex);
    }

    public void ShowItemDexContext()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(true);

        RefreshLeftSquadPanel();
        ShowContext(OverlayContextType.ItemDex);
    }

    public void ShowSettingsContext()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(true);

        RefreshLeftSquadPanel();
        ShowContext(OverlayContextType.Settings);
    }

    public void RefreshAll()
    {
        if (!IsAnyPanelOpen || bridge == null)
            return;

        RefreshLeftSquadPanel();

        switch (currentContext)
        {
            case OverlayContextType.Loot:
                if (lootPanelView != null && bridge.CurrentLootContext != null)
                    lootPanelView.RefreshView();
                break;

            case OverlayContextType.Stash:
                if (stashPanelView != null)
                    stashPanelView.RefreshView();
                break;

            case OverlayContextType.Quest:
                if (questPanelUI != null)
                    questPanelUI.Refresh();
                break;
        }
    }

    public void CloseOverlay()
    {
        if (bridge != null)
            bridge.CloseLootContainer();
            
        if (itemContextMenuUI != null)
            itemContextMenuUI.Hide();

        CloseOverlayImmediate();
    }

    private void CloseOverlayImmediate()
    {
        if (overlayRoot != null)
            overlayRoot.SetActive(false);

        SetPanelActive(lootPanelRoot, false);
        SetPanelActive(mapPanelRoot, false);
        SetPanelActive(stashPanelRoot, false);
        SetPanelActive(questPanelRoot, false);
        SetPanelActive(characterDexPanelRoot, false);
        SetPanelActive(itemDexPanelRoot, false);
        SetPanelActive(settingsPanelRoot, false);

        currentContext = OverlayContextType.None;
        RefreshMenuBar();
    }

    private void ShowContext(OverlayContextType context)
    {
        SetPanelActive(lootPanelRoot, context == OverlayContextType.Loot);
        SetPanelActive(mapPanelRoot, context == OverlayContextType.Map);
        SetPanelActive(stashPanelRoot, context == OverlayContextType.Stash);
        SetPanelActive(questPanelRoot, context == OverlayContextType.Quest);
        SetPanelActive(characterDexPanelRoot, context == OverlayContextType.CharacterDex);
        SetPanelActive(itemDexPanelRoot, context == OverlayContextType.ItemDex);
        SetPanelActive(settingsPanelRoot, context == OverlayContextType.Settings);

        currentContext = context;
        RefreshMenuBar();
        Debug.Log($"[RaidOverlayUIController] Context Changed -> {currentContext}");
    }

    private void SetPanelActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    private void RefreshLeftSquadPanel()
    {
        if (bridge == null || squadPanelView == null)
            return;

        squadPanelView.Bind(
            bridge.Squad,
            itemDatabase,
            bridge.ItemRepository
        );
    }

    private void OnSlotDoubleClicked(ContainerPanelView panel, ItemSlotView slotView)
    {
        if (slotView == null || slotView.BindData == null)
            return;

        SlotBindData bindData = slotView.BindData;

        if (bindData.IsLocked || bindData.ItemInstance == null)
            return;

        ItemMoveResult result = HandleSlotDoubleClick(panel, bindData);

        if (result.Success && bridge != null)
        {
            bridge.RecalculateAndApply();
        }

        Debug.Log($"[RaidOverlayUIController] 이동 결과: {result.Message}");
        RefreshAll();
    }

    private ItemMoveResult HandleSlotDoubleClick(ContainerPanelView panel, SlotBindData bindData)
    {
        if (bridge == null)
            return ItemMoveResult.Fail("브리지가 없습니다.");

        if (panel == lootPanelView)
            return bridge.TryMoveLootSlotToInventory(bindData);

        if (panel == stashPanelView)
            return bridge.TryMoveBetweenStashAndInventory(bindData);

        if (IsEquipmentPanel(panel))
            return bridge.TryMoveEquipmentSlotToInventory(bindData);

        if (IsInventoryPanel(panel))
        {
            if (currentContext == OverlayContextType.Loot)
                return bridge.TryMoveInventorySlotSmart(bindData);

            if (currentContext == OverlayContextType.Stash)
                return bridge.TryMoveInventorySlotToStashSmart(bindData);

            return bridge.TryMoveInventorySlotSmart(bindData);
        }

        return ItemMoveResult.Fail("처리할 수 없는 패널입니다.");
    }

    private bool IsEquipmentPanel(ContainerPanelView panel)
    {
        return squadPanelView != null && squadPanelView.IsEquipmentPanel(panel);
    }

    private bool IsInventoryPanel(ContainerPanelView panel)
    {
        return squadPanelView != null && squadPanelView.IsInventoryPanel(panel);
    }

    private void RefreshMenuBar()
    {
        // 1) Loot 컨텍스트면 메뉴바 전체 숨김
        bool showMenuBar = currentContext != OverlayContextType.Loot;

        if (menuBarRoot != null)
            menuBarRoot.SetActive(showMenuBar);

        if (!showMenuBar)
            return;

        bool isInRaid = sessionStateController != null && sessionStateController.IsInRaid;
        bool isInBase = sessionStateController != null && sessionStateController.IsInBase;

        // 2) 레이드에서는 Map만 보이고 Stash는 숨김
        if (mapButtonObject != null)
            mapButtonObject.SetActive(isInRaid);

        // 3) 거점에서는 Stash만 보이고 Map은 숨김
        if (stashButtonObject != null)
            stashButtonObject.SetActive(isInBase);
    }

    private void OnSlotRightClicked(ContainerPanelView panel, ItemSlotView slotView)
    {
        if (slotView == null || slotView.BindData == null)
            return;

        SlotBindData bindData = slotView.BindData;

        if (bindData.ItemInstance == null || bindData.ItemDefinition == null)
            return;

        if (itemContextMenuUI != null)
        {
            itemContextMenuUI.Show(Input.mousePosition, bindData);
        }
    }

    private void OnItemContextActionSelected(ItemContextActionType actionType, SlotBindData bindData)  //마우스우클릭패널 기능연결
    {
        if (bindData == null || bindData.ItemInstance == null || bindData.ItemDefinition == null)
            return;

        switch (actionType)
        {
            case ItemContextActionType.Use:
            {
                ItemMoveResult useResult = bridge.TryUseItem(bindData);

                if (useResult.Success)
                    Debug.Log($"[ItemContext] 사용 성공: {bindData.ItemDefinition.DisplayName}");
                else
                    Debug.LogWarning($"[ItemContext] 사용 실패: {useResult.Message}");

                break;
            }
            case ItemContextActionType.RegisterQuickSlot:
            {
                ItemMoveResult quickSlotResult = bridge.TryRegisterQuickSlot(bindData);

                if (quickSlotResult.Success)
                    Debug.Log($"[ItemContext] 퀵슬롯 등록 성공: {quickSlotResult.Message}");
                else
                    Debug.LogWarning($"[ItemContext] 퀵슬롯 등록 실패: {quickSlotResult.Message}");
                break;
            }

            case ItemContextActionType.Drop:
                Debug.Log($"[ItemContext] 버리기: {bindData.ItemDefinition.DisplayName}");
                HandleDropAction(bindData);
                break;

            case ItemContextActionType.ToggleFavoriteMark:
                if (bindData.ItemDefinition.IsFavoriteMarkable && bridge != null)
                {
                    bridge.MarkerRepository.ToggleFavorite(bindData.ItemDefinition.ItemId);
                    Debug.Log($"[ItemContext] 즐겨찾기 토글: {bindData.ItemDefinition.DisplayName}");
                }
                break;

            case ItemContextActionType.Close:
                Debug.Log("[ItemContext] 닫기");
                break;
        }

        RefreshAll();
    }

    private void HandleDropAction(SlotBindData bindData)
    {
        if (bindData == null || bindData.ItemInstance == null || bindData.ItemDefinition == null)
            return;

        if (bridge == null || droppedItemSpawner == null || playerTransform == null)
            return;

        if (!bindData.ItemDefinition.IsDroppable)
        {
            Debug.Log("[ItemContext] 이 아이템은 버릴 수 없습니다.");
            return;
        }

        Vector3 dropPosition = playerTransform.position + playerTransform.forward * 1.2f;

        droppedItemSpawner.SpawnDroppedItem(
            dropPosition,
            bindData.ItemDefinition.ItemId,
            bindData.ItemInstance.StackCount,
            bindData.ItemDefinition.DisplayName
        );

        // 원본 슬롯 비우기
        if (bindData.SourceSlot != null)
        {
            bindData.SourceSlot.ItemInstanceId = null;
        }

        Debug.Log($"[ItemContext] 바닥에 버림: {bindData.ItemDefinition.DisplayName}");
    }
    //ui에서 아이템 드래그앤드롭
    private void OnSlotBeginDrag(ContainerPanelView panel, ItemSlotView slotView) 
    {
        if (slotView == null || slotView.BindData == null)
            return;

        SlotBindData bindData = slotView.BindData;
        if (bindData.ItemInstance == null || bindData.ItemDefinition == null || bindData.IsLocked)
            return;

        dropHandledThisDrag = false;
        bool isSplitDrag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        currentDragPayload = new ItemDragPayload(panel, bindData, isSplitDrag);

        if (itemDragVisual != null)
            itemDragVisual.Show(bindData.ItemInstance, bindData.ItemDefinition);
    }

    private void OnSlotEndDrag(ContainerPanelView panel, ItemSlotView slotView)
    {
        StartCoroutine(EndDragCleanupNextFrame());
    }
    private System.Collections.IEnumerator EndDragCleanupNextFrame()
    {
        yield return null;

        if (!dropHandledThisDrag)
        {
            ClearDragState();
        }

        dropHandledThisDrag = false;
    }

    private void ClearDragState()
    {
        currentDragPayload = null;

        if (itemDragVisual != null)
            itemDragVisual.Hide();
    }

    private void OnSlotDroppedOn(ContainerPanelView targetPanel, ItemSlotView targetSlotView)
    {
        if (currentDragPayload == null || targetSlotView == null || targetSlotView.BindData == null)
        {
            ClearDragState();
            return;
        }
        dropHandledThisDrag = true;

        SlotBindData source = currentDragPayload.SourceBindData;
        SlotBindData target = targetSlotView.BindData;

        ItemMoveResult result = HandleDragDrop(currentDragPayload.SourcePanel, source, targetPanel, target);

        if (result.Success && bridge != null)
        {
            bridge.RecalculateAndApply();
        }

        Debug.Log($"[DragDrop] 결과: {result.Message}");

        ClearDragState();
        RefreshAll();
    }

    private ItemMoveResult HandleDragDrop(
        ContainerPanelView sourcePanel,
        SlotBindData source,
        ContainerPanelView targetPanel,
        SlotBindData target)
    {
        if (bridge == null)
            return ItemMoveResult.Fail("브리지가 없습니다.");

        if (source == null || source.SourceSlot == null || source.SourceContainer == null)
            return ItemMoveResult.Fail("원본 슬롯 정보가 없습니다.");

        if (target == null || target.SourceSlot == null || target.SourceContainer == null)
            return ItemMoveResult.Fail("대상 슬롯 정보가 없습니다.");

        if (!target.SourceSlot.IsUnlocked)
            return ItemMoveResult.Fail("잠긴 슬롯에는 이동할 수 없습니다.");

        if (source.SourceSlot == target.SourceSlot && source.SourceContainer == target.SourceContainer)
            return ItemMoveResult.Fail("같은 슬롯입니다.");

        // 분할 드래그는 빈 슬롯에만 허용
        if (currentDragPayload != null && currentDragPayload.IsSplitDrag)
        {
            if (!target.SourceSlot.IsEmpty)
                return ItemMoveResult.Fail("분할은 빈 슬롯에만 놓을 수 있습니다.");

            if (splitStackPopupUI != null)
            {
                splitStackPopupUI.Show(
                    source.ItemInstance,
                    source.SourceContainer,
                    source.SourceSlot,
                    target.SourceContainer,
                    target.SourceSlot
                );

                return ItemMoveResult.Ok("분할 팝업 오픈");
            }

            return bridge.TrySplitItemToTargetSlot(
                source.ItemInstance,
                source.SourceContainer,
                source.SourceSlot,
                target.SourceContainer,
                target.SourceSlot
            );
        }

        // 일반 드래그는 비어 있지 않아도 서비스가 이동/병합/실패를 판정
        return bridge.TryMoveItemBetweenSlots(
            source.ItemInstance,
            source.SourceContainer,
            source.SourceSlot,
            target.SourceContainer,
            target.SourceSlot
        );
    }

    private void OnQuickSlotDroppedOn(QuickSlotView quickSlotView)
    {
        if (currentDragPayload == null || quickSlotView == null)
        {
            ClearDragState();
            return;
        }

        SlotBindData source = currentDragPayload.SourceBindData;
        if (source == null || source.ItemInstance == null || source.ItemDefinition == null)
        {
            ClearDragState();
            return;
        }

        ItemMoveResult result = bridge.TryRegisterQuickSlotAt(source, quickSlotView.SlotIndex);

        Debug.Log($"[QuickSlot Drag] 결과: {result.Message}");

        ClearDragState();
        RefreshAll();
    }

    private void OnQuickSlotRightClicked(QuickSlotView quickSlotView)
    {
        if (quickSlotView == null || bridge == null)
            return;

        ItemMoveResult result = bridge.TryClearQuickSlot(quickSlotView.SlotIndex);
        Debug.Log($"[QuickSlot] 해제 결과: {result.Message}");
    }

    public void RefreshLossProtectionHighlights(PlayerSquadBridge bridge)
    {
        if (bridge == null)
            return;

        if (inventoryP1Highlight != null)
            inventoryP1Highlight.SetHighlighted(bridge.IsInventoryProtected(PositionIndex.Position1));

        if (inventoryP2Highlight != null)
            inventoryP2Highlight.SetHighlighted(bridge.IsInventoryProtected(PositionIndex.Position2));

        if (inventoryP3Highlight != null)
            inventoryP3Highlight.SetHighlighted(bridge.IsInventoryProtected(PositionIndex.Position3));

        if (equipmentP1Highlight != null)
            equipmentP1Highlight.SetHighlighted(bridge.IsEquipmentProtected(PositionIndex.Position1));

        if (equipmentP2Highlight != null)
            equipmentP2Highlight.SetHighlighted(bridge.IsEquipmentProtected(PositionIndex.Position2));

        if (equipmentP3Highlight != null)
            equipmentP3Highlight.SetHighlighted(bridge.IsEquipmentProtected(PositionIndex.Position3));
    }

    private void HandleSquadVisualStateChanged()
    {
        RefreshLossProtectionHighlights(bridge);
    }

    private void OnDestroy()
    {
        if (bridge != null)
            bridge.OnSquadVisualStateChanged -= HandleSquadVisualStateChanged;
    }
}