using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockCostDisplayUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text costText;

    public void Bind(ItemDefinitionBase itemDef, int requiredAmount, int ownedAmount)
    {
        if (itemDef == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (iconImage != null)
        {
            iconImage.enabled = itemDef.Icon != null;
            iconImage.sprite = itemDef.Icon;
        }

        if (costText != null)
        {
            costText.text = $"{itemDef.DisplayName} x{requiredAmount} ({ownedAmount})";
            costText.color = ownedAmount >= requiredAmount
                ? Color.white
                : new Color(1f, 0.45f, 0.45f, 1f);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}