using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerMgr :BaseManager<PlayerMgr>
{
    private bool isCursor;
    public bool CurrentIsCursor => isCursor;
    public event UnityAction<bool> OnCursorChanged;
    public event UnityAction<bool> OnPlayerStateChange; 
    private PlayerGrowth playerGrowth = new PlayerGrowth();
    public PlayerSaveData saveData;
    //后续进入游戏选择角色时候为它赋值
    public int currentPlayerId = 0;
    public PlayerObject player;
    public bool canInput = true;
    public InputSystem_Actions inputActions;
    //0为目前测试的单手剑
    public int currentWeaponType = 0;
    string playerJsonFileName => "Character/" + nameof(CharacterConfig.CharacterType.Player) + "/" + currentPlayerId + ".json";
    private PlayerMgr()
    {
        saveData = JsonMgr.Instance.LoadData<PlayerSaveData>(playerJsonFileName);
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
    }
    public void PlayerStateChange(bool state)
    {
        //TODO 后续可在此处让显示死亡后的UI
        //true为死亡
        OnPlayerStateChange?.Invoke(state);
    }
    public void SetCursorState(bool state)
    {
        if (state == isCursor)
            return;
        isCursor = state;
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
        OnCursorChanged?.Invoke(state);
    }
    public int GetAtkValue()
    {
        int atkValue = 0;
        atkValue = saveData.level * player.Config.atkPerLv + player.Config.atk;
        return atkValue;
    }
    public void AddExp(int exp)
    {
        playerGrowth.AddExp(exp);
    }
    public void SavePlayerData()
    {
        JsonMgr.Instance.SaveData(saveData,playerJsonFileName);
    }
    public void InitPlayer(PlayerObject player)
    {
        this.player = player;
    }

    public void CloseInput()
    {
        inputActions.Disable();
    }

    public void OpenInput()
    {
        inputActions.Enable();
    }
    public void ClearPlayerMgr()
    {
        //TODO 游戏结束时候调用函数来释放
        player = null;
        inputActions.Disable();
    }
}
