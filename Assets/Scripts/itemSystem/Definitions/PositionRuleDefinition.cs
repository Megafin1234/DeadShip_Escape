using UnityEngine;

[CreateAssetMenu(menuName = "Item System/Definitions/Position Rule")]
public class PositionRuleDefinition : ScriptableObject
{
    [Header("Position")]
    [SerializeField] private PositionIndex positionIndex;

    [Header("Equipment Layout")]
    [SerializeField] private EquipSlotType[] equipSlotLayout = new EquipSlotType[4];

    [Header("Inventory Capacity")]
    [SerializeField] private int baseInventorySlots = 12;
    [SerializeField] private int maxGearBonusSlots = 0;
    [SerializeField] private int maxUpgradeBonusSlots = 0;

    public PositionIndex PositionIndex => positionIndex;
    public EquipSlotType[] EquipSlotLayout => equipSlotLayout;

    public int BaseInventorySlots => baseInventorySlots;
    public int MaxGearBonusSlots => maxGearBonusSlots;
    public int MaxUpgradeBonusSlots => maxUpgradeBonusSlots;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (equipSlotLayout == null || equipSlotLayout.Length != 4)
        {
            var newArray = new EquipSlotType[4];

            if (equipSlotLayout != null)
            {
                for (int i = 0; i < Mathf.Min(equipSlotLayout.Length, 4); i++)
                    newArray[i] = equipSlotLayout[i];
            }

            equipSlotLayout = newArray;
        }

        if (baseInventorySlots < 0)
            baseInventorySlots = 0;

        if (maxGearBonusSlots < 0)
            maxGearBonusSlots = 0;

        if (maxUpgradeBonusSlots < 0)
            maxUpgradeBonusSlots = 0;
    }
#endif
}