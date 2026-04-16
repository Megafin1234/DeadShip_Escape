using UnityEngine;

/// <summary>
/// 바닥에 떨어진 아이템.
/// F 상호작용 시 스쿼드 인벤토리에 자동 획득 시도.
/// 일정 시간 뒤 자동 제거.
/// </summary>
public class WorldDroppedItemInteractable : InteractableBase
{
    [Header("Drop Data")]
    [SerializeField] private string definitionId;
    [SerializeField] private int stackCount = 1;
    [SerializeField] private UnityEngine.UI.Image iconImage;

    [Header("Lifetime")]
    [SerializeField] private float despawnTime = 60f;

    [Header("Display")]
    [SerializeField] private string displayName = "Dropped Item";

    private void Start()
    {
        Destroy(gameObject, despawnTime);
    }

    public void Initialize(string itemDefinitionId, int amount, string itemDisplayName)
    {
        definitionId = itemDefinitionId;
        stackCount = amount;
        displayName = itemDisplayName;

        PlayerSquadBridge bridge = PlayerSquadBridge.Instance;
        if (bridge != null)
        {
            ItemDefinitionBase def = bridge.GetItemDefinition(definitionId);

            if (def != null && iconImage != null)
            {
                iconImage.sprite = def.Icon;
            }
        }
    }
    protected override void Interact(Transform interactor)
    {
        PlayerSquadBridge bridge = PlayerSquadBridge.Instance;
        if (bridge == null)
            return;

        ItemDefinitionBase def = bridge.GetItemDefinition(definitionId);
        if (def == null)
        {
            Debug.LogWarning("[WorldDroppedItemInteractable] 아이템 정의를 찾지 못했습니다.");
            return;
        }

        ItemInstance item = new ItemInstance(definitionId, stackCount);
        bridge.ItemRepository.Add(item);

        ItemMoveResult result = bridge.TryPickupDroppedItem(item);

        Debug.Log($"[WorldDroppedItemInteractable] 줍기 결과: {result.Message}");

        if (result.Success)
        {
            Destroy(gameObject);
        }
        else
        {
            // 실패 시 repository에 넣은 임시 item 정리 필요할 수 있음
            // 현재 구조상 Remove가 없다면 나중에 추가해도 됨.
        }
    }
}