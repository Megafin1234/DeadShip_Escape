using System;

[Serializable]
public struct QuestEvent
{
    public QuestEventType eventType;
    public string targetId;
    public int amount;

    public QuestEvent(QuestEventType eventType, string targetId, int amount)
    {
        this.eventType = eventType;
        this.targetId = targetId;
        this.amount = amount;
    }
}