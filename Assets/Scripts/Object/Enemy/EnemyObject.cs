using System;
using UnityEngine;

public class EnemyObject : CharacterObject
{
    [SerializeField] private int enemyId;
    public int EnemyId => enemyId;
    public HealthBarController hpController;
    protected override void Awake()
    {
        base.Awake();
        //TODO 测试使用 先写死后续动态更新
        Init(enemyId,CharacterConfig.CharacterType.Enemy);
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
      hpController.updateHealthBar(health.CurrentHp,health.MaxHp);
    }
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        UpdateHealthBar();
    }
}
