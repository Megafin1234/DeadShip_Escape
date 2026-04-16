using System;

[Serializable]
public class SharedSquadCombatState
{
    public int CurrentHealth;
    public int MaxHealth;

    public bool IsDead;

    public void ResetToFull()
    {
        CurrentHealth = MaxHealth;
        IsDead = false;
    }
}