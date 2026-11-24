using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerObject : CharacterObject
{
    private bool isQuicking;
    public Animator animator;
    public CharacterController controller;
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float acceleration = 30f; // 加速度
    [SerializeField] private float deceleration = 45f; // 减速度
    private float vSpeed;
    public float gravity = -9.81f;
    // private Vector2 currentInput;
    private float currentSpeed;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float rotationSpeed = 720f;
    private float dashTimer;       // 剩余冲刺时间
    private Vector3 dashDirection; 
    private float moveWeight;
    private bool isAttacking = false;
    protected new PlayerHealth health =>base.health as PlayerHealth;
    public new PlayerHealth Health => health;
    private void ChangeState(bool nowState)
    {
        animator.SetBool("IsDead", nowState);
        isAttacking = false;
    }
    protected override void Awake()
    {
        base.Awake();
        PlayerMgr.Instance.InitPlayer(this);
        if (animator == null)
            animator = GetComponent<Animator>();
        if (controller == null)
            controller = GetComponent<CharacterController>();
        //TODO 测试写死 后续动态更新
        Init(0,CharacterConfig.CharacterType.Player);
        PlayerMgr.Instance.OnPlayerLevelChange += UpdateStats;
        PlayerMgr.Instance.OnPlayerStateChange += ChangeState;
    }
    protected override int GetLevel()
    {
        return PlayerMgr.Instance.saveData.level;
    }

    public void UpdateStats()
    {
        Stats.Recalculate(GetLevel());
        health.UpdateStats(Stats.MaxHp, Stats.Def);
    }
    public void SetIsAttacking(bool isAttacking)
    {
        this.isAttacking = isAttacking;
    }
    private void Update()
    {
        if (isAttacking || health.isDead)
            return;
        HandleMovement();
        if (PlayerMgr.Instance.inputActions.Player.Sprint.WasPressedThisFrame())
        {
            Quick();
        }
    }
    private void HandleMovement()
    {
        Vector2 input = PlayerMgr.Instance.inputActions.Player.Move.ReadValue<Vector2>();
        input = Vector2.ClampMagnitude(input, 1f);
        bool hasInput = input.sqrMagnitude > 0.01f;
        bool isRunning = PlayerMgr.Instance.inputActions.Player.Sprint.IsPressed();
        float targetSpeed = hasInput
            ? input.magnitude * (isRunning ? runSpeed : walkSpeed)
            : 0f;
        float changeSpeed = hasInput ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            changeSpeed * Time.deltaTime
        );
        Transform camTrans = Camera.main.transform;
        Vector3 forward = Vector3.ProjectOnPlane(camTrans.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 desiredDirection = right * input.x + forward * input.y;
        
        if (dashTimer <= 0f && hasInput &&!isQuicking)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        moveWeight = Mathf.Clamp01(animator.GetFloat("MoveWeight"));
        Vector3 move = transform.forward * currentSpeed*moveWeight;
        
        if (dashTimer > 0f)
        {
            move = dashDirection * dashSpeed;
            dashTimer -= Time.deltaTime;
        }
        animator.SetFloat(
            "Velocity",
            Mathf.Clamp01(currentSpeed / runSpeed)
        );

        if (controller.isGrounded && vSpeed < 0f)
            vSpeed = -2f;

        vSpeed += gravity * Time.deltaTime;
        Vector3 velocity = move;
        velocity.y = vSpeed;
        controller.Move(velocity * Time.deltaTime);
    }
    /*private void HandleMovement()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        input = Vector2.ClampMagnitude(input, 1f);
        bool isRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        Vector2 targetVelocity = input * targetSpeed;
        float changeSpeed = input.sqrMagnitude > 0.01f
            ? acceleration
            : deceleration;

        currentInput = Vector2.MoveTowards(
            currentInput,
            targetVelocity,
            changeSpeed * Time.deltaTime
        );
        
        Transform camTrans = Camera.main.transform;
        Vector3 forward = Vector3.ProjectOnPlane(camTrans.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(camTrans.right, Vector3.up).normalized;
        
        Vector3 move = right * currentInput.x + forward * currentInput.y;
        if (dashTimer > 0f)
        {
            move = dashDirection * dashSpeed;
            dashTimer -= Time.deltaTime;
        }else if (input.sqrMagnitude > 0.01f)
        {
            Vector3 moveDir = forward*input.y + right*input.x;
            moveDir = Vector3.ClampMagnitude(moveDir, 1f);
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation,targetRot,rotationSpeed*Time.deltaTime);
        }
        // animator.SetFloat("XSpeed", Mathf.Clamp(currentInput.x/runSpeed,-1,1));
        // animator.SetFloat("YSpeed",Mathf.Clamp(currentInput.y/runSpeed,-1,1));
        animator.SetFloat("Velocity", move.magnitude/runSpeed);
        if (controller.isGrounded && vSpeed < 0)
            vSpeed = -2f;
        vSpeed += gravity * Time.deltaTime;
        // Vector3 velocity = move * moveSpeed;
        Vector3 velocity = move;
        velocity.y = vSpeed;
        controller.Move(velocity * Time.deltaTime);
    }*/
    private void OnDestroy()
    {
        PlayerMgr.Instance.OnPlayerLevelChange -= UpdateStats;
        PlayerMgr.Instance.OnPlayerStateChange -= ChangeState;
    }

    private void Quick()
    {
        if (isQuicking)
            return;
        animator.SetTrigger("Quick");
    }
    public void StartQuick()
    {
        if (isQuicking)
            return;
        isQuicking = true;
        Dash();
    }

    private void Dash()
    {
        dashDirection = transform.forward;
        dashTimer = dashDuration;
    }
    public void EndQuick()
    {
        isQuicking = false;
    }
    public bool GetIsQuick()
    {
        return isQuicking;
    }
    public void SwitchSocket(string socketName)
    {
        // print(socketName);
    }
}
