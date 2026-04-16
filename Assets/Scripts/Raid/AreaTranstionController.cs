using System.Collections;
using UnityEngine;

public class AreaTransitionController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SessionStateController sessionStateController;
    [SerializeField] private RaidSessionController raidSessionController;

    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform companionTransform;
    [SerializeField] private Vector3 companionOffset = new Vector3(-1.2f, 0f, -0.8f);

    [Header("Spawn Points")]
    [SerializeField] private Transform baseSpawnPoint;
    [SerializeField] private Transform raidSpawnPoint;

    [Header("Transition UI")]
    [SerializeField] private ScreenFadeUI screenFadeUI;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.45f;
    [SerializeField] private float blackHoldDuration = 0.2f;

    private bool isTransitioning = false;

    public bool IsTransitioning => isTransitioning;

    public void MoveToRaid()
    {
        if (isTransitioning)
            return;

        StartCoroutine(MoveToRaidRoutine());
    }

    public void MoveToBase()
    {
        if (isTransitioning)
            return;

        StartCoroutine(MoveToBaseRoutine());
    }

    private IEnumerator MoveToRaidRoutine()
    {
        isTransitioning = true;

        // 1. 화면 암전
        if (screenFadeUI != null)
            yield return screenFadeUI.FadeOutRoutine(fadeDuration);

        // 2. 완전 암전 상태 잠깐 유지
        yield return new WaitForSeconds(blackHoldDuration);

        // 3. 공간 상태 변경
        if (sessionStateController != null)
            sessionStateController.EnterRaid();

        // 4. 플레이어 순간이동
        TeleportPlayer(raidSpawnPoint);

        // 5. 이동 직후도 잠깐 블랙 유지
        yield return new WaitForSeconds(blackHoldDuration);

        // 6. 화면 복귀
        if (screenFadeUI != null)
            yield return screenFadeUI.FadeInRoutine(fadeDuration);

        isTransitioning = false;
    }

    private IEnumerator MoveToBaseRoutine()
    {
        isTransitioning = true;

        // 1. 화면 암전
        if (screenFadeUI != null)
            yield return screenFadeUI.FadeOutRoutine(fadeDuration);

        // 2. 완전 암전 상태 잠깐 유지
        yield return new WaitForSeconds(blackHoldDuration);

        // 3. 공간 상태 변경
        if (sessionStateController != null)
            sessionStateController.EnterBase();

        // 4. 플레이어 순간이동
        TeleportPlayer(baseSpawnPoint);

        // 5. 복귀 직후도 잠깐 블랙 유지
        yield return new WaitForSeconds(blackHoldDuration);

        // 6. 복귀 후처리
        if (raidSessionController != null)
            raidSessionController.HandlePostReturnToBase();

        // 7. 화면 복귀
        if (screenFadeUI != null)
            yield return screenFadeUI.FadeInRoutine(fadeDuration);

        isTransitioning = false;
    }

    private void TeleportPlayer(Transform targetPoint)
    {
        if (playerTransform == null || targetPoint == null)
        {
            Debug.LogWarning("[AreaTransitionController] playerTransform 또는 targetPoint가 비어 있습니다.");
            return;
        }

        CharacterController cc = playerTransform.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        // 필요하면 Y를 아주 약간 올려도 됨
        playerTransform.position = targetPoint.position;
        playerTransform.rotation = targetPoint.rotation;

        if (cc != null)
            cc.enabled = true;

        if (companionTransform != null && companionTransform.gameObject.activeSelf)
        {
            CharacterController companionCC = companionTransform.GetComponent<CharacterController>();
            if (companionCC != null)
                companionCC.enabled = false;

            Vector3 companionTargetPos = targetPoint.position + targetPoint.rotation * companionOffset;
            companionTransform.position = companionTargetPos;
            companionTransform.rotation = targetPoint.rotation;

            if (companionCC != null)
                companionCC.enabled = true;
        }
    }
}