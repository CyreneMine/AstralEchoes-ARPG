using UnityEngine;
using UnityEngine.InputSystem;

public class TestDialogue : MonoBehaviour
{
    private InteractPanel interactPanel;
    private bool canOverlap = true;
    void Start()
    {
        //预热面板 避免卡顿
        UIManager.Instance.ShowPanel<InteractPanel>();
        UIManager.Instance.HidePanel<InteractPanel>();
    }
    
    void Update()
    {
        if (UIManager.Instance.GetPanel<DialoguePanel>() != null)
            return;

        // GamePanel gamePanel = UIManager.instance.GetPanel<GamePanel>();
        // if (gamePanel == null)
        //     return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f,
            1 << LayerMask.NameToLayer("Player"));
        // 持续检查距离，离开范围时关闭提示并允许下次重新显示。
        if (colliders == null || colliders.Length == 0)
        {
            if (UIManager.Instance.GetPanel<InteractPanel>() != null)
                UIManager.Instance.GetPanel<GamePanel>().CloseInteractPanel();
            ReStartOverlap();
            return;
        }

        // 只限制重复打开提示，不停止上面的距离检测。
        if (!canOverlap)
            return;
        UIManager.Instance.GetPanel<GamePanel>().OpenInteractPanel();
        interactPanel = UIManager.Instance.GetPanel<InteractPanel>();
        if (interactPanel == null)
            return;
        interactPanel.SetOtherObj(gameObject);
        canOverlap = false;
    }

    public void StartDialogue()
    {
        UIManager.Instance.ShowPanel<DialoguePanel>();
        DialogueMgr.Instance.StartDialogue("Test/Test1_1");
    }
    public void ReStartOverlap()
    {
        canOverlap = true;
    }
}
