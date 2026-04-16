using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class QuickSlotDropReceiver : MonoBehaviour, IDropHandler
{
    public Action<QuickSlotDropReceiver> OnDropped;

    private QuickSlotView quickSlotView;
    public QuickSlotView QuickSlotView => quickSlotView;

    private void Awake()
    {
        quickSlotView = GetComponent<QuickSlotView>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnDropped?.Invoke(this);
    }
}