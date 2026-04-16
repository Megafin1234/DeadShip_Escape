using UnityEngine;
using UnityEngine.UI;

public class LossProtectionPanelHighlight : MonoBehaviour
{
    [Header("Optional Visuals")]
    [SerializeField] private GameObject highlightRoot;
    [SerializeField] private Image borderImage;
    [SerializeField] private GameObject badgeRoot;

    [Header("Colors")]
    [SerializeField] private Color activeBorderColor = new Color(1f, 0.95f, 0.6f, 1f);
    [SerializeField] private Color inactiveBorderColor = new Color(1f, 1f, 1f, 0.15f);

    [SerializeField] private bool hideWhenInactive = true;

    public void SetHighlighted(bool isHighlighted)
    {
        if (highlightRoot != null)
        {
            if (hideWhenInactive)
                highlightRoot.SetActive(isHighlighted);
            else
                highlightRoot.SetActive(true);
        }

        if (borderImage != null)
        {
            borderImage.color = isHighlighted ? activeBorderColor : inactiveBorderColor;
        }

        if (badgeRoot != null)
        {
            badgeRoot.SetActive(isHighlighted);
        }
    }
}