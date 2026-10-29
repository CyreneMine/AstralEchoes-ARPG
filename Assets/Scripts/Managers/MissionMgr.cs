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
            if (data.missionId==missionId)
            {
                return data.currentStageIndex;
            }
        }
        return 0;
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
        }
    }
}
