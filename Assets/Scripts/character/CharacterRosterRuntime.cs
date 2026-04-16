using System.Collections.Generic;

[System.Serializable]
public class CharacterRosterRuntime
{
    public List<CharacterDefinition> OwnedCharacters = new();

    public bool Contains(string characterId)
    {
        for (int i = 0; i < OwnedCharacters.Count; i++)
        {
            if (OwnedCharacters[i] != null &&
                OwnedCharacters[i].CharacterId == characterId)
                return true;
        }
        return false;
    }

    public CharacterDefinition GetById(string characterId)
    {
        for (int i = 0; i < OwnedCharacters.Count; i++)
        {
            if (OwnedCharacters[i] != null &&
                OwnedCharacters[i].CharacterId == characterId)
                return OwnedCharacters[i];
        }
        return null;
    }
}