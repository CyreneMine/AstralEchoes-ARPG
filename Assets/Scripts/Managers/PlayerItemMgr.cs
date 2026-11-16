using System.Collections.Generic;
using UnityEngine.Events;

public class PlayerItemMgr :BaseManager<PlayerItemMgr>
{
    private PlayerItemSaveData saveData;
    public event UnityAction<int,int> OnItemCountChanged;
    private PlayerItemMgr()
    {
        saveData = JsonMgr.Instance.LoadData<PlayerItemSaveData>($"PlayerItemSaveData"+PlayerMgr.Instance.currentPlayerId);
    }
    public int GetItemCount(int itemID)
    {
        int count = 0;
        foreach (ItemCountData data in saveData.items)
        {
            if (data.itemID == itemID)
            {
                count = data.itemCount;
            }
        }
        return count;
    }

    public void AddItem(int itemID, int itemCount)
    {
        bool added = false;
        foreach (ItemCountData data in saveData.items)
        {
            if (data.itemID == itemID)
            {
                data.itemCount += itemCount;
                added = true;
                OnItemCountChanged?.Invoke(itemID,data.itemCount);
                Save();
                return;
            }
        }

        if (!added)
        {
            saveData.items.Add(new ItemCountData(itemID, itemCount));
        }
        OnItemCountChanged?.Invoke(itemID,itemCount);
        Save();
    }

    public bool RemoveItem(int itemID, int count)
    {
        bool removed = false;
        foreach (ItemCountData data in saveData.items)
        {
            if (data.itemID == itemID)
            {
                data.itemCount -= count;
                if (data.itemCount < 0)
                {
                    data.itemCount += count;
                    //TODO 后续可以弹出文字告诉数量不足
                    return false;
                }
                removed = true;
                OnItemCountChanged?.Invoke(itemID,data.itemCount);
                if (data.itemCount == 0)
                {
                    saveData.items.Remove(data);
                    //C# 不允许在 foreach 遍历 List 时修改它 删除后循环取下一个元素时会抛 InvalidOperationException
                    Save();
                    return true;
                }
            }
        }
        Save();
        return removed;
    }

    public IReadOnlyList<ItemCountData> GetAllItems()
    {
        IReadOnlyList<ItemCountData> items = saveData.items;
        return items;
    }
    private void Save()
    {
        JsonMgr.Instance.SaveData(saveData, $"PlayerItemSaveData"+PlayerMgr.Instance.currentPlayerId);
    }
}
