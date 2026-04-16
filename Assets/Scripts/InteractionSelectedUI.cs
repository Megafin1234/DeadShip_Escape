using UnityEngine;

public class InteractionSelectedUI : MonoBehaviour  // 상호작용 F를 띄워주는 UI
{
    [SerializeField] private RectTransform rectTransform;

    private Camera mainCam;
    private Transform target;
    private Vector3 worldOffset;

    public void Initialize(Camera cameraRef)
    {
        mainCam = cameraRef;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }

    public void Bind(Transform targetTransform, Vector3 offset)
    {
        target = targetTransform;
        worldOffset = offset;
        gameObject.SetActive(true);
    }

    public void Unbind()
    {
        target = null;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (target == null || mainCam == null || rectTransform == null)
            return;

        Vector3 screenPos = mainCam.WorldToScreenPoint(target.position + worldOffset);

        if (screenPos.z < 0f)
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
            return;
        }

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        rectTransform.position = screenPos;
    }
}