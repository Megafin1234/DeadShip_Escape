using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SquadFormationRosterSlotView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IDropHandler
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private CharacterDefinition characterDefinition;

    private CanvasGroup canvasGroup;
    private float lastClickTime = 0f;
    private const float DoubleClickThreshold = 0.25f;

    public CharacterDefinition CharacterDefinition => characterDefinition;

    public Action<SquadFormationRosterSlotView> OnBeginDragEvent;
    public Action<SquadFormationRosterSlotView> OnEndDragEvent;
    public Action<SquadFormationRosterSlotView> OnDoubleClickEvent;
    public Action<SquadFormationRosterSlotView> OnDroppedOn;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        RefreshView();
    }

    public void RefreshView()
    {
        if (portraitImage != null)
        {
            if (characterDefinition != null && characterDefinition.Portrait != null)
            {
                portraitImage.enabled = true;
                portraitImage.sprite = characterDefinition.Portrait;
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

    public void SetCharacter(CharacterDefinition character)
    {
        characterDefinition = character;
        RefreshView();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (characterDefinition == null)
            return;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        OnBeginDragEvent?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        OnEndDragEvent?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (characterDefinition == null)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (Time.time - lastClickTime <= DoubleClickThreshold)
        {
            OnDoubleClickEvent?.Invoke(this);
        }

        lastClickTime = Time.time;
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnDroppedOn?.Invoke(this);
    }
}