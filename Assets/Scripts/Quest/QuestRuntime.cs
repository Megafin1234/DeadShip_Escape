using System;
using System.Collections.Generic;

[Serializable]
public class QuestRuntime
{
    public QuestData questData;
    public List<QuestConditionProgress> progresses = new();
    public bool isCompleted;
    public bool rewardClaimed;

    public QuestRuntime(QuestData data)
    {
        questData = data;
        isCompleted = false;
        rewardClaimed = false;

        if (questData != null)
        {
            for (int i = 0; i < questData.Conditions.Count; i++)
            {
                progresses.Add(new QuestConditionProgress(questData.Conditions[i]));
            }
        }
    }

    public void ApplyEvent(QuestEvent questEvent)
    {
        if (isCompleted || questData == null)
            return;

        for (int i = 0; i < progresses.Count; i++)
        {
            progresses[i].ApplyEvent(questEvent);
        }

        isCompleted = CheckCompleted();
    }

    private bool CheckCompleted()
    {
        if (progresses.Count == 0)
            return true;

        for (int i = 0; i < progresses.Count; i++)
        {
            if (!progresses[i].IsComplete)
                return false;
        }

        return true;
    }
}