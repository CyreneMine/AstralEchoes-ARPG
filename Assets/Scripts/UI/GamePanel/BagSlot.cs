using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BagSlot : MonoBehaviour
{
    public Image icon;
    public TMP_Text amount;
    public Button btn;
    public void Init(int currentId,int currentAmount)
    {
        icon.sprite = ConfigMgr.Instance.GetItemConfigByID(currentId).itemIcon;
        amount.text = currentAmount.ToString();
    }
}
