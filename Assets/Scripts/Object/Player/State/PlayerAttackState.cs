public class PlayerAttackState:PlayerState
{
    public PlayerAttackState(PlayerController owner) : base(owner)
    {
    }

    public override void Enter()
    {
        owner.Combat.Attack();
    }

    public override void Update()
    {
        if (!owner.Animator.GetBool("Attacking"))
        {
            if (owner.AttackPressed)
            {
                owner.Combat.Attack();
                return;
            }
            if (owner.inputActionsPlayer.Sprint.WasPressedThisFrame())
            {
                owner.ChangeState(owner.PlayerDodgingState);
                return;
            }
            
            if (!owner.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            {
                owner.ChangeState(owner.PlayerMovementState);
            }
        }
    }

    public override void Exit()
    {
        
    }
    
}
