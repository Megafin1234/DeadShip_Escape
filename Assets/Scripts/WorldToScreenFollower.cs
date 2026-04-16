using UnityEngine;

public class WorldToScreenUIFollower : MonoBehaviour
{
    public Transform target;
    public Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    public Vector2 screenOffset = new Vector2(60f, 0f);

    private RectTransform rectTransform;
    private Camera mainCamera;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null || mainCamera == null) return;

        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        rectTransform.position = screenPos + (Vector3)screenOffset;
    }
}