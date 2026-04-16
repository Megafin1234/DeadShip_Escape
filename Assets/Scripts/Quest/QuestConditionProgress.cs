using System;

[Serializable]
public class QuestConditionProgress
{
    public QuestConditionData conditionData;
    public int currentAmount;

    public bool IsComplete => conditionData != null && currentAmount >= conditionData.requiredAmount;

    public QuestConditionProgress(QuestConditionData data)
    {
        conditionData = data;
        currentAmount = 0;
    }

    public void ApplyEvent(QuestEvent questEvent)
    {
        if (conditionData == null)
            return;

        bool typeMatch =
            (conditionData.conditionType == QuestConditionType.KillEnemy && questEvent.eventType == QuestEventType.KillEnemy) ||
            (conditionData.conditionType == QuestConditionType.SubmitItem && questEvent.eventType == QuestEventType.SubmitItem) ||
            (conditionData.conditionType == QuestConditionType.Extract && questEvent.eventType == QuestEventType.Extract);

        if (!typeMatch)
            return;

        if (!string.IsNullOrEmpty(conditionData.targetId) && conditionData.targetId != questEvent.targetId)
            return;

        currentAmount += questEvent.amount;
        if (currentAmount > conditionData.requiredAmount)
            currentAmount = conditionData.requiredAmount;
    }
}