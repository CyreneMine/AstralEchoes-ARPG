using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MissionItem : MonoBehaviour
{
    public TMP_Text txtTitle,txtLocation,txtTarget,txtDescription,txtTitleName;
    public GameObject reward;
    public Button btnThis;
    private MissionData missionData;
    private int missionId;
    /// <summary>
    /// 在代码动态创建它的时候调用来初始化
    /// </summary>
    /// <param name="missionId"></param>
    public void Init(int missionId,TMP_Text txtTarget,TMP_Text txtDescription,GameObject reward,TMP_Text txtTitleName)
    {
        this.txtTitleName = txtTitleName;
        this.missionId = missionId;
        this.reward = reward;
        this.txtTarget = txtTarget;
        this.txtDescription = txtDescription;
        missionData = ConfigMgr.Instance.GetMissionData(missionId);
        txtTitle.text = missionData.missionName;
        //后续根据任务目标给出坐标
        txtLocation.text = "GMScene";
        btnThis.onClick.AddListener(() =>
        {
            OnClick();
        });
    }

    public void OnClick()
    {
        foreach (Transform child in reward.transform)
        {
            Destroy(child.gameObject);
        }
        MissionStage nowMS = new MissionStage();
        nowMS = missionData.stages[MissionMgr.Instance.GetStageIndex(missionId)];
        txtTitleName.text = missionData.missionName;
        switch (nowMS.missionType)
        {
            //TODO 后续配置表出来后 根据id动态填写名字
            case MissionType.Battle:
                txtTarget.text =$"击败{txtTarget.text = nowMS.ToString()}:({MissionMgr.Instance.GetMissionSaveData(missionId).currentCount}/{nowMS.targetCount})";
                break;
            case MissionType.Dialogue:
                txtTarget.text = $"与{ConfigMgr.Instance.npcConfig.npcConfigs[nowMS.targetId].displayName}对话";
                break;
            case MissionType.Interaction:
                txtTarget.text = $"前往{nowMS.targetId}区域";
                break;
        }
        txtDescription.text = missionData.missionDescription;
        foreach (ItemRewardData data in missionData.itemRewards)
        {
            GameObject obj = Instantiate(Resources.Load<GameObject>("UI/RewardSlot"));
            obj.transform.SetParent(reward.transform,false);
            RewardSlot rewardSlot = obj.GetComponent<RewardSlot>();
            rewardSlot.Init(data.itemId,data.count);
        }
    }
}
