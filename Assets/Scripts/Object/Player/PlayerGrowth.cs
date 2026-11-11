public class PlayerGrowth
{
    private CharacterConfig.CharacterData playerConfigData;
    private PlayerSaveData saveData;
    int lvUpNeedExp => saveData.level * playerConfigData.expPerLv + playerConfigData.initExpPerLv;
    public void AddExp(int exp)
    {
       initData();
       //添加经验
       saveData.exp += exp;
       CheckLevelUp();
       PlayerMgr.Instance.saveData = saveData;
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
        saveData.level++;
        saveData.exp -= lvUpNeedExp;
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
