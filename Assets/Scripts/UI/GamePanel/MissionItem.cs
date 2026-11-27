using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MissionItem : MonoBehaviour
{
    public TMP_Text txtTitle, txtLocation, txtTarget, txtDescription, txtTitleName;
    public GameObject reward;
    public Button btnThis;
    private MissionData missionData;
    private int missionId;

    /// <summary>
    /// 在代码动态创建它的时候调用来初始化
    /// </summary>
    /// <param name="missionId"></param>
    public void Init(int missionId, TMP_Text txtTarget, TMP_Text txtDescription, GameObject reward, TMP_Text txtTitleName)
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
        btnThis.onClick.AddListener(() => { OnClick(); });
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
        string targetName;
        switch (nowMS.missionType)
        {
            case MissionType.Battle:
                targetName = ConfigMgr.Instance.GetCharacterData(CharacterConfig.CharacterType.Enemy, nowMS.targetId).name;
                txtTarget.text = $"击败{targetName}:({MissionMgr.Instance.GetMissionSaveData(missionId).currentCount}/{nowMS.targetCount})";
                break;
            case MissionType.Dialogue:
                targetName = ConfigMgr.Instance.GetNpcConfig(nowMS.targetId).displayName;
                txtTarget.text = $"与{targetName}对话";
                break;
            case MissionType.Interaction:
                txtTarget.text = $"前往{nowMS.targetId}区域";
                break;
        }

        txtDescription.text = missionData.missionDescription;
        foreach (ItemRewardData data in missionData.itemRewards)
        {
            GameObject obj = Instantiate(Resources.Load<GameObject>("UI/RewardSlot"));
            obj.transform.SetParent(reward.transform, false);
            RewardSlot rewardSlot = obj.GetComponent<RewardSlot>();
            rewardSlot.Init(data.itemId, data.count);
        }
    }
}