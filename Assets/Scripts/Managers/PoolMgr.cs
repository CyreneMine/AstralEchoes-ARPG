using System.Collections.Generic;
using UnityEngine;

public class PoolMgr : BaseManager<PoolMgr>
{
    public Dictionary<string,Stack<GameObject>> poolDic = new Dictionary<string,Stack<GameObject>>();
    private PoolMgr(){}

    public GameObject GetObj(string poolName)
    {
        GameObject obj;
        if (poolDic.ContainsKey(poolName))
        {
            obj = poolDic[poolName].Pop();
            obj.gameObject.SetActive(true);
        }
        else
        {
            obj = GameObject.Instantiate(Resources.Load("PoolPrefabs"+poolName)) as GameObject;
            obj.name = poolName;
        }
        return obj;
    }

    public void PushObj(GameObject obj)
    {
        obj.gameObject.SetActive(false);
        if (!poolDic.ContainsKey(obj.name))
            poolDic.Add(obj.name,new Stack<GameObject>());
        poolDic[obj.name].Push(obj);
    }

    public void ClearPool()
    {
        poolDic.Clear();
    }
}
