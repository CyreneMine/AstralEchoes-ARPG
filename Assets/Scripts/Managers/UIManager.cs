using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager:BaseManager<UIManager>
{
    private Transform canvas;
    private UIManager()
    {
        foreach (Canvas existing in GameObject.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (existing.isRootCanvas && existing.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas = existing.transform;
                break;
            }
        }
        if (canvas == null)
            canvas = GameObject.Instantiate(Resources.Load<GameObject>("UI/Canvas")).transform;
        GameObject.DontDestroyOnLoad(canvas.gameObject);
    }
    private Dictionary<string, BasePanel> panels = new Dictionary<string, BasePanel>();
    
    public T ShowPanel<T>() where T : BasePanel
    {
        string name = typeof(T).Name;
        if (panels.ContainsKey(name))
        {
            return panels[name] as T;
        }
        // 接管场景中已搭建的面板，保留其UI引用和布局。
        T scenePanel = canvas.GetComponentInChildren<T>(true);
        if (scenePanel != null)
        {
            scenePanel.gameObject.SetActive(true);
            panels.Add(name, scenePanel);
            scenePanel.ShowMe();
            return scenePanel;
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
