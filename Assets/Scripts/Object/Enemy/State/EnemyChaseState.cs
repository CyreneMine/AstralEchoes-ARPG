using UnityEngine;

/// <summary>
/// chase 追逐
/// </summary>
public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyAI owner) : base(owner)
    {
    }

    private Vector3 targetPos;

    public override void Enter()
    {
    }

    public override void Update()
    {
        targetPos = PlayerMgr.Instance.player.transform.position;
        owner.Agent.SetDestination(targetPos);
        float totalDistance = Vector3.Distance(owner.transform.position, owner.ChaseStartPos);
        float targetDistance = Vector3.Distance(owner.transform.position, targetPos);
        if (targetDistance > owner.Config.loseRange || totalDistance > owner.Config.maxRange || owner.PlayerIsDead())
        {
            owner.ChangeState(owner.ReturnState);
            return;
        }
        if (owner.CanAttackPlayer())
            owner.ChangeState(owner.AttackState);
    }

    public override void Exit()
    {
    }
}