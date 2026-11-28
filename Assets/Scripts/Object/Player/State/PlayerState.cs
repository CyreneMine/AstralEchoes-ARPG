public abstract class PlayerState :IState
{
    protected PlayerController owner;

    public PlayerState(PlayerController owner)
    {
        this.owner = owner;
    }
    public abstract void Enter();

    public abstract void Update();

    public abstract void Exit();
}
