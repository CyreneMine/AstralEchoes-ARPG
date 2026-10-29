using UnityEngine;

public class CharacterObject : MonoBehaviour
{
    [SerializeField]private CharacterConfig.CharacterData config;
    public CharacterConfig.CharacterData Config => config;
    public CharacterHealth Health => health;
    protected CharacterHealth health;
    protected virtual void Awake()
    {
        health = GetComponent<CharacterHealth>();
        if (health ==null)
        {
            health = gameObject.AddComponent<CharacterHealth>();
        }
        //后续改成字符串拼接来动态决定加载内容
        // config = Resources.Load<CharacterConfig>("GameData/Character/"+config.type+"_"+config.id);
        // Init(config);
        // health.Init(config.maxHp,config.def);
        
    }

    public void Init(int id,CharacterConfig.CharacterType type)
    {
        CharacterConfig.CharacterData source = ConfigMgr.Instance.GetCharacterData(type, id);
        if (source == null)
        {
            Debug.LogError($"角色配置不存在：{type}/{id}", this);
            return;
        }
        // SO中的数据由角色共享；运行时调试和属性变化使用独立副本。
        config = new CharacterConfig.CharacterData
        {
            id = source.id, type = source.type, atk = source.atk,
            def = source.def, maxHp = source.maxHp
        };
        health.Init(config.maxHp,config.def);
    }
    public void SetRuntimeAttributes(int attack, int defense, int maximumHealth)
    {
        if (config == null || health == null)
            return;
        config.atk = Mathf.Max(0, attack);
        config.def = Mathf.Max(0, defense);
        config.maxHp = Mathf.Max(1, maximumHealth);
        health.SetStats(config.maxHp, config.def);
    }

    public virtual void TakeDamage(int damage)
    {
        health.TakeDamage(damage);
    }
}
