using UnityEngine;

public class PlayerObject : CharacterObject
{
    public Animator animator;
    public CharacterController controller;
    private Vector3 dashDirection;
    private float moveWeight;
    protected new PlayerHealth health => base.health as PlayerHealth;
    public new PlayerHealth Health => health;

    protected override void Awake()
    {
        base.Awake();
        PlayerMgr.Instance.InitPlayer(this);
        if (animator == null)
            animator = GetComponent<Animator>();
        if (controller == null)
            controller = GetComponent<CharacterController>();
        //TODO 测试写死 后续动态更新
        Init(0, CharacterConfig.CharacterType.Player);
        PlayerMgr.Instance.OnPlayerLevelChange += UpdateStats;
    }

    protected override int GetLevel()
    {
        return PlayerMgr.Instance.saveData.level;
    }

    public void ClearBuff()
    {
        Stats.ClearPlayerBuff();
        UpdateStats();
    }

    public void AddBuff(BuffType type, float value)
    {
        Stats.AddBuff(type, value);
        UpdateStats();
    }

    public void UpdateStats()
    {
        Stats.Recalculate(GetLevel());
        health.UpdateStats(Stats.MaxHp, Stats.Def);
    }

    private void OnDestroy()
    {
        PlayerMgr.Instance.OnPlayerLevelChange -= UpdateStats;
    }
}