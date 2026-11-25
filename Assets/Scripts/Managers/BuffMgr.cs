using System.Collections.Generic;
using UnityEngine;

public class BuffMgr:BaseManager<BuffMgr>
{
    private List<BuffConfig> buffConfigs;
    private List<BuffConfig> hasBuffs = new List<BuffConfig>();
    private List<BuffConfig> remainingBuffs = new List<BuffConfig>();
    private Dictionary<int, int> timers = new Dictionary<int, int>();
    public int poolCount => remainingBuffs.Count;
    private BuffMgr()
    {
        buffConfigs = new List<BuffConfig>(ConfigMgr.Instance.GetBuffConfigs());
        remainingBuffs = new List<BuffConfig>(buffConfigs);
    }
    public void AddBuff(int id)
    {
        //给予buff
        BuffConfig buffConfig = ConfigMgr.Instance.GetBuffConfigById(id);
        if (buffConfig == null)
        {
            Debug.LogError("没有这个id的buff");
            return;
        }
        hasBuffs.Add(buffConfig);
        remainingBuffs.Remove(buffConfig);
        int timerId = TimerMgr.Instance.AddTimer(buffConfig.totalTime, () =>
        {
            RemoveBuff(id);
        });
        timers.Add(id, timerId);
        PlayerMgr.Instance.player.AddBuff(buffConfig.buffType,buffConfig.percent);
    }
    public float GetBuffRemaining(int currentIndex)
    {
        //currentIndex buff的配置id       timerId 计时器id
        float remaining = 0;
        if(timers.TryGetValue(currentIndex, out int timerId))
        {
            remaining = TimerMgr.Instance.GetRemainingByIndex(timerId);    
        }
        return remaining;
    }
    public void RemoveBuff(int id)
    {
        foreach (BuffConfig config in hasBuffs)
        {
            if (config.id == id)
            {
                PlayerMgr.Instance.player.AddBuff(config.buffType,-config.percent);
                hasBuffs.Remove(config);
                remainingBuffs.Add(config);
                timers.Remove(id);
                return;
            }
        }
    }

    public void ClearAll()
    {
        foreach (KeyValuePair<int, int> timer in timers)
        {
            TimerMgr.Instance.RemoveTimer(timer.Value);
        }
        hasBuffs.Clear();
        timers.Clear();
        buffConfigs = new List<BuffConfig>(ConfigMgr.Instance.GetBuffConfigs());
        remainingBuffs = new List<BuffConfig>(buffConfigs);
        PlayerMgr.Instance.player.ClearBuff();
    }
}
