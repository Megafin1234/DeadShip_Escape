using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlotDropReceiver : MonoBehaviour, IDropHandler  //드롭을 받을 수 있는 슬롯임을 체크
{
    public Action<ItemSlotDropReceiver> OnDropped;

    private ItemSlotView slotView;
    public ItemSlotView SlotView => slotView;

    private void Awake()
    {
        slotView = GetComponent<ItemSlotView>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnDropped?.Invoke(this);
    }
}