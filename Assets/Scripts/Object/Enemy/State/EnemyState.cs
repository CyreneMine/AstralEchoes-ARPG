public abstract class EnemyState :IState
{
    protected EnemyAI owner;
    public EnemyState(EnemyAI owner)
    {
        this.owner = owner;
    }
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
