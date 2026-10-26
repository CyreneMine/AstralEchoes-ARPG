using UnityEngine;

public class GamePanel : BasePanel
{
    [SerializeField] private InteractPanel interactPanel;
    public override void Init()
    {
        
    }

    public void OpenInteractPanel()
    {
        interactPanel = UIManager.Instance.ShowPanel<InteractPanel>();
    }

    public void CloseInteractPanel()
    {
        UIManager.Instance.HidePanel<InteractPanel>(false);
    }
}
