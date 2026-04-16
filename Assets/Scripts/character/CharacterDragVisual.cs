using UnityEngine;
using UnityEngine.UI;

public class CharacterDragVisual : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private Image portraitImage;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        Hide();
    }

    private void Update()
    {
        if (root != null && root.gameObject.activeSelf)
        {
            root.position = Input.mousePosition;
        }
    }

    public void Show(CharacterDefinition character)
    {
        if (character == null)
            return;

        if (portraitImage != null)
        {
            portraitImage.enabled = character.Portrait != null;
            portraitImage.sprite = character.Portrait;
        }

        if (root != null)
            root.gameObject.SetActive(true);

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    public void Hide()
    {
        if (root != null)
            root.gameObject.SetActive(false);

        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }
    }
}