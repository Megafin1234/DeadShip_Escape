using System.Collections.Generic;

[System.Serializable]
public class RaidResultSummary
{
    public RaidSessionResultType ResultType = RaidSessionResultType.None;

    public List<string> GainedItemNames = new();
    public List<string> LostItemNames = new();

    public int GainedItemCount => GainedItemNames != null ? GainedItemNames.Count : 0;
    public int LostItemCount => LostItemNames != null ? LostItemNames.Count : 0;

    public void Clear()
    {
        ResultType = RaidSessionResultType.None;
        GainedItemNames.Clear();
        LostItemNames.Clear();
    }
}