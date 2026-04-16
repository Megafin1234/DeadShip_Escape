using UnityEngine;

public class PlayerSquadViewBinder : MonoBehaviour
{
    [Header("Optional Existing Components")]
    [SerializeField] private MonoBehaviour playerHealthComponent;
    [SerializeField] private MonoBehaviour playerShooterComponent;

    public void ApplyFromSquad(SquadRuntime squad)
    {
        if (squad == null)
            return;

        ApplyHealth(squad);
        ApplyCombatStats(squad);
    }

    private void ApplyHealth(SquadRuntime squad)
    {
        // 여기서는 실제 기존 코드 시그니처를 몰라서
        // 디버그 로그만 두고, 나중에 실제 PlayerHealth 코드 보고 맞춤 연결
        Debug.Log($"[PlayerSquadViewBinder] HP Sync -> {squad.CombatState.CurrentHealth}/{squad.CombatState.MaxHealth}");
    }

    private void ApplyCombatStats(SquadRuntime squad)
    {
        Debug.Log(
            $"[PlayerSquadViewBinder] Combat Sync -> " +
            $"ATK={squad.DerivedStats.Attack}, " +
            $"Range={squad.DerivedStats.AttackRange}, " +
            $"FireInterval={squad.DerivedStats.FireInterval}");
    }
}