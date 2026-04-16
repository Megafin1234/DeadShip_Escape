using UnityEngine;

public class StashInteractable : InteractableBase
{
    [Header("Dependencies")]
    [SerializeField] private SessionStateController sessionStateController;
    [SerializeField] private RaidOverlayUIController overlayUIController;

    protected override void Interact(Transform interactor)
    {
        if (sessionStateController == null || overlayUIController == null)
        {
            Debug.LogWarning("[StashInteractable] 필요한 참조가 연결되지 않았습니다.");
            return;
        }

        if (!sessionStateController.IsInBase)
        {
            Debug.Log("[StashInteractable] 거점 상태가 아니므로 창고를 열 수 없습니다.");
            return;
        }

        overlayUIController.ShowStashContext();
        Debug.Log("[StashInteractable] 창고 UI 열기");
    }
}