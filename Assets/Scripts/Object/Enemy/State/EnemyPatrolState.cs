using UnityEngine;

public class EnemyPatrolState : EnemyState
{
    public EnemyPatrolState(EnemyAI owner) : base(owner){}

    public override void Enter()
    {
        owner.MoveToCurrentPatrolPoint();
    }
    public override void Update()
    {
        if (owner.CanLookPlayer())
        {
            owner.ChangeState(owner.ChaseState);
            return;
        }
            
        if (owner.HasArrived())
        {
            owner.ChangeState(owner.WaitState);
            return;
        }
    }

    public override void Exit()
    {
        
    }
}
