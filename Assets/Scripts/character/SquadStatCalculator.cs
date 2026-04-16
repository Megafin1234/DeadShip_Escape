using UnityEngine;
public class SquadStatCalculator
{
    private readonly ItemDatabase itemDatabase;
    private readonly ItemInstanceRepository itemRepository;

    public SquadStatCalculator(ItemDatabase itemDatabase, ItemInstanceRepository itemRepository)
    {
        this.itemDatabase = itemDatabase;
        this.itemRepository = itemRepository;
    }

    public DerivedSquadStats Recalculate(SquadRuntime squad)
    {
        DerivedSquadStats result = new DerivedSquadStats();
        result.MoveSpeed = 5f;                   ////////////////////////////////////////////////////////// 이동속도가 0일까봐 기본이속 넣은거 

        if (squad == null)
            return result;

        for (int i = 0; i < squad.Positions.Count; i++) //캐릭터의 기본 공/체 합산
        {
            SquadPositionRuntime position = squad.Positions[i];
            if (position == null || position.CharacterDefinition == null)
                continue;

            result.Attack += position.CharacterDefinition.BaseAttack;
            result.Health += position.CharacterDefinition.BaseHealth;

            ApplyModifiers(result, position.CharacterDefinition.PassiveModifiers);
        }

        ApplyEquipmentModifiers(squad, result); //장착중인 장비 합산
        ApplyActiveBuffs(squad,result);// 적용중인 버프 합산

        squad.DerivedStats = result;

        squad.CombatState.MaxHealth = result.Health;

        if (squad.CombatState.CurrentHealth <= 0)
            squad.CombatState.CurrentHealth = result.Health;

        if (squad.CombatState.CurrentHealth > result.Health)
            squad.CombatState.CurrentHealth = result.Health;

        squad.CombatState.IsDead = squad.CombatState.CurrentHealth <= 0;

        return result;
    }

    private void ApplyEquipmentModifiers(SquadRuntime squad, DerivedSquadStats stats)
    {
        if (squad == null || squad.EquipmentContainer == null)
            return;

        for (int i = 0; i < squad.EquipmentContainer.Slots.Count; i++)
        {
            SlotState slot = squad.EquipmentContainer.Slots[i];
            if (slot == null || slot.IsEmpty)
                continue;

            ItemInstance instance = itemRepository.Get(slot.ItemInstanceId);
            if (instance == null)
                continue;

            ItemDefinitionBase def = itemDatabase.GetDefinition(instance.DefinitionId);
            if (def is not EquipmentDefinition equipDef)
                continue;

            ApplyModifiers(stats, equipDef.StatModifiers);

            if (equipDef is WeaponDefinition weaponDef)
                ApplyWeaponStats(stats, weaponDef.WeaponStats);

            if (equipDef is SpecialEquipmentDefinition specialDef)
                ApplyModifiers(stats, specialDef.PassiveEffects);
        }
    }

    private void ApplyModifiers(DerivedSquadStats stats, StatModifierData[] modifiers) //캐릭터의 패시브 효과 합산
    {
        if (modifiers == null)
            return;

        for (int i = 0; i < modifiers.Length; i++)
        {
            StatModifierData mod = modifiers[i];

            switch (mod.statType)
            {
                case StatType.Attack:
                    stats.Attack += (int)mod.value;
                    break;

                case StatType.Health:
                    stats.Health += (int)mod.value;
                    break;

                case StatType.MoveSpeed:
                    stats.MoveSpeed += mod.value;
                    break;

                case StatType.CarryWeight:
                    stats.CarryWeight += mod.value;
                    break;

                case StatType.HungerRecovery:
                    stats.HungerRecovery += mod.value;
                    break;

                case StatType.OxygenEfficiency:
                    stats.OxygenEfficiency += mod.value;
                    break;

                case StatType.ContaminationRecovery:
                    stats.ContaminationRecovery += mod.value;
                    break;

                case StatType.BulletSpreadVertical:
                    stats.BulletSpreadVertical += mod.value;
                    break;

                case StatType.BulletSpreadHorizontal:
                    stats.BulletSpreadHorizontal += mod.value;
                    break;

                case StatType.FireInterval:
                    stats.FireInterval += mod.value;
                    break;

                case StatType.AttackRange:
                    stats.AttackRange += mod.value;
                    break;
            }
        }
    }

    private void ApplyWeaponStats(DerivedSquadStats stats, WeaponStatBlock weaponStats)
    {
        stats.Attack += weaponStats.attack;
        stats.AttackRange += weaponStats.attackRange;
        stats.FireInterval += weaponStats.fireInterval;
        stats.BulletSpreadVertical += weaponStats.spreadVertical;
        stats.BulletSpreadHorizontal += weaponStats.spreadHorizontal;
    }

    private void ApplyActiveBuffs(SquadRuntime squad, DerivedSquadStats stats)
    {
        if (squad == null || squad.ActiveBuffs == null)
            return;

        Debug.Log($"[SquadStatCalculator] ApplyActiveBuffs count = {squad.ActiveBuffs.Count}");

        for (int i = 0; i < squad.ActiveBuffs.Count; i++)
        {
            ActiveBuffRuntime buff = squad.ActiveBuffs[i];
            Debug.Log($"[SquadStatCalculator] buff -> {buff.BuffType}, value:{buff.Value}, remain:{buff.RemainingTime}");

            switch (buff.BuffType)
            {
                case ActiveBuffType.MoveSpeed:
                    stats.MoveSpeed += buff.Value;
                    break;

                case ActiveBuffType.Attack:
                    stats.Attack += Mathf.RoundToInt(buff.Value);
                    break;
            }
        }
    }
}