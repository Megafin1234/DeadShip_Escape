using UnityEngine;

public class DoorSwitch : InteractableBase
{
    [Header("Door Link")]
    [SerializeField] private SlidingDoor linkedDoor;

    [Header("Switch Option")]
    [SerializeField] private bool useToggle = true;

    public override bool IsBusy()
    {
        return linkedDoor != null && linkedDoor.IsBusy();
    }

    public override float GetInteractionLockTime()
    {
        if (linkedDoor == null)
            return base.GetInteractionLockTime();

        return Mathf.Max(base.GetInteractionLockTime(), linkedDoor.GetTransitionDuration());
    }

    protected override void Interact(Transform interactor)
    {
        if (linkedDoor == null)
        {
            Debug.LogWarning($"{name}: linkedDoor is not assigned.");
            return;
        }

        if (useToggle)
            linkedDoor.ToggleDoor();
        else
            linkedDoor.OpenDoor();
    }
}