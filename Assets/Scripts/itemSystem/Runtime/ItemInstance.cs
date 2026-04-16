using System;

[Serializable]
public class ItemInstance
{
    public string InstanceId;
    public string DefinitionId;

    public int StackCount;
    public int CurrentUseCount;
    public float CurrentDurability;


    public ItemInstance(string definitionId, int stack = 1)
    {
        InstanceId = Guid.NewGuid().ToString();
        DefinitionId = definitionId;

        StackCount = stack;
        CurrentUseCount = 0;
        CurrentDurability = 1f;
    }
}