using UnityEngine;

public class UpgradeUnlockInteractable : InteractableBase
{
    [SerializeField] private UpgradeUnlockUIController upgradeUI;

    public override bool IsBusy()
    {
        return false;
    }

    protected override void Interact(Transform interactor)
    {
        if (upgradeUI != null)
        {
            upgradeUI.Show();
            Debug.Log("[UpgradeUnlockInteractable] 해금 UI 오픈");
        }
    }
}