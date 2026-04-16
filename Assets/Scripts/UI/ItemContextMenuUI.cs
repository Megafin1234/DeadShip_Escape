using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemContextMenuUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Buttons")]
    [SerializeField] private Button useButton;
    [SerializeField] private Button quickSlotButton;
    [SerializeField] private Button dropButton;
    [SerializeField] private Button favoriteMarkButton;
    [SerializeField] private Button questMarkButton;
    [SerializeField] private TMP_Text favoriteMarkButtonText;

    [SerializeField] private Button closeButton;

    [Header("Labels")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemDescriptionText;
    [SerializeField] private TMP_Text itemCategoryText;
    [SerializeField] private TMP_Text itemRarityText;
    [SerializeField] private TMP_Text itemWeightText;

    private SlotBindData currentBindData;

    public Action<ItemContextActionType, SlotBindData> OnActionSelected;

    private void Awake()
    {
        HideImmediate();

        if (useButton != null)
            useButton.onClick.AddListener(() => InvokeAction(ItemContextActionType.Use));

        if (quickSlotButton != null)
            quickSlotButton.onClick.AddListener(() => InvokeAction(ItemContextActionType.RegisterQuickSlot));

        if (dropButton != null)
            dropButton.onClick.AddListener(() => InvokeAction(ItemContextActionType.Drop));

        if (favoriteMarkButton != null)
            favoriteMarkButton.onClick.AddListener(() => InvokeAction(ItemContextActionType.ToggleFavoriteMark));

        if (closeButton != null)
            closeButton.onClick.AddListener(() => InvokeAction(ItemContextActionType.Close));
    }

    public bool IsOpen => root != null && root.activeSelf;

    public void Show(Vector2 screenPosition, SlotBindData bindData)
    {
        if (bindData == null || bindData.ItemDefinition == null) return;

        currentBindData = bindData;

        if (root != null)
        {
            root.SetActive(true);
            RectTransform rect = root.GetComponent<RectTransform>();
            
            if (rect != null)
            {
                // 1. 피벗을 좌상단(0, 1)으로 강제하면 계산이 매우 쉬워집니다.
                rect.pivot = new Vector2(0f, 1f);

                // 2. 기본 오프셋 적용
                Vector2 offset = new Vector2(10f, -10f); // 마우스 오른쪽 아래로 살짝 내림
                Vector2 target = screenPosition + offset;

                // 3. UI 크기 (Content Size Fitter 사용 시 즉시 갱신 필요)
                Canvas.ForceUpdateCanvases(); //강제로 전체 ui불러오는 함수 사용에 유의
                Vector2 size = rect.rect.size;

                // 4. 화면 오른쪽 밖으로 나가는가? -> 왼쪽으로 밀기
                if (target.x + size.x > Screen.width)
                {
                    target.x = screenPosition.x - size.x - 10f;
                }

                // 5. 화면 아래쪽 밖으로 나가는가? -> 위로 밀기
                if (target.y - size.y < 0f)
                {
                    target.y = screenPosition.y + size.y + 10f;
                }

                // 6. 최종 화면 클램프 (안전장치)
                target.x = Mathf.Clamp(target.x, 0f, Screen.width - size.x);
                target.y = Mathf.Clamp(target.y, size.y, Screen.height);

                // 7. 위치 적용
                rect.position = target;
            }
        }

        RefreshItemInfo(bindData); // 띄워줄 아이템 정보 갱신
        RefreshButtons(bindData);
    }

    public void Hide()
    {
        currentBindData = null;
        HideImmediate();
    }

    private void HideImmediate()
    {
        if (root != null)
            root.SetActive(false);
    }

    private void RefreshButtons(SlotBindData bindData)
    {
        bool canUse = bindData.ItemDefinition.IsUsable;
        bool canDrop = bindData.ItemInstance != null;
        bool canFavoriteMark = bindData.ItemDefinition.IsFavoriteMarkable;
        bool canRegisterQuickSlot = bindData.ItemDefinition.IsUsable;

        if (useButton != null)
            useButton.gameObject.SetActive(canUse);

        if (quickSlotButton != null)
            quickSlotButton.gameObject.SetActive(canRegisterQuickSlot);

        if (dropButton != null)
            dropButton.gameObject.SetActive(canDrop);

        if (favoriteMarkButton != null)
            favoriteMarkButton.gameObject.SetActive(canFavoriteMark);

        if (favoriteMarkButtonText != null && bindData.ItemDefinition != null)
        {
            bool isMarked = PlayerSquadBridge.Instance != null &&
                            PlayerSquadBridge.Instance.MarkerRepository.IsFavoriteMarked(bindData.ItemDefinition.ItemId);

            favoriteMarkButtonText.text = isMarked ? "즐겨찾기 해제" : "즐겨찾기 표시";
        }



        if (closeButton != null)
            closeButton.gameObject.SetActive(true);
    }

    private void RefreshItemInfo(SlotBindData bindData)
    {
        ItemDefinitionBase def = bindData.ItemDefinition;
        if (def == null)
            return;

        if (itemIconImage != null)
        {
            itemIconImage.enabled = def.Icon != null;
            itemIconImage.sprite = def.Icon;
        }

        if (itemNameText != null)
            itemNameText.text = def.DisplayName;

        if (itemDescriptionText != null)
            itemDescriptionText.text = def.Description;

        if (itemCategoryText != null)
            itemCategoryText.text = $"종류: {def.ItemCategory}";
            itemCategoryText.text = $"종류: {GetCategoryDisplayName(def.ItemCategory)}";

        if (itemRarityText != null)
            itemRarityText.text = $"등급: {def.Rarity}";
            itemRarityText.text = $"등급: {GetRarityDisplayName(def.Rarity)}";

        if (itemWeightText != null)
            itemWeightText.text = $"무게: {def.Weight:0.##}";
    }

    private string GetRarityDisplayName(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "일반";
            case ItemRarity.Uncommon: return "고급";
            case ItemRarity.Rare: return "희귀";
            case ItemRarity.Epic: return "영웅";
            case ItemRarity.Legendary: return "전설";
            default: return rarity.ToString();
        }
    }
    private string GetCategoryDisplayName(ItemCategory itemCategory)
    {
        switch (itemCategory)
        {
            case ItemCategory.None: return "일반";
            case ItemCategory.Valuable: return "귀중품";
            case ItemCategory.Material: return "재료";
            case ItemCategory.Consumable: return "소모품";
            case ItemCategory.Equipment: return "장비";
            default: return itemCategory.ToString();
        }
    }

    private void InvokeAction(ItemContextActionType actionType)
    {
        OnActionSelected?.Invoke(actionType, currentBindData);

        if (actionType == ItemContextActionType.Close)
        {
            Hide();
            return;
        }

        // 일단 액션 후 닫기
        Hide();
    }
}