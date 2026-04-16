using System;
using System.Collections.Generic;

[Serializable]
public class SquadRuntime
{
    public string SquadId;

    public ContainerState InventoryContainer;
    public ContainerState EquipmentContainer;
    public ContainerState QuickSlotContainer;
    public PositionRuleDefinition Position1Rule;
    public PositionRuleDefinition Position2Rule;
    public PositionRuleDefinition Position3Rule;

    public List<SquadPositionRuntime> Positions = new();
    public List<QuickSlotRuntime> QuickSlots = new();

    public DerivedSquadStats DerivedStats = new DerivedSquadStats();
    public SharedSquadCombatState CombatState = new SharedSquadCombatState();
    public RaidStateType RaidState = RaidStateType.None;

    public bool IsInRaid => RaidState == RaidStateType.InRaid;
    public bool HasExtracted => RaidState == RaidStateType.Extracted;
    public bool IsWiped => RaidState == RaidStateType.Wiped;

            /// <summary>
        /// 현재 스쿼드에 적용 중인 시간제 버프 목록.
        /// 예:
        /// - 이동속도 +2 / 10초
        /// - 공격력 +10 / 8초
        /// 
        /// 이 리스트는 PlayerSquadBridge.Update()에서 시간 감소 처리되고,
        /// SquadStatCalculator.Recalculate() 이후 ApplyActiveBuffs()에서 실제 수치 반영됩니다.
        /// </summary>
    public List<ActiveBuffRuntime> ActiveBuffs = new();

    public SquadRuntime()
    {
        SquadId = Guid.NewGuid().ToString();

        InventoryContainer = new ContainerState(ContainerType.SquadInventory);
        EquipmentContainer = new ContainerState(ContainerType.SquadEquipment);
        QuickSlotContainer = new ContainerState(ContainerType.QuickSlot);

        Positions.Add(new SquadPositionRuntime(PositionIndex.Position1));
        Positions.Add(new SquadPositionRuntime(PositionIndex.Position2));
        Positions.Add(new SquadPositionRuntime(PositionIndex.Position3));
    }
    public PositionRuleDefinition GetPositionRule(PositionIndex position)
    {
        return position switch
        {
            PositionIndex.Position1 => Position1Rule,
            PositionIndex.Position2 => Position2Rule,
            PositionIndex.Position3 => Position3Rule,
            _ => null
        };
    }

    public int GetLocalEquipmentSlotIndex(SlotState slot)
    {
        if (slot == null || slot.SlotKind != SlotKind.Equipment)
            return -1;

        // Position별 장비 슬롯 4개 고정이라고 가정
        // 예: P1 = 0~3, P2 = 4~7, P3 = 8~11
        return slot.PositionIndex switch
        {
            PositionIndex.Position1 => slot.SlotIndex,
            PositionIndex.Position2 => slot.SlotIndex - 4,
            PositionIndex.Position3 => slot.SlotIndex - 8,
            _ => -1
        };
    }
    
    public CharacterDefinition GetCharacter(PositionIndex position)
    {
        for (int i = 0; i < Positions.Count; i++)
        {
            if (Positions[i].PositionIndex == position)
                return Positions[i].CharacterDefinition;
        }

        return null;
    }
}