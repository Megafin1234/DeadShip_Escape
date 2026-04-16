using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레이드 한 판의 시작/종료/결과 처리를 담당.
/// 
/// 역할:
/// - 레이드 시작 시 세션 시작
/// - 탈출 성공 처리
/// - 사망 실패 처리
/// - 거점 복귀 후 후처리
/// - 결과 요약 데이터 생성
/// 
/// 주의:
/// - 현재 위치(Base/Raid)는 SessionStateController가 담당
/// - 실제 이동은 AreaTransitionController가 담당
/// - 실제 스쿼드/아이템 처리는 PlayerSquadBridge가 담당
/// </summary>
public class RaidSessionController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerSquadBridge bridge;
    [SerializeField] private SessionStateController sessionStateController;
    [SerializeField] private AreaTransitionController transitionController;
    [SerializeField] private RaidWorldResetController raidWorldResetController;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private RaidExtractionZone raidExtractionZone;

    [Header("UI")]
    [SerializeField] private RaidResultPanelUI raidResultPanelUI;

    [Header("Runtime")]
    [SerializeField] private bool isRaidRunning = false;
    [SerializeField] private RaidSessionResultType lastResult = RaidSessionResultType.None;

    [Header("Wipe Settings")]
    [SerializeField] private float wipeReturnDelay = 1.0f;

    [Header("Result Summary")]
    [SerializeField] private RaidResultSummary currentSummary = new RaidResultSummary();

    private bool wipeRoutineRunning = false;

    // 레이드 시작 시점 소지품 스냅샷
    private List<string> raidStartItemSnapshot = new List<string>();

    // 손실 처리 직전 스냅샷 (wipe 계산용)
    private List<string> preResolutionItemSnapshot = new List<string>();

    public bool IsRaidRunning => isRaidRunning;
    public RaidSessionResultType LastResult => lastResult;
    public RaidResultSummary CurrentSummary => currentSummary;

    private void Update()
    {
        if (!isRaidRunning || wipeRoutineRunning)
            return;

        if (sessionStateController == null || !sessionStateController.IsInRaid)
            return;

        if (bridge == null || bridge.Squad == null)
            return;

        if (bridge.Squad.CombatState.IsDead)
        {
            StartCoroutine(HandleWipeRoutine());
        }
    }

    public void StartRaidSession()
    {
        if (isRaidRunning)
        {
            Debug.LogWarning("[RaidSessionController] 이미 레이드가 진행 중입니다.");
            return;
        }

        isRaidRunning = true;
        wipeRoutineRunning = false;
        lastResult = RaidSessionResultType.None;
        currentSummary.Clear();

        Debug.Log("[RaidSessionController] 레이드 세션 시작");

        // 레이드 시작 시점 스냅샷 저장
        CaptureRaidStartSnapshot();

        // 월드 초기화
        if (raidWorldResetController != null)
        {
            raidWorldResetController.ResetRaidWorld();
        }

        // 브리지 레이드 진입 처리
        if (bridge != null)
        {
            bridge.EnterRaid();
        }

        if (raidExtractionZone != null)
        {
            raidExtractionZone.ResetZoneState();
        }
    }

    public void CompleteRaidByExtraction()
    {
        if (!isRaidRunning)
        {
            Debug.LogWarning("[RaidSessionController] 진행 중인 레이드가 없습니다.");
            return;
        }

        // 종료 직전 스냅샷
        CapturePreResolutionSnapshot();

        lastResult = RaidSessionResultType.Extracted;
        isRaidRunning = false;
        wipeRoutineRunning = false;

        Debug.Log("[RaidSessionController] 레이드 탈출 성공");

        if (bridge != null)
        {
            bridge.Extract();
        }

        BuildExtractionSummary();
    }

    public void CompleteRaidByWipe()
    {
        if (!isRaidRunning)
        {
            Debug.LogWarning("[RaidSessionController] 진행 중인 레이드가 없습니다.");
            return;
        }

        // 손실 처리 전 스냅샷
        CapturePreResolutionSnapshot();

        lastResult = RaidSessionResultType.Wiped;
        isRaidRunning = false;
        wipeRoutineRunning = false;

        Debug.Log("[RaidSessionController] 레이드 실패(사망)");

        if (bridge != null)
        {
            bridge.ApplyRaidWipeLoss();
        }

        BuildWipeSummary();
    }

    public void HandlePostReturnToBase()
    {
        Debug.Log($"[RaidSessionController] 거점 복귀 완료 / 결과 = {lastResult}");

        if (bridge != null)
        {
            bridge.RecoverAfterReturnToBase();
        }
        CleanupEnemyLoot();

        if (lastResult == RaidSessionResultType.Extracted)
        {
            if (questManager != null)
            {
                questManager.PublishEvent(new QuestEvent(QuestEventType.Extract, "", 1));
            }
        }
        ShowResultUI();
    }

    private void CleanupEnemyLoot()
    {
        GameObject[] lootObjects = GameObject.FindGameObjectsWithTag("EnemyLoot");

        for (int i = 0; i < lootObjects.Length; i++)
        {
            Destroy(lootObjects[i]);
        }

        Debug.Log($"[Raid] Enemy Loot 정리 완료: {lootObjects.Length}개 제거");
    }
    
    private IEnumerator HandleWipeRoutine()
    {
        wipeRoutineRunning = true;

        Debug.Log($"[RaidSessionController] 사망 감지 / {wipeReturnDelay}초 뒤 거점 복귀");

        yield return new WaitForSeconds(wipeReturnDelay);

        CompleteRaidByWipe();

        if (transitionController != null)
            transitionController.MoveToBase();
    }

    private void CaptureRaidStartSnapshot()
    {
        raidStartItemSnapshot.Clear();

        if (bridge == null)
            return;

        raidStartItemSnapshot = bridge.GetCurrentRaidCarryItemNames();
    }

    private void CapturePreResolutionSnapshot()
    {
        preResolutionItemSnapshot.Clear();

        if (bridge == null)
            return;

        preResolutionItemSnapshot = bridge.GetCurrentRaidCarryItemNames();
    }

    private void BuildExtractionSummary()
    {
        currentSummary.Clear();
        currentSummary.ResultType = RaidSessionResultType.Extracted;

        if (bridge == null)
            return;

        List<string> currentItems = bridge.GetCurrentRaidCarryItemNames();

        // 시작 시점 대비 새로 생긴 아이템 = gained
        List<string> tempStart = new List<string>(raidStartItemSnapshot);

        for (int i = 0; i < currentItems.Count; i++)
        {
            string currentName = currentItems[i];

            int foundIndex = tempStart.IndexOf(currentName);
            if (foundIndex >= 0)
            {
                tempStart.RemoveAt(foundIndex);
            }
            else
            {
                currentSummary.GainedItemNames.Add(currentName);
            }
        }

        Debug.Log($"[RaidSessionController] Extraction Summary / gained={currentSummary.GainedItemCount}, lost={currentSummary.LostItemCount}");
    }

    private void BuildWipeSummary()
    {
        currentSummary.Clear();
        currentSummary.ResultType = RaidSessionResultType.Wiped;

        if (bridge == null)
            return;

        List<string> currentItemsAfterLoss = bridge.GetCurrentRaidCarryItemNames();

        // 1. 시작 시점 대비 현재까지 남아 있는 "새로 얻은 아이템" 계산
        List<string> tempStartForGain = new List<string>(raidStartItemSnapshot);

        for (int i = 0; i < currentItemsAfterLoss.Count; i++)
        {
            string currentName = currentItemsAfterLoss[i];

            int foundIndex = tempStartForGain.IndexOf(currentName);
            if (foundIndex >= 0)
            {
                tempStartForGain.RemoveAt(foundIndex);
            }
            else
            {
                currentSummary.GainedItemNames.Add(currentName);
            }
        }

        // 2. 손실 처리 전 스냅샷 대비 사라진 아이템 = lost
        List<string> tempCurrent = new List<string>(currentItemsAfterLoss);

        for (int i = 0; i < preResolutionItemSnapshot.Count; i++)
        {
            string beforeName = preResolutionItemSnapshot[i];

            int foundIndex = tempCurrent.IndexOf(beforeName);
            if (foundIndex >= 0)
            {
                tempCurrent.RemoveAt(foundIndex);
            }
            else
            {
                currentSummary.LostItemNames.Add(beforeName);
            }
        }

        Debug.Log($"[RaidSessionController] Wipe Summary / gained={currentSummary.GainedItemCount}, lost={currentSummary.LostItemCount}");
    }

    private void ShowResultUI()
    {
        Debug.Log($"[RaidSessionController] ShowResultUI 호출 / panel null? {raidResultPanelUI == null}");

        if (raidResultPanelUI == null)
            return;

        raidResultPanelUI.ShowSummary(currentSummary);
    }
}