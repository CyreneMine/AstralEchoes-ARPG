using UnityEngine;

public class CharacterObject : MonoBehaviour
{
    [SerializeField]private CharacterConfig.CharacterData config;
    public CharacterConfig.CharacterData Config => config;
    protected CharacterHealth health;
    public CharacterHealth Health => health;
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
        config = ConfigMgr.Instance.GetCharacterData(type, id);
        health.Init(config.maxHp,config.def);
    }
    public virtual void TakeDamage(int damage)
    {
        health.TakeDamage(damage);
    }
}
