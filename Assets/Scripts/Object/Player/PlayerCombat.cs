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
        if (!PlayerMgr.Instance.canInput)
            return;
        PlayerObject player = PlayerMgr.Instance.player;
        if (player == null || (player.Health != null && player.Health.isDead))
            return;
        if (PlayerMgr.Instance.inputActions.Player.Attack.WasPressedThisFrame())
        {
            Attack();
        }
    }

    public void OnAttackCheck()
    {
        switch (PlayerMgr.Instance.currentWeaponType)
        {
            case 0:
                Collider[] colliders = Physics.OverlapSphere(transform.position + transform.forward, 2, 1 << LayerMask.NameToLayer("Enemy"));
                foreach (Collider other in colliders)
                {
                    EnemyObject enemyObject = other.GetComponent<EnemyObject>();
                    
                    enemyObject.TakeDamage(PlayerMgr.Instance.GetAtkValue());
                }
                break;
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
