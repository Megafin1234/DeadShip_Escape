using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlotView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text stackCountText;
    [SerializeField] private TMP_Text hotkeyText;
    [SerializeField] private GameObject emptyStateObject;

    private int slotIndex;
    public int SlotIndex => slotIndex;

    public Action<QuickSlotView> OnDroppedOn;
    public Action<QuickSlotView> OnRightClick;

    private void Awake()
    {
        QuickSlotDropReceiver receiver = GetComponent<QuickSlotDropReceiver>();
        if (receiver == null)
            receiver = gameObject.AddComponent<QuickSlotDropReceiver>();

        receiver.OnDropped += HandleDropped;
    }

    private void HandleDropped(QuickSlotDropReceiver receiver)
    {
        OnDroppedOn?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick?.Invoke(this);
        }
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;

        if (hotkeyText != null)
            hotkeyText.text = (index + 1).ToString();
    }

    public void Bind(ItemInstance itemInstance, ItemDefinitionBase itemDefinition)
    {
        bool hasItem = itemInstance != null && itemDefinition != null;

        if (iconImage != null)
        {
            iconImage.enabled = hasItem;
            iconImage.sprite = hasItem ? itemDefinition.Icon : null;
        }

        if (stackCountText != null)
        {
            if (hasItem && itemInstance.StackCount > 1)
            {
                stackCountText.gameObject.SetActive(true);
                stackCountText.text = itemInstance.StackCount.ToString();
            }
            else
            {
                stackCountText.gameObject.SetActive(false);
            }
        }

        if (emptyStateObject != null)
            emptyStateObject.SetActive(!hasItem);
    }

    public void Clear()
    {
        Bind(null, null);
    }
}