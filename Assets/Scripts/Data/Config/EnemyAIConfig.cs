using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAIConfig",menuName = "SO/EnemyAIConfig")]
public class EnemyAIConfig : ScriptableObject
{
    public float attackRange;
    public float detectRange;
    public float loseRange;
    public float maxRange;
}
