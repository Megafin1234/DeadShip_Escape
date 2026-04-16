using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Search")]
    [SerializeField] private float searchRadius = 3.0f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Selection")]
    [SerializeField] private float forwardWeight = 0.35f;
    [SerializeField] private float baseInputCooldown = 0.15f;

    [Header("Indicator UI")]
    [SerializeField] private InteractionDotUI dotPrefab;
    [SerializeField] private RectTransform dotCanvasRoot;
    [SerializeField] private Vector3 indicatorOffset = new Vector3(0f, 0.35f, 0f);

    [Header("Selected Prompt UI")]
    [SerializeField] private InteractionSelectedUI selectedPromptUI;
    [SerializeField] private Vector3 selectedPromptOffset = new Vector3(0f, 0.35f, 0f);

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool logEveryFrame = false;

    private Camera mainCam;
    private float interactionLockTimer = 0f;

    private readonly List<InteractableBase> visibleInteractables = new();
    private readonly List<InteractionDotUI> dotPool = new();

    private InteractableBase currentBestTarget;
    private string lastPromptState = "";
    private PlayerSquadBridge bridge;
    public void SetBridge(PlayerSquadBridge squadBridge)
    {
        bridge = squadBridge;
    }

    private void Start()
    {
        mainCam = Camera.main;

        if (enableDebugLog)
        {
            Debug.Log($"[PlayerInteraction] Start - mainCam: {(mainCam != null ? mainCam.name : "NULL")}");
            Debug.Log($"[PlayerInteraction] selectedPromptUI: {(selectedPromptUI != null ? selectedPromptUI.name : "NULL")}");
        }

        if (selectedPromptUI != null)
        {
            selectedPromptUI.Initialize(mainCam);
            selectedPromptUI.Unbind();

            if (enableDebugLog)
            {
                Debug.Log("[PlayerInteraction] selectedPromptUI Initialize + Unbind 완료");
            }
        }
        else if (enableDebugLog)
        {
            Debug.LogWarning("[PlayerInteraction] selectedPromptUI가 Inspector에 연결되지 않았습니다.");
        }
    }

    private void Update()
    {
        if (bridge != null && !bridge.CanPlayerInteract())
        {
            HideAllInteractionUI();
            return;
        }
        UpdateInteractionLock();
        RefreshInteractables();
        RefreshDots();
        UpdateBestTarget();
        RefreshSelectedPrompt();
        HandleInteractionInput();

        if (enableDebugLog && logEveryFrame)
        {
            Debug.Log(
                $"[PlayerInteraction][Frame] visible={visibleInteractables.Count}, " +
                $"best={(currentBestTarget != null ? currentBestTarget.name : "NULL")}, " +
                $"lock={interactionLockTimer:F2}"
            );
        }
        
    }

    private void UpdateInteractionLock()
    {
        if (interactionLockTimer > 0f)
            interactionLockTimer -= Time.deltaTime;
    }

    private void RefreshInteractables()
    {
        visibleInteractables.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, interactableLayer);
        HashSet<InteractableBase> uniqueSet = new();

        for (int i = 0; i < hits.Length; i++)
        {
            InteractableBase interactable = hits[i].GetComponentInParent<InteractableBase>();
            if (interactable == null)
                continue;

            if (uniqueSet.Contains(interactable))
                continue;

            if (!interactable.CanShowIndicator(transform))
                continue;

            uniqueSet.Add(interactable);
            visibleInteractables.Add(interactable);
        }
    }

    private void RefreshDots()
    {
        EnsureDotPoolSize(visibleInteractables.Count);

        for (int i = 0; i < dotPool.Count; i++)
        {
            if (i < visibleInteractables.Count)
            {
                Transform anchor = visibleInteractables[i].GetIndicatorAnchor();
                dotPool[i].Bind(anchor, indicatorOffset);
            }
            else
            {
                dotPool[i].Unbind();
            }
        }
    }

    private void EnsureDotPoolSize(int requiredCount)
    {
        if (dotPrefab == null || dotCanvasRoot == null || mainCam == null)
            return;

        while (dotPool.Count < requiredCount)
        {
            InteractionDotUI newDot = Instantiate(dotPrefab, dotCanvasRoot);
            newDot.Initialize(mainCam);
            newDot.Unbind();
            dotPool.Add(newDot);
        }
    }

    private void UpdateBestTarget()
    {
        currentBestTarget = GetBestTarget();
    }

    private void RefreshSelectedPrompt()
    {
        if (selectedPromptUI == null)
        {
            LogPromptState("selectedPromptUI == null");
            return;
        }

        if (interactionLockTimer > 0f)
        {
            selectedPromptUI.Unbind();
            LogPromptState($"hidden - interactionLockTimer > 0 ({interactionLockTimer:F2})");
            return;
        }

        if (currentBestTarget == null)
        {
            selectedPromptUI.Unbind();
            LogPromptState("hidden - currentBestTarget == null");
            return;
        }

        bool canInteract = currentBestTarget.CanInteract(transform);

        if (!canInteract)
        {
            selectedPromptUI.Unbind();
            float dist = Vector3.Distance(transform.position, currentBestTarget.transform.position);
            LogPromptState($"hidden - bestTarget exists but CanInteract=false, target={currentBestTarget.name}, dist={dist:F2}");
            return;
        }

        Transform anchor = currentBestTarget.GetIndicatorAnchor();

        if (anchor == null)
        {
            selectedPromptUI.Unbind();
            LogPromptState($"hidden - anchor == null, target={currentBestTarget.name}");
            return;
        }

        selectedPromptUI.Bind(anchor, selectedPromptOffset);

        float finalDist = Vector3.Distance(transform.position, currentBestTarget.transform.position);
        LogPromptState(
            $"bind - target={currentBestTarget.name}, anchor={anchor.name}, dist={finalDist:F2}, offset={selectedPromptOffset}"
        );
    }

    private void HandleInteractionInput()
    {
        if (interactionLockTimer > 0f)
            return;

        if (!Input.GetKeyDown(KeyCode.F))
            return;

        if (currentBestTarget == null)
        {
            if (enableDebugLog)
                Debug.Log("[PlayerInteraction] F 입력했지만 currentBestTarget이 null입니다.");
            return;
        }

        bool interacted = currentBestTarget.TryInteract(transform, out float targetLockTime);

        if (enableDebugLog)
        {
            Debug.Log(
                $"[PlayerInteraction] F 입력 - target={currentBestTarget.name}, " +
                $"interacted={interacted}, targetLockTime={targetLockTime:F2}"
            );
        }

        if (!interacted)
            return;

        interactionLockTimer = Mathf.Max(baseInputCooldown, targetLockTime);
    }

    private InteractableBase GetBestTarget()
    {
        InteractableBase bestTarget = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < visibleInteractables.Count; i++)
        {
            InteractableBase candidate = visibleInteractables[i];
            if (candidate == null)
                continue;

            Vector3 toTarget = candidate.transform.position - transform.position;
            toTarget.y = 0f;

            float distanceScore = toTarget.magnitude;

            Vector3 dir = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : transform.forward;
            float dot = Vector3.Dot(transform.forward, dir);

            float facingPenalty = (1f - Mathf.Clamp01((dot + 1f) * 0.5f)) * forwardWeight;
            float totalScore = distanceScore + facingPenalty;

            if (totalScore < bestScore)
            {
                bestScore = totalScore;
                bestTarget = candidate;
            }
        }

        return bestTarget;
    }

    private void LogPromptState(string state)
    {
        if (!enableDebugLog)
            return;

        if (logEveryFrame || lastPromptState != state)
        {
            //Debug.Log($"[PlayerInteraction][SelectedPrompt] {state}");
            lastPromptState = state;
        }
    }

    public bool IsInteractionLocked()
    {
        return interactionLockTimer > 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
    private void HideAllInteractionUI()
    {
        for (int i = 0; i < dotPool.Count; i++)
        {
            dotPool[i].Unbind();
        }

        if (selectedPromptUI != null)
        {
            selectedPromptUI.Unbind();
        }

        visibleInteractables.Clear();
        currentBestTarget = null;
    }

    public void ResetRuntimeStateAfterRespawn()
    {
        interactionLockTimer = 0f;
        HideAllInteractionUI();
    }
}