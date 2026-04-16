using UnityEngine;

public class characterSelectInteractable : InteractableBase
{
    [SerializeField] private SquadFormationPanelUI characterSelectUI;

    public override bool IsBusy()
    {
        return false;
    }

    protected override void Interact(Transform interactor)
    {
        if (characterSelectUI != null)
        {
            characterSelectUI.Show();
            Debug.Log("[CharacterSelectInteractable] 캐릭터 편성 UI 오픈");
        }
    }
}