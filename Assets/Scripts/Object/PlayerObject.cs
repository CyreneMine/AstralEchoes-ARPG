using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerObject : MonoBehaviour
{
    private bool isQuicking;
    public Animator animator;
    public CharacterController controller;
    public InputSystem_Actions inputActions;
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float acceleration = 30f; // 加速度
    [SerializeField] private float deceleration = 45f; // 减速度
    private float vSpeed;
    public float gravity = -9.81f;
    private Vector2 currentInput;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float rotationSpeed = 720f;
    private float dashTimer;       // 剩余冲刺时间
    private Vector3 dashDirection; // 冲刺方向
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            Quick();
        }
    }

    private void HandleMovement()
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
        Vector3 move = new Vector3(currentInput.x, 0, currentInput.y);
        if (dashTimer > 0f)
        {
            move = dashDirection * dashSpeed;
            dashTimer -= Time.deltaTime;
        }
        animator.SetFloat("XSpeed", Mathf.Clamp(currentInput.x/runSpeed,-1,1));
        animator.SetFloat("YSpeed",Mathf.Clamp(currentInput.y/runSpeed,-1,1));
        if (controller.isGrounded && vSpeed < 0)
            vSpeed = -2f;
        vSpeed += gravity * Time.deltaTime;
        // Vector3 velocity = move * moveSpeed;
        Vector3 velocity = move;
        velocity.y = vSpeed;
        
        if (input.sqrMagnitude < 0.01f)
            return;
        Transform camTrans = Camera.main.transform;
        Vector3 forward = Vector3.ProjectOnPlane(camTrans.forward, camTrans.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(camTrans.right, camTrans.up).normalized;
        
        Vector3 moveDir = forward*input.y + right*input.x;
        moveDir = Vector3.ClampMagnitude(moveDir, 1f);
        
        controller.Move(velocity * Time.deltaTime);
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
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
