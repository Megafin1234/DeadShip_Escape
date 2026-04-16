using System;

[Serializable]
public class OpenedLootContext
{
    public LootContainerRuntime LootContainer;
    public SquadRuntime Squad;

    public bool IsOpen => LootContainer != null && Squad != null;

    public OpenedLootContext(LootContainerRuntime lootContainer, SquadRuntime squad)
    {
        LootContainer = lootContainer;
        Squad = squad;
    }
}