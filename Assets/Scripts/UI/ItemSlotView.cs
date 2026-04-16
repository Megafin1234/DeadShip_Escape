using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotView : MonoBehaviour, 
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text stackCountText;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private GameObject selectedFrame;
    [SerializeField] private GameObject favoriteMarkIcon;
    [SerializeField] private GameObject questMarkIcon;

    private SlotBindData bindData;
    private CanvasGroup canvasGroup;

    public SlotBindData BindData => bindData;

    public Action<ItemSlotView> OnDoubleClick;
    public Action<ItemSlotView> OnRightClick;
    public Action<ItemSlotView> OnLeftClick;
    public Action<ItemSlotView> OnBeginDragEvent;
    public Action<ItemSlotView> OnEndDragEvent;

    private float lastClickTime;
    public bool wasDragged;
    private const float DoubleClickThreshold = 0.25f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Bind(SlotBindData data)
    {
        bindData = data;

        bool hasItem = data.ItemInstance != null && data.ItemDefinition != null;
        bool isLocked = data.IsLocked;

        if (iconImage != null)
        {
            iconImage.enabled = hasItem && !isLocked;
            iconImage.sprite = hasItem ? data.ItemDefinition.Icon : null;
        }

        if (stackCountText != null)
        {
            if (hasItem && data.ItemInstance.StackCount > 1 && !isLocked)
            {
                stackCountText.gameObject.SetActive(true);
                stackCountText.text = data.ItemInstance.StackCount.ToString();
            }
            else
            {
                stackCountText.gameObject.SetActive(false);
            }
        }

        if (lockOverlay != null)
            lockOverlay.SetActive(isLocked);

        if (selectedFrame != null)
            selectedFrame.SetActive(false);
        
        if (favoriteMarkIcon != null)
            favoriteMarkIcon.SetActive(data.IsFavoriteMarked);

        if (questMarkIcon != null)
            questMarkIcon.SetActive(data.IsQuestMarked);

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts=true;
    }

    public void SetSelected(bool selected)
    {
        if (selectedFrame != null)
            selectedFrame.SetActive(selected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (wasDragged)
        {
            wasDragged = false;
            return;
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick?.Invoke(this);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Time.time - lastClickTime < DoubleClickThreshold)
            {
                OnDoubleClick?.Invoke(this);
            }
            /*else
            {
                OnLeftClick?.Invoke(this);
            }*/
            lastClickTime = Time.time;
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (bindData == null || bindData.ItemInstance == null || bindData.IsLocked)
            return;
        wasDragged = true;

        if (canvasGroup != null)
        canvasGroup.blocksRaycasts = false;

        OnBeginDragEvent?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 실제 마우스 추적은 DragVisual이 담당하므로 여기선 비워도 됨
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        if (canvasGroup != null)
        canvasGroup.blocksRaycasts = true;

        OnEndDragEvent?.Invoke(this);
    }

    private void OnDisable()
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }
}