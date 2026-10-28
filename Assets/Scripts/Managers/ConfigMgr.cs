using System;
using System.Collections.Generic;
using UnityEngine;

public class ConfigMgr:BaseManager<ConfigMgr>
{
    public Dictionary<CharacterConfig.CharacterType,CharacterConfig>  datas = new Dictionary<CharacterConfig.CharacterType, CharacterConfig>();

    private ConfigMgr()
    {
        foreach (CharacterConfig.CharacterType value in Enum.GetValues(typeof(CharacterConfig.CharacterType)))
        {
            CharacterConfig config = Resources.Load<CharacterConfig>("GameData/Character/"+value+"Config");
            datas.Add(value, config);
        }
    }

    public CharacterConfig.CharacterData GetCharacterData(CharacterConfig.CharacterType type,int id)
    {
        if (datas.ContainsKey(type))
        {
            return datas[type].characterDatas[id];
        }
        else
        {
            Debug.Log("数据源为空");
            return null;
        }
    }
}
