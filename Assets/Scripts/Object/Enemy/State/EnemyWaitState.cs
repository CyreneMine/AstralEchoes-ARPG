using UnityEngine;

public class EnemyWaitState : EnemyState
{
    public EnemyWaitState(EnemyAI owner) : base(owner){}
    private float waitUntil;
    public override void Enter()
    {
        owner.Agent.isStopped = true;
        waitUntil = Time.time + owner.WaitTime;
        owner.NextPatrolIndex();
    }

    public override void Update()
    {
        if (owner.CanLookPlayer())
        {
            owner.StartChase();
            return;
        }
        if (waitUntil < Time.time)
        {
            owner.ChangeState(owner.PatrolState);
            return;
        }
    }

    public override void Exit()
    {
        owner.Agent.isStopped = false;
    }
}
