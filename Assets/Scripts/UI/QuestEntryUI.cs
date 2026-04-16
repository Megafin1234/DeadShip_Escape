using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestEntryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button claimButton;
    private QuestRuntime boundRuntime;
    private QuestManager questManager;

    private void Awake()
    {
        if (claimButton != null)
            claimButton.onClick.AddListener(HandleClaimClicked);
    }

    public void Bind(QuestRuntime runtime, QuestManager manager)
    {
        boundRuntime = runtime;
        questManager = manager;

        Refresh();
    }

    public void Refresh()
    {
        if (boundRuntime == null || boundRuntime.questData == null)
            return;

        if (titleText != null)
            titleText.text = boundRuntime.questData.DisplayName;

        if (descriptionText != null)
            descriptionText.text = boundRuntime.questData.Description;

        if (progressText != null)
            progressText.text = BuildProgressText(boundRuntime);

        if (statusText != null)
        {
            if (boundRuntime.rewardClaimed)
                statusText.text = "보상 수령 완료";
            else if (boundRuntime.isCompleted)
                statusText.text = "완료";
            else
                statusText.text = "진행 중";
        }

        if (claimButton != null)
            claimButton.interactable = boundRuntime.isCompleted && !boundRuntime.rewardClaimed;
    }

    private string BuildProgressText(QuestRuntime runtime)
    {
        if (runtime.progresses == null || runtime.progresses.Count == 0)
            return "진행도 없음";

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

    private void HandleClaimClicked()
    {
        if (boundRuntime == null || questManager == null)
            return;

        bool success = questManager.TryClaimReward(boundRuntime);
        Debug.Log($"[QuestEntryUI] 보상 수령 결과 = {success}");
        Refresh();
    }
}