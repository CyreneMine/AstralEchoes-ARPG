public class PlayerMgr
{
    public static PlayerMgr instance { get; } = new PlayerMgr();
    public PlayerObject player;
    private PlayerMgr()
    {}

    public void InitPlayer(PlayerObject player)
    {
        this.player = player;
    }
}
