using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BagPanel : BasePanel
{
    public DetailPanel detailPanel;
    public Transform bagParent;
    public GameObject bagSlotPrefab;
    public Button btnClose;
    public TMP_Text moneyText;
    private Dictionary<int, BagSlot> bagDataDic = new Dictionary<int, BagSlot>();
    public override void Init()
    {
        PlayerMgr.Instance.SetCursorState(true);
        btnClose.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<BagPanel>();
        });
        UpdateBag();
        PlayerItemMgr.Instance.OnItemCountChanged += OnItemCountChanged;
    }

    
    public void SetDetailById(int id)
    {
        detailPanel.SetDetailById(id);
    }
    public void OnItemCountChanged(int itemId, int newCount)
    {
        UpdateBag();
    }
    public void UpdateBag()
    {
        //金币在被扣除成0的时候数据会保留旧数据因为没有新数据接入
        moneyText.text = "0";
        foreach (Transform child in bagParent)
        {
            Destroy(child.gameObject);
        }
        bagDataDic.Clear();
        int firstSlotId = -1;
        IReadOnlyList<ItemCountData> items = PlayerItemMgr.Instance.GetAllItems();
        foreach (ItemCountData item in items)
        {
            ItemType type = ConfigMgr.Instance.GetItemConfigByID(item.itemID).itemType;
            if (type == ItemType.exp)
                continue;
            if (type == ItemType.money)
            {
                moneyText.text = item.itemCount.ToString();
                continue;
            }
            GameObject obj = Instantiate(bagSlotPrefab,bagParent,false);
            BagSlot bagSlot = obj.GetComponent<BagSlot>();
            bagSlot.Init(item.itemID, item.itemCount);
            bagDataDic.Add(item.itemID,bagSlot);
            if (firstSlotId < 0)
                firstSlotId = item.itemID;
        }
        if (firstSlotId >= 0)
            detailPanel.SetDetailById(firstSlotId);
        else
        {
            detailPanel.Clear();
        }
    }
    private void OnDestroy()
    {
        PlayerItemMgr.Instance.OnItemCountChanged -= OnItemCountChanged;
    }
    
    public override void ShowMe()
    {
        base.ShowMe();
        UIManager.Instance.HidePanel<GamePanel>();
        Time.timeScale = 0;
    }

    public override void HideMe(UnityAction callback = null)
    {
        callback += () =>
        {
            PlayerMgr.Instance.SetCursorState(false);
            Time.timeScale = 1;
        };
        base.HideMe(callback);
        UIManager.Instance.ShowPanel<GamePanel>();
    }
}
