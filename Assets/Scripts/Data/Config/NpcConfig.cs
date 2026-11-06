using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NpcConfig", menuName = "SO/NpcConfig")]
public class NpcConfig:ScriptableObject
{
    public List<NpcData> npcConfigs = new List<NpcData>();
}
[System.Serializable]
public class NpcData
{
    public int id;
    public string displayName;
    public List<int> missionIds = new List<int>();
}

