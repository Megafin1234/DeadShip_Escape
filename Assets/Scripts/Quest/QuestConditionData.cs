using System;
using UnityEngine;

[Serializable]
public class QuestConditionData
{
    public QuestConditionType conditionType;
    public string targetId;
    public int requiredAmount = 1;
}