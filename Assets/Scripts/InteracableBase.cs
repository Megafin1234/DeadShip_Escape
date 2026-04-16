using UnityEngine;

public abstract class InteractableBase : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] protected float interactionDistance = 2.2f;
    [SerializeField] protected float indicatorDistance = 7.0f;
    [SerializeField] protected bool isInteractable = true;
    [SerializeField] protected Transform indicatorAnchor;

    [Header("Interaction Lock")]
    [SerializeField] protected float defaultInteractionLockTime = 0.15f;

    public virtual bool CanShowIndicator(Transform interactor)
    {
        if (!isInteractable || interactor == null)
            return false;

        float distance = Vector3.Distance(interactor.position, transform.position);
        return distance <= indicatorDistance;
    }

    public virtual bool CanInteract(Transform interactor)
    {
        if (!isInteractable || interactor == null)
            return false;

        if (IsBusy())
            return false;

        float distance = Vector3.Distance(interactor.position, transform.position);
        return distance <= interactionDistance;
    }

    public virtual Transform GetIndicatorAnchor()
    {
        return indicatorAnchor != null ? indicatorAnchor : transform;
    }

    public virtual bool IsBusy()
    {
        return false;
    }

    public virtual float GetInteractionLockTime()
    {
        return defaultInteractionLockTime;
    }

    public float GetIndicatorDistance()
    {
        return indicatorDistance;
    }

    public float GetInteractionDistance()
    {
        return interactionDistance;
    }

    public bool TryInteract(Transform interactor, out float appliedLockTime)
    {
        appliedLockTime = 0f;

        if (!CanInteract(interactor))
            return false;

        Interact(interactor);

        appliedLockTime = Mathf.Max(0f, GetInteractionLockTime());
        return true;
    }

    protected abstract void Interact(Transform interactor);
}