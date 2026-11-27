public class EnemyDeadState : EnemyState
{
    public EnemyDeadState(EnemyAI owner) : base(owner){}

    public override void Enter()
    {
        owner.Agent.isStopped = true;
        //TODO 可以进行入池 后续补全
    }

    public override void Update()
    {
        
    }

    public override void Exit()
    {
        owner.Agent.isStopped = false;
    }
}