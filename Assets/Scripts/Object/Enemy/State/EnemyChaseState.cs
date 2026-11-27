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
    private Vector3 startPos;

    public override void Enter()
    {
        startPos = owner.transform.position;
    }

    public override void Update()
    {
        targetPos = PlayerMgr.Instance.player.transform.position;
        owner.Agent.SetDestination(targetPos);
        float totalDistance = Vector3.Distance(owner.transform.position, startPos);
        float targetDistance = Vector3.Distance(owner.transform.position, targetPos);
        if (targetDistance > owner.Config.loseRange || totalDistance > owner.Config.maxRange || owner.PlayerIsDead())
        {
            owner.ChangeState(owner.ReturnState);
            return;
        }
        if (owner.CanAttackPlayer())
            owner.Combat.TryAttack();
    }

    public override void Exit()
    {
    }
}