using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerMgr:BaseManager<TimerMgr>
{
    private Dictionary<int,TimeData> timers = new Dictionary<int,TimeData>();
    private List<int> removeList = new List<int>();
    private int index = 0;
    private TimerMgr()
    {
        MonoMgr.Instance.OnUpdate += Update;
    }
    public void Update()
    {
        removeList.Clear();
        if (timers.Count > 0)
        {
            foreach (KeyValuePair<int, TimeData> timer in timers)
            {
                timer.Value.remaining -= Time.deltaTime;
                if (timer.Value.remaining <= 0)
                {
                    removeList.Add(timer.Key);
                }
            }
            if (removeList.Count == 0)
                return;
            foreach (int i in removeList)
            {
                //如果已经被删除了 就找下一个
                if (!timers.TryGetValue(i, out TimeData timeData))
                    continue;
                RemoveTimer(i);
                timeData.action.Invoke();
            }
        }
    }
    /// <summary>
    /// 添加计时器 返回对应下标
    /// </summary>
    /// <param name="remaining"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public int AddTimer(float remaining, UnityAction action)
    {
        var timeData = new TimeData(remaining, action);
        timers.Add(index, timeData);
        return index++;
    }

    public bool RemoveTimer(int removeIndex)
    {
        return timers.Remove(removeIndex);
    }

    public float GetRemainingByIndex(int currentIndex)
    {
        float remaining;
        if (timers.TryGetValue(currentIndex, out TimeData timeData))
            remaining = timeData.remaining;
        else
            remaining = -1; //-1 表示已经结束或者不存在
        return remaining;
    }
}
public class TimeData
{
    public TimeData(float remaining, UnityAction action)
    {
        this.remaining = remaining;
        this.action = action;
    }
    public float remaining;//剩余时间
    public UnityAction action;
}
