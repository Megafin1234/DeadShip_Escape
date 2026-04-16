using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private List<QuestData> initialQuests = new(); // 초기 테스트용 들고시작하는 퀘스트. 시작퀘스트용으로 사용
    [SerializeField] private PlayerSquadBridge bridge;

    private readonly List<QuestRuntime> activeQuests = new();

    public IReadOnlyList<QuestRuntime> ActiveQuests => activeQuests;

    private void Awake()
    {
        activeQuests.Clear();

        /*for (int i = 0; i < initialQuests.Count; i++)
        {
            if (initialQuests[i] != null)
                activeQuests.Add(new QuestRuntime(initialQuests[i]));
        }*/  // 시작퀘스트용. 
    }

    public void PublishEvent(QuestEvent questEvent)
    {
        for (int i = 0; i < activeQuests.Count; i++)
        {
            bool wasComplete = activeQuests[i].isCompleted;
            activeQuests[i].ApplyEvent(questEvent);

            if (!wasComplete && activeQuests[i].isCompleted)
            {
                Debug.Log($"[Quest] 완료: {activeQuests[i].questData.DisplayName}");
            }
        }
    }

    public bool TryClaimReward(QuestRuntime runtime)
    {
        if (runtime == null || runtime.questData == null)
            return false;

        if (!runtime.isCompleted || runtime.rewardClaimed)
            return false;

        if (bridge == null)
            return false;

        for (int i = 0; i < runtime.questData.Rewards.Count; i++)
        {
            QuestRewardData reward = runtime.questData.Rewards[i];
            bridge.TryAddRewardItemToStash(reward.rewardItemDefinitionId, reward.amount);
        }

        runtime.rewardClaimed = true;
        return true;
    }

    public bool AcceptQuest(QuestData data) //퀘 수주
    {
        if (data == null)
            return false;

        if (HasQuest(data))
            return false;

        activeQuests.Add(new QuestRuntime(data));
        Debug.Log($"[Quest] 수주: {data.DisplayName}");
        return true;
    }

    public bool HasQuest(QuestData data)
    {
        if (data == null)
            return false;

        return GetRuntime(data.QuestId) != null;
    }

    public QuestRuntime GetRuntime(string questId) //수주중인 퀘 확인
    {
        if (string.IsNullOrEmpty(questId))
            return null;

        for (int i = 0; i < activeQuests.Count; i++)
        {
            QuestRuntime runtime = activeQuests[i];
            if (runtime != null && runtime.questData != null && runtime.questData.QuestId == questId)
                return runtime;
        }

        return null;
    }

    public bool IsQuestInProgress(QuestData data)
    {
        QuestRuntime runtime = GetRuntime(data.QuestId);
        return runtime != null && !runtime.isCompleted;
    }

    public bool IsRewardReady(QuestData data)
    {
        QuestRuntime runtime = GetRuntime(data.QuestId);
        return runtime != null && runtime.isCompleted && !runtime.rewardClaimed;
    }

    public bool IsQuestFullyCompleted(QuestData data) 
    {
        QuestRuntime runtime = GetRuntime(data.QuestId);
        return runtime != null && runtime.isCompleted && runtime.rewardClaimed;
    }

    public bool CanTurnInQuest(QuestRuntime runtime)
    {
        if (runtime == null || runtime.questData == null)
            return false;

        bool hasSubmitCondition = false;

        for (int i = 0; i < runtime.questData.Conditions.Count; i++)
        {
            QuestConditionData condition = runtime.questData.Conditions[i];

            if (condition.conditionType != QuestConditionType.SubmitItem)
                continue;

            hasSubmitCondition = true;

            if (bridge == null)
                return false;

            bool enough = bridge.HasEnoughUnlockCosts(new List<UnlockCost>
            {
                new UnlockCost
                {
                    itemDefinitionId = condition.targetId,
                    amount = condition.requiredAmount
                }
            });

            if (!enough)
                return false;
        }

        // 제출형 조건이 하나라도 있을 때만 true 가능
        return hasSubmitCondition;
    }

    public bool TryTurnInQuest(QuestRuntime runtime)
    {
        if (runtime == null || runtime.questData == null)
            return false;

        if (runtime.isCompleted)
            return false;

        bool hasSubmitCondition = false;

        for (int i = 0; i < runtime.questData.Conditions.Count; i++)
        {
            QuestConditionData condition = runtime.questData.Conditions[i];

            if (condition.conditionType != QuestConditionType.SubmitItem)
                continue;

            hasSubmitCondition = true;

            if (bridge == null)
                return false;

            bool success = bridge.TrySubmitQuestItems(condition.targetId, condition.requiredAmount, this);
            if (!success)
                return false;
        }

        // 제출형 조건이 없는 퀘스트는 제출 자체가 불가
        return hasSubmitCondition;
    }
}