using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyRewardDatabase",menuName = "SO/EnemyRewardDatabase", order = 1)]
public class EnemyRewardDatabase:ScriptableObject
{
    public List<EnemyReward> data;
}
[System.Serializable]
public class EnemyReward
{
    public int EnemyId;
    public List<ItemRewardData> rewards;
}
