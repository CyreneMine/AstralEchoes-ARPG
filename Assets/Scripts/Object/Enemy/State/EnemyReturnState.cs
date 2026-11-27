public class EnemyReturnState : EnemyState
{
    public EnemyReturnState(EnemyAI owner) : base(owner){}

    public override void Enter()
    {
        owner.MoveToCurrentPatrolPoint();
    }

    public override void Update()
    {
        if (owner.HasArrived())
            owner.ChangeState(owner.WaitState);
    }

    public override void Exit()
    {
        
    }
}
