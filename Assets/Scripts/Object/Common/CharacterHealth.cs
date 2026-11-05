using System;
using UnityEngine;

public class CharacterHealth : MonoBehaviour
{
    [SerializeField] private int currentHp;
    private int maxHp, def;
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public int Defense => def;
    public bool isDead;
    public event Action<int, int> HealthChanged;

    public void Init(int maxHp, int def)
    {
        isDead = false;
        this.maxHp = Mathf.Max(1, maxHp);
        this.def = Mathf.Max(0, def);
        SetCurrentHealth(this.maxHp);
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead)
            return;
        DamageDirect(CalculateDamage(damage));
        Debug.Log($"Current HP: {currentHp},Damage: {damage},maxHp: {maxHp}");
        if (currentHp <= 0)
        {
            Dead();
        }
    }

    public virtual void Dead()
    {
        isDead = true;
    }
    // 管理员扣血等直接生命操作，不经过防御计算。
    public void DamageDirect(int amount)
    {
        if (!isDead)
            SetCurrentHealth(currentHp - Mathf.Max(0, amount));
    }

    public void Heal(int amount)
    {
        if (!isDead && amount > 0)
            SetCurrentHealth((int)Math.Min((long)maxHp, (long)currentHp + amount));
    }

    public void SetCurrentHealth(int value)
    {
        currentHp = Mathf.Clamp(value, 0, maxHp);
        isDead = currentHp == 0;
        HealthChanged?.Invoke(currentHp, maxHp);
    }

    public void SetStats(int maximumHealth, int defense)
    {
        maxHp = Mathf.Max(1, maximumHealth);
        def = Mathf.Max(0, defense);
        SetCurrentHealth(currentHp);
    }

    private int CalculateDamage(int damage)
    {
        return Mathf.Max(0, Mathf.Max(0, damage) - def);
    }
}
