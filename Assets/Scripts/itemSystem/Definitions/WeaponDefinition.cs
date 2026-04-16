using UnityEngine;

[CreateAssetMenu(menuName = "Item System/Equipment/Weapon")]
public class WeaponDefinition : EquipmentDefinition
{
    [Header("Weapon Stats")]
    [SerializeField] private WeaponStatBlock weaponStats;

    public WeaponStatBlock WeaponStats => weaponStats;
}