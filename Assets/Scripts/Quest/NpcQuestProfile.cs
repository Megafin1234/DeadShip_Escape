using UnityEngine;

[CreateAssetMenu(menuName = "Game/NPC Quest Profile")]
public class NpcQuestProfile : ScriptableObject
{
    [Header("NPC")]
    [SerializeField] private string npcId;
    [SerializeField] private string npcDisplayName;

    [Header("Quest")]
    [SerializeField] private QuestData quest;

    [Header("Dialogues")]
    [TextArea(2, 5)] [SerializeField] private string defaultDialogue;
    [TextArea(2, 5)] [SerializeField] private string availableDialogue;
    [TextArea(2, 5)] [SerializeField] private string inProgressDialogue;
    [TextArea(2, 5)] [SerializeField] private string readyToTurnInDialogue;
    [TextArea(2, 5)] [SerializeField] private string completeDialogue;

    public string NpcId => npcId;
    public string NpcDisplayName => npcDisplayName;
    public QuestData Quest => quest;

    public string DefaultDialogue => defaultDialogue;
    public string AvailableDialogue => availableDialogue;
    public string InProgressDialogue => inProgressDialogue;
    public string ReadyToTurnInDialogue => readyToTurnInDialogue;
    public string CompleteDialogue => completeDialogue;
}