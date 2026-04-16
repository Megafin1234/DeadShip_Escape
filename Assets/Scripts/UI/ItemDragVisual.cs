using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDragVisual : MonoBehaviour  //드래그 중 아이템 아이콘이 마우스 따라다니게
{
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text stackCountText;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Hide();
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            rectTransform.position = Input.mousePosition;
        }
    }

    public void Show(ItemInstance itemInstance, ItemDefinitionBase itemDefinition)
    {
        if (itemInstance == null || itemDefinition == null)
            return;

        gameObject.SetActive(true);

        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = itemDefinition.Icon;
        }

        if (stackCountText != null)
        {
            if (itemInstance.StackCount > 1)
            {
                stackCountText.gameObject.SetActive(true);
                stackCountText.text = itemInstance.StackCount.ToString();
            }
            else
            {
                stackCountText.gameObject.SetActive(false);
            }
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}