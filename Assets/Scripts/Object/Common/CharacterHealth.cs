using UnityEngine;

public class CharacterHealth :MonoBehaviour
{
    [SerializeField]private int currentHp;
    private int maxHp,def;
    public bool isDead;
    public void Init(int maxHp,int def)
    {
        this.maxHp = maxHp;
        this.def = def;
        this.currentHp = maxHp;
        isDead = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;
        currentHp -= CalculateDamage(damage);
        if (currentHp <= 0)
        {
            currentHp = 0;
            isDead = true;
        }
    }

    private int CalculateDamage(int damage)
    {
        //伤害计算公式: 暂定
        int finalDamage = damage - def;
        if (finalDamage > 0)
            return finalDamage;
        else
            return 0;
    }
}
