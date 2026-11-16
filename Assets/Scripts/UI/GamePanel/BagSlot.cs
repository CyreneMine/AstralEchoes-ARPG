using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BagSlot : MonoBehaviour
{
    public Image icon;
    public TMP_Text amount;

    public void Init(Sprite sprite,int currentAmount)
    {
        icon.sprite = sprite;
        amount.text = currentAmount.ToString();
    }
}
