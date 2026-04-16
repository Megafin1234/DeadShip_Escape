public class SharedHealthService
{
    public void InitializeFromDerivedStats(SquadRuntime squad)
    {
        if (squad == null)
            return;

        squad.CombatState.MaxHealth = squad.DerivedStats.Health;

        if (squad.CombatState.CurrentHealth <= 0)
            squad.CombatState.CurrentHealth = squad.CombatState.MaxHealth;

        if (squad.CombatState.CurrentHealth > squad.CombatState.MaxHealth)
            squad.CombatState.CurrentHealth = squad.CombatState.MaxHealth;

        squad.CombatState.IsDead = squad.CombatState.CurrentHealth <= 0;
    }

    public void EnterRaid(SquadRuntime squad)
    {
        if (squad == null)
            return;

        squad.RaidState = RaidStateType.InRaid;
        squad.CombatState.ResetToFull();
    }

    public void ApplyDamage(SquadRuntime squad, int damage)
    {
        if (squad == null || damage <= 0)
            return;

        if (squad.CombatState.IsDead)
            return;

        squad.CombatState.CurrentHealth -= damage;

        if (squad.CombatState.CurrentHealth <= 0)
        {
            squad.CombatState.CurrentHealth = 0;
            squad.CombatState.IsDead = true;
            squad.RaidState = RaidStateType.Wiped;
        }
    }

    public void Heal(SquadRuntime squad, int amount)
    {
        if (squad == null || amount <= 0)
            return;

        if (squad.CombatState.IsDead)
            return;

        squad.CombatState.CurrentHealth += amount;

        if (squad.CombatState.CurrentHealth > squad.CombatState.MaxHealth)
            squad.CombatState.CurrentHealth = squad.CombatState.MaxHealth;
    }

    public void Extract(SquadRuntime squad)
    {
        if (squad == null)
            return;

        if (squad.CombatState.IsDead)
            return;

        squad.RaidState = RaidStateType.Extracted;
    }
}