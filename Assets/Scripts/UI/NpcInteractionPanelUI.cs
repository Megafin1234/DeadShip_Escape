using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcInteractionPanelUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Dependencies")]
    [SerializeField] private PlayerSquadBridge bridge;
    [SerializeField] private RaidOverlayUIController overlayUIController;

    [Header("Npc")]
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;
    [Header("Quest Info")]
    [SerializeField] private GameObject questInfoRoot;
    [SerializeField] private TMP_Text questTitleText;
    [SerializeField] private TMP_Text questDescriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button rewardButton;
    [SerializeField] private Button closeButton;

    private NpcQuestProfile currentProfile;
    private QuestRuntime currentRuntime;
    private QuestManager currentQuestManager;

    private void Awake()
    {
        HideImmediate();

        if (acceptButton != null)
            acceptButton.onClick.AddListener(HandleAcceptClicked);

        if (submitButton != null)
            submitButton.onClick.AddListener(HandleSubmitClicked);

        if (rewardButton != null)
            rewardButton.onClick.AddListener(HandleRewardClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    private void Update()
    {
        if (root == null || !root.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            Hide();
    }

    public void ShowAvailable(NpcQuestProfile profile, QuestManager questManager)
    {
        currentProfile = profile;
        currentQuestManager = questManager;
        currentRuntime = null;

        SetRootVisible(true);

        if (questInfoRoot != null)
            questInfoRoot.SetActive(true);
        BindCommonTexts(profile);
        SetDialogue(profile != null ? profile.AvailableDialogue : "");
        SetState("수주 가능");
        SetProgress("");

        SetAcceptButton(true);
        SetSubmitButton(false);
        SetRewardButton(false);
    }

    public void ShowInProgress(NpcQuestProfile profile, QuestRuntime runtime, QuestManager questManager)
    {
        currentProfile = profile;
        currentRuntime = runtime;
        currentQuestManager = questManager;

        SetRootVisible(true);
        if (questInfoRoot != null)
            questInfoRoot.SetActive(true);
        BindCommonTexts(profile);

        bool hasSubmitCondition = HasSubmitCondition(runtime);
        bool canTurnIn = hasSubmitCondition && questManager != null && runtime != null && questManager.CanTurnInQuest(runtime);

        if (canTurnIn)
        {
            SetDialogue(profile != null ? profile.ReadyToTurnInDialogue : "");
            SetState("제출 가능");
        }
        else
        {
            SetDialogue(profile != null ? profile.InProgressDialogue : "");
            SetState("진행 중");
        }

        SetProgress(BuildProgressText(runtime));

        SetAcceptButton(false);
        SetSubmitButton(canTurnIn);
        SetRewardButton(false);
    }

    public void ShowComplete(NpcQuestProfile profile, QuestRuntime runtime, QuestManager questManager)
    {
        currentProfile = profile;
        currentRuntime = runtime;
        currentQuestManager = questManager;

        SetRootVisible(true);

        if (questInfoRoot != null)
            questInfoRoot.SetActive(true);

        BindCommonTexts(profile);
        SetDialogue(profile != null ? profile.CompleteDialogue : "");
        SetState("완료");
        SetProgress(BuildProgressText(runtime));

        SetAcceptButton(false);
        SetSubmitButton(false);
        SetRewardButton(runtime != null && !runtime.rewardClaimed);
    }

    public void ShowDialogueOnly(NpcQuestProfile profile)
    {
        currentProfile = profile;
        currentRuntime = null;
        currentQuestManager = null;

        SetRootVisible(true);

        if (questInfoRoot != null)
            questInfoRoot.SetActive(false);

        BindCommonTexts(profile);
        SetDialogue(profile != null ? profile.DefaultDialogue : "");
        SetState("대화");
        SetProgress("");

        SetAcceptButton(false);
        SetSubmitButton(false);
        SetRewardButton(false);
    }

    public void Hide()
    {
        SetRootVisible(false);
        ClearRuntime();
    }

    private void HideImmediate()
    {
        if (root != null)
            root.SetActive(false);

        if (bridge != null)
            bridge.SetModalBlocker("NpcInteraction", false);

        ClearRuntime();
    }

    private void SetRootVisible(bool visible)
    {
        if (root != null)
            root.SetActive(visible);

        if (bridge != null)
            bridge.SetModalBlocker("NpcInteraction", visible);
    }

    private void BindCommonTexts(NpcQuestProfile profile)
    {
        if (npcNameText != null)
            npcNameText.text = profile != null ? profile.NpcDisplayName : "";

        if (questTitleText != null)
            questTitleText.text = profile != null && profile.Quest != null ? profile.Quest.DisplayName : "";

        if (questDescriptionText != null)
            questDescriptionText.text = profile != null && profile.Quest != null ? profile.Quest.Description : "";
    }

    private void SetDialogue(string value)
    {
        if (dialogueText != null)
            dialogueText.text = value;
    }

    private void SetState(string value)
    {
        if (stateText != null)
            stateText.text = value;
    }

    private void SetProgress(string value)
    {
        if (progressText != null)
            progressText.text = value;
    }

    private void SetAcceptButton(bool active)
    {
        if (acceptButton != null)
            acceptButton.gameObject.SetActive(active);
    }

    private void SetSubmitButton(bool active)
    {
        if (submitButton != null)
            submitButton.gameObject.SetActive(active);
    }

    private void SetRewardButton(bool active)
    {
        if (rewardButton != null)
            rewardButton.gameObject.SetActive(active);
    }

    private string BuildProgressText(QuestRuntime runtime)
    {
        if (runtime == null || runtime.progresses == null || runtime.progresses.Count == 0)
            return "";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < runtime.progresses.Count; i++)
        {
            QuestConditionProgress progress = runtime.progresses[i];
            if (progress == null || progress.conditionData == null)
                continue;

            string label = GetConditionLabel(progress.conditionData);
            sb.Append($"{label} {progress.currentAmount}/{progress.conditionData.requiredAmount}");

            if (i < runtime.progresses.Count - 1)
                sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GetConditionLabel(QuestConditionData condition)
    {
        switch (condition.conditionType)
        {
            case QuestConditionType.KillEnemy:
                return $"처치({condition.targetId})";
            case QuestConditionType.SubmitItem:
                return $"제출({condition.targetId})";
            case QuestConditionType.Extract:
                return "탈출";
            default:
                return "조건";
        }
    }

    private void HandleAcceptClicked()
    {
        if (currentProfile == null || currentQuestManager == null || currentProfile.Quest == null)
            return;

        bool success = currentQuestManager.AcceptQuest(currentProfile.Quest);
        Debug.Log($"[NpcInteractionPanelUI] 퀘스트 수주 결과 = {success}");

        if (!success)
            return;

        QuestRuntime runtime = currentQuestManager.GetRuntime(currentProfile.Quest.QuestId);
        ShowInProgress(currentProfile, runtime, currentQuestManager);

        if (overlayUIController != null)
            overlayUIController.RefreshAll();
    }

    private void HandleSubmitClicked()
    {
        if (currentRuntime == null || currentQuestManager == null)
            return;

        bool success = currentQuestManager.TryTurnInQuest(currentRuntime);
        Debug.Log($"[NpcInteractionPanelUI] 퀘스트 제출 결과 = {success}");

        if (!success)
            return;

        ShowComplete(currentProfile, currentRuntime, currentQuestManager);

        if (overlayUIController != null)
            overlayUIController.RefreshAll();
    }

    private void HandleRewardClicked()
    {
        if (currentRuntime == null || currentQuestManager == null)
            return;

        bool success = currentQuestManager.TryClaimReward(currentRuntime);
        Debug.Log($"[NpcInteractionPanelUI] 퀘스트 보상 수령 결과 = {success}");

        if (!success)
            return;

        ShowDialogueOnly(currentProfile);

        if (overlayUIController != null)
            overlayUIController.RefreshAll();
    }

    private void ClearRuntime()
    {
        currentProfile = null;
        currentRuntime = null;
        currentQuestManager = null;
    }

    private bool HasSubmitCondition(QuestRuntime runtime)
    {
        if (runtime == null || runtime.questData == null)
            return false;

        for (int i = 0; i < runtime.questData.Conditions.Count; i++)
        {
            QuestConditionData condition = runtime.questData.Conditions[i];
            if (condition.conditionType == QuestConditionType.SubmitItem)
                return true;
        }

        return false;
    }
}