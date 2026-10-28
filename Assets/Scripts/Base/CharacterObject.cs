using UnityEngine;

public class CharacterObject : MonoBehaviour
{
    [SerializeField]private CharacterConfig config;
    private CharacterHealth health;
    protected virtual void Awake()
    {
        health = GetComponent<CharacterHealth>();
        if (health ==null)
        {
            health = gameObject.AddComponent<CharacterHealth>();
        }
        //后续改成字符串拼接来动态决定加载内容
        config = Resources.Load<CharacterConfig>("GameData/Character/Enemy_Test");
        health.Init(config.maxHp,config.def);
    }
    public virtual void TakeDamage(int damage)
    {
        health.TakeDamage(damage);
    }
}
