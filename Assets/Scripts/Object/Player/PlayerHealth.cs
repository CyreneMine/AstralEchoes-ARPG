using UnityEngine;

public class PlayerHealth : CharacterHealth
{
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        // UIManager.Instance.GetPanel<GamePanel>().UpdateHpBar(CurrentHp,MaxHp);
    }

    public override void Dead()
    {
        base.Dead();
        PlayerMgr.Instance.PlayerStateChange(true);
    }

    public override void Respawn()
    {
        base.Respawn();
        PlayerMgr.Instance.PlayerStateChange(false);
    }
}
