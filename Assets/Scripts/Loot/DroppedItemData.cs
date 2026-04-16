using System;

[Serializable]
public class DroppedItemData
{
    public string definitionId;
    public int stackCount;

    public DroppedItemData(string definitionId, int stackCount)
    {
        this.definitionId = definitionId;
        this.stackCount = stackCount;
    }
}