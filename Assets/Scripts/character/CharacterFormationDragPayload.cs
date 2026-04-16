using UnityEngine;

public enum FormationDragSourceType
{
    None = 0,
    Roster = 1,
    Party = 2
}

public class CharacterFormationDragPayload
{
    public FormationDragSourceType SourceType;
    public CharacterDefinition CharacterDefinition;
    public PositionIndex SourcePositionIndex;

    public CharacterFormationDragPayload(
        FormationDragSourceType sourceType,
        CharacterDefinition characterDefinition,
        PositionIndex sourcePositionIndex = PositionIndex.None)
    {
        SourceType = sourceType;
        CharacterDefinition = characterDefinition;
        SourcePositionIndex = sourcePositionIndex;
    }
}