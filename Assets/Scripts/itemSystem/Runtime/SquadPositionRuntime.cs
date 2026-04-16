using System;

[Serializable]
public class SquadPositionRuntime
{
    public PositionIndex PositionIndex;

    public string CharacterId;
    public CharacterDefinition CharacterDefinition;

    public int InventoryStartIndex;
    public int InventoryCount;

    public SquadPositionRuntime(PositionIndex index)
    {
        PositionIndex = index;
    }
}