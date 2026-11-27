using System;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    private Animator animator;
    private CharacterConfig.CharacterData config;
    private CharacterHealth health;
    private EnemyObject enemyObject;
    private float lastTime;
    private bool isAttacking;
    public bool IsAttacking => isAttacking&&Time.time <attackStartTime+maxAttackTime;
    private float attackStartTime;
    [SerializeField]private float maxAttackTime = 3f;
    private void Awake()
    {
        enemyObject = GetComponent<EnemyObject>();
        animator = GetComponent<Animator>();
        config = ConfigMgr.Instance.GetCharacterData(CharacterConfig.CharacterType.Enemy, enemyObject.EnemyId);
        health = GetComponent<CharacterHealth>();
    }
    public void TryAttack()
    {
        if (health.isDead)
            return;
        if (Time.time > config.atkCooldown+lastTime)
        {
            if (animator !=null)
            {
                animator.SetTrigger("fire");
                isAttacking = true;
                attackStartTime = Time.time;
                lastTime = Time.time;
            }
        }
    }

    public void OnAttackCheck()
    {
        //动画事件
        Collider[] colliders = Physics.OverlapSphere(transform.position, config.atkRange, 1 << LayerMask.NameToLayer("Player"));
        foreach (Collider collider in colliders)
        {
            if (health.isDead)
                return;
            collider.GetComponent<CharacterHealth>().TakeDamage(config.atk);
        }
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
    }
}
