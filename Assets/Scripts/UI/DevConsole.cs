using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// GMScene 的开发控制台：调试角色运行时状态，并支持清空任务存档。
public class DevConsole : MonoBehaviour
{
    [SerializeField] private GameObject window;
    [SerializeField] private PlayerObject targetPlayer;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_InputField amountInput;
    [SerializeField] private TMP_InputField attackInput;
    [SerializeField] private TMP_InputField defenseInput;
    [SerializeField] private TMP_InputField maxHealthInput;

    private readonly List<Behaviour> pausedBehaviours = new List<Behaviour>();
    // 打开前的光标模式；控制台通过用户的 PlayerMgr.SetCursorState 显示光标，镜头由 CameraCursorLock 自动锁定。
    private bool previousCursorMode;
    private bool isOpen;
    private float refreshAt;
    private CharacterHealth boundHealth;
    // 调试读写集中在控制台中，角色与生命脚本使用原有接口。
    private static readonly FieldInfo CurrentHealthField = typeof(CharacterHealth).GetField("currentHp", BindingFlags.Instance | BindingFlags.NonPublic);
    // 最终属性由 CharacterStats 持有；控制台直接改写它的计算结果，再通过 CharacterHealth.UpdateStats 应用到生命组件。
    private static readonly FieldInfo StatsAttackField = typeof(CharacterStats).GetField("atk", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo StatsDefenseField = typeof(CharacterStats).GetField("def", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo StatsMaxHealthField = typeof(CharacterStats).GetField("maxHp", BindingFlags.Instance | BindingFlags.NonPublic);
    // 事件只能在 CharacterHealth 内部触发；控制台改血后通过它的编译器生成字段通知血条刷新。
    private static readonly FieldInfo HealthChangedField = typeof(CharacterHealth).GetField("OnHealthChange", BindingFlags.Instance | BindingFlags.NonPublic);
    public bool IsOpen => isOpen;

    private void Start()
    {
        if (window != null)
            window.SetActive(false);
        attackInput.onEndEdit.AddListener(OnAttributeEndEdit);
        defenseInput.onEndEdit.AddListener(OnAttributeEndEdit);
        maxHealthInput.onEndEdit.AddListener(OnAttributeEndEdit);
        BindPlayer();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.f1Key.wasPressedThisFrame)
            Toggle();
        else if (keyboard != null && isOpen && keyboard.escapeKey.wasPressedThisFrame)
            Close();

        // 控制台打开期间松开 Alt 会退出光标模式，这里重新进入，保证光标可用、镜头保持锁定。
        if (isOpen && !PlayerMgr.Instance.CurrentIsCursor)
            PlayerMgr.Instance.SetCursorState(true);

        if (isOpen && Time.unscaledTime >= refreshAt)
        {
            refreshAt = Time.unscaledTime + 0.15f;
            BindPlayer();
            RefreshDisplay();
        }
    }

    private bool BindPlayer()
    {
        if (targetPlayer == null)
            targetPlayer = PlayerMgr.Instance.player;
        boundHealth = targetPlayer != null ? targetPlayer.GetComponent<CharacterHealth>() : null;
        return targetPlayer != null && targetPlayer.Config != null && targetPlayer.Stats != null && boundHealth != null && boundHealth.MaxHp > 0;
    }

    private void OnAttributeEndEdit(string _) => ApplyAttributes();

    // 直接改写生命值。归零时走玩家自己的 Dead()（死亡广播、动画、敌人停止），之后通知血条刷新。
    private void SetConsoleHealth(int value)
    {
        CurrentHealthField.SetValue(boundHealth, Mathf.Clamp(value, 0, boundHealth.MaxHp));
        if (boundHealth.CurrentHp == 0 && !boundHealth.isDead)
            boundHealth.Dead();
        NotifyHealthChanged();
        RefreshDisplay();
    }

    private void NotifyHealthChanged()
    {
        if (HealthChangedField == null)
        {
            Debug.LogWarning("DevConsole：找不到 CharacterHealth.OnHealthChange，血条不会随控制台刷新。");
            return;
        }
        var handler = HealthChangedField.GetValue(boundHealth) as UnityEngine.Events.UnityAction<int, int>;
        handler?.Invoke(boundHealth.CurrentHp, boundHealth.MaxHp);
    }

    // 只改写这名角色运行时的最终属性，不碰共享 SO；下次升级或"恢复初始值"时会按等级重新计算并覆盖。
    private void ApplyConsoleAttributes(int attack, int defense, int maximumHealth)
    {
        CharacterStats stats = targetPlayer.Stats;
        StatsAttackField.SetValue(stats, Mathf.Max(0, attack));
        StatsDefenseField.SetValue(stats, Mathf.Max(0, defense));
        StatsMaxHealthField.SetValue(stats, Mathf.Max(1, maximumHealth));
        // 用户的接口：更新上限与防御，当前生命限制在新上限内并刷新血条。
        boundHealth.UpdateStats(stats.MaxHp, stats.Def);
        RefreshDisplay();
    }

    public void Toggle() => SetOpen(!isOpen);
    public void Close() => SetOpen(false);

    public void SetOpen(bool value)
    {
        if (isOpen == value)
            return;
        isOpen = value;
        if (window != null)
            window.SetActive(value);
        if (value)
        {
            BindPlayer();
            pausedBehaviours.Clear();
            // 光标模式只拦截攻击和镜头，移动仍需暂停玩家组件。
            if (targetPlayer != null)
            {
                Pause(targetPlayer);
                Pause(targetPlayer.GetComponent<PlayerCombat>());
            }
            previousCursorMode = PlayerMgr.Instance.CurrentIsCursor;
            PlayerMgr.Instance.SetCursorState(true);
            RefreshDisplay();
        }
        else
        {
            foreach (Behaviour behaviour in pausedBehaviours)
                if (behaviour != null)
                    behaviour.enabled = true;
            pausedBehaviours.Clear();
            PlayerMgr.Instance.SetCursorState(previousCursorMode);
        }
    }

    private void Pause(Behaviour behaviour)
    {
        if (behaviour == null || !behaviour.enabled)
            return;
        pausedBehaviours.Add(behaviour);
        behaviour.enabled = false;
    }

    private bool Ready()
    {
        if (BindPlayer())
            return true;
        Say("玩家尚未完成初始化，请稍后重试。", true);
        return false;
    }

    private void Say(string message, bool error = false)
    {
        if (messageText == null)
            return;
        messageText.text = message;
        messageText.color = error ? new Color(1f, 0.5f, 0.45f) : new Color(0.55f, 0.87f, 0.9f);
    }

    private void RefreshDisplay()
    {
        if (statusText == null)
            return;
        if (targetPlayer == null || targetPlayer.Config == null || boundHealth == null)
        {
            statusText.text = "等待玩家初始化…";
            return;
        }
        CharacterStats stats = targetPlayer.Stats;
        if (stats == null)
        {
            statusText.text = "等待玩家初始化…";
            return;
        }
        string level = PlayerMgr.Instance.saveData != null ? $"Lv.{PlayerMgr.Instance.saveData.level}    " : "";
        statusText.text = $"{targetPlayer.name}    {level}\n生命  {boundHealth.CurrentHp} / {boundHealth.MaxHp}    状态  {(boundHealth.isDead ? "死亡" : "存活")}\n攻击  {stats.Atk}    防御  {stats.Def}";
        SyncField(attackInput, stats.Atk);
        SyncField(defenseInput, stats.Def);
        SyncField(maxHealthInput, boundHealth.MaxHp);
    }

    private static void SyncField(TMP_InputField field, int value)
    {
        if (field != null && !field.isFocused && field.text != value.ToString())
            field.SetTextWithoutNotify(value.ToString());
    }

    private bool ReadAmount(out int value)
    {
        if (int.TryParse(amountInput.text, out value) && value > 0 && value <= 1000000)
            return true;
        Say("操作数值请输入 1～1000000 的整数。", true);
        return false;
    }

    private void Damage(int value, bool useDefense = false)
    {
        if (!Ready()) return;
        if (boundHealth.isDead) { Say("玩家已死亡，可使用“复活并回满”。", true); return; }
        if (useDefense)
            targetPlayer.TakeDamage(value);
        else
            SetConsoleHealth(boundHealth.CurrentHp - value);
        RefreshDisplay();
        Say(useDefense ? $"已施加 {value} 点原始伤害，经过防御计算。" : $"已直接扣除 {value} 点生命。 ");
    }

    private void Heal(int value)
    {
        if (!Ready()) return;
        if (boundHealth.isDead) { Say("玩家已死亡，请使用“复活并回满”。", true); return; }
        SetConsoleHealth((int)System.Math.Min((long)boundHealth.MaxHp, (long)boundHealth.CurrentHp + value));
        Say($"已恢复 {value} 点生命，最高不超过最大生命。 ");
    }

    public void Damage10() => Damage(10);
    public void Damage25() => Damage(25);
    public void Heal10() => Heal(10);
    public void Heal25() => Heal(25);
    public void DamageCustom() { if (ReadAmount(out int amount)) Damage(amount); }
    public void HealCustom() { if (ReadAmount(out int amount)) Heal(amount); }
    public void TestDamage() { if (ReadAmount(out int amount)) Damage(amount, true); }

    public void FullHealth()
    {
        if (Ready()) Heal(boundHealth.MaxHp);
    }

    public void KillPlayer()
    {
        if (!Ready()) return;
        if (boundHealth.isDead) { Say("玩家已经处于死亡状态。", true); return; }
        SetConsoleHealth(0);
        Say("玩家生命归零，已调用 Dead() 进入死亡流程。 ");
    }

    // 复活走玩家自己的 Respawn()；玩家未死亡时只回满，避免多发一次复活广播。
    public void RevivePlayer()
    {
        if (!Ready()) return;
        if (!boundHealth.isDead)
        {
            Heal(boundHealth.MaxHp);
            Say("玩家未死亡，已恢复满生命。 ");
            return;
        }
        boundHealth.Respawn();
        RefreshDisplay();
        Say("已调用 Respawn()，玩家复活并恢复满生命。 ");
    }

    public void ApplyAttributes()
    {
        if (!Ready()) return;
        if (!int.TryParse(attackInput.text, out int attack) || attack < 0 || attack > 1000000 ||
            !int.TryParse(defenseInput.text, out int defense) || defense < 0 || defense > 1000000 ||
            !int.TryParse(maxHealthInput.text, out int maximum) || maximum < 1 || maximum > 1000000)
        {
            Say("攻击/防御为 0～1000000；最大生命为 1～1000000。", true);
            return;
        }
        ApplyConsoleAttributes(attack, defense, maximum);
        Say("运行时属性已应用；当前生命保留并限制在新上限内。升级或恢复初始值后按等级重新计算。");
    }

    private void Step(int attack, int defense, int maximum)
    {
        if (!Ready()) return;
        CharacterStats stats = targetPlayer.Stats;
        ApplyConsoleAttributes(
            Mathf.Clamp(stats.Atk + attack, 0, 1000000),
            Mathf.Clamp(stats.Def + defense, 0, 1000000),
            Mathf.Clamp(boundHealth.MaxHp + maximum, 1, 1000000));
        Say("运行时属性已调整。 ");
    }

    public void AttackUp() => Step(1, 0, 0);
    public void AttackDown() => Step(-1, 0, 0);
    public void DefenseUp() => Step(0, 1, 0);
    public void DefenseDown() => Step(0, -1, 0);
    public void MaxHealthUp() => Step(0, 0, 10);
    public void MaxHealthDown() => Step(0, 0, -10);

    public void ResetAttributes()
    {
        if (!Ready()) return;
        // Init 会直接清除死亡标记，绕过 Respawn 的复活广播，所以死亡时先复活。
        if (boundHealth.isDead) { Say("玩家已死亡，请先使用“复活并回满”。", true); return; }
        int id = targetPlayer.Config.id;
        var type = targetPlayer.Config.type;
        targetPlayer.Init(id, type);
        targetPlayer.SetIsAttacking(false);
        NotifyHealthChanged();
        RefreshDisplay();
        Say("已按配置表和当前等级重新计算属性，并回满生命。 ");
    }

    // 保存空进度并同步内存，防止旧任务数据再次被写回存档。
    public void ClearMissionSave()
    {
        try
        {
            MissionMgr manager = MissionMgr.Instance;
            var emptyProgress = new MissionProgressData();
            JsonMgr.Instance.SaveData(emptyProgress, "MissionData");
            manager.playerMissionProgress = emptyProgress;
            Say("任务存档及当前任务进度已清空。重新打开任务面板查看。");
        }
        catch (System.Exception error)
        {
            Say("清空任务存档失败：" + error.Message, true);
            Debug.LogException(error);
        }
    }

    private void OnDisable()
    {
        if (isOpen)
            SetOpen(false);
        boundHealth = null;
    }
}
