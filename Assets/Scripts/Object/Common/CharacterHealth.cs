using System;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHealth : MonoBehaviour
{
    public event UnityAction<float,float> OnHealthChange; 
    [SerializeField] private float currentHp;
    private float maxHp, def;
    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool isDead;
    
    public void Init(float maxHp, float def)
    {
        isDead = false;
        this.maxHp = maxHp;
        this.def = def;
        this.currentHp = maxHp;
    }
    public void UpdateStats(float maxHp, float def)
    {
        float ratio = this.currentHp / this.maxHp;
        this.maxHp = maxHp;
        this.def = def;
        UpdateHealthByRatio(ratio);
        currentHp = Mathf.Clamp(currentHp, 0, MaxHp);
        OnHealthChange?.Invoke(currentHp, maxHp);
    }
    

    public virtual void TakeDamage(float damage)
    {
        if (isDead)
            return;
        currentHp -= CalculateDamage(damage);
        if (currentHp <= 0)
        {
            currentHp = 0;
            Dead();
        }
        OnHealthChange?.Invoke(currentHp, maxHp);
    }

    public virtual void Dead()
    {
        isDead = true;
    }
    protected virtual float CalculateDamage(float damage)
    {
        //伤害计算公式: 暂定
        float finalDamage = damage - def;
        if (finalDamage > 0)
            return finalDamage;
        else
            return 0;
    }

    protected virtual void UpdateHealthByRatio(float ratio)
    {
        currentHp = maxHp*ratio;
    }
    protected virtual void Heal(float amount)
    {
        currentHp += amount;
        currentHp = Mathf.Clamp(currentHp, 0, MaxHp);
        OnHealthChange?.Invoke(currentHp, maxHp);
    }
    public virtual void Respawn()
    {
        isDead = false;
        Heal(MaxHp);
    }
}
