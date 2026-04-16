using UnityEngine;

public class PlayerCombatContext : MonoBehaviour
{
    [SerializeField] private float targetMaintainTime = 3f;

    private float targetTimer = 0f;

    public Transform CurrentTarget { get; private set; }

    private void Update()
    {
        if (CurrentTarget == null)
            return;

        targetTimer -= Time.deltaTime;
        if (targetTimer <= 0f)
        {
            CurrentTarget = null;
        }
    }

    public void SetTarget(Transform target)
    {
        if (target == null)
            return;

        CurrentTarget = target;
        targetTimer = targetMaintainTime;

        Debug.Log($"[PlayerCombatContext] 타겟 설정: {target.name}");
    }

    public void ClearTarget()
    {
        CurrentTarget = null;
        targetTimer = 0f;
    }
}