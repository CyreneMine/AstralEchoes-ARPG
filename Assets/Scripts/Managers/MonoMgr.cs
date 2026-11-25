using System;
using UnityEngine;
using UnityEngine.Events;

public class MonoMgr :SingletonMono<MonoMgr>
{
    public event UnityAction OnUpdate;
    private void Awake()
    {
        
    }
    private void Update()
    {
        OnUpdate?.Invoke();
    }
}
