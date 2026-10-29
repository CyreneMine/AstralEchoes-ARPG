using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GamePanel : BasePanel
{
    [SerializeField] private InteractPanel interactPanel;
    public Button btnMission;
    public override void Init()
    {
        btnMission.onClick.AddListener(() =>
        {
            Cursor.lockState = CursorLockMode.None;
            UIManager.Instance.ShowPanel<MissionPanel>();
            PlayerMgr.Instance.inputActions.Disable();
        });
    }
    public void OpenInteractPanel()
    {
        interactPanel = UIManager.Instance.ShowPanel<InteractPanel>();
    }

    public void CloseInteractPanel()
    {
        UIManager.Instance.HidePanel<InteractPanel>(false);
    }

    protected override void Update()
    {
        base.Update();
        if (Keyboard.current.leftAltKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (Keyboard.current.leftAltKey.wasReleasedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public override void ShowMe()
    {
        base.ShowMe();
        PlayerMgr.Instance.inputActions.Enable();
    }
}
