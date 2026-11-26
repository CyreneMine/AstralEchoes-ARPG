using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffOption :MonoBehaviour
{
    public TMP_Text txtName, txtDesc;
    public Image imgIcon;
    public Button btnThis;

    public void Init(BuffConfig config)
    {
        txtName.text = config.buffName;
        txtDesc.text = config.buffDesc;
        switch (config.buffType)
        {
            case BuffType.atk:
                imgIcon.sprite = Resources.Load<Sprite>("Icon/Buff/0");
                break;
            case BuffType.def:
                imgIcon.sprite = Resources.Load<Sprite>("Icon/Buff/1");
                break;
            case BuffType.maxHp:
                imgIcon.sprite = Resources.Load<Sprite>("Icon/Buff/2");
                break;
        }
        btnThis.onClick.AddListener(() =>
        {
            BuffMgr.Instance.SelectBuff(config.id);
            UIManager.Instance.HidePanel<BuffPanel>();
        });
    }
}
