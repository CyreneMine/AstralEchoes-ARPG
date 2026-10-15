using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class BasePanel : MonoBehaviour
{
    public abstract void Init();
    private bool isShow = false;
    private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 5f;
    private UnityAction callback;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (isShow && canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += fadeSpeed * Time.deltaTime;
            if (canvasGroup.alpha >= 1)
            {
                canvasGroup.alpha = 1;
            }
        }else if (!isShow && canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
            if (canvasGroup.alpha <= 0)
            {
                canvasGroup.alpha = 0;
                callback?.Invoke();
            }
        }
    }
    public virtual void ShowMe()
    {
        canvasGroup.alpha = 0;
        isShow = true;
    }
    public virtual void HideMe(UnityAction callback = null)
    {
        canvasGroup.alpha = 1;
        this.callback = callback;
        isShow = false;
    }
}
