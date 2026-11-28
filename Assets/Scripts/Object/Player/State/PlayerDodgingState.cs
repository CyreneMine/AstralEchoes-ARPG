using UnityEngine;

public class PlayerDodgingState : PlayerState
{
    public PlayerDodgingState(PlayerController owner) : base(owner)
    {
    }
    float dashTimer;
    public override void Enter()
    {
        dashTimer = owner.DashDuration;
        owner.currentSpeed = owner.DashSpeed;
        owner.Animator.SetTrigger("Quick");
    }

    public override void Update()
    {
        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0)
            owner.ChangeState(owner.PlayerMovementState);
        //判断有没有被攻击 或者直接无敌
        owner.SetHorizontalVelocity(owner.transform.forward * owner.currentSpeed);
    }

    public override void Exit()
    {
    }
}