using System;
using System.Collections.Generic;

public class MissionMgr:BaseManager<MissionMgr>
{
    public MissionProgressData playerMissionProgress;

    private MissionMgr()
    {
        playerMissionProgress = JsonMgr.Instance.LoadData<MissionProgressData>("MissionData");
    }

    public int GetStageIndex(int missionId)
    {
        foreach (MissionSaveData data in playerMissionProgress.missions)
        {
            if (data.missionId == missionId)
            {
                return data.currentStageIndex;
            }
        }

        return 0;
    }
    /// <summary>
    /// 检查是否完成了任务
    /// </summary>
    /// <param name="missionType"></param>
    /// <param name="targetId"></param>
    /// <param name="amount"></param>
    public void CheckFinishMission(MissionType missionType,int targetId,int amount = 1)
    {
        switch (missionType)
        {
            case MissionType.Battle:
                foreach (MissionSaveData data in playerMissionProgress.missions)
                {
                    MissionData missionData = ConfigMgr.Instance.GetMissionData(data.missionId);
                    if (missionData.stages[data.currentStageIndex].targetId == targetId
                        &&ConfigMgr.Instance.GetMissionData(data.missionId)
                            .stages[MissionMgr.Instance.GetMissionSaveData(data.missionId).currentStageIndex]
                            .missionType == MissionType.Battle)
                    {
                        if (data.status)
                            continue;
                        data.currentCount += amount;
                        if (data.currentCount >= missionData.stages[data.currentStageIndex].targetCount)
                        {
                            data.status = true;
                        }
                        if (data.status&&missionData.stages.Count-1 == data.currentStageIndex)
                        {
                            FinishMission(data.missionId);
                        }
                    }
                    if (data.status && data.status&&missionData.stages.Count-1 > data.currentStageIndex)
                    {
                        UpdateStage(data,missionData);
                    }
                }
                JsonMgr.Instance.SaveData(playerMissionProgress,"MissionData");
                break;
            case MissionType.Dialogue:
                foreach (MissionSaveData data in playerMissionProgress.missions)
                {
                    MissionData missionData = ConfigMgr.Instance.GetMissionData(data.missionId);
                    if (targetId == missionData.stages[data.currentStageIndex].targetId)
                    {
                        data.status = true;
                        if (data.status&&missionData.stages.Count-1 == data.currentStageIndex)
                        {
                            FinishMission(data.missionId);
                        }
                    }
                }
                JsonMgr.Instance.SaveData(playerMissionProgress,"MissionData");
                break;
            case MissionType.Interaction:
                
                break;
        }
    }
    /// <summary>
    /// 结算任务
    /// </summary>
    /// <param name="missionId"></param>
    public void FinishMission(int missionId)
    {
        foreach (MissionSaveData data in playerMissionProgress.missions)
        {
            if (data.missionId == missionId)
            {
                if (!data.isFinished)
                {
                    MissionData missionData = ConfigMgr.Instance.GetMissionData(missionId);
                    List<ItemRewardData> rewards = missionData.itemRewards;
                    data.isFinished = true;
                    RewardMgr.Instance.SettlementReward(rewards);
                }
            }
        }
       
    }
    public MissionSaveData GetMissionSaveData(int missionId)
    {
        foreach (MissionSaveData data in playerMissionProgress.missions)
        {
            if (data.missionId == missionId)
            {
                return data;
            }
        }
        return null;
    }
    public void AcceptMission(int missionId)
    {
        MissionSaveData missionSaveData;
        MissionData  missionData;
        if (!GetAcceptMission(missionId,out missionSaveData,out missionData))
        {
            foreach (MissionData data in ConfigMgr.Instance.missionConfig.missionDatas)
            {
                if (data.missionId == missionId)
                {
                    MissionSaveData newMission = new MissionSaveData();
                    newMission.missionId = missionId;
                    newMission.currentStageIndex = 0;
                    newMission.currentCount = 0;
                    newMission.status = false;
                    playerMissionProgress.missions.Add(newMission);
                    JsonMgr.Instance.SaveData(playerMissionProgress,"MissionData");
                    break;
                }
            }
            
        }
        else
        {
            UpdateStage(missionSaveData,missionData);
        }
    }
    public bool GetAcceptMission(int missionId,out MissionSaveData missionSaveData,out MissionData missionData)
    {
        missionSaveData = null;
        missionData = null;
        bool accepted = false;
        for (int j = 0; j < playerMissionProgress.missions.Count; j++)
        {
            if (playerMissionProgress.missions[j].missionId == missionId)
            {
                missionSaveData = playerMissionProgress.missions[j];
                accepted = true;
                foreach (MissionData data in ConfigMgr.Instance.missionConfig.missionDatas)
                {
                    if (data.missionId == missionId)
                    {
                        if (data.stages.Count-1 > playerMissionProgress.missions[j].currentStageIndex)
                        {
                            missionData = data;
                            break;
                        }
                    }
                }
            }
        }
        return accepted;
    }

    public void UpdateStage(MissionSaveData missionSaveData,MissionData missionData)
    {
        if (missionData != null)
        {
            missionSaveData.currentStageIndex++;
            missionSaveData.currentCount = 0;
            missionSaveData.status = false;
        }
    }
}
