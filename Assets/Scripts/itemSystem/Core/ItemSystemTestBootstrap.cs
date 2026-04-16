using System.Text;
using TMPro;
using UnityEngine;

public class ItemSystemTestBootstrap : MonoBehaviour
{
    [Header("Static Data")]
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private PositionRuleDefinition position1Rule;
    [SerializeField] private PositionRuleDefinition position2Rule;
    [SerializeField] private PositionRuleDefinition position3Rule;

    [Header("Character Definitions")]
    [SerializeField] private CharacterDefinition position1Character;
    [SerializeField] private CharacterDefinition position2Character;
    [SerializeField] private CharacterDefinition position3Character;

    [Header("Debug UI")]
    [SerializeField] private TMP_Text inventoryStateText;
    [SerializeField] private TMP_Text equipmentStateText;
    [SerializeField] private TMP_Text stashStateText;
    [SerializeField] private TMP_Text lootStateText;
    [SerializeField] private TMP_Text logText;
    [SerializeField] private TMP_Text squadStatsText;
    [SerializeField] private TMP_Text combatStateText;
    [SerializeField] private TMP_Text lootContainerStateText;

    private SquadRuntime squad;
    private ContainerState stashContainer;
    private ContainerState lootContainer;

    private ItemInstanceRepository itemRepo;
    private ItemMoveService itemMoveService;
    private EquipService equipService;
    private AutoMoveService autoMoveService;
    private SquadStatCalculator squadStatCalculator;
    private SharedHealthService sharedHealthService;
    private RaidSquadContext raidContext;
    private LootContainerRuntime testLootContainer;
    private OpenedLootContext openedLootContext;
    private LootContainerFactory lootFactory;
    private LootTransferService lootTransferService;

    private void Start()
    {
        InitializeStaticData();
        InitializeRuntime();
        InitializeServices();
        SeedTestItems();
        ValidateSeedDefinitions();
        RefreshDebugView();
        RefreshCombatStateView();
        Log("테스트 환경 초기화 완료");
        testLootContainer = lootFactory.CreateTestContainer("Test Chest");
        openedLootContext = new OpenedLootContext(testLootContainer, squad);
        RefreshLootView();
    }

    private void InitializeStaticData()
    {
        itemDatabase.Initialize();
    }

    private void InitializeRuntime()
    {
        squad = new SquadRuntime();

        squad.Position1Rule = position1Rule;
        squad.Position2Rule = position2Rule;
        squad.Position3Rule = position3Rule;

        itemRepo = new ItemInstanceRepository();

        stashContainer = new ContainerState(ContainerType.Stash);
        lootContainer = new ContainerState(ContainerType.LootContainer);

        squad.Positions[0].CharacterDefinition = position1Character;
        squad.Positions[0].CharacterId = position1Character != null ? position1Character.CharacterId : null;

        squad.Positions[1].CharacterDefinition = position2Character;
        squad.Positions[1].CharacterId = position2Character != null ? position2Character.CharacterId : null;

        squad.Positions[2].CharacterDefinition = position3Character;
        squad.Positions[2].CharacterId = position3Character != null ? position3Character.CharacterId : null;

        InitializeEquipmentSlots();
        InitializeInventorySlots();
        InitializeExtraContainers();
    }

    private void InitializeServices()
    {
        itemMoveService = new ItemMoveService(itemDatabase, itemRepo);
        equipService = new EquipService(itemMoveService);
        autoMoveService = new AutoMoveService(itemDatabase, itemRepo, itemMoveService);
        squadStatCalculator = new SquadStatCalculator(itemDatabase, itemRepo);
        sharedHealthService = new SharedHealthService();
        lootFactory = new LootContainerFactory(itemRepo);
        lootTransferService = new LootTransferService(autoMoveService, itemRepo);
    }

    private void InitializeEquipmentSlots()
    {
        // P1 : 0~3
        for (int i = 0; i < 4; i++)
            squad.EquipmentContainer.Slots.Add(new SlotState(i, SlotKind.Equipment, PositionIndex.Position1));

        // P2 : 4~7
        for (int i = 4; i < 8; i++)
            squad.EquipmentContainer.Slots.Add(new SlotState(i, SlotKind.Equipment, PositionIndex.Position2));

        // P3 : 8~11
        for (int i = 8; i < 12; i++)
            squad.EquipmentContainer.Slots.Add(new SlotState(i, SlotKind.Equipment, PositionIndex.Position3));
    }

    private void InitializeInventorySlots()
    {
        // 예시: 포지션당 최대 24칸
        CreateInventorySegment(PositionIndex.Position1, 0, 24, 12);
        CreateInventorySegment(PositionIndex.Position2, 24, 24, 12);
        CreateInventorySegment(PositionIndex.Position3, 48, 24, 12);
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

    private void InitializeExtraContainers()
    {
        for (int i = 0; i < 20; i++)
            stashContainer.Slots.Add(new SlotState(i, SlotKind.Inventory, PositionIndex.None));

        for (int i = 0; i < 10; i++)
            lootContainer.Slots.Add(new SlotState(i, SlotKind.Loot, PositionIndex.None));
    }

    private void SeedTestItems()
    {
        AddItemToFirstEmptySlot(
            squad.InventoryContainer,
            SlotKind.Inventory,
            new ItemInstance("weapon_test_rifle", 1));

        AddItemToFirstEmptySlot(
            squad.InventoryContainer,
            SlotKind.Inventory,
            new ItemInstance("equip_test_armor", 1));

        AddItemToFirstEmptySlot(
            squad.InventoryContainer,
            SlotKind.Inventory,
            new ItemInstance("equip_test_gloves", 1));

        AddItemToFirstEmptySlot(
            squad.InventoryContainer,
            SlotKind.Inventory,
            new ItemInstance("equip_test_boots", 1));

        AddItemToFirstEmptySlot(
            squad.InventoryContainer,
            SlotKind.Inventory,
            new ItemInstance("equip_test_bag", 1));

        AddItemToFirstEmptySlot(
            squad.InventoryContainer,
            SlotKind.Inventory,
            new ItemInstance("special_test_scanner", 1));

        AddItemToFirstEmptySlot(
            stashContainer,
            SlotKind.Inventory,
            new ItemInstance("material_steel_plate", 10));

        AddItemToFirstEmptySlot(
            lootContainer,
            SlotKind.Loot,
            new ItemInstance("consumable_medkit", 2));
    }

    private void AddItemToFirstEmptySlot(ContainerState container, SlotKind slotKind, ItemInstance item)
    {
        itemRepo.Add(item);

        SlotState slot = container.FindFirstEmptySlot(slotKind);
        if (slot != null)
        {
            slot.ItemInstanceId = item.InstanceId;
            Debug.Log($"[ItemSystemTest] 아이템 배치: def={item.DefinitionId}, instance={item.InstanceId}, container={container.ContainerType}, slot={slot.SlotIndex}");
        }
        else
        {
            Debug.LogWarning($"[ItemSystemTest] 빈 슬롯 없음: def={item.DefinitionId}, container={container.ContainerType}");
        }
    }
    private void RefreshLootView()
    {
        if (lootContainerStateText == null || testLootContainer == null)
            return;

        lootContainerStateText.text = BuildContainerText(testLootContainer.DisplayName, testLootContainer.Container);
    }

    public void DebugPrintState()
    {
        RefreshDebugView();
        Log("현재 상태 출력");
    }

    public void DebugAutoEquipFirstInventoryItem()
    {
        SlotState inventorySlot = FindFirstFilledSlot(squad.InventoryContainer, SlotKind.Inventory);
        if (inventorySlot == null)
        {
            Log("인벤토리에 아이템이 없습니다.");
            return;
        }

        ItemInstance item = itemRepo.Get(inventorySlot.ItemInstanceId);
        ItemMoveResult result = autoMoveService.AutoEquipFromInventory(item, inventorySlot, squad);

        Log(result.Message);
        RefreshDebugView();
    }

    public void DebugAutoUnequipFirstEquipmentItem()
    {
        SlotState equipmentSlot = FindFirstFilledSlot(squad.EquipmentContainer, SlotKind.Equipment);
        if (equipmentSlot == null)
        {
            Log("장비 슬롯에 아이템이 없습니다.");
            return;
        }

        ItemInstance item = itemRepo.Get(equipmentSlot.ItemInstanceId);
        ItemMoveResult result = autoMoveService.AutoUnequipToInventory(item, equipmentSlot, squad);

        Log(result.Message);
        RefreshDebugView();
    }

    public void DebugMoveFirstLootToInventory()
    {
        SlotState lootSlot = FindFirstFilledSlot(lootContainer, SlotKind.Loot);
        if (lootSlot == null)
        {
            Log("루팅 컨테이너가 비어 있습니다.");
            return;
        }

        ItemInstance item = itemRepo.Get(lootSlot.ItemInstanceId);
        ItemMoveResult result = autoMoveService.AutoMoveFromLootToInventory(item, lootContainer, lootSlot, squad);

        Log(result.Message);
        RefreshDebugView();
    }

    public void DebugMoveFirstInventoryToStash()
    {
        SlotState source = FindFirstFilledSlot(squad.InventoryContainer, SlotKind.Inventory);
        if (source == null)
        {
            Log("인벤토리에 아이템이 없습니다.");
            return;
        }

        ItemInstance item = itemRepo.Get(source.ItemInstanceId);
        ItemMoveResult result = autoMoveService.AutoMoveBetweenStashAndInventory(
            item,
            squad.InventoryContainer,
            source,
            stashContainer,
            squad);

        Log(result.Message);
        RefreshDebugView();
    }

    public void DebugMoveFirstStashToInventory()
    {
        SlotState source = FindFirstFilledSlot(stashContainer, SlotKind.Inventory);
        if (source == null)
        {
            Log("창고가 비어 있습니다.");
            return;
        }

        ItemInstance item = itemRepo.Get(source.ItemInstanceId);
        ItemMoveResult result = autoMoveService.AutoMoveBetweenStashAndInventory(
            item,
            stashContainer,
            source,
            squad.InventoryContainer,
            squad);

        Log(result.Message);
        RefreshDebugView();
    }

    public void DebugRecalculateSquadStats()
    {
        DerivedSquadStats stats = squadStatCalculator.Recalculate(squad);
        Log("스쿼드 스탯 재계산 완료");

        if (squadStatsText != null)
        {
            squadStatsText.text =
                $"Attack: {stats.Attack}\n" +
                $"Health: {stats.Health}\n" +
                $"Current HP: {squad.CombatState.CurrentHealth}\n" +
                $"MoveSpeed: {stats.MoveSpeed}\n" +
                $"CarryWeight: {stats.CarryWeight}\n" +
                $"AttackRange: {stats.AttackRange}\n" +
                $"FireInterval: {stats.FireInterval}\n" +
                $"SpreadV: {stats.BulletSpreadVertical}\n" +
                $"SpreadH: {stats.BulletSpreadHorizontal}";
        }
        RefreshCombatStateView();
    }
    public void DebugEnterRaid()
    {
        sharedHealthService.EnterRaid(squad);
        RefreshCombatStateView();
        Log("레이드 진입 처리");
    }
    public void DebugApplyDamage10()
    {
        sharedHealthService.ApplyDamage(squad, 10);
        RefreshCombatStateView();
        Log("공유 체력에 10 데미지 적용");
    }
    public void DebugHeal10()
    {
        sharedHealthService.Heal(squad, 10);
        RefreshCombatStateView();
        Log("공유 체력 10 회복");
    }
    public void DebugExtract()
    {
        sharedHealthService.Extract(squad);
        RefreshCombatStateView();
        Log("탈출 처리");
    }
    public void DebugMoveFirstLootItemToSquad()
    {
        if (openedLootContext == null)
        {
            Log("열린 루팅 컨텍스트가 없습니다.");
            return;
        }

        ItemMoveResult result = lootTransferService.MoveFirstLootItemToSquad(openedLootContext);

        Log(result.Message);
        RefreshDebugView();
    }

    private SlotState FindFirstFilledSlot(ContainerState container, SlotKind slotKind)
    {
        for (int i = 0; i < container.Slots.Count; i++)
        {
            SlotState slot = container.Slots[i];
            if (slot.SlotKind == slotKind && slot.IsUnlocked && !slot.IsEmpty)
                return slot;
        }

        return null;
    }

    private void RefreshDebugView()
    {
        if (inventoryStateText != null)
            inventoryStateText.text = BuildContainerText("Inventory", squad.InventoryContainer);

        if (equipmentStateText != null)
            equipmentStateText.text = BuildContainerText("Equipment", squad.EquipmentContainer);

        if (stashStateText != null)
            stashStateText.text = BuildContainerText("Stash", stashContainer);

        if (lootStateText != null)
            lootStateText.text = BuildContainerText("Loot", lootContainer);
        RefreshLootView();
    }

    private void RefreshCombatStateView()
    {
        if (combatStateText == null || squad == null)
            return;

        combatStateText.text =
            $"RaidState: {squad.RaidState}\n" +
            $"Current HP: {squad.CombatState.CurrentHealth}\n" +
            $"Max HP: {squad.CombatState.MaxHealth}\n" +
            $"IsDead: {squad.CombatState.IsDead}";
    }

    private string BuildContainerText(string title, ContainerState container)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(title);

        for (int i = 0; i < container.Slots.Count; i++)
        {
            SlotState slot = container.Slots[i];

            string itemLabel = "Empty";

            if (!slot.IsEmpty)
            {
                ItemInstance instance = itemRepo.Get(slot.ItemInstanceId);

                if (instance == null)
                {
                    itemLabel = $"BROKEN_INSTANCE[{slot.ItemInstanceId}]";
                }
                else
                {
                    ItemDefinitionBase def = itemDatabase.GetDefinition(instance.DefinitionId);

                    if (def != null)
                        itemLabel = $"{def.DisplayName} x{instance.StackCount}";
                    else
                        itemLabel = $"UNKNOWN_DEF[{instance.DefinitionId}] x{instance.StackCount}";
                }
            }

            sb.AppendLine(
                $"[{slot.SlotIndex}] " +
                $"Kind={slot.SlotKind}, Pos={slot.PositionIndex}, " +
                $"Unlocked={slot.IsUnlocked}, Item={itemLabel}");
        }

        return sb.ToString();
    }

    private void Log(string message)
    {
        Debug.Log($"[ItemSystemTest] {message}");

        if (logText != null)
            logText.text = message;
    }

    private void ValidateSeedDefinitions()
    {
        ValidateDefinition("weapon_test_rifle");
        ValidateDefinition("equip_test_armor");
        ValidateDefinition("equip_test_gloves");
        ValidateDefinition("equip_test_boots");
        ValidateDefinition("equip_test_bag");
        ValidateDefinition("special_test_scanner");
        ValidateDefinition("material_steel_plate");
        ValidateDefinition("consumable_medkit");
    }

    private void ValidateDefinition(string definitionId)
    {
        ItemDefinitionBase def = itemDatabase.GetDefinition(definitionId);

        if (def == null)
        {
            Debug.LogError($"[ItemSystemTest] 정의 없음: requested={definitionId}");
        }
        else
        {
            Debug.Log($"[ItemSystemTest] 정의 확인됨: requested={definitionId}, actualItemId={def.ItemId}, displayName={def.DisplayName}");
        }
    }
}