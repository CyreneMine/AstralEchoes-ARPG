using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.Cinemachine;
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
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private bool isOpen;
    private float refreshAt;
    private CharacterHealth boundHealth;
    // 调试读写集中在控制台中，角色与生命脚本使用原有接口。
    private static readonly FieldInfo ConfigField = typeof(CharacterObject).GetField("config", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo CurrentHealthField = typeof(CharacterHealth).GetField("currentHp", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo MaximumHealthField = typeof(CharacterHealth).GetField("maxHp", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo DefenseField = typeof(CharacterHealth).GetField("def", BindingFlags.Instance | BindingFlags.NonPublic);
    private int CurrentDefense => (int)DefenseField.GetValue(boundHealth);
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
        return targetPlayer != null && targetPlayer.Config != null && boundHealth != null && boundHealth.MaxHp > 0;
    }

    private void OnAttributeEndEdit(string _) => ApplyAttributes();

    private void SetConsoleHealth(int value)
    {
        CurrentHealthField.SetValue(boundHealth, Mathf.Clamp(value, 0, boundHealth.MaxHp));
        boundHealth.isDead = boundHealth.CurrentHp == 0;
        RefreshDisplay();
    }

    private void ApplyConsoleAttributes(int attack, int defense, int maximumHealth)
    {
        // 仅在使用调试按钮时创建副本，保留配置中的成长等全部序列化字段。
        var runtimeConfig = JsonUtility.FromJson<CharacterConfig.CharacterData>(JsonUtility.ToJson(targetPlayer.Config));
        runtimeConfig.atk = Mathf.Max(0, attack);
        runtimeConfig.def = Mathf.Max(0, defense);
        runtimeConfig.maxHp = Mathf.Max(1, maximumHealth);
        ConfigField.SetValue(targetPlayer, runtimeConfig);
        MaximumHealthField.SetValue(boundHealth, runtimeConfig.maxHp);
        DefenseField.SetValue(boundHealth, runtimeConfig.def);
        SetConsoleHealth(boundHealth.CurrentHp);
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
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            pausedBehaviours.Clear();
            if (targetPlayer != null)
            {
                Pause(targetPlayer);
                Pause(targetPlayer.GetComponent<PlayerCombat>());
            }
            foreach (var cameraInput in FindObjectsByType<CinemachineInputAxisController>(FindObjectsSortMode.None))
                Pause(cameraInput);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            RefreshDisplay();
        }
        else
        {
            foreach (Behaviour behaviour in pausedBehaviours)
                if (behaviour != null)
                    behaviour.enabled = true;
            pausedBehaviours.Clear();
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
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
        var data = targetPlayer.Config;
        statusText.text = $"{targetPlayer.name}\n生命  {boundHealth.CurrentHp} / {boundHealth.MaxHp}    状态  {(boundHealth.isDead ? "死亡" : "存活")}\n攻击  {data.atk}    防御  {CurrentDefense}";
        SyncField(attackInput, data.atk);
        SyncField(defenseInput, CurrentDefense);
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
        SetConsoleHealth(0);
        targetPlayer.SetIsAttacking(false);
        if (targetPlayer.animator != null)
            targetPlayer.animator.SetFloat("Velocity", 0);
        Say("玩家生命归零，已进入死亡状态。 ");
    }

    public void RevivePlayer()
    {
        if (!Ready()) return;
        SetConsoleHealth(boundHealth.MaxHp);
        targetPlayer.SetIsAttacking(false);
        Say("玩家已复活并恢复满生命。 ");
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
        Say("运行时属性已应用；当前生命保留并限制在新上限内。 ");
        RefreshDisplay();
    }

    private void Step(int attack, int defense, int maximum)
    {
        if (!Ready()) return;
        ApplyConsoleAttributes(
            Mathf.Clamp(targetPlayer.Config.atk + attack, 0, 1000000),
            Mathf.Clamp(CurrentDefense + defense, 0, 1000000),
            Mathf.Clamp(boundHealth.MaxHp + maximum, 1, 1000000));
        Say("运行时属性已调整。 ");
        RefreshDisplay();
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
        int id = targetPlayer.Config.id;
        var type = targetPlayer.Config.type;
        targetPlayer.Init(id, type);
        targetPlayer.SetIsAttacking(false);
        RefreshDisplay();
        Say("已恢复该角色配置表中的属性，并回满生命。 ");
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
