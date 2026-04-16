using System.Collections.Generic;

/// <summary>
/// 아이템 종류 기준 마커 저장소.
/// Favorite는 유저가 토글.
/// Quest는 시스템(퀘스트/제작/목표)이 세팅.
/// </summary>
public class ItemMarkerStateRepository
{
    private readonly HashSet<string> favoriteMarkedItemIds = new();
    private readonly HashSet<string> questMarkedItemIds = new();

    public bool IsFavoriteMarked(string itemId)
    {
        return !string.IsNullOrEmpty(itemId) && favoriteMarkedItemIds.Contains(itemId);
    }

    public bool IsQuestMarked(string itemId)
    {
        return !string.IsNullOrEmpty(itemId) && questMarkedItemIds.Contains(itemId);
    }

    public void ToggleFavorite(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        if (!favoriteMarkedItemIds.Add(itemId))
            favoriteMarkedItemIds.Remove(itemId);
    }

    public void SetQuestMarked(string itemId, bool marked)
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        if (marked)
            questMarkedItemIds.Add(itemId);
        else
            questMarkedItemIds.Remove(itemId);
    }
}