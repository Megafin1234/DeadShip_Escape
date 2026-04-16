using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSquadBridge : MonoBehaviour
{
    [Header("Squad Runtime")]
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private PositionRuleDefinition position1Rule;
    [SerializeField] private PositionRuleDefinition position2Rule;
    [SerializeField] private PositionRuleDefinition position3Rule;

    [Header("Characters")]
    [SerializeField] private CharacterDefinition position1Character;
    [SerializeField] private CharacterDefinition position2Character;
    [SerializeField] private CharacterDefinition position3Character;

    [Header("Optional Runtime Debug")]
    [SerializeField] private TMP_Text squadStateText;

    [Header("Existing World Objects")]
    [SerializeField] private GameObject playerBody;
    [SerializeField] private GameObject companionBody;

    [Header("UI")]
    [SerializeField] private RaidOverlayUIController raidOverlayUIController;
    [SerializeField] private QuickSlotPanelView quickSlotPanelView;

    private SquadRuntime squad;
    private ItemInstanceRepository itemRepository;

    private SquadStatCalculator squadStatCalculator;
    private SharedHealthService sharedHealthService;

    private ItemMoveService itemMoveService;
    private AutoMoveService autoMoveService;

    private RaidSquadContext raidContext;

    private LootContainerFactory lootFactory;
    private LootTransferService lootTransferService;
    private OpenedLootContext currentLootContext;
    private ItemMarkerStateRepository markerRepository;
    private ItemUseService itemUseService;

    private const int InventorySlotsPerPanel = 24;
    private const int EquipmentSlotsPerPanel = 4;

    public static PlayerSquadBridge Instance { get; private set; }

    public SquadRuntime Squad => squad;
    public RaidSquadContext RaidContext => raidContext;

    public LootContainerFactory LootFactory => lootFactory;
    public OpenedLootContext CurrentLootContext => currentLootContext;
    public RaidOverlayUIController RaidOverlayUIController => raidOverlayUIController;
    public ItemInstanceRepository ItemRepository => itemRepository;
    public ItemDatabase ItemDatabase => itemDatabase;
    public ItemMarkerStateRepository MarkerRepository => markerRepository;
    public System.Action OnSquadVisualStateChanged;
    private ContainerState stashContainer;

    private bool isFormationPanelOpen = false;

    private readonly HashSet<string> modalBlockers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[PlayerSquadBridge] 중복 Instance가 감지되었습니다.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildSquad();
        BuildServices();
        SeedDebugItems();
        BindExistingComponents();
        RecalculateAndApply();
    }

    private void Update()
    {
        UpdateActiveBuffs(Time.deltaTime);
    }

    private void BuildSquad()
    {
        squad = new SquadRuntime();

        squad.Position1Rule = position1Rule;
        squad.Position2Rule = position2Rule;
        squad.Position3Rule = position3Rule;

        squad.Positions[0].CharacterDefinition = position1Character;
        squad.Positions[0].CharacterId = position1Character != null ? position1Character.CharacterId : null;

        squad.Positions[1].CharacterDefinition = position2Character;
        squad.Positions[1].CharacterId = position2Character != null ? position2Character.CharacterId : null;

        squad.Positions[2].CharacterDefinition = position3Character;
        squad.Positions[2].CharacterId = position3Character != null ? position3Character.CharacterId : null;

        itemRepository = new ItemInstanceRepository();
        stashContainer = new ContainerState(ContainerType.Stash);
        for (int i = 0; i < 210; i++)
        {
            SlotState slot = new SlotState(i, SlotKind.Inventory, PositionIndex.None);
            slot.IsUnlocked = i < 70; // 처음 70칸만 개방, 나머지는 잠금
            stashContainer.Slots.Add(slot);
        }

        InitializeEquipmentSlots();
        InitializeInventorySlots();
        InitializeQuickSlots();

        raidContext = new RaidSquadContext(squad)
        {
            PlayerBody = playerBody,
            CompanionBody = companionBody
        };
    }

    private void BuildServices()
    {
        squadStatCalculator = new SquadStatCalculator(itemDatabase, itemRepository);
        sharedHealthService = new SharedHealthService();

        itemMoveService = new ItemMoveService(itemDatabase, itemRepository);
        autoMoveService = new AutoMoveService(itemDatabase, itemRepository, itemMoveService);
        itemUseService = new ItemUseService();

        lootFactory = new LootContainerFactory(itemRepository);
        markerRepository = new ItemMarkerStateRepository();
        lootTransferService = new LootTransferService(autoMoveService, itemRepository);
    }

    private void InitializeEquipmentSlots()
    {
        for (int i = 0; i < 4; i++)
            squad.EquipmentContainer.Slots.Add(new SlotState(i, SlotKind.Equipment, PositionIndex.Position1));

        for (int i = 4; i < 8; i++)
            squad.EquipmentContainer.Slots.Add(new SlotState(i, SlotKind.Equipment, PositionIndex.Position2));

        for (int i = 8; i < 12; i++)
            squad.EquipmentContainer.Slots.Add(new SlotState(i, SlotKind.Equipment, PositionIndex.Position3));
    }

    private void InitializeInventorySlots()
    {
        CreateInventorySegment(PositionIndex.Position1, 0, 24, 12);
        CreateInventorySegment(PositionIndex.Position2, 24, 24, 12);
        CreateInventorySegment(PositionIndex.Position3, 48, 24, 12);
    }
    private void InitializeQuickSlots()
    {
        squad.QuickSlots.Clear();

        // 일단 4칸으로 시작
        for (int i = 0; i < 4; i++)
        {
            squad.QuickSlots.Add(new QuickSlotRuntime(i));
        }
    }

    private void CreateInventorySegment(PositionIndex position, int startIndex, int count, int unlockedCount)
    {
        for (int i = 0; i < count; i++)
        {
            SlotState slot = new SlotState(startIndex + i, SlotKind.Inventory, position);
            slot.IsUnlocked = i < unlockedCount;
            squad.InventoryContainer.Slots.Add(slot);
        }
    }

    public void EnterRaid()
    {
        RecalculateAndApply();
        sharedHealthService.EnterRaid(squad);
        PushStateToView();
    }

    public void RecalculateAndApply()
    {
        squadStatCalculator.Recalculate(squad);
        sharedHealthService.InitializeFromDerivedStats(squad);
        Debug.Log($"[Bridge] RecalculateAndApply -> ATK:{squad.DerivedStats.Attack}, Move:{squad.DerivedStats.MoveSpeed}, BuffCount:{squad.ActiveBuffs.Count}");
        PushStateToView();
        OnSquadVisualStateChanged?.Invoke();
    }

    public void ApplyIncomingDamage(int damage)
    {
        sharedHealthService.ApplyDamage(squad, damage);
        PushStateToView();
    }

    public void HealIncoming(int amount)
    {
        sharedHealthService.Heal(squad, amount);
        PushStateToView();
    }

    public void Extract()
    {
        sharedHealthService.Extract(squad);
        PushStateToView();
    }

    public bool CanPlayerAct()
    {
        if (squad == null)
            return false;

        if (squad.CombatState.IsDead)
            return false;

        if (squad.RaidState == RaidStateType.Wiped)
            return false;

        if (raidOverlayUIController != null && raidOverlayUIController.IsAnyPanelOpen)
        return false;

        if (HasAnyModalBlocker())
            return false;

        return true;
    }
     public void SetModalBlocker(string key, bool isActive)  //ui가 떠있을때 조작을 금지시킴
    {
        if (string.IsNullOrEmpty(key))
            return;

        if (isActive)
            modalBlockers.Add(key);
        else
            modalBlockers.Remove(key);
    }

    public bool HasAnyModalBlocker()
    {
        return modalBlockers.Count > 0;
    }

    public bool CanPlayerMove()
    {
        return CanPlayerAct();
    }

    public bool CanPlayerShoot()
    {
        return CanPlayerAct();
    }

    public bool CanPlayerInteract()
    {
        return CanPlayerAct();
    }

    public void OpenLootContainer(LootContainerRuntime lootContainer)
    {
        if (lootContainer == null || squad == null)
            return;

        currentLootContext = new OpenedLootContext(lootContainer, squad);
        lootContainer.IsOpened = true;

        Debug.Log($"[PlayerSquadBridge] LootContainer Opened: {lootContainer.DisplayName}");
    }

    public void CloseLootContainer()
    {
        if (currentLootContext != null && currentLootContext.LootContainer != null)
        {
            currentLootContext.LootContainer.IsOpened = false;
            Debug.Log($"[PlayerSquadBridge] LootContainer Closed: {currentLootContext.LootContainer.DisplayName}");
        }

        currentLootContext = null;
    }

    public ItemMoveResult LootOneFromCurrentContainer()
    {
        if (currentLootContext == null || !currentLootContext.IsOpen)
            return ItemMoveResult.Fail("열린 루팅 컨테이너가 없습니다.");

        ItemMoveResult result = lootTransferService.MoveFirstLootItemToSquad(currentLootContext);
            if (result.Success)
        {
            DebugPrintSquadInventory();
        }

        if (currentLootContext.LootContainer != null)
        {
            currentLootContext.LootContainer.RefreshEmptyState();

            if (currentLootContext.LootContainer.IsEmpty)
            {
                Debug.Log($"[PlayerSquadBridge] LootContainer Empty: {currentLootContext.LootContainer.DisplayName}");
            }
        }

        return result;
    }
    public void DebugPrintSquadInventory()
    {
        if (squad == null || squad.InventoryContainer == null)
        {
            Debug.Log("[PlayerSquadBridge] Squad inventory 없음");
            return;
        }

        Debug.Log("=== Squad Inventory ===");

        for (int i = 0; i < squad.InventoryContainer.Slots.Count; i++)
        {
            SlotState slot = squad.InventoryContainer.Slots[i];
            if (slot.IsEmpty)
                continue;

            Debug.Log($"[Inventory Slot {slot.SlotIndex}] ItemInstanceId = {slot.ItemInstanceId}");
        }
    }
    public ItemInstanceRepository GetItemRepository()
    {
        return itemRepository;
    }
    public ContainerState GetOrCreateStashContainer()
    {
        return stashContainer;
    }

    private void PushStateToView()
    {
        ApplyStatsToPlayerBody();
        ApplyStatsToCompanionBody();
        UpdateCompanionActiveState();
        RefreshDebugText();
        RefreshQuickSlotUI();
    }

    private void ApplyStatsToPlayerBody()
    {
        if (playerBody == null)
            return;

        var binder = playerBody.GetComponent<PlayerSquadViewBinder>();
        if (binder != null)
            binder.ApplyFromSquad(squad);
        PlayerController controller = playerBody.GetComponent<PlayerController>();
        if (controller != null)
            controller.SetMoveSpeed(squad.DerivedStats.MoveSpeed);
    }

    private void ApplyStatsToCompanionBody()
    {
        if (companionBody == null)
            return;

        var binder = companionBody.GetComponent<PlayerSquadViewBinder>();
        if (binder != null)
            binder.ApplyFromSquad(squad);
        PlayerController controller = companionBody.GetComponent<PlayerController>();
        if (controller != null)
            controller.SetMoveSpeed(squad.DerivedStats.MoveSpeed);
    }

    private void RefreshDebugText()
    {
        if (squadStateText == null || squad == null)
            return;

        squadStateText.text =
            $"RaidState: {squad.RaidState}\n" +
            $"Attack: {squad.DerivedStats.Attack}\n" +
            $"MaxHP: {squad.CombatState.MaxHealth}\n" +
            $"CurHP: {squad.CombatState.CurrentHealth}\n" +
            $"Dead: {squad.CombatState.IsDead}";
    }

    private void BindExistingComponents()
    {
        BindPlayerBodyComponents();
        BindCompanionBodyComponents();
    }

    private void BindPlayerBodyComponents()
    {
        if (playerBody == null)
            return;

        PlayerHealth health = playerBody.GetComponent<PlayerHealth>();
        if (health != null)
            health.SetBridge(this);

        PlayerShooter shooter = playerBody.GetComponent<PlayerShooter>();
        if (shooter != null)
            shooter.SetBridge(this);

        PlayerController controller = playerBody.GetComponent<PlayerController>();
        if (controller != null)
            controller.SetBridge(this);

        PlayerInteraction interaction = playerBody.GetComponent<PlayerInteraction>();
        if (interaction != null)
            interaction.SetBridge(this);
    }

    private void BindCompanionBodyComponents()
    {
        if (companionBody == null)
            return;

        PlayerHealth health = companionBody.GetComponent<PlayerHealth>();
        if (health != null)
            health.SetBridge(this);

        CompanionShooter companionShooter = companionBody.GetComponent<CompanionShooter>();
        if (companionShooter != null)
            companionShooter.SetBridge(this);

        CompanionAIController companionAI = companionBody.GetComponent<CompanionAIController>();
        if (companionAI != null && playerBody != null)
            companionAI.SetPlayerTarget(playerBody.transform);
    }

    public ItemMoveResult TryMoveLootSlotToInventory(SlotBindData bindData)
    {
        if (bindData == null || currentLootContext == null || !currentLootContext.IsOpen)
            return ItemMoveResult.Fail("열린 루팅 컨테이너가 없습니다.");

        if (bindData.SourceSlot == null || bindData.SourceContainer == null)
            return ItemMoveResult.Fail("원본 슬롯 정보가 없습니다.");

        if (bindData.ItemInstance == null)
            return ItemMoveResult.Fail("아이템 인스턴스를 찾을 수 없습니다.");

        ItemMoveResult result = autoMoveService.AutoMoveFromLootToInventory(
            bindData.ItemInstance,
            bindData.SourceContainer,
            bindData.SourceSlot,
            squad
        );

        currentLootContext.LootContainer.RefreshEmptyState();
        return result;
    }

    public ItemMoveResult TryMoveInventorySlotToLoot(SlotBindData bindData)
    {
        if (bindData == null || currentLootContext == null || !currentLootContext.IsOpen)
            return ItemMoveResult.Fail("열린 루팅 컨테이너가 없습니다.");

        if (bindData.SourceSlot == null || bindData.SourceContainer == null)
            return ItemMoveResult.Fail("원본 슬롯 정보가 없습니다.");

        if (bindData.ItemInstance == null)
            return ItemMoveResult.Fail("아이템 인스턴스를 찾을 수 없습니다.");

        return autoMoveService.AutoMoveFromInventoryToLoot(
            bindData.ItemInstance,
            bindData.SourceSlot,
            squad,
            currentLootContext.LootContainer.Container
        );
    }

    public ItemMoveResult TryMoveEquipmentSlotToInventory(SlotBindData bindData)
    {
        if (bindData == null)
            return ItemMoveResult.Fail("슬롯 정보가 없습니다.");

        if (bindData.SourceSlot == null)
            return ItemMoveResult.Fail("원본 장비 슬롯 정보가 없습니다.");

        if (bindData.ItemInstance == null)
            return ItemMoveResult.Fail("장비 아이템 인스턴스를 찾을 수 없습니다.");

        return autoMoveService.AutoUnequipToInventory(
            bindData.ItemInstance,
            bindData.SourceSlot,
            squad
        );
    }

    public ItemMoveResult TryMoveInventorySlotSmart(SlotBindData bindData)
    {
        if (bindData == null)
            return ItemMoveResult.Fail("슬롯 정보가 없습니다.");

        if (bindData.SourceSlot == null)
            return ItemMoveResult.Fail("원본 인벤토리 슬롯 정보가 없습니다.");

        if (bindData.ItemInstance == null)
            return ItemMoveResult.Fail("아이템 인스턴스를 찾을 수 없습니다.");

        // 1) 먼저 장착 시도
        ItemMoveResult equipResult = autoMoveService.AutoEquipFromInventory(
            bindData.ItemInstance,
            bindData.SourceSlot,
            squad
        );

        if (equipResult.Success)
            return equipResult;

        // 2) 장착 실패 시, 루팅 UI가 열려 있으면 루팅 컨테이너로 이동 시도
        if (currentLootContext != null && currentLootContext.IsOpen)
        {
            ItemMoveResult lootResult = autoMoveService.AutoMoveFromInventoryToLoot(
                bindData.ItemInstance,
                bindData.SourceSlot,
                squad,
                currentLootContext.LootContainer.Container
            );

            return lootResult;
        }

        return equipResult;
    }

    public ItemMoveResult TryMoveInventorySlotToStashSmart(SlotBindData bindData)
    {
        if (bindData == null)
            return ItemMoveResult.Fail("슬롯 정보가 없습니다.");

        if (bindData.SourceSlot == null)
            return ItemMoveResult.Fail("원본 인벤토리 슬롯 정보가 없습니다.");

        if (bindData.ItemInstance == null)
            return ItemMoveResult.Fail("아이템 인스턴스를 찾을 수 없습니다.");

        ItemMoveResult equipResult = autoMoveService.AutoEquipFromInventory(
            bindData.ItemInstance,
            bindData.SourceSlot,
            squad
        );

        if (equipResult.Success)
            return equipResult;

        return autoMoveService.AutoMoveBetweenStashAndInventory(
            bindData.ItemInstance,
            squad.InventoryContainer,
            bindData.SourceSlot,
            stashContainer,
            squad
        );
    }

    public ItemMoveResult TryMoveBetweenStashAndInventory(SlotBindData bindData)
    {
        if (bindData == null)
            return ItemMoveResult.Fail("슬롯 정보가 없습니다.");

        if (bindData.SourceSlot == null || bindData.SourceContainer == null)
            return ItemMoveResult.Fail("원본 슬롯 정보가 없습니다.");

        if (bindData.ItemInstance == null)
            return ItemMoveResult.Fail("아이템 인스턴스를 찾을 수 없습니다.");

        ContainerState target =
            bindData.SourceContainer == stashContainer
            ? squad.InventoryContainer
            : stashContainer;

        return autoMoveService.AutoMoveBetweenStashAndInventory(
            bindData.ItemInstance,
            bindData.SourceContainer,
            bindData.SourceSlot,
            target,
            squad
        );
    }
    /*아이템 줍기 관련*/
    public ItemDefinitionBase GetItemDefinition(string definitionId)  ///////////////////////////////////아이템 정의 조회 helper
    {
        if (itemDatabase == null || string.IsNullOrEmpty(definitionId))
            return null;

        return itemDatabase.GetDefinition(definitionId);
    }

    public ItemMoveResult TryPickupDroppedItem(ItemInstance item)
    {
        if (item == null)
            return ItemMoveResult.Fail("줍기 아이템이 없습니다.");


        SlotState empty = squad.InventoryContainer.FindFirstEmptySlot(SlotKind.Inventory);
        if (empty == null)
            return ItemMoveResult.Fail("인벤토리에 빈 칸이 없습니다.");

        empty.ItemInstanceId = item.InstanceId;
        return ItemMoveResult.Ok("아이템을 획득했습니다.");
    }
    /*아이템 사용 관련*/

    public ItemMoveResult TryUseItem(SlotBindData bindData)  //아이템을 사용한다
    {
        if (bindData == null || bindData.ItemInstance == null || bindData.ItemDefinition == null)
            return ItemMoveResult.Fail("사용할 아이템 정보가 없습니다.");

        // ItemDefinitionBase 기준으로 사용 가능 여부 확인
        if (!bindData.ItemDefinition.IsUsable)
            return ItemMoveResult.Fail("이 아이템은 사용할 수 없습니다.");

        // 현재 구조에서는 ConsumableDefinition만 사용 아이템으로 처리
        // 나중에 장비 사용형, 설치형, 특수 사용형이 생기면
        // 여기에서 as 캐스팅 분기를 더 늘릴 수 있습니다.
        ConsumableDefinition consumable = bindData.ItemDefinition as ConsumableDefinition;
        if (consumable == null)
            return ItemMoveResult.Fail("사용 아이템 정의 형식이 아닙니다.");

        return itemUseService.UseItem(bindData.ItemInstance, consumable, this);
    }
    public ItemMoveResult ConsumeOneItem(ItemInstance item)  // 사용한 아이템은 개수가 감소한다,사라진다, 소비된다
    {
        if (item == null)
            return ItemMoveResult.Fail("소비할 아이템이 없습니다.");

        // 사용 시 스택을 1 감소
        item.StackCount -= 1;

        // 스택이 0 이하가 되면 슬롯에서 제거
        if (item.StackCount <= 0)
        {
            ClearItemInstanceFromAllContainers(item.InstanceId);
        }

        return ItemMoveResult.Ok("아이템 1개 소비");
    }

    private void ClearItemInstanceFromAllContainers(string instanceId)   /////아이템 슬롯 인스턴스 날리기
    {
        ClearFromContainer(squad.InventoryContainer, instanceId);
        ClearFromContainer(squad.EquipmentContainer, instanceId);

        if (stashContainer != null)
            ClearFromContainer(stashContainer, instanceId);

        if (currentLootContext != null && currentLootContext.LootContainer != null)
            ClearFromContainer(currentLootContext.LootContainer.Container, instanceId);

        ClearFromQuickSlots(instanceId);
    }

    private void ClearFromContainer(ContainerState container, string instanceId)
    {
        if (container == null || string.IsNullOrEmpty(instanceId))
            return;

        for (int i = 0; i < container.Slots.Count; i++)
        {
            if (container.Slots[i].ItemInstanceId == instanceId)
            {
                container.Slots[i].ItemInstanceId = null;
                return;
            }
        }
    }

    private void ClearFromQuickSlots(string instanceId)
    {
        if (squad == null || squad.QuickSlots == null || string.IsNullOrEmpty(instanceId))
            return;

        for (int i = 0; i < squad.QuickSlots.Count; i++)
        {
            if (squad.QuickSlots[i].ItemInstanceId == instanceId)
            {
                squad.QuickSlots[i].Clear();
            }
        }
    }
    public void AddTimedBuff(ActiveBuffType buffType, float value, float duration)
    {
        if (squad == null)
            return;

        // 나중에 같은 종류 버프를 갱신형으로 할지, 중첩형으로 할지는 여기서 정책 결정 가능
        // 현재는 단순히 새 버프를 추가하는 방식
        squad.ActiveBuffs.Add(new ActiveBuffRuntime(buffType, value, duration));
        Debug.Log($"[Bridge] AddTimedBuff -> type:{buffType}, value:{value}, duration:{duration}, count:{squad.ActiveBuffs.Count}");
    }
    private void UpdateActiveBuffs(float deltaTime) // 매 프레임 버프현황 갱신
    {
        if (squad == null || squad.ActiveBuffs == null || squad.ActiveBuffs.Count == 0)
            return;

        bool changed = false;

        // 뒤에서부터 지워야 안전
        for (int i = squad.ActiveBuffs.Count - 1; i >= 0; i--)
        {
            squad.ActiveBuffs[i].RemainingTime -= deltaTime;

            if (squad.ActiveBuffs[i].RemainingTime <= 0f)
            {
                squad.ActiveBuffs.RemoveAt(i);
                changed = true;
            }
        }

        // 버프가 끝나서 리스트가 바뀌었다면 수치를 다시 계산
        if (changed)
        {
            RecalculateAndApply();
        }
    }

    public ItemMoveResult TryRegisterQuickSlot(SlotBindData bindData)   //퀵슬롯 버튼을 눌렀을때 자동으로 빈 슬롯에 들어가는 함수
    {
        if (bindData == null || bindData.ItemInstance == null || bindData.ItemDefinition == null)
            return ItemMoveResult.Fail("퀵슬롯 등록 대상 아이템이 없습니다.");

        if (!bindData.ItemDefinition.IsUsable)
            return ItemMoveResult.Fail("사용 가능한 아이템만 퀵슬롯에 등록할 수 있습니다.");

        if (squad == null || squad.QuickSlots == null || squad.QuickSlots.Count == 0)
            return ItemMoveResult.Fail("퀵슬롯이 초기화되지 않았습니다.");

        ItemInstance targetItem = bindData.ItemInstance;

        // 1. 루팅 컨테이너에 있는 아이템이면 먼저 인벤토리로 이동해야 함
        if (bindData.SourceContainer != null &&
            bindData.SourceContainer.ContainerType == ContainerType.LootContainer)
        {
            if (bindData.SourceSlot == null)
                return ItemMoveResult.Fail("루팅 슬롯 정보가 없습니다.");

            ItemMoveResult moveResult = autoMoveService.AutoMoveFromLootToInventory(
                bindData.ItemInstance,
                bindData.SourceContainer,
                bindData.SourceSlot,
                squad
            );

            if (!moveResult.Success)
                return ItemMoveResult.Fail($"인벤토리로 가져오지 못해 퀵슬롯 등록 실패: {moveResult.Message}");

            // 이동 후에도 같은 ItemInstance를 그대로 쓰므로 targetItem은 그대로 유지 가능
        }
        else if (bindData.SourceContainer != null &&
                bindData.SourceContainer.ContainerType != ContainerType.SquadInventory)
        {
            // 지금 단계에선 인벤/루팅만 허용
            return ItemMoveResult.Fail("퀵슬롯 등록은 인벤토리 또는 루팅 컨테이너 아이템만 가능합니다.");
        }

        // 2. 이미 등록된 아이템이면 중복 등록 막기
        for (int i = 0; i < squad.QuickSlots.Count; i++)
        {
            if (squad.QuickSlots[i].ItemInstanceId == targetItem.InstanceId)
            {
                PushStateToView();
                return ItemMoveResult.Ok($"이미 퀵슬롯 {i + 1}번에 등록되어 있습니다.");
            }
        }

        // 3. 첫 번째 빈 슬롯에 등록
        for (int i = 0; i < squad.QuickSlots.Count; i++)
        {
            if (squad.QuickSlots[i].IsEmpty)
            {
                squad.QuickSlots[i].ItemInstanceId = targetItem.InstanceId;
                PushStateToView(); // ← 이게 즉시 UI 갱신 핵심
                return ItemMoveResult.Ok($"퀵슬롯 {i + 1}번에 등록했습니다.");
            }
        }

        return ItemMoveResult.Fail("빈 퀵슬롯이 없습니다.");
    }

    public ItemMoveResult TryRegisterQuickSlotAt(SlotBindData bindData, int quickSlotIndex)  //드래그 드롭으로 원하는 퀵슬롯에 내려놓을때 쓰는 함수
    {
        if (bindData == null || bindData.ItemInstance == null || bindData.ItemDefinition == null)
            return ItemMoveResult.Fail("퀵슬롯 등록 대상 아이템이 없습니다.");

        if (!bindData.ItemDefinition.IsUsable)
            return ItemMoveResult.Fail("사용 가능한 아이템만 퀵슬롯에 등록할 수 있습니다.");

        if (squad == null || squad.QuickSlots == null)
            return ItemMoveResult.Fail("퀵슬롯이 초기화되지 않았습니다.");

        if (quickSlotIndex < 0 || quickSlotIndex >= squad.QuickSlots.Count)
            return ItemMoveResult.Fail("잘못된 퀵슬롯 인덱스입니다.");

        // 루팅 컨테이너에 있는 아이템이면 먼저 인벤토리로 가져와야 함
        if (bindData.SourceContainer != null &&
            bindData.SourceContainer.ContainerType == ContainerType.LootContainer)
        {
            if (bindData.SourceSlot == null)
                return ItemMoveResult.Fail("루팅 슬롯 정보가 없습니다.");

            ItemMoveResult moveResult = autoMoveService.AutoMoveFromLootToInventory(
                bindData.ItemInstance,
                bindData.SourceContainer,
                bindData.SourceSlot,
                squad
            );

            if (!moveResult.Success)
                return ItemMoveResult.Fail($"인벤토리로 가져오지 못해 퀵슬롯 등록 실패: {moveResult.Message}");
        }
        else if (bindData.SourceContainer != null &&
                bindData.SourceContainer != squad.InventoryContainer)
        {
            return ItemMoveResult.Fail("퀵슬롯 등록은 인벤토리 또는 루팅 컨테이너 아이템만 가능합니다.");
        }

        QuickSlotRuntime targetQuickSlot = squad.QuickSlots[quickSlotIndex];
        targetQuickSlot.ItemInstanceId = bindData.ItemInstance.InstanceId;

        PushStateToView();
        return ItemMoveResult.Ok($"퀵슬롯 {quickSlotIndex + 1}번에 등록했습니다.");
    }

    public ItemMoveResult TryUseQuickSlot(int quickSlotIndex)
    {
        if (squad == null || squad.QuickSlots == null)
            return ItemMoveResult.Fail("퀵슬롯이 초기화되지 않았습니다.");

        if (quickSlotIndex < 0 || quickSlotIndex >= squad.QuickSlots.Count)
            return ItemMoveResult.Fail("잘못된 퀵슬롯 인덱스입니다.");

        QuickSlotRuntime quickSlot = squad.QuickSlots[quickSlotIndex];
        if (quickSlot == null || quickSlot.IsEmpty)
            return ItemMoveResult.Fail("비어 있는 퀵슬롯입니다.");

        string instanceId = quickSlot.ItemInstanceId;
        ItemInstance itemInstance = itemRepository.Get(instanceId);
        if (itemInstance == null)
        {
            quickSlot.Clear();
            PushStateToView();
            return ItemMoveResult.Fail("퀵슬롯 아이템 인스턴스를 찾을 수 없어 등록을 해제했습니다.");
        }

        ItemDefinitionBase definition = itemDatabase.GetDefinition(itemInstance.DefinitionId);
        if (definition == null)
        {
            quickSlot.Clear();
            PushStateToView();
            return ItemMoveResult.Fail("퀵슬롯 아이템 정의를 찾을 수 없어 등록을 해제했습니다.");
        }

        if (!definition.IsUsable)
            return ItemMoveResult.Fail("이 퀵슬롯 아이템은 사용할 수 없습니다.");

        // 실제로 인벤토리에 존재하는지 확인
        SlotState inventorySlot = FindInventorySlotByInstanceId(instanceId);
        if (inventorySlot == null)
        {
            quickSlot.Clear();
            PushStateToView();
            return ItemMoveResult.Fail("퀵슬롯 아이템이 인벤토리에 없어 등록을 해제했습니다.");
        }

        SlotBindData bindData = new SlotBindData
        {
            SlotState = inventorySlot,
            SourceSlot = inventorySlot,
            SourceContainer = squad.InventoryContainer,
            ItemInstance = itemInstance,
            ItemDefinition = definition,
            IsLocked = !inventorySlot.IsUnlocked,
            IsInsuranceProtected = false,
            IsFavoriteMarked = markerRepository != null && markerRepository.IsFavoriteMarked(definition.ItemId),
            IsQuestMarked = markerRepository != null && markerRepository.IsQuestMarked(definition.ItemId)
        };

        ItemMoveResult result = TryUseItem(bindData);

        // 사용 후 아이템이 사라졌으면 퀵슬롯 정리
        ItemInstance stillExists = itemRepository.Get(instanceId);
        if (stillExists == null || stillExists.StackCount <= 0)
        {
            quickSlot.Clear();
        }

        PushStateToView();
        return result;
    }

    public ItemMoveResult TryClearQuickSlot(int quickSlotIndex)  //우클릭으로 퀵슬롯 해제하는 함수
    {
        if (squad == null || squad.QuickSlots == null)
            return ItemMoveResult.Fail("퀵슬롯이 초기화되지 않았습니다.");

        if (quickSlotIndex < 0 || quickSlotIndex >= squad.QuickSlots.Count)
            return ItemMoveResult.Fail("잘못된 퀵슬롯 인덱스입니다.");

        QuickSlotRuntime slot = squad.QuickSlots[quickSlotIndex];
        if (slot == null || slot.IsEmpty)
            return ItemMoveResult.Fail("이미 비어 있는 퀵슬롯입니다.");

        slot.Clear();
        PushStateToView();

        return ItemMoveResult.Ok($"퀵슬롯 {quickSlotIndex + 1}번을 해제했습니다.");
    }

    public ItemMoveResult TryMoveItemBetweenSlots(
        ItemInstance item,
        ContainerState sourceContainer,
        SlotState sourceSlot,
        ContainerState targetContainer,
        SlotState targetSlot)
    {
        if (item == null)
            return ItemMoveResult.Fail("이동할 아이템이 없습니다.");

        if (sourceContainer == null || targetContainer == null)
            return ItemMoveResult.Fail("컨테이너 정보가 없습니다.");

        if (sourceSlot == null || targetSlot == null)
            return ItemMoveResult.Fail("슬롯 정보가 없습니다.");

        Debug.Log($"[Bridge Drag] BEFORE  source={sourceSlot.SlotIndex}:{sourceSlot.ItemInstanceId}, target={targetSlot.SlotIndex}:{targetSlot.ItemInstanceId}");

        ItemMoveResult result = itemMoveService.MoveItem(
            item,
            sourceContainer,
            sourceSlot,
            targetContainer,
            targetSlot,
            squad
        );

        Debug.Log($"[Bridge Drag] RESULT success={result.Success}, msg={result.Message}");
        Debug.Log($"[Bridge Drag] AFTER   source={sourceSlot.SlotIndex}:{sourceSlot.ItemInstanceId}, target={targetSlot.SlotIndex}:{targetSlot.ItemInstanceId}");

        return result;
    }

    private SlotState FindInventorySlotByInstanceId(string instanceId)
    {
        if (squad == null || squad.InventoryContainer == null || string.IsNullOrEmpty(instanceId))
            return null;

        for (int i = 0; i < squad.InventoryContainer.Slots.Count; i++)
        {
            SlotState slot = squad.InventoryContainer.Slots[i];
            if (slot.ItemInstanceId == instanceId)
                return slot;
        }

        return null;
    }

    private void RefreshQuickSlotUI()
    {
        if (quickSlotPanelView != null)
            quickSlotPanelView.RefreshView(this);
    }

    /// <summary>
    /// 레이드 실패 시 손실 규칙 적용.
    /// 
    /// 현재 규칙:
    /// - 지원형(또는 inventory 보호 플래그)이 있는 포지션의 인벤토리 패널은 보호
    /// - 화력형(또는 equipment 보호 플래그)이 있는 포지션의 장비 슬롯 4칸은 보호
    /// - 그 외 인벤토리/장비 아이템은 손실
    /// - 창고는 안전
    /// - 사라진 아이템과 연결된 퀵슬롯은 자동 정리
    /// </summary>
    public void ApplyRaidWipeLoss()
    {
        if (squad == null)
            return;

        HashSet<int> protectedInventoryPanels = GetLossProtectedInventoryPanelIndices();
        HashSet<int> protectedEquipmentPanels = GetLossProtectedEquipmentPanelIndices();

        Debug.Log($"[PlayerSquadBridge] ApplyRaidWipeLoss / protectedInventoryPanels = {string.Join(",", protectedInventoryPanels)}");
        Debug.Log($"[PlayerSquadBridge] ApplyRaidWipeLoss / protectedEquipmentPanels = {string.Join(",", protectedEquipmentPanels)}");

        ClearEquipmentSlotsExceptProtectedPanels(protectedEquipmentPanels);
        ClearInventorySlotsExceptProtectedPanels(protectedInventoryPanels);
        CleanupInvalidQuickSlots();

        RecalculateAndApply();
    }

    /// <summary>
    /// 현재 스쿼드 편성을 보고 손실 보호 인벤토리 패널 인덱스를 계산.
    /// 반환값은 0-based:
    /// 0 = P1
    /// 1 = P2
    /// 2 = P3
    /// </summary>
    private HashSet<int> GetLossProtectedInventoryPanelIndices()
    {
        HashSet<int> result = new HashSet<int>();

        if (squad == null || squad.Positions == null)
            return result;

        for (int i = 0; i < squad.Positions.Count; i++)
        {
            SquadPositionRuntime position = squad.Positions[i];
            if (position == null || position.CharacterDefinition == null)
                continue;

            if (position.CharacterDefinition.GrantsLossProtectionInventory)
            {
                result.Add(i);
            }
        }
        return result;
    }

    /// <summary>
    /// 현재 스쿼드 편성을 보고 장비 보호 포지션 인덱스를 계산.
    /// 반환값은 0-based:
    /// 0 = P1
    /// 1 = P2
    /// 2 = P3
    /// </summary>
    private HashSet<int> GetLossProtectedEquipmentPanelIndices()
    {
        HashSet<int> result = new HashSet<int>();

        if (squad == null || squad.Positions == null)
            return result;

        for (int i = 0; i < squad.Positions.Count; i++)
        {
            SquadPositionRuntime position = squad.Positions[i];
            if (position == null || position.CharacterDefinition == null)
                continue;

            if (position.CharacterDefinition.GrantsLossProtectionEquipment)
            {
                result.Add(i);
            }
        }
        return result;
    }

    /// <summary>
    /// 보호 포지션이 아닌 장비 슬롯 아이템만 손실.
    /// 장비 슬롯은 포지션당 4칸이라고 가정.
    /// P1 = 0~3
    /// P2 = 4~7
    /// P3 = 8~11
    /// </summary>
    private void ClearEquipmentSlotsExceptProtectedPanels(HashSet<int> protectedPanels)
    {
        if (squad == null || squad.EquipmentContainer == null)
            return;

        for (int i = 0; i < squad.EquipmentContainer.Slots.Count; i++)
        {
            SlotState slot = squad.EquipmentContainer.Slots[i];
            if (slot.IsEmpty)
                continue;

            int panelIndex = i / EquipmentSlotsPerPanel;

            if (protectedPanels.Contains(panelIndex))
                continue;

            RemoveItemInstanceCompletely(slot.ItemInstanceId);
        }
    }

    /// <summary>
    /// 보호 패널이 아닌 인벤토리 슬롯 아이템 전부 손실.
    /// </summary>
    private void ClearInventorySlotsExceptProtectedPanels(HashSet<int> protectedPanels)
    {
        if (squad == null || squad.InventoryContainer == null)
            return;

        for (int i = 0; i < squad.InventoryContainer.Slots.Count; i++)
        {
            SlotState slot = squad.InventoryContainer.Slots[i];
            if (slot.IsEmpty)
                continue;

            int panelIndex = i / InventorySlotsPerPanel;

            if (protectedPanels.Contains(panelIndex))
                continue;

            RemoveItemInstanceCompletely(slot.ItemInstanceId);
        }
    }

    /// <summary>
    /// 특정 아이템 인스턴스를
    /// - 인벤토리
    /// - 장비
    /// - 루팅
    /// - 퀵슬롯
    /// 에서 완전히 제거하고 repository에서도 삭제.
    /// 창고는 여기서 안 건드립니다.
    /// </summary>
    private void RemoveItemInstanceCompletely(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return;

        ClearFromContainer(squad.InventoryContainer, instanceId);
        ClearFromContainer(squad.EquipmentContainer, instanceId);

        if (currentLootContext != null && currentLootContext.LootContainer != null)
            ClearFromContainer(currentLootContext.LootContainer.Container, instanceId);

        ClearFromQuickSlots(instanceId);

        if (itemRepository != null)
            itemRepository.Remove(instanceId);
    }

    /// <summary>
    /// 손실 처리 후 유효하지 않은 퀵슬롯 참조 정리.
    /// </summary>
    private void CleanupInvalidQuickSlots()
    {
        if (squad == null || squad.QuickSlots == null)
            return;

        for (int i = 0; i < squad.QuickSlots.Count; i++)
        {
            QuickSlotRuntime quickSlot = squad.QuickSlots[i];
            if (quickSlot == null || quickSlot.IsEmpty)
                continue;

            ItemInstance item = itemRepository.Get(quickSlot.ItemInstanceId);
            if (item == null)
            {
                quickSlot.Clear();
            }
        }
    }
    public void RecoverAfterReturnToBase()
    {
        if (squad == null)
            return;

        // 1. 일시 버프 제거
        if (squad.ActiveBuffs != null)
            squad.ActiveBuffs.Clear();

        // 2. 죽음 상태 해제
        squad.CombatState.IsDead = false;
        squad.RaidState = RaidStateType.None;

        // 3. 먼저 스탯 재계산
        RecalculateAndApply();

        // 4. 체력 최대치 회복
        squad.CombatState.CurrentHealth = squad.CombatState.MaxHealth;
        squad.CombatState.IsDead = false;

        // 5. 플레이어/동료 바디 복구
        RecoverPlayerBody(playerBody);
        RecoverCompanionBody(companionBody);

        // 6. 다시 최종 반영
        RecalculateAndApply();

        Debug.Log("[PlayerSquadBridge] 거점 복귀 후 상태 복구 완료");
    }

    private void RecoverPlayerBody(GameObject bodyObject)
    {
        if (bodyObject == null)
            return;

        // 1. 오브젝트 활성화
        if (!bodyObject.activeSelf)
            bodyObject.SetActive(true);

        // 2. PlayerHealth 복구
        PlayerHealth health = bodyObject.GetComponent<PlayerHealth>();
        if (health != null)
            health.ResetAfterRespawn();

        // 3. CharacterController
        CharacterController cc = bodyObject.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = true;

        // 4. PlayerController
        PlayerController controller = bodyObject.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = true;
            controller.ResetRuntimeStateAfterRespawn();
        }

        // 5. PlayerShooter
        PlayerShooter shooter = bodyObject.GetComponent<PlayerShooter>();
        if (shooter != null)
        {
            shooter.enabled = true;
            shooter.ResetRuntimeStateAfterRespawn();
        }

        // 6. PlayerAmmo
        PlayerAmmo ammo = bodyObject.GetComponent<PlayerAmmo>();
        if (ammo != null)
        {
            ammo.enabled = true;
            ammo.ResetRuntimeStateAfterRespawn();
        }

        // 7. PlayerInteraction
        PlayerInteraction interaction = bodyObject.GetComponent<PlayerInteraction>();
        if (interaction != null)
        {
            interaction.enabled = true;
            interaction.ResetRuntimeStateAfterRespawn();
        }

        RestoreRenderersAndColliders(bodyObject);

        Debug.Log("[Bridge] 플레이어 바디 복구 완료");
    }

    private void RecoverCompanionBody(GameObject bodyObject)
    {
        if (bodyObject == null)
            return;

        if (!bodyObject.activeSelf)
            bodyObject.SetActive(true);

        PlayerHealth health = bodyObject.GetComponent<PlayerHealth>();
        if (health != null)
            health.ResetAfterRespawn();

        CharacterController cc = bodyObject.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = true;

        // 플레이어용 컴포넌트는 companion에 다시 켜지지 않게 함
        PlayerController playerController = bodyObject.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.enabled = false;


        CompanionAIController companionAI = bodyObject.GetComponent<CompanionAIController>();
        if (companionAI != null)
            companionAI.enabled = true;

        CompanionShooter companionShooter = bodyObject.GetComponent<CompanionShooter>();
        if (companionShooter != null)
        {
            companionShooter.enabled = true;
            companionShooter.ResetRuntimeStateAfterRespawn();
        }

        RestoreRenderersAndColliders(bodyObject);
        Debug.Log("[Bridge] 동료 바디 복구 완료");
    }

    private void RestoreRenderersAndColliders(GameObject bodyObject)
    {
        Renderer[] renderers = bodyObject.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
            r.enabled = true;

        Collider[] colliders = bodyObject.GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders)
            c.enabled = true;
    }

    //레이드 종료 UI 출력용
    public List<string> GetCurrentRaidCarryItemNames()
    {
        List<string> results = new List<string>();

        if (squad == null)
            return results;

        CollectItemNamesFromContainer(squad.InventoryContainer, results);
        CollectItemNamesFromContainer(squad.EquipmentContainer, results);

        return results;
    }

    private void CollectItemNamesFromContainer(ContainerState container, List<string> results)
    {
        if (container == null || results == null)
            return;

        for (int i = 0; i < container.Slots.Count; i++)
        {
            SlotState slot = container.Slots[i];
            if (slot == null || slot.IsEmpty || string.IsNullOrEmpty(slot.ItemInstanceId))
                continue;

            ItemInstance item = itemRepository.Get(slot.ItemInstanceId);
            if (item == null)
                continue;

            ItemDefinitionBase def = itemDatabase.GetDefinition(item.DefinitionId);
            if (def == null)
                continue;

            // 현재는 이름만 기록
            // 나중에 수량까지 보고 싶으면 "이름 x수량" 형태로 바꾸면 됨
            results.Add(def.DisplayName);
        }
    }

    //캐릭터 선택 시스템 관련
    public bool SetCharacterToPosition(PositionIndex position, CharacterDefinition newCharacter)
    {
        if (squad == null)
            return false;

        for (int i = 0; i < squad.Positions.Count; i++)
        {
            var pos = squad.Positions[i];

            if (pos.PositionIndex != position)
                continue;

            // 같은 캐릭터 중복 방지 (선택사항)
            if (newCharacter != null)
            {
                for (int j = 0; j < squad.Positions.Count; j++)
                {
                    if (j == i) continue;

                    if (squad.Positions[j].CharacterDefinition == newCharacter)
                    {
                        Debug.LogWarning("[Bridge] 이미 다른 슬롯에 배정된 캐릭터");
                        return false;
                    }
                }
            }

            pos.CharacterDefinition = newCharacter;
            pos.CharacterId = newCharacter != null ? newCharacter.CharacterId : null;

            Debug.Log($"[Bridge] {position} 캐릭터 변경 -> {newCharacter?.DisplayName}");

            RecalculateAndApply(); // 핵심
            return true;
        }

        return false;
    }
    public bool ClearCharacterAtPosition(PositionIndex position)
    {
        if (squad == null)
            return false;

        for (int i = 0; i < squad.Positions.Count; i++)
        {
            var pos = squad.Positions[i];
            if (pos.PositionIndex != position)
                continue;

            pos.CharacterDefinition = null;
            pos.CharacterId = null;

            Debug.Log($"[Bridge] {position} 캐릭터 해제");
            RecalculateAndApply();
            return true;
        }

        return false;
    }
    public CharacterDefinition GetCharacterAtPosition(PositionIndex position)
    {
    if (squad == null)
        return null;

    return squad.GetCharacter(position);
    }

    public void SetFormationPanelOpen(bool isOpen)
    {
        isFormationPanelOpen = isOpen;
    }

    public bool SwapCharactersBetweenPositions(PositionIndex a, PositionIndex b)
    {
        if (squad == null)
            return false;

        SquadPositionRuntime posA = null;
        SquadPositionRuntime posB = null;

        for (int i = 0; i < squad.Positions.Count; i++)
        {
            if (squad.Positions[i].PositionIndex == a)
                posA = squad.Positions[i];

            if (squad.Positions[i].PositionIndex == b)
                posB = squad.Positions[i];
        }

        if (posA == null || posB == null)
            return false;

        CharacterDefinition tempDef = posA.CharacterDefinition;
        string tempId = posA.CharacterId;

        posA.CharacterDefinition = posB.CharacterDefinition;
        posA.CharacterId = posB.CharacterId;

        posB.CharacterDefinition = tempDef;
        posB.CharacterId = tempId;

        Debug.Log($"[Bridge] {a} <-> {b} 캐릭터 교체");
        RecalculateAndApply();
        return true;
    }

    //컴패니언 관련
    private void UpdateCompanionActiveState()
    {
        if (companionBody == null || squad == null)
            return;

        CharacterDefinition companionCharacter = squad.GetCharacter(PositionIndex.Position2);
        bool shouldActive = companionCharacter != null;

        companionBody.SetActive(shouldActive);

        if (shouldActive)
        {
            CompanionAIController ai = companionBody.GetComponent<CompanionAIController>();
            if (ai != null && playerBody != null)
            {
                ai.SetPlayerTarget(playerBody.transform);
            }
        }

        Debug.Log($"[Bridge] Companion Active = {shouldActive}, Char = {companionCharacter?.DisplayName}");
    }

    //장비보호관련
    public bool IsInventoryProtected(PositionIndex positionIndex)
    {
        if (squad == null || squad.Positions == null)
            return false;

        for (int i = 0; i < squad.Positions.Count; i++)
        {
            var pos = squad.Positions[i];
            if (pos == null || pos.PositionIndex != positionIndex || pos.CharacterDefinition == null)
                continue;

            return pos.CharacterDefinition.GrantsLossProtectionInventory;
        }

        return false;
    }

    public bool IsEquipmentProtected(PositionIndex positionIndex)
    {
        if (squad == null || squad.Positions == null)
            return false;

        for (int i = 0; i < squad.Positions.Count; i++)
        {
            var pos = squad.Positions[i];
            if (pos == null || pos.PositionIndex != positionIndex || pos.CharacterDefinition == null)
                continue;

            return pos.CharacterDefinition.GrantsLossProtectionEquipment;
        }

        return false;
    }

    //인벤토리/창고 슬롯 해금 관련
    public bool UnlockInventorySlots(PositionIndex positionIndex, int unlockCount)
    {
        if (squad == null || squad.InventoryContainer == null)
            return false;

        int unlocked = 0;

        for (int i = 0; i < squad.InventoryContainer.Slots.Count; i++)
        {
            SlotState slot = squad.InventoryContainer.Slots[i];

            if (slot.PositionIndex != positionIndex)
                continue;

            if (slot.IsUnlocked)
                continue;

            slot.IsUnlocked = true;
            unlocked++;

            if (unlocked >= unlockCount)
                break;
        }

        if (unlocked > 0)
        {
            Debug.Log($"[Bridge] {positionIndex} 인벤 슬롯 {unlocked}칸 해금");
            RecalculateAndApply();
            return true;
        }

        Debug.LogWarning($"[Bridge] {positionIndex} 인벤 해금 가능한 슬롯이 없습니다.");
        return false;
    }

    public bool UnlockStashSlots(int unlockCount)
    {
        if (stashContainer == null)
            return false;

        int unlocked = 0;

        for (int i = 0; i < stashContainer.Slots.Count; i++)
        {
            SlotState slot = stashContainer.Slots[i];

            if (slot.IsUnlocked)
                continue;

            slot.IsUnlocked = true;
            unlocked++;

            if (unlocked >= unlockCount)
                break;
        }

        if (unlocked > 0)
        {
            Debug.Log($"[Bridge] 창고 슬롯 {unlocked}칸 해금");
            RecalculateAndApply();
            return true;
        }

        Debug.LogWarning("[Bridge] 창고 해금 가능한 슬롯이 없습니다.");
        return false;
    }

    public bool CanUnlockInventorySlots(PositionIndex positionIndex, int unlockCount)
    {
        if (squad == null || squad.InventoryContainer == null)
            return false;

        int lockedCount = 0;

        for (int i = 0; i < squad.InventoryContainer.Slots.Count; i++)
        {
            SlotState slot = squad.InventoryContainer.Slots[i];

            if (slot.PositionIndex != positionIndex)
                continue;

            if (!slot.IsUnlocked)
                lockedCount++;
        }

        return lockedCount >= unlockCount;
    }

    public bool CanUnlockStashSlots(int unlockCount)
    {
        if (stashContainer == null)
            return false;

        int lockedCount = 0;

        for (int i = 0; i < stashContainer.Slots.Count; i++)
        {
            if (!stashContainer.Slots[i].IsUnlocked)
                lockedCount++;
        }

        return lockedCount >= unlockCount;
    }
    //아이템 분할
    public ItemMoveResult TrySplitItemToTargetSlotWithAmount(
        ItemInstance sourceItem,
        ContainerState sourceContainer,
        SlotState sourceSlot,
        ContainerState targetContainer,
        SlotState targetSlot,
        int splitAmount)
    {
        if (sourceItem == null)
            return ItemMoveResult.Fail("원본 아이템이 없습니다.");

        if (sourceContainer == null || sourceSlot == null || targetContainer == null || targetSlot == null)
            return ItemMoveResult.Fail("컨테이너 또는 슬롯 정보가 없습니다.");

        if (sourceSlot.IsEmpty)
            return ItemMoveResult.Fail("원본 슬롯이 비어 있습니다.");

        if (sourceSlot.ItemInstanceId != sourceItem.InstanceId)
            return ItemMoveResult.Fail("원본 슬롯의 아이템과 전달된 아이템이 일치하지 않습니다.");

        if (!targetSlot.IsUnlocked)
            return ItemMoveResult.Fail("잠긴 슬롯에는 놓을 수 없습니다.");

        if (!targetSlot.IsEmpty)
            return ItemMoveResult.Fail("대상 슬롯이 비어 있지 않습니다.");

        ItemDefinitionBase itemDef = itemDatabase.GetDefinition(sourceItem.DefinitionId);
        if (itemDef == null)
            return ItemMoveResult.Fail("아이템 정의를 찾을 수 없습니다.");

        if (itemDef.MaxStack <= 1)
            return ItemMoveResult.Fail("이 아이템은 분할할 수 없습니다.");

        if (sourceItem.StackCount <= 1)
            return ItemMoveResult.Fail("수량이 1개라 분할할 수 없습니다.");

        if (splitAmount <= 0 || splitAmount >= sourceItem.StackCount)
            return ItemMoveResult.Fail("분할 수량이 올바르지 않습니다.");

        if (!ItemSlotRules.CanPlaceItemInSlot(itemDef, targetSlot, squad, itemDatabase))
            return ItemMoveResult.Fail("대상 슬롯에 이 아이템을 넣을 수 없습니다.");

        StackService stackService = new StackService(itemDatabase);
        ItemInstance splitItem = stackService.Split(sourceItem, splitAmount, itemRepository);

        if (splitItem == null)
            return ItemMoveResult.Fail("분할에 실패했습니다.");

        targetSlot.ItemInstanceId = splitItem.InstanceId;
        return ItemMoveResult.Ok("분할 성공");
    }

    public ItemMoveResult TrySplitItemToTargetSlot(
        ItemInstance sourceItem,
        ContainerState sourceContainer,
        SlotState sourceSlot,
        ContainerState targetContainer,
        SlotState targetSlot)
    {
        if (sourceItem == null)
            return ItemMoveResult.Fail("원본 아이템이 없습니다.");

        int splitAmount = sourceItem.StackCount / 2;
        return TrySplitItemToTargetSlotWithAmount(
            sourceItem,
            sourceContainer,
            sourceSlot,
            targetContainer,
            targetSlot,
            splitAmount
        );
    }

    //거점 성장을 위한 아이템 탐색을 위한 슬롯 순회 이벤트
    public bool HasEnoughUnlockCosts(List<UnlockCost> costs)
    {
        if (costs == null || costs.Count == 0)
            return true;

        for (int i = 0; i < costs.Count; i++)
        {
            UnlockCost cost = costs[i];
            if (cost == null || string.IsNullOrEmpty(cost.itemDefinitionId) || cost.amount <= 0)
                continue;

            int owned = GetTotalItemCountForUnlock(cost.itemDefinitionId);
            if (owned < cost.amount)
                return false;
        }

        return true;
    }

    public int GetOwnedItemCount(string definitionId)
    {
        if (string.IsNullOrEmpty(definitionId))
            return 0;

        return GetTotalItemCountForUnlock(definitionId);
    }

    public bool ConsumeUnlockCosts(List<UnlockCost> costs)
    {
        if (costs == null || costs.Count == 0)
            return true;

        if (!HasEnoughUnlockCosts(costs))
            return false;

        for (int i = 0; i < costs.Count; i++)
        {
            UnlockCost cost = costs[i];
            if (cost == null || string.IsNullOrEmpty(cost.itemDefinitionId) || cost.amount <= 0)
                continue;

            bool success = ConsumeItemAmountForUnlock(cost.itemDefinitionId, cost.amount);
            if (!success)
                return false;
        }

        RecalculateAndApply();
        return true;
    }

    private int GetTotalItemCountForUnlock(string definitionId)
    {
        int total = 0;

        total += GetTotalItemCountInContainer(squad.InventoryContainer, definitionId);
        total += GetTotalItemCountInContainer(GetOrCreateStashContainer(), definitionId);

        return total;
    }

    private int GetTotalItemCountInContainer(ContainerState container, string definitionId)
    {
        if (container == null || itemRepository == null)
            return 0;

        int total = 0;

        for (int i = 0; i < container.Slots.Count; i++)
        {
            SlotState slot = container.Slots[i];
            if (!slot.IsUnlocked || slot.IsEmpty)
                continue;

            ItemInstance item = itemRepository.Get(slot.ItemInstanceId);
            if (item == null)
                continue;

            if (item.DefinitionId != definitionId)
                continue;

            total += item.StackCount;
        }

        return total;
    }

    private bool ConsumeItemAmountForUnlock(string definitionId, int amount)
    {
        if (amount <= 0)
            return true;

        if (!ConsumeFromContainerForUnlock(squad.InventoryContainer, definitionId, ref amount))
            return false;

        if (amount > 0)
        {
            if (!ConsumeFromContainerForUnlock(GetOrCreateStashContainer(), definitionId, ref amount))
                return false;
        }

        return amount == 0;
    }

    private bool ConsumeFromContainerForUnlock(ContainerState container, string definitionId, ref int remaining)
    {
        if (container == null || itemRepository == null)
            return false;

        for (int i = 0; i < container.Slots.Count; i++)
        {
            if (remaining <= 0)
                return true;

            SlotState slot = container.Slots[i];
            if (!slot.IsUnlocked || slot.IsEmpty)
                continue;

            ItemInstance item = itemRepository.Get(slot.ItemInstanceId);
            if (item == null)
                continue;

            if (item.DefinitionId != definitionId)
                continue;

            if (item.StackCount <= remaining)
            {
                remaining -= item.StackCount;
                itemRepository.Remove(item.InstanceId);
                slot.ItemInstanceId = null;
            }
            else
            {
                item.StackCount -= remaining;
                remaining = 0;
                return true;
            }
        }

        return true;
    }

    //퀘스트 관련 - 거점 성장 재료 차감이랑 유사
    public bool TrySubmitQuestItems(string definitionId, int amount, QuestManager questManager)
    {
        if (string.IsNullOrEmpty(definitionId) || amount <= 0)
            return false;

        List<UnlockCost> tempCosts = new List<UnlockCost>
        {
            new UnlockCost
            {
                itemDefinitionId = definitionId,
                amount = amount
            }
        };

        if (!HasEnoughUnlockCosts(tempCosts))
            return false;

        bool consumed = ConsumeUnlockCosts(tempCosts);
        if (!consumed)
            return false;

        if (questManager != null)
        {
            questManager.PublishEvent(new QuestEvent(QuestEventType.SubmitItem, definitionId, amount));
        }

        return true;
    }

    //퀘스트 보상 지급 관련 - 현재는 창고로 꽃음
    public bool TryAddRewardItemToStash(string definitionId, int amount)
    {
        if (string.IsNullOrEmpty(definitionId) || amount <= 0)
            return false;

        ItemDefinitionBase def = itemDatabase.GetDefinition(definitionId);
        if (def == null)
            return false;

        int remaining = amount;

        InventoryQueryService queryService = new InventoryQueryService(itemDatabase);
        StackService stackService = new StackService(itemDatabase);

        ContainerState stash = GetOrCreateStashContainer();

        // 1. 기존 스택에 합치기
        while (remaining > 0)
        {
            ItemInstance tempItem = new ItemInstance(definitionId, remaining);

            SlotState stackableSlot = queryService.FindFirstStackableSlot(
                tempItem,
                stash,
                itemRepository,
                SlotKind.Inventory   // stash도 inventory 슬롯 사용
            );

            if (stackableSlot == null)
                break;

            ItemInstance targetItem = itemRepository.Get(stackableSlot.ItemInstanceId);
            if (targetItem == null)
                break;

            int moved = stackService.MergeInto(tempItem, targetItem);
            remaining -= moved;
        }

        // 2. 빈 슬롯 생성
        while (remaining > 0)
        {
            SlotState emptySlot = FindFirstUnlockedEmptyStashSlot();
            if (emptySlot == null)
                return false;

            int stackAmount = Mathf.Min(remaining, def.MaxStack);

            ItemInstance newItem = new ItemInstance(definitionId, stackAmount);
            itemRepository.Add(newItem);

            emptySlot.ItemInstanceId = newItem.InstanceId;

            remaining -= stackAmount;
        }

        RecalculateAndApply();
        return true;
    }
    private SlotState FindFirstUnlockedEmptyStashSlot()
    {
        if (stashContainer == null)
            return null;

        for (int i = 0; i < stashContainer.Slots.Count; i++)
        {
            SlotState slot = stashContainer.Slots[i];

            if (!slot.IsUnlocked || !slot.IsEmpty)
                continue;

            return slot;
        }

        return null;
    }

    //임시 아이템 생성
    private void SeedDebugItems()
    {
        AddDebugItemToInventory(PositionIndex.Position1, "material_steel_plate", 6);
        AddDebugItemToInventory(PositionIndex.Position2, "material_steel_plate", 10);
        AddDebugItemToInventory(PositionIndex.Position2, "material_steel_plate", 10);
    }

    private bool AddDebugItemToInventory(PositionIndex positionIndex, string definitionId, int stackCount)
    {
        if (itemDatabase == null || squad == null || itemRepository == null)
            return false;

        ItemDefinitionBase def = itemDatabase.GetDefinition(definitionId);
        if (def == null)
        {
            Debug.LogWarning($"[DebugSeed] 정의를 찾을 수 없음: {definitionId}");
            return false;
        }

        for (int i = 0; i < squad.InventoryContainer.Slots.Count; i++)
        {
            SlotState slot = squad.InventoryContainer.Slots[i];

            if (slot.PositionIndex != positionIndex)
                continue;

            if (!slot.IsUnlocked || !slot.IsEmpty)
                continue;

            ItemInstance item = new ItemInstance(definitionId, stackCount);
            itemRepository.Add(item);
            slot.ItemInstanceId = item.InstanceId;

            Debug.Log($"[DebugSeed] {definitionId} x{stackCount} -> {positionIndex} 슬롯 {slot.SlotIndex}");
            return true;
        }

        Debug.LogWarning($"[DebugSeed] {positionIndex}에 빈 슬롯 없음");
        return false;
    }
}