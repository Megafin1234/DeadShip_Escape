using UnityEngine;

public abstract class ItemDefinitionBase : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] [TextArea(2, 5)] private string description;

    [Header("Visual")]
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject worldPrefab;

    [Header("Classification")]
    [SerializeField] private ItemCategory itemCategory;
    [SerializeField] private ItemRarity rarity;

    [Header("Economy")]
    [SerializeField] private int sellPrice;
    [SerializeField] private float weight = 1f;
    [SerializeField] private int maxStack = 1;

    [Header("Flags")]
    [SerializeField] private bool isDroppable = true;
    [SerializeField] private bool isStorable = true;
    [SerializeField] private bool isQuestTrackable = true;
    [SerializeField] private bool isFavoriteMarkable = true;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public string Description => description;

    public Sprite Icon => icon;
    public GameObject WorldPrefab => worldPrefab;

    public ItemCategory ItemCategory => itemCategory;
    public ItemRarity Rarity => rarity;

    public int SellPrice => sellPrice;
    public float Weight => weight;
    public int MaxStack => maxStack;

    public bool IsDroppable => isDroppable;
    public bool IsStorable => isStorable;
    public bool IsQuestTrackable => isQuestTrackable;
    public bool IsFavoriteMarkable => isFavoriteMarkable;

    public virtual bool IsUsable => false;
    public virtual bool IsEquippable => false;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (maxStack < 1)
            maxStack = 1;

        if (weight < 0f)
            weight = 0f;

        if (sellPrice < 0)
            sellPrice = 0;
    }
#endif
}