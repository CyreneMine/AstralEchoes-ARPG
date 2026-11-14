using UnityEngine;

public class PlayerHealth : CharacterHealth
{
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        // UIManager.Instance.GetPanel<GamePanel>().UpdateHpBar(CurrentHp,MaxHp);
    }
    // public
}
