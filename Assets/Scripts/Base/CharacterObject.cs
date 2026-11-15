using UnityEngine;

public class CharacterObject : MonoBehaviour
{
    [SerializeField]private CharacterConfig.CharacterData config;
    public CharacterConfig.CharacterData Config => config;
    protected CharacterHealth health;
    public CharacterHealth Health => health;
    private CharacterStats stats;
    public CharacterStats Stats => stats;
    protected virtual void Awake()
    {
        health = GetComponent<CharacterHealth>();
        if (health ==null)
        {
            health = gameObject.AddComponent<CharacterHealth>();
        }
    }

    public void Init(int id,CharacterConfig.CharacterType type)
    {
        config = ConfigMgr.Instance.GetCharacterData(type, id);
        stats = new CharacterStats(config);
        Stats.Recalculate(GetLevel());
        health.Init(Stats.MaxHp,Stats.Def);
    }
    public virtual void TakeDamage(int damage)
    {
        health.TakeDamage(damage);
    }

    protected virtual int GetLevel()
    {
        return config.initLv;
    }
}
