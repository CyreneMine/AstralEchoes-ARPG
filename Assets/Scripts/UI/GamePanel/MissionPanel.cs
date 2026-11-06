using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MissionPanel : BasePanel
{
    public TMP_Text txtTarget,txtDescription,txtTitleName;
    public GameObject reward;
    public GameObject missionListContent;
    public Button closeBtn;
    private bool hasSelected = false;
    public override void Init()
    {
        Cursor.lockState = CursorLockMode.None;
        closeBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<MissionPanel>();
            Cursor.lockState = CursorLockMode.Locked;
        });
        List<MissionSaveData> list = MissionMgr.Instance.playerMissionProgress.missions;
        foreach (MissionSaveData data in list)
        {
            if (data.isFinished)
                continue;
            GameObject obj = Instantiate( Resources.Load<GameObject>("UI/MissionItem"));
            obj.transform.SetParent(missionListContent.transform);
            MissionItem missionItem = obj.GetComponent<MissionItem>();
            missionItem.Init(data.missionId,txtTarget,txtDescription,reward,txtTitleName);
            if (!hasSelected)
            {
                missionItem.OnClick();
                hasSelected = true;
            }
        }
    }

    public override void ShowMe()
    {
        base.ShowMe();
        UIManager.Instance.HidePanel<GamePanel>();
    }

    public override void HideMe(UnityAction callback = null)
    {
        base.HideMe(callback);
        UIManager.Instance.ShowPanel<GamePanel>();
    }

    protected override void Update()
    {
        base.Update();
    }
}
