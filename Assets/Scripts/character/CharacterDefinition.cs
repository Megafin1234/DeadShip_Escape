using UnityEngine;

public enum CharacterClassType
{
    None = 0,
    Assault = 1,
    Support = 2,
    Survival = 3
}

[CreateAssetMenu(menuName = "Squad/Character Definition")]
public class CharacterDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string characterId;
    [SerializeField] private string displayName;
    [SerializeField] [TextArea(2, 4)] private string description;

    [Header("Visual")]
    [SerializeField] private Sprite portrait;
    [SerializeField] private GameObject characterPrefab;

    [Header("Class")]
    [SerializeField] private CharacterClassType classType;

    [Header("Base Stats")]
    [SerializeField] private int baseAttack = 50;
    [SerializeField] private int baseHealth = 50;

    [Header("Passive Modifiers")]
    [SerializeField] private StatModifierData[] passiveModifiers;

    [Header("Raid Rules")]
    [SerializeField] private bool grantsLossProtectionInventory = false;
    [SerializeField] private bool grantsLossProtectionEquipment = false;

    public string CharacterId => characterId;
    public string DisplayName => displayName;
    public string Description => description;

    public Sprite Portrait => portrait;
    public GameObject CharacterPrefab => characterPrefab;

    public CharacterClassType ClassType => classType;

    public int BaseAttack => baseAttack;
    public int BaseHealth => baseHealth;

    public StatModifierData[] PassiveModifiers => passiveModifiers;
    public bool GrantsLossProtectionInventory => grantsLossProtectionInventory;
    public bool GrantsLossProtectionEquipment => grantsLossProtectionEquipment;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (baseAttack < 0)
            baseAttack = 0;

        if (baseHealth < 1)
            baseHealth = 1;
    }
#endif
}