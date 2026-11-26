using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuffPanel :BasePanel
{
    public GameObject optionObj;
    public Transform optionParent;
    public Button btnClose;
    public override void Init()
    {
        BuffMgr.Instance.StartRandomBuff();
        List<BuffConfig> randomBuffs = BuffMgr.Instance.RandomBuffs;
        if (randomBuffs.Count <= 0)
        {
            UIManager.Instance.HidePanel<BuffPanel>();
            UIManager.Instance.ShowPanel<GamePanel>();
        }
        foreach (BuffConfig buff in randomBuffs)
        {
            GameObject obj = Instantiate(optionObj, optionParent,false);
            BuffOption option = obj.GetComponent<BuffOption>();
            option.Init(buff);
        }
        btnClose.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<BuffPanel>();
            UIManager.Instance.ShowPanel<GamePanel>();
        });
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
