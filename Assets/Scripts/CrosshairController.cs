using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public RectTransform crosshairRect;

    void Start()
    {
        if (crosshairRect != null)
        {
            crosshairRect.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (crosshairRect == null) return;

        bool isLeftPressed = Input.GetMouseButton(0);
        bool isRightPressed = Input.GetMouseButton(1);

        bool shouldShowCrosshair = isLeftPressed || isRightPressed;

        crosshairRect.gameObject.SetActive(shouldShowCrosshair);

        if (shouldShowCrosshair)
        {
            crosshairRect.position = Input.mousePosition;
        }
    }
}