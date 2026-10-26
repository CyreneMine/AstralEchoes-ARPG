using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    private Animator animator;
    //TODO 后续补全攻击的连贯 设置预输入阈值
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (PlayerMgr.Instance.inputActions.Player.Attack.WasPressedThisFrame())
        {
            Attack();
        }
    }

    public void Attack()
    {
        PlayerMgr.Instance.player.SetIsAttacking(true);
        animator.SetTrigger("Atk");
        animator.SetBool("Attacking",true);
    }

    public void AttackEnd()
    {
        PlayerMgr.Instance.player.SetIsAttacking(false);
        animator.SetBool("Attacking",false);
    }
}
