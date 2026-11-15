using UnityEngine;

public class EnemyHealth: CharacterHealth
{
    
    public override void Dead()
    {
        base.Dead();
        //TODO 后续判断是否是任务怪 以及给玩家加经验和金币
        EnemyReward enemyReward = ConfigMgr.Instance.GetEnemyRewardById(GetComponent<EnemyObject>().EnemyId);
        if (enemyReward == null)
        {
            Debug.LogError("未找到怪物的奖励列表");
        }
        else
        {
            RewardMgr.Instance.SettlementReward(enemyReward.rewards);
        }
        MissionMgr.Instance.CheckFinishMission(MissionType.Battle,gameObject.GetComponent<EnemyObject>().EnemyId);
    }
}
