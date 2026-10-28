using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractPanel : BasePanel
{
    public Button[] buttonObjs;
    public GameObject otherObj;

    protected override void Update()
    {
        base.Update();
        if (gameObject.activeSelf)
        {
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                ClickDialogue();
            }
        }
    }

    public void SetOtherObj(GameObject obj)
    {
        otherObj = obj;
    }
    public override void Init()
    {
        buttonObjs = transform.GetComponentsInChildren<Button>();
        if (buttonObjs == null || buttonObjs.Length > 0)
        {
            foreach (Button buttonObj in buttonObjs)
            {
                //TODO 暂时写法 后续用数据结构统一管理
                Button button = buttonObj;
                if (button.GetComponentInChildren<TMP_Text>().text.Equals("对话"))
                {
                    button.onClick.AddListener(ClickDialogue);
                }
                else
                {
                    //TODO 留空后续补全功能
                }
                
            }
        }
    }
    public void ClickDialogue()
    {
        Cursor.lockState = CursorLockMode.None;
        //如果触发按钮显示 必定存在GamePanel
        if (otherObj.GetComponent<TestDialogue>() !=null)
        {
            otherObj.GetComponent<TestDialogue>().StartDialogue();
        }
        UIManager.Instance.GetPanel<GamePanel>().CloseInteractPanel();
        otherObj.GetComponent<TestDialogue>().ReStartOverlap();
    }

    public override void ShowMe()
    {
        base.ShowMe();
    }

    public override void HideMe(UnityAction callback = null)
    {
        base.HideMe(callback);
    }
}
