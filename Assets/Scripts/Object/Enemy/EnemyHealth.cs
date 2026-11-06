public class EnemyHealth: CharacterHealth
{
    
    public override void Dead()
    {
        base.Dead();
        //TODO 后续判断是否是任务怪 以及给玩家加经验和金币
        MissionMgr.Instance.CheckFinishMission(MissionType.Battle,gameObject.GetComponent<EnemyObject>().EnemyId);
    }
}
