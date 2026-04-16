public class StackService
{
    private readonly ItemDatabase itemDatabase;

    public StackService(ItemDatabase itemDatabase)
    {
        this.itemDatabase = itemDatabase;
    }

    public bool CanStack(ItemInstance a, ItemInstance b)
    {
        if (a == null || b == null)
            return false;

        if (a.DefinitionId != b.DefinitionId)
            return false;

        ItemDefinitionBase def = itemDatabase.GetDefinition(a.DefinitionId);
        if (def == null)
            return false;

        return def.MaxStack > 1;
    }

    public int MergeInto(ItemInstance source, ItemInstance target)
    {
        if (!CanStack(source, target))
            return 0;

        ItemDefinitionBase def = itemDatabase.GetDefinition(source.DefinitionId);
        int maxStack = def.MaxStack;

        int space = maxStack - target.StackCount;
        if (space <= 0)
            return 0;

        int moved = source.StackCount <= space ? source.StackCount : space;

        target.StackCount += moved;
        source.StackCount -= moved;

        return moved;
    }

    public ItemInstance Split(ItemInstance source, int amount, ItemInstanceRepository repo)
    {
        if (source == null || repo == null)
            return null;

        if (amount <= 0 || amount >= source.StackCount)
            return null;

        ItemInstance newItem = new ItemInstance(source.DefinitionId, amount);
        source.StackCount -= amount;

        repo.Add(newItem);
        return newItem;
    }

    public ItemInstance SplitHalf(ItemInstance source, ItemInstanceRepository repo)
    {
        if (source == null || repo == null)
            return null;

        if (source.StackCount <= 1)
            return null;

        int splitAmount = source.StackCount / 2;
        if (splitAmount <= 0)
            return null;

        return Split(source, splitAmount, repo);
    }
}