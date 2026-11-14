using UnityEngine;
using UnityEngine.Events;

public class CharacterHealth : MonoBehaviour
{
    public event UnityAction<int,int> OnHealthChange; 
    [SerializeField] private int currentHp;
    private int maxHp, def;
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public bool isDead;

    public void Init(int maxHp, int def)
    {
        isDead = false;
        this.maxHp = maxHp;
        this.def = def;
        this.currentHp = maxHp;
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead)
            return;
        currentHp -= CalculateDamage(damage);
        if (currentHp <= 0)
        {
            currentHp = 0;
            isDead = true;
        }
        print($"Current HP: {currentHp},Damage: {damage},maxHp: {maxHp}");
        if (currentHp <= 0)
        {
            Dead();
        }
        OnHealthChange?.Invoke(currentHp, maxHp);
    }

    public virtual void Dead()
    {
        isDead = true;
    }
    protected virtual int CalculateDamage(int damage)
    {
        //伤害计算公式: 暂定
        int finalDamage = damage - def;
        if (finalDamage > 0)
            return finalDamage;
        else
            return 0;
    }
}
