using UnityEngine;

public class QuestInteractable : InteractableBase
{
    [Header("Dependencies")]
    [SerializeField] private SessionStateController sessionStateController;
    [SerializeField] private RaidOverlayUIController overlayUIController;

    protected override void Interact(Transform interactor)
    {
        if (sessionStateController == null || overlayUIController == null)
        {
            Debug.LogWarning("[QuestInteractable] 필요한 참조가 연결되지 않았습니다.");
            return;
        }

        if (!sessionStateController.IsInBase)
        {
            Debug.Log("[QuestInteractable] 거점 상태가 아니므로 퀘스트 창을 열 수 없습니다.");
            return;
        }

        overlayUIController.ShowQuestContext();
        Debug.Log("[QuestInteractable] 퀘스트 UI 열기");
    }
}