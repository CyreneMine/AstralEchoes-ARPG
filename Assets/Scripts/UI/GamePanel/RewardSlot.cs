using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSlot : MonoBehaviour
{
    public TMP_Text txtName,txtAmount;
    public Image imgIcon;
    public void Init(int itemId,int itemAmount)
    {
        ItemConfig config = ConfigMgr.Instance.GetItemConfigByID(itemId);
        imgIcon.sprite = config.itemIcon;
        txtName.text = null;
        if (config.itemIcon == null)
        {
            imgIcon.enabled = false;
            txtName.text = config.itemName;
        }
        txtAmount.text = itemAmount.ToString();
    }
}
