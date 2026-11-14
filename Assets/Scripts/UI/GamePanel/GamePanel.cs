using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GamePanel : BasePanel
{
    
    [SerializeField] private PlayerHealthBar playerHealthBar;
    [SerializeField] private InteractPanel interactPanel;
    public Button btnMission;
    public override void Init()
    {
        btnMission.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel<MissionPanel>();
            PlayerMgr.Instance.inputActions.Disable();
        });
    }
    protected override void Update()
    {
        base.Update();
        if (UIManager.Instance.GetPanel<MissionPanel>() != null)
            return;
        if (Keyboard.current.leftAltKey.wasPressedThisFrame)
            PlayerMgr.Instance.SetCursorState(true);
        if (Keyboard.current.leftAltKey.wasReleasedThisFrame)
            PlayerMgr.Instance.SetCursorState(false);
    }

    // public void OpenCloseMouseInput(bool state)
    // {
    //     
    // }
    public void UpdateHpBar(int currentHp, int maxHp)
    {
        playerHealthBar.UpdatePlayerHealthBar(currentHp, maxHp);
    }
    public void OpenInteractPanel()
    {
        interactPanel = UIManager.Instance.ShowPanel<InteractPanel>();
    }

    public void CloseInteractPanel()
    {
        UIManager.Instance.HidePanel<InteractPanel>(false);
    }
    public override void ShowMe()
    {
        base.ShowMe();
        PlayerMgr.Instance.inputActions.Enable();
    }
}
