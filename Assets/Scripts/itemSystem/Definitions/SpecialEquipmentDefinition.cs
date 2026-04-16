using UnityEngine;

[CreateAssetMenu(menuName = "Item System/Equipment/Special Equipment")]
public class SpecialEquipmentDefinition : EquipmentDefinition
{
    [Header("Special Equipment")]
    [SerializeField] private SpecialEquipmentType specialEquipmentType;

    [Header("Passive Effects")]
    [SerializeField] private StatModifierData[] passiveEffects;

    public SpecialEquipmentType SpecialEquipmentType => specialEquipmentType;
    public StatModifierData[] PassiveEffects => passiveEffects;
}