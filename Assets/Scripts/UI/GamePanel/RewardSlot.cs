using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardSlot : MonoBehaviour
{
    public TMP_Text txtName,txtAmount;
    public void Init(int itemId,int itemAmount)
    {
        //后续根据配置表 动态显示物品名字
        txtName.text = "金币";
        txtAmount.text = itemAmount.ToString();
    }
}
