using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailPanel :MonoBehaviour
{
    public TMP_Text txtItemName,txtCount,txtDesc;
    public Image imageIcon;
    private void Awake()
    {
        Clear();
    }
    public void Clear()
    {
        txtCount.text = null;
        txtItemName.text = null;
        txtDesc.text = null;
        imageIcon.enabled = false;
    }
    public void SetDetailById(int itemId)
    {
        //外部已经判断过类型 此处只需要根据传入id设置面板就好了
        imageIcon.enabled = true;
        ItemConfig itemConfig = ConfigMgr.Instance.GetItemConfigByID(itemId);
        txtCount.text = PlayerItemMgr.Instance.GetItemCount(itemId).ToString();
        txtItemName.text = itemConfig.itemName;
        txtDesc.text = itemConfig.itemDesc;
        imageIcon.sprite = itemConfig.itemIcon;
    }
}
