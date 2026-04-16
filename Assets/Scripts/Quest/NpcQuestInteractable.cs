using UnityEngine;

public class NpcQuestInteractable : InteractableBase
{
    [Header("Dependencies")]
    [SerializeField] private NpcQuestProfile profile;
    [SerializeField] private QuestManager questManager;
    [SerializeField] private NpcInteractionPanelUI npcInteractionPanelUI;

    protected override void Interact(Transform interactor)
    {
        if (profile == null || questManager == null || npcInteractionPanelUI == null)
        {
            Debug.LogWarning("[NpcQuestInteractable] 필요한 참조가 연결되지 않았습니다.");
            return;
        }

        if (profile.Quest == null)
        {
            npcInteractionPanelUI.ShowDialogueOnly(profile);
            return;
        }

        QuestRuntime runtime = questManager.GetRuntime(profile.Quest.QuestId);

        if (runtime == null)
        {
            npcInteractionPanelUI.ShowAvailable(profile, questManager);
            return;
        }

        if (!runtime.isCompleted)
        {
            npcInteractionPanelUI.ShowInProgress(profile, runtime, questManager);
            return;
        }

        if (!runtime.rewardClaimed)
        {
            npcInteractionPanelUI.ShowComplete(profile, runtime, questManager);
            return;
        }

        npcInteractionPanelUI.ShowDialogueOnly(profile);
    }
}