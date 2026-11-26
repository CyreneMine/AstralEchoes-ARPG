using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BuffMgr : BaseManager<BuffMgr>
{
    private int choiceCount = 0;
    public int ChoiceCount => choiceCount;
    private List<BuffConfig> buffConfigs;
    private List<BuffConfig> hasBuffs = new List<BuffConfig>();
    private List<BuffConfig> remainingBuffs = new List<BuffConfig>();
    private List<BuffConfig> randomBuffs = new List<BuffConfig>();
    public List<BuffConfig> RandomBuffs => randomBuffs;
    private Dictionary<int, int> timers = new Dictionary<int, int>();
    public event UnityAction<int> onChoiceCountChange;
    public int poolCount => remainingBuffs.Count;

    private BuffMgr()
    {
        buffConfigs = new List<BuffConfig>(ConfigMgr.Instance.GetBuffConfigs());
        remainingBuffs = new List<BuffConfig>(buffConfigs);
    }
    public void SelectBuff(int id)
    {
        if (!randomBuffs.Contains(ConfigMgr.Instance.GetBuffConfigById(id)))
            return;
        choiceCount--;
        AddBuff(id);
        StartRandomBuff();
        onChoiceCountChange?.Invoke(choiceCount);
    }
    public void AddChoiceCount()
    {
        choiceCount++;
        onChoiceCountChange?.Invoke(choiceCount);
    }
    public void StartRandomBuff()
    {
        randomBuffs.Clear();
        if (remainingBuffs.Count == 0)
        {
            //TODO 这里后面可以改成浮动文字 “buff全部获取没有了 类似的提示”
            return;
        }
        List<int> ids = new List<int>();
        for (int i = 0; i < Mathf.Min(3, remainingBuffs.Count); i++)
        {
            int randomIndex = Random.Range(0, remainingBuffs.Count);
            while (ids.Contains(randomIndex))
            {
                randomIndex = Random.Range(0, remainingBuffs.Count);
            }
            ids.Add(randomIndex);
            randomBuffs.Add(remainingBuffs[randomIndex]);
        }
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
        int timerId = TimerMgr.Instance.AddTimer(buffConfig.totalTime, () => { RemoveBuff(id); });
        timers.Add(id, timerId);
        PlayerMgr.Instance.player.AddBuff(buffConfig.buffType, buffConfig.percent);
    }

    public float GetBuffRemaining(int currentIndex)
    {
        //currentIndex buff的配置id       timerId 计时器id
        float remaining = 0;
        if (timers.TryGetValue(currentIndex, out int timerId))
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
                PlayerMgr.Instance.player.AddBuff(config.buffType, -config.percent);
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

        choiceCount = 0;
        randomBuffs.Clear();
        hasBuffs.Clear();
        timers.Clear();
        buffConfigs = new List<BuffConfig>(ConfigMgr.Instance.GetBuffConfigs());
        remainingBuffs = new List<BuffConfig>(buffConfigs);
        onChoiceCountChange?.Invoke(choiceCount);
        PlayerMgr.Instance.player.ClearBuff();
    }
}