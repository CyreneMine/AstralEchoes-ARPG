using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    public static UIManager instance { get; } = new UIManager();
    private Transform canvas;
    private UIManager()
    {
        GameObject canvasObj = GameObject.Instantiate(Resources.Load<GameObject>("UI/Canvas"));
        canvas = canvasObj.transform;
        GameObject.DontDestroyOnLoad(canvasObj);
    }
    private Dictionary<string, BasePanel> panels = new Dictionary<string, BasePanel>();
    
    public T ShowPanel<T>() where T : BasePanel
    {
        string name = typeof(T).Name;
        if (panels.ContainsKey(name))
        {
            return panels[name] as T;
        }
        GameObject panel = GameObject.Instantiate(Resources.Load<GameObject>($"UI/{name}"));
        panel.transform.SetParent(canvas,false);
        T panelScript = panel.GetComponent<T>();
        panels.Add(name,panelScript);
        panelScript.ShowMe();
        return panelScript;
    }
    public void HidePanel<T>(bool isFade = true) where T : BasePanel
    {
        string name = typeof(T).Name;
        if (panels.ContainsKey(name))
        {
            if (isFade)
            {
                panels[name].HideMe(() =>
                {
                    GameObject.Destroy(panels[name].gameObject);
                    panels.Remove(name);
                });
            }
            else
            {
                GameObject.Destroy(panels[name].gameObject);
                panels.Remove(name);
            }
        }
    }
    public T GetPanel<T>() where T : BasePanel
    {
        string name = typeof(T).Name;
        if (panels.ContainsKey(name))
        {
            return panels[name] as T;
        }
        return null;
    }
}
