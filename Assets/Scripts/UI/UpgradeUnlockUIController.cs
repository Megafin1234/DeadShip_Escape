using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUnlockUIController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Dependencies")]
    [SerializeField] private PlayerSquadBridge bridge;
    [SerializeField] private RaidOverlayUIController raidOverlayUIController;

    [Header("Buttons")]
    [SerializeField] private Button unlockInventoryP1Button;
    [SerializeField] private Button unlockInventoryP2Button;
    [SerializeField] private Button unlockInventoryP3Button;
    [SerializeField] private Button unlockStashButton;
    [SerializeField] private Button closeButton;

    [Header("Unlock Settings")]
    [SerializeField] private int inventoryUnlockStep = 4;
    [SerializeField] private int stashUnlockStep = 8;

    [Header("Costs")]
    [SerializeField] private List<UnlockCost> inventoryP1Costs = new();
    [SerializeField] private List<UnlockCost> inventoryP2Costs = new();
    [SerializeField] private List<UnlockCost> inventoryP3Costs = new();
    [SerializeField] private List<UnlockCost> stashCosts = new();

    [Header("Cost Displays")]
    [SerializeField] private UnlockCostDisplayUI inventoryP1CostDisplay;
    [SerializeField] private UnlockCostDisplayUI inventoryP2CostDisplay;
    [SerializeField] private UnlockCostDisplayUI inventoryP3CostDisplay;
    [SerializeField] private UnlockCostDisplayUI stashCostDisplay;

    private void Awake()
    {
        HideImmediate();
        BindButtons();
    }

    private void Update()
    {
        if (root == null || !root.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            Hide();
    }

    private void BindButtons()
    {
        if (unlockInventoryP1Button != null)
            unlockInventoryP1Button.onClick.AddListener(() => TryUnlockInventory(PositionIndex.Position1, inventoryP1Costs));

        if (unlockInventoryP2Button != null)
            unlockInventoryP2Button.onClick.AddListener(() => TryUnlockInventory(PositionIndex.Position2, inventoryP2Costs));

        if (unlockInventoryP3Button != null)
            unlockInventoryP3Button.onClick.AddListener(() => TryUnlockInventory(PositionIndex.Position3, inventoryP3Costs));

        if (unlockStashButton != null)
            unlockStashButton.onClick.AddListener(() => TryUnlockStash(stashCosts));

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);

        if (bridge != null)
            bridge.SetModalBlocker("UpgradeUnlock", true);

        RefreshButtons();
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        if (bridge != null)
            bridge.SetModalBlocker("UpgradeUnlock", false);
    }

    private void HideImmediate()
    {
        if (root != null)
            root.SetActive(false);

        if (bridge != null)
            bridge.SetModalBlocker("UpgradeUnlock", false);
    }

    private void TryUnlockInventory(PositionIndex positionIndex, List<UnlockCost> costs)
    {
        if (bridge == null)
            return;

        if (!bridge.CanUnlockInventorySlots(positionIndex, inventoryUnlockStep))
        {
            Debug.LogWarning($"[UpgradeUnlockUI] 더 이상 해금 가능한 슬롯이 없습니다. / {positionIndex}");
            RefreshButtons();
            return;
        }

        if (!bridge.HasEnoughUnlockCosts(costs))
        {
            Debug.LogWarning($"[UpgradeUnlockUI] 재료 부족 - {positionIndex}");
            RefreshButtons();
            return;
        }

        bool consumed = bridge.ConsumeUnlockCosts(costs);
        if (!consumed)
        {
            Debug.LogWarning($"[UpgradeUnlockUI] 재료 차감 실패 - {positionIndex}");
            RefreshButtons();
            return;
        }

        bool success = bridge.UnlockInventorySlots(positionIndex, inventoryUnlockStep);
        Debug.Log($"[UpgradeUnlockUI] 인벤 해금 {positionIndex} 결과 = {success}");

        if (!success)
        {
            // 해금 실패 시 차감 롤백은 1차에서는 생략하고, 실제론 이 상황이 안 나오게 CanUnlock으로 막음
            Debug.LogWarning("[UpgradeUnlockUI] 슬롯 해금 실패");
        }

        if (success && raidOverlayUIController != null)
            raidOverlayUIController.RefreshAll();

        RefreshButtons();
    }

    private void TryUnlockStash(List<UnlockCost> costs)
    {
        if (bridge == null)
            return;

        if (!bridge.CanUnlockStashSlots(stashUnlockStep))
        {
            Debug.LogWarning("[UpgradeUnlockUI] 더 이상 해금 가능한 창고 슬롯이 없습니다.");
            RefreshButtons();
            return;
        }

        if (!bridge.HasEnoughUnlockCosts(costs))
        {
            Debug.LogWarning("[UpgradeUnlockUI] 창고 해금 재료 부족");
            RefreshButtons();
            return;
        }

        bool consumed = bridge.ConsumeUnlockCosts(costs);
        if (!consumed)
        {
            Debug.LogWarning("[UpgradeUnlockUI] 창고 해금 재료 차감 실패");
            RefreshButtons();
            return;
        }

        bool success = bridge.UnlockStashSlots(stashUnlockStep);
        Debug.Log($"[UpgradeUnlockUI] 창고 해금 결과 = {success}");

        if (success && raidOverlayUIController != null)
            raidOverlayUIController.RefreshAll();

        RefreshButtons();
    }

    private void RefreshButtons()
    {
        if (bridge == null)
            return;

        if (unlockInventoryP1Button != null)
        {
            unlockInventoryP1Button.interactable =
                bridge.CanUnlockInventorySlots(PositionIndex.Position1, inventoryUnlockStep) &&
                bridge.HasEnoughUnlockCosts(inventoryP1Costs);
        }

        if (unlockInventoryP2Button != null)
        {
            unlockInventoryP2Button.interactable =
                bridge.CanUnlockInventorySlots(PositionIndex.Position2, inventoryUnlockStep) &&
                bridge.HasEnoughUnlockCosts(inventoryP2Costs);
        }

        if (unlockInventoryP3Button != null)
        {
            unlockInventoryP3Button.interactable =
                bridge.CanUnlockInventorySlots(PositionIndex.Position3, inventoryUnlockStep) &&
                bridge.HasEnoughUnlockCosts(inventoryP3Costs);
        }

        if (unlockStashButton != null)
        {
            unlockStashButton.interactable =
                bridge.CanUnlockStashSlots(stashUnlockStep) &&
                bridge.HasEnoughUnlockCosts(stashCosts);
        }

        RefreshCostDisplays();
    }

    private void RefreshCostDisplays()
    {
        RefreshSingleCostDisplay(inventoryP1Costs, inventoryP1CostDisplay);
        RefreshSingleCostDisplay(inventoryP2Costs, inventoryP2CostDisplay);
        RefreshSingleCostDisplay(inventoryP3Costs, inventoryP3CostDisplay);
        RefreshSingleCostDisplay(stashCosts, stashCostDisplay);
    }

    private void RefreshSingleCostDisplay(List<UnlockCost> costs, UnlockCostDisplayUI display)
    {
        if (display == null)
            return;

        if (bridge == null || costs == null || costs.Count == 0 || costs[0] == null)
        {
            display.Hide();
            return;
        }

        UnlockCost cost = costs[0];
        ItemDefinitionBase itemDef = bridge.GetItemDefinition(cost.itemDefinitionId);
        int owned = bridge.GetOwnedItemCount(cost.itemDefinitionId);

        display.Bind(itemDef, cost.amount, owned);
    }
}