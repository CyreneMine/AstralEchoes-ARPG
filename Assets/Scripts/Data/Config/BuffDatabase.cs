using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName =  "BuffDatabase", menuName = "SO/BuffDatabase")]
public class BuffDatabase:ScriptableObject
{
    public List<BuffConfig> buffs = new List<BuffConfig>();
}
[System.Serializable]
public class BuffConfig
{
    public int id;
    public string buffName;
    public string buffDesc;
    public float percent;
    public float totalTime;
    public BuffType buffType;
}
public enum BuffType
{
    atk,
    def,
    maxHp
    //TODO 有时间可以扩展 比如移速 攻速等等等等
}