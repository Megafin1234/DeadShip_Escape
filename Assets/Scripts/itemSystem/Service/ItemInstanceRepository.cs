using System.Collections.Generic;

public class ItemInstanceRepository
{
    private readonly Dictionary<string, ItemInstance> items = new();

    public void Add(ItemInstance item)
    {
        if (item == null || string.IsNullOrEmpty(item.InstanceId))
            return;

        items[item.InstanceId] = item;
    }

    public ItemInstance Get(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return null;

        items.TryGetValue(instanceId, out ItemInstance result);
        return result;
    }

    public void Remove(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return;

        items.Remove(instanceId);
    }
}