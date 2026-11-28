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
        animator.SetTrigger("Atk");
        animator.SetBool("Attacking",true);
    }
}
