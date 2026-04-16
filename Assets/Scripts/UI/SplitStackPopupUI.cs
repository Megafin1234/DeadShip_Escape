using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SplitStackPopupUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_InputField amountInputField;
    [SerializeField] private Button splitHalfButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Dependencies")]
    [SerializeField] private PlayerSquadBridge bridge;
    [SerializeField] private RaidOverlayUIController raidOverlayUIController;

    private ItemInstance pendingSourceItem;
    private ContainerState pendingSourceContainer;
    private SlotState pendingSourceSlot;
    private ContainerState pendingTargetContainer;
    private SlotState pendingTargetSlot;

    private void Awake()
    {
        HideImmediate();

        if (splitHalfButton != null)
            splitHalfButton.onClick.AddListener(HandleSplitHalf);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(HandleConfirmAmount);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(Hide);
    }

    private void Update()
    {
        if (root != null && root.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Hide();
    }

    public void Show(
        ItemInstance sourceItem,
        ContainerState sourceContainer,
        SlotState sourceSlot,
        ContainerState targetContainer,
        SlotState targetSlot)
    {
        pendingSourceItem = sourceItem;
        pendingSourceContainer = sourceContainer;
        pendingSourceSlot = sourceSlot;
        pendingTargetContainer = targetContainer;
        pendingTargetSlot = targetSlot;

        if (titleText != null)
            titleText.text = "분할할 수량을 선택하세요";

        if (amountInputField != null)
            amountInputField.text = "수량:";

        if (root != null)
            root.SetActive(true);

        if (bridge != null)
            bridge.SetModalBlocker("SplitPopup", true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        if (bridge != null)
            bridge.SetModalBlocker("SplitPopup", false);

        ClearPending();
    }

    private void HideImmediate()
    {
        if (root != null)
            root.SetActive(false);

        if (bridge != null)
            bridge.SetModalBlocker("SplitPopup", false);

        ClearPending();
    }

    private void HandleSplitHalf()
    {
        if (pendingSourceItem == null || bridge == null)
            return;

        int splitAmount = pendingSourceItem.StackCount / 2;
        if (splitAmount <= 0)
            return;

        ItemMoveResult result = bridge.TrySplitItemToTargetSlotWithAmount(
            pendingSourceItem,
            pendingSourceContainer,
            pendingSourceSlot,
            pendingTargetContainer,
            pendingTargetSlot,
            splitAmount
        );

        Debug.Log($"[SplitPopup] 절반 분할 결과: {result.Message}");

        if (result.Success && raidOverlayUIController != null)
        {
            raidOverlayUIController.RefreshAll();
        }

        Hide();
    }

    private void HandleConfirmAmount()
    {
        if (pendingSourceItem == null || amountInputField == null || bridge == null)
            return;

        if (!int.TryParse(amountInputField.text, out int splitAmount))
            return;

        ItemMoveResult result = bridge.TrySplitItemToTargetSlotWithAmount(
            pendingSourceItem,
            pendingSourceContainer,
            pendingSourceSlot,
            pendingTargetContainer,
            pendingTargetSlot,
            splitAmount
        );

        Debug.Log($"[SplitPopup] 수량 분할 결과: {result.Message}");

        if (result.Success && raidOverlayUIController != null)
        {
            raidOverlayUIController.RefreshAll();
        }

        Hide();
    }

    private void ClearPending()
    {
        pendingSourceItem = null;
        pendingSourceContainer = null;
        pendingSourceSlot = null;
        pendingTargetContainer = null;
        pendingTargetSlot = null;
    }
}