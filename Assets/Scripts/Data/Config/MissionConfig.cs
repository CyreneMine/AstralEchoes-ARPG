using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "MissionConfig", menuName = "SO/MissionConfig")]
public class MissionConfig : ScriptableObject
{
    public List<MissionData> missionDatas;
}
[System.Serializable]
public class MissionData
{
    public string missionName;
    public string missionDescription;
    public int missionId;
    public List<MissionStage> stages;
    public List<ItemRewardData> itemRewards;
}

public enum MissionType
{
    Battle,
    Dialogue,
    Interaction
}

[System.Serializable]
public class MissionStage
{
    public MissionType missionType;
    public int targetCount;
    public int targetId;
}
