using UnityEngine;

public class PlayerMovementState : PlayerState
{
    public PlayerMovementState(PlayerController owner) : base(owner)
    {
    }
    private float moveWeight;
    public override void Enter()
    {
        owner.inputActionsPlayer = PlayerMgr.Instance.inputActions.Player;
    }

    public override void Update()
    {
        
        if (owner.inputActionsPlayer.Sprint.WasPressedThisFrame())
        {
            owner.ChangeState(owner.PlayerDodgingState);
            return;
        }

        if (owner.AttackPressed)
        {
            owner.ChangeState(owner.PlayerAttackState);
            return;
        }
        Spin();
        Move();
        owner.Animator.SetFloat("Velocity", owner.currentSpeed/owner.RunSpeed);
    }

    private void Move()
    {
        float changeSpeed = owner.HasInputMove ? owner.Acceleration : owner.Deceleration;
        float targetSpeed = owner.GetTargetSpeed();
        owner.currentSpeed = Mathf.MoveTowards(owner.currentSpeed, targetSpeed, changeSpeed * Time.deltaTime);
        moveWeight = owner.Animator.GetFloat("MoveWeight");
        Vector3 horizontalVelocity = owner.transform.forward * owner.currentSpeed*moveWeight;
        owner.SetHorizontalVelocity(horizontalVelocity);
    }

    private void Spin()
    {
        //拿到摄像机的方向
        Transform camTrs = Camera.main.transform;
        Vector3 forward = Vector3.ProjectOnPlane(camTrs.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 direction = right * owner.MoveInput.x + forward * owner.MoveInput.y;
        if (owner.HasInputMove)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            owner.transform.rotation = Quaternion.RotateTowards(owner.transform.rotation, targetRotation, Time.deltaTime * owner.RotationSpeed);
        }
    }

    public override void Exit()
    {
        owner.currentSpeed = 0f;
        owner.Animator.SetFloat("Velocity", 0f);
    }
}