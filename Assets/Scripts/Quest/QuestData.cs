using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quest Data")]
public class QuestData : ScriptableObject
{
    [SerializeField] private string questId;
    [SerializeField] private string displayName;
    [SerializeField] [TextArea(2, 4)] private string description;
    [SerializeField] private List<QuestConditionData> conditions = new();
    [SerializeField] private List<QuestRewardData> rewards = new();

    public string QuestId => questId;
    public string DisplayName => displayName;
    public string Description => description;
    public List<QuestConditionData> Conditions => conditions;
    public List<QuestRewardData> Rewards => rewards;
}