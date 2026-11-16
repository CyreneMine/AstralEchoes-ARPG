using System.Collections.Generic;
public class PlayerItemSaveData
{
    public int playerID;
    public List<ItemCountData> items = new List<ItemCountData>();
}

public class ItemCountData
{
    public ItemCountData()
    {
        
    }
    public ItemCountData(int itemID,int amount)
    {
        this.itemID = itemID;
        itemCount = amount;
    }
    public int itemID;
    public int itemCount;
}
