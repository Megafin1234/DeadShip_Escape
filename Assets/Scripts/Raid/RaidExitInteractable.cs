using System.Collections;
using UnityEngine;

public class RaidExitInteractable : InteractableBase
{
    [Header("Dependencies")]
    [SerializeField] private RaidSessionController raidSessionController;
    [SerializeField] private AreaTransitionController transitionController;
    [SerializeField] private RaidOverlayUIController overlayUIController;

    [Header("Extract Settings")]
    [SerializeField] private float extractDelay = 5f;

    private bool isExtracting = false;
    private Coroutine extractCoroutine;

    public override bool IsBusy()
    {
        return isExtracting;
    }

    protected override void Interact(Transform interactor)
    {
        if (isExtracting)
            return;

        if (raidSessionController == null)
        {
            Debug.LogWarning("[RaidExitInteractable] RaidSessionController가 연결되지 않았습니다.");
            return;
        }

        if (transitionController == null)
        {
            Debug.LogWarning("[RaidExitInteractable] AreaTransitionController가 연결되지 않았습니다.");
            return;
        }

        extractCoroutine = StartCoroutine(ExtractRoutine());
    }

    private IEnumerator ExtractRoutine()
    {
        isExtracting = true;
        Debug.Log($"[RaidExitInteractable] 탈출 시작... {extractDelay}초 대기");

        yield return new WaitForSeconds(extractDelay);

        if (overlayUIController != null && overlayUIController.IsAnyPanelOpen)
        {
            overlayUIController.CloseOverlay();
        }

        raidSessionController.CompleteRaidByExtraction();
        transitionController.MoveToBase();

        Debug.Log("[RaidExitInteractable] 레이드 -> 거점 전환 완료");

        isExtracting = false;
        extractCoroutine = null;
    }
}