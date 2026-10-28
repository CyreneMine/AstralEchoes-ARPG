public class PlayerMgr :BaseManager<PlayerMgr>
{
    public PlayerObject player;
    public InputSystem_Actions inputActions;
    //0为目前测试的单手剑
    public int currentWeaponType = 0;
    private PlayerMgr()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
    }

    public void InitPlayer(PlayerObject player)
    {
        this.player = player;
    }

    public void CloseInput()
    {
        inputActions.Disable();
    }

    public void OpenInput()
    {
        inputActions.Enable();
    }
    public void ClearPlayerMgr()
    {
        //TODO 游戏结束时候调用函数来释放
        player = null;
        inputActions.Disable();
    }
}
