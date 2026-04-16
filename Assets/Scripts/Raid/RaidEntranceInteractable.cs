using UnityEngine;

public class RaidEntranceInteractable : InteractableBase
{
    [Header("Dependencies")]
    [SerializeField] private RaidSessionController raidSessionController;
    [SerializeField] private AreaTransitionController transitionController;
    [SerializeField] private RaidOverlayUIController overlayUIController;

    [Header("Options")]
    [SerializeField] private bool openOverlayAfterEnter = false;

    protected override void Interact(Transform interactor)
    {
        if (raidSessionController == null)
        {
            Debug.LogWarning("[RaidEntranceInteractable] RaidSessionController가 연결되지 않았습니다.");
            return;
        }

        if (transitionController == null)
        {
            Debug.LogWarning("[RaidEntranceInteractable] AreaTransitionController가 연결되지 않았습니다.");
            return;
        }

        raidSessionController.StartRaidSession();
        transitionController.MoveToRaid();

        Debug.Log("[RaidEntranceInteractable] 거점 -> 레이드 전환");

        if (openOverlayAfterEnter && overlayUIController != null)
        {
            overlayUIController.OpenDefaultOverlay();
        }
    }
}