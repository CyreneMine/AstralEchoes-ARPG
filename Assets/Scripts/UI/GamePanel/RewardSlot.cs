using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardSlot : MonoBehaviour
{
    public TMP_Text txtName,txtAmount;
    public void Init(int itemId,int itemAmount)
    {
        txtName.text = ConfigMgr.Instance.GetItemConfigByID(itemId).itemName;
        txtAmount.text = itemAmount.ToString();
    }
}
