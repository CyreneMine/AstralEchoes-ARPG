using UnityEngine;

public class EnemyAttackState : EnemyState
{
    public EnemyAttackState(EnemyAI owner) : base(owner)
    {
    }

    public override void Enter()
    {
        owner.Agent.isStopped = true;
        owner.Agent.velocity = Vector3.zero;
    }

    public override void Update()
    {
        if (owner.Combat.IsAttacking)
            return;
        Vector3 dir = PlayerMgr.Instance.player.transform.position - owner.transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude >0.1f)
            owner.transform.rotation = Quaternion.RotateTowards(owner.transform.rotation, Quaternion.LookRotation(dir), 360f * Time.deltaTime);
        if (owner.CanAttackPlayer())
            owner.Combat.TryAttack();
        else
            owner.ChangeState(owner.ChaseState);
    }

    public override void Exit()
    {
        owner.Agent.isStopped = false;
    }
}