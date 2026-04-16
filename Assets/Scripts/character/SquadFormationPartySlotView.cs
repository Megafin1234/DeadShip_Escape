using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SquadFormationPartySlotView : MonoBehaviour,
    IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private PositionIndex positionIndex;

    private CharacterDefinition currentCharacter;
    private CanvasGroup canvasGroup;
    private float lastClickTime = 0f;
    private const float DoubleClickThreshold = 0.25f;

    public PositionIndex PositionIndex => positionIndex;
    public CharacterDefinition CurrentCharacter => currentCharacter;

    public Action<SquadFormationPartySlotView> OnDroppedOn;
    public Action<SquadFormationPartySlotView> OnBeginDragEvent;
    public Action<SquadFormationPartySlotView> OnEndDragEvent;
    public Action<SquadFormationPartySlotView> OnDoubleClickEvent;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        RefreshView(null);
    }

    public void RefreshView(CharacterDefinition character)
    {
        currentCharacter = character;

        if (portraitImage != null)
        {
            if (currentCharacter != null && currentCharacter.Portrait != null)
            {
                portraitImage.enabled = true;
                portraitImage.sprite = currentCharacter.Portrait;
            }
            else
            {
                portraitImage.sprite = null;
                portraitImage.enabled = false;
            }
        }

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnDroppedOn?.Invoke(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (currentCharacter == null)
            return;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        OnBeginDragEvent?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 비워둬도 됨
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        OnEndDragEvent?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentCharacter == null)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (Time.time - lastClickTime <= DoubleClickThreshold)
        {
            OnDoubleClickEvent?.Invoke(this);
        }

        lastClickTime = Time.time;
    }
}