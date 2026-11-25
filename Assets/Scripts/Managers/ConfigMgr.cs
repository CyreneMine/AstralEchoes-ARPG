using System;
using System.Collections.Generic;
using UnityEngine;

public class ConfigMgr:BaseManager<ConfigMgr>
{
    public Dictionary<CharacterConfig.CharacterType,CharacterConfig> characterConfigs = new Dictionary<CharacterConfig.CharacterType, CharacterConfig>();
    public MissionConfig missionConfig;
    public NpcConfig npcConfig;
    public ItemDatabase itemDatabase;
    public EnemyRewardDatabase enemyRewardDatabase;
    public BuffDatabase buffDatabase;
    private ConfigMgr()
    {
        foreach (CharacterConfig.CharacterType value in Enum.GetValues(typeof(CharacterConfig.CharacterType)))
        {
            CharacterConfig config = Resources.Load<CharacterConfig>("GameData/Character/"+value+"Config");
            characterConfigs.Add(value, config);
        }
        missionConfig = Resources.Load<MissionConfig>("GameData/Mission/MissionConfig");
        npcConfig = Resources.Load<NpcConfig>("GameData/Npc/NpcConfig");
        itemDatabase = Resources.Load<ItemDatabase>("GameData/Item/ItemDatabase");
        enemyRewardDatabase = Resources.Load<EnemyRewardDatabase>("GameData/Enemy/EnemyRewardDatabase");
        buffDatabase = Resources.Load<BuffDatabase>("GameData/Buff/BuffDatabase");
    }

    public BuffConfig GetBuffConfigById(int buffId)
    {
        foreach (BuffConfig config in buffDatabase.buffs)
        {
            if (config.id == buffId)
            {
                return config;
            }
        }
        return null;
    }

    public List<BuffConfig> GetBuffConfigs()
    {
        return buffDatabase.buffs;
    }
    public EnemyReward GetEnemyRewardById(int id)
    {
        EnemyReward e = null;
        foreach (EnemyReward reward in enemyRewardDatabase.data)
        {
            if (reward.EnemyId == id)
            {
                e = reward;
                break;
            }
        }
        return e;
    }
    public ItemConfig GetItemConfigByID(int itemId)
    {
        ItemConfig data = null;
        foreach (ItemConfig item in itemDatabase.items)
        {
            if (item.itemID == itemId)
            {
                data = item;
                break;
            }
        }
        return data;
    }
    public NpcData GetNpcConfig(int npcId)
    {
        foreach (NpcData data in npcConfig.npcConfigs)
        {
            if (data.id == npcId)
            {
                return data;
            }
        }
        return null;
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
