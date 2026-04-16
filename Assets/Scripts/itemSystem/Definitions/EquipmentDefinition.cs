using UnityEngine;

public abstract class EquipmentDefinition : ItemDefinitionBase
{
    [Header("Equipment")]
    [SerializeField] private EquipmentType equipmentType;

    [Header("Stat Modifiers")]
    [SerializeField] private StatModifierData[] statModifiers;

    public EquipmentType EquipmentType => equipmentType;
    public StatModifierData[] StatModifiers => statModifiers;

    public override bool IsEquippable => true;
}