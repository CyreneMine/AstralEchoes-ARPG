using UnityEngine;
using UnityEngine.InputSystem;

namespace AstralEchoes.Testing
{
    /// <summary>临时四向移动：Input System → CharacterController → Animator。</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class CyreneMovementTest : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private InputActionAsset inputActions = null;
        [SerializeField] private Camera movementCamera;
        [SerializeField, Min(0.1f)] private float moveSpeed = 2.5f;
        [SerializeField, Min(0f)] private float animationDamping = 0.1f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private bool followCamera = true;

        private static readonly int SpeedId = Animator.StringToHash("speed");
        private static readonly int HorizontalId = Animator.StringToHash("HSpeed");
        private static readonly int VerticalId = Animator.StringToHash("VSpeed");
        private CharacterController controller;
        private InputActionAsset runtimeActions;
        private InputAction moveAction;
        private Vector3 cameraOffset;
        private Vector3 movementForward;
        private Vector3 movementRight;
        private float verticalSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (movementCamera == null) movementCamera = Camera.main;
            if (animator == null || animator.runtimeAnimatorController == null || inputActions == null)
            {
                Debug.LogError("移动测试缺少 Animator、Controller 或 Input Actions，请运行 Tools/AstralEchoes/配置 WASD 移动测试。", this);
                enabled = false;
                return;
            }

            // 使用独立的运行时副本，启停这个组件不会关闭其他系统的 Move Action。
            runtimeActions = Instantiate(inputActions);
            moveAction = runtimeActions.FindAction("Player/Move", true);
            animator.applyRootMotion = false;
            movementForward = movementCamera != null ? movementCamera.transform.forward : transform.forward;
            movementForward = Vector3.ProjectOnPlane(movementForward, Vector3.up).normalized;
            if (movementForward.sqrMagnitude < 0.01f) movementForward = Vector3.forward;
            movementRight = Vector3.Cross(Vector3.up, movementForward).normalized;
            if (movementCamera != null) cameraOffset = movementCamera.transform.position - transform.position;
        }

        private void OnEnable() => moveAction?.Enable();

        private void OnDisable()
        {
            moveAction?.Disable();
            verticalSpeed = 0f;
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetFloat(SpeedId, 0f);
                animator.SetFloat(HorizontalId, 0f);
                animator.SetFloat(VerticalId, 0f);
            }
        }

        private void OnDestroy()
        {
            if (runtimeActions != null) Destroy(runtimeActions);
        }

        private void Update()
        {
            if (moveAction == null) return;
            Vector2 input = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
            Vector3 direction = movementRight * input.x + movementForward * input.y;
            if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            verticalSpeed += gravity * Time.deltaTime;
            controller.Move((direction * moveSpeed + Vector3.up * verticalSpeed) * Time.deltaTime);

            // 固定朝向的侧移测试；混合树使用角色局部坐标，防止左右/前后动画错配。
            Vector3 local = transform.InverseTransformDirection(direction);
            animator.SetFloat(SpeedId, input.magnitude, animationDamping, Time.deltaTime);
            animator.SetFloat(HorizontalId, local.x, animationDamping, Time.deltaTime);
            animator.SetFloat(VerticalId, local.z, animationDamping, Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (followCamera && movementCamera != null)
                movementCamera.transform.position = transform.position + cameraOffset;
        }
    }
}
