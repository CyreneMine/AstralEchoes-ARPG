using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardMgr:BaseManager<RewardMgr>
{
    private RewardMgr(){}
    public void SettlementReward(List<ItemRewardData> items)
    {
        foreach (ItemRewardData item in items)
        {
            ItemConfig itemConfig = ConfigMgr.Instance.GetItemConfigByID(item.itemId);
            if (itemConfig == null)
            {
                Debug.LogError("找不到对应道具配置");
                continue;
            }
            switch (itemConfig.itemType)
            {
                case ItemType.exp:
                    PlayerMgr.Instance.AddExp(item.count);
                    break;
                case ItemType.money:
                    break;
                case ItemType.consumable:
                    break;
                case ItemType.missionItem:
                    break;
                case ItemType.stuff:
                    break;
            }
        }
    }
}
