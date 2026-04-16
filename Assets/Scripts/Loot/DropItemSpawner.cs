using UnityEngine;

public class DroppedItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject droppedItemPrefab;
    [SerializeField] private Transform droppedItemsRoot;

    public GameObject SpawnDroppedItem(
        Vector3 position,
        string definitionId,
        int stackCount,
        string displayName)
    {
        if (droppedItemPrefab == null)
        {
            Debug.LogWarning("[DroppedItemSpawner] droppedItemPrefab이 없습니다.");
            return null;
        }

        GameObject obj;

        if (droppedItemsRoot != null)
            obj = Instantiate(droppedItemPrefab, position, Quaternion.identity, droppedItemsRoot);
        else
            obj = Instantiate(droppedItemPrefab, position, Quaternion.identity);

        WorldDroppedItemInteractable dropped = obj.GetComponent<WorldDroppedItemInteractable>();
        if (dropped != null)
        {
            dropped.Initialize(definitionId, stackCount, displayName);
        }

        return obj;
    }
}