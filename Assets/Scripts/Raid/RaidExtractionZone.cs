using TMPro;
using UnityEngine;

public class RaidExtractionZone : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private RaidSessionController raidSessionController;
    [SerializeField] private AreaTransitionController transitionController;
    [SerializeField] private RaidOverlayUIController overlayUIController;
    [SerializeField] private SessionStateController sessionStateController;

    [Header("UI")]
    [SerializeField] private TMP_Text extractTimerText;

    [Header("Settings")]
    [SerializeField] private float requiredStayTime = 5f;

    private float currentStayTime = 0f;
    private bool playerInside = false;
    private bool extractionTriggered = false;

    private void Awake()
    {
        HideTimerUI();
    }

    private void OnEnable()
    {
        ResetZoneState();
    }

    private void Update()
    {
        // 레이드 중이 아니면 작동 안 함
        if (sessionStateController != null && !sessionStateController.IsInRaid)
            return;

        if (!playerInside || extractionTriggered)
            return;

        currentStayTime += Time.deltaTime;

        float remain = Mathf.Max(0f, requiredStayTime - currentStayTime);
        UpdateTimerUI(remain);

        if (currentStayTime >= requiredStayTime)
        {
            TriggerExtraction();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        if (sessionStateController != null && !sessionStateController.IsInRaid)
            return;

        if (extractionTriggered)
            return;

        playerInside = true;
        currentStayTime = 0f;

        ShowTimerUI();
        UpdateTimerUI(requiredStayTime);

        Debug.Log("[RaidExtractionZone] 플레이어 진입 - 탈출 카운트 시작");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        if (extractionTriggered)
            return;

        playerInside = false;
        currentStayTime = 0f;

        HideTimerUI();

        Debug.Log("[RaidExtractionZone] 플레이어 이탈 - 탈출 카운트 리셋");
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        // 플레이어 루트에 태그가 달려 있어도 잡히게
        if (other.CompareTag("Player"))
            return true;

        if (other.transform.root != null && other.transform.root.CompareTag("Player"))
            return true;

        return false;
    }

    private void TriggerExtraction()
    {
        if (extractionTriggered)
            return;

        extractionTriggered = true;
        playerInside = false;
        HideTimerUI();

        Debug.Log("[RaidExtractionZone] 탈출 성공");

        if (overlayUIController != null && overlayUIController.IsAnyPanelOpen)
        {
            overlayUIController.CloseOverlay();
        }

        if (raidSessionController != null)
        {
            raidSessionController.CompleteRaidByExtraction();
        }
        else
        {
            Debug.LogWarning("[RaidExtractionZone] RaidSessionController가 연결되지 않았습니다.");
        }

        if (transitionController != null)
        {
            transitionController.MoveToBase();
        }
        else
        {
            Debug.LogWarning("[RaidExtractionZone] AreaTransitionController가 연결되지 않았습니다.");
        }
    }

    private void ShowTimerUI()
    {
        if (extractTimerText != null)
            extractTimerText.gameObject.SetActive(true);
    }

    private void HideTimerUI()
    {
        if (extractTimerText != null)
            extractTimerText.gameObject.SetActive(false);
    }

    private void UpdateTimerUI(float remain)
    {
        if (extractTimerText != null)
        {
            extractTimerText.text = $"탈출 중... {remain:F1}s";
        }
    }

    public void ResetZoneState()
    {
        currentStayTime = 0f;
        playerInside = false;
        extractionTriggered = false;
        HideTimerUI();
    }
}