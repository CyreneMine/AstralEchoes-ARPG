using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController :MonoBehaviour
{
    public PlayerMovementState PlayerMovementState => playerMovementState;

    public PlayerDodgingState PlayerDodgingState => playerDodgingState;

    public float WalkSpeed => walkSpeed;

    public float RunSpeed => runSpeed;

    public PlayerCombat Combat => combat;

    public PlayerHealth Health => health;

    public PlayerObject Player => player;

    public bool IsRunning => isRunning;

    public float Acceleration => acceleration;

    public float Deceleration => deceleration;

    public CharacterController Controller => controller;

    public Animator Animator => animator;

    public float RotationSpeed => rotationSpeed;
    public float DashSpeed => dashSpeed;

    public float DashDuration => dashDuration;

    public PlayerAttackState PlayerAttackState => playerAttackState;

    public PlayerDeadState PlayerDeadState => playerDeadState;

    public bool AttackPressed => attackPressed;

    private PlayerCombat combat;
    private PlayerHealth health;
    private PlayerObject player;
    private CharacterController controller;
    private Animator animator;
    private StateMachine stateMachine;
    private PlayerMovementState playerMovementState;
    private PlayerDodgingState playerDodgingState;
    private PlayerAttackState playerAttackState;
    private PlayerDeadState playerDeadState;
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float acceleration = 30f; // 加速度
    [SerializeField] private float deceleration = 45f; // 减速度
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    private bool isRunning = false;
    private bool hasInputMove = false;
    [SerializeField]private float gravity = -9.81f;
    private float vSpeed;
    private Vector3 horizontalVelocity;
    public float currentSpeed;
    private Vector2 moveInput;
    public InputSystem_Actions.PlayerActions inputActionsPlayer;
    public bool HasInputMove => hasInputMove;
    private bool attackPressed = false;
    public Vector2 MoveInput => moveInput;

    private void Awake()
    {
        inputActionsPlayer =  PlayerMgr.Instance.inputActions.Player;
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        player = GetComponent<PlayerObject>();
        health = GetComponent<PlayerHealth>();
        combat = GetComponent<PlayerCombat>();
        stateMachine = new StateMachine();
        playerMovementState = new PlayerMovementState(this);
        playerDodgingState = new PlayerDodgingState(this);
        playerAttackState = new PlayerAttackState(this);
        playerDeadState = new PlayerDeadState(this);
        stateMachine.ChangeState(playerMovementState);
    }

    public void ChangeState(IState newState)
    {
        stateMachine.ChangeState(newState);
    }
    
    public void SetPlayerState(bool isDead)
    {
        if (isDead)
            ChangeState(playerDeadState);
        else
            ChangeState(playerMovementState);
    }
    
    private void OnEnable()
    {
        PlayerMgr.Instance.OnPlayerStateChange +=SetPlayerState;
    }

    private void OnDisable()
    {
        PlayerMgr.Instance.OnPlayerStateChange -=SetPlayerState;
    }

    public void SetHorizontalVelocity(Vector3 horizontalVelocity)
    {
        this.horizontalVelocity = horizontalVelocity;
    }
    private void Update()
    {
        KeyDetection();
        horizontalVelocity = Vector3.zero;
        stateMachine.Update();
        if (controller.isGrounded && vSpeed < 0f)
            vSpeed = -2f;
        vSpeed += gravity * Time.deltaTime;
        Vector3 velocity = horizontalVelocity;
        velocity.y = vSpeed;
        controller.Move(velocity * Time.deltaTime);
    }
    
    public float GetTargetSpeed()
    {
        return hasInputMove ? (isRunning ? runSpeed : walkSpeed) : 0;
    }
    private void KeyDetection()
    {
        attackPressed = !PlayerMgr.Instance.CurrentIsCursor && inputActionsPlayer.Attack.WasPressedThisFrame();
        isRunning = inputActionsPlayer.Sprint.IsPressed();
        moveInput = Vector2.ClampMagnitude(inputActionsPlayer.Move.ReadValue<Vector2>(), 1);
        hasInputMove = moveInput.sqrMagnitude > 0.01f;
    }
    public void AttackEnd()
    {
        
        animator.SetBool("Attacking",false);
    }
    
}
