using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "SO/ItemDatabase", order = 1)]
public class ItemDatabase:ScriptableObject
{
    public List<ItemConfig> items;
}
[System.Serializable]
public class ItemConfig
{
    public int itemID;
    public string itemName;
    public string itemDesc;
    public Sprite itemIcon;
    public int maxCount;
    public ItemType itemType;
}
public enum ItemType
{
    exp,
    money,
    consumable,
    missionItem,
    stuff
}