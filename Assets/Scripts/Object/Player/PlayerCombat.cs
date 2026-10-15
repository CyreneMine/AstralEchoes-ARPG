using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Attack();
        }
    }

    public void Attack()
    {
        PlayerMgr.instance.player.SetIsAttacking(true);
        animator.SetTrigger("Atk");
        animator.SetBool("Attacking",true);
    }

    public void AttackEnd()
    {
        PlayerMgr.instance.player.SetIsAttacking(false);
        animator.SetBool("Attacking",false);
    }
}
