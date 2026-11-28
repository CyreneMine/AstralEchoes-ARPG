public class PlayerDeadState :PlayerState
{
    public PlayerDeadState(PlayerController owner) : base(owner)
    {
    }

    public override void Enter()
    {
        owner.currentSpeed = 0;
        owner.Animator.SetBool("IsDead", true);
    }

    public override void Update()
    {
        
    }

    public override void Exit()
    {
        owner.Animator.SetBool("IsDead", false);
    }
}
