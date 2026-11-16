using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BagPanel : BasePanel
{
    public Transform bagParent;
    public GameObject bagSlotPrefab;
    public Button btnClose;
    public override void Init()
    {
        PlayerMgr.Instance.SetCursorState(true);
        btnClose.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<BagPanel>();
        });
        //TODO 开始创建slot
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
