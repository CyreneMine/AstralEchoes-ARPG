using System;
using System.Collections.Generic;
using UnityEngine;

public class ConfigMgr:BaseManager<ConfigMgr>
{
    public Dictionary<CharacterConfig.CharacterType,CharacterConfig> characterConfigs = new Dictionary<CharacterConfig.CharacterType, CharacterConfig>();
    public MissionConfig missionConfig;
    private ConfigMgr()
    {
        foreach (CharacterConfig.CharacterType value in Enum.GetValues(typeof(CharacterConfig.CharacterType)))
        {
            CharacterConfig config = Resources.Load<CharacterConfig>("GameData/Character/"+value+"Config");
            characterConfigs.Add(value, config);
        }
        missionConfig = Resources.Load<MissionConfig>("GameData/Mission/MissionConfig");
    }
    public CharacterConfig.CharacterData GetCharacterData(CharacterConfig.CharacterType type,int id)
    {
        if (characterConfigs.ContainsKey(type))
        {
            return characterConfigs[type].characterDatas[id];
        }
        else
        {
            Debug.Log("数据源为空");
            return null;
        }
    }

    public MissionData GetMissionData(int missionId)
    {
        foreach (MissionData data in missionConfig.missionDatas)
        {
            if (data.missionId ==missionId)
            {
                return data;
            }
        }

        return null;
    }
}
