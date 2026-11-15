public class PlayerGrowth
{
    private CharacterConfig.CharacterData playerConfigData;
    private PlayerSaveData saveData;
    int lvUpNeedExp => (saveData.level-playerConfigData.initLv) * playerConfigData.expPerLv + playerConfigData.initExpPerLv;
    public void AddExp(int exp)
    {
       initData();
       //添加经验
       saveData.exp += exp;
       CheckLevelUp();
       PlayerMgr.Instance.PlayerLevelChanged();
       PlayerMgr.Instance.SavePlayerData();
    }

    public void CheckLevelUp()
    {
        if (lvUpNeedExp <= saveData.exp )
        {
            LevelUp();
        }
    }
    public void LevelUp()
    {
        if (saveData.level >= playerConfigData.maxLv)
        {
            return;
        }
        saveData.exp -= lvUpNeedExp;
        saveData.level++;
        CheckLevelUp();
        
    }

    private void initData()
    {
        saveData = PlayerMgr.Instance.saveData;
        if (playerConfigData == null)
        {
            int playerId = PlayerMgr.Instance.currentPlayerId;
            playerConfigData = ConfigMgr.Instance.GetCharacterData(CharacterConfig.CharacterType.Player, playerId);
        }    
    }
}
