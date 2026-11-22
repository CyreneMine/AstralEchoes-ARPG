using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 管理员控制台（Claude Code 维护）。
// 界面在 Awake 时全部由代码生成，Canvas 预制件里只需要一个挂着本组件的 DevConsole 物体（自带 Canvas + GraphicRaycaster）。
// 只调用用户已有的公开接口；没有公开接口的运行时状态通过反射读写，不往玩法脚本里加配套代码。
public class DevConsole : MonoBehaviour
{
    [SerializeField] private PlayerObject targetPlayer;
    // 留空时从 Resources/DevConsole/DevConsoleFont 加载。
    [SerializeField] private TMP_FontAsset fontOverride;

    // ---- 布局常量（参考分辨率 1920×1080） ----
    // GamePanel 右上角的按钮栏 Right_Up 高 149（背包按钮在最右侧），开关按钮和窗口都放在这条栏下方，
    // 控制台关闭或打开时都不压住 HUD 按钮。HUD 布局改动后同步这里的数值。
    private const float HudTopBand = 149, EdgeMargin = 24, LauncherGap = 12;
    private const float LauncherWidth = 220, LauncherHeight = 42;
    private const float LauncherY = HudTopBand + LauncherGap;
    private const float WindowTop = LauncherY + LauncherHeight + 8;
    private const float WindowWidth = 900, WindowHeight = 700;
    private const float PageX = 168, PageY = 144, PageWidth = 716, PageHeight = 470;
    private static readonly string[] TabTitles = { "生命", "属性 · 等级", "道具", "存档" };
    public const int HealthTab = 0, StatsTab = 1, ItemTab = 2, SaveTab = 3;

    private static readonly Color Background = new Color(0.045f, 0.065f, 0.11f, 0.98f);
    private static readonly Color PanelColor = new Color(0.09f, 0.13f, 0.20f);
    private static readonly Color FieldColor = new Color(0.12f, 0.17f, 0.25f);
    private static readonly Color Accent = new Color(0.17f, 0.48f, 0.56f);
    private static readonly Color AccentBright = new Color(0.4f, 0.9f, 0.85f);
    private static readonly Color Neutral = new Color(0.25f, 0.31f, 0.42f);
    private static readonly Color Danger = new Color(0.55f, 0.21f, 0.25f);
    private static readonly Color HealColor = new Color(0.15f, 0.4f, 0.32f);
    private static readonly Color Muted = new Color(0.55f, 0.67f, 0.77f);
    private static readonly Color TabIdle = new Color(0.11f, 0.15f, 0.22f);
    private static readonly Color RowIdle = new Color(0.11f, 0.15f, 0.22f);
    private static readonly Color RowSelected = new Color(0.17f, 0.36f, 0.43f);
    private static readonly Color IconEmpty = new Color(0.18f, 0.24f, 0.34f);

    // ---- 反射：调试读写集中在控制台中，角色与生命脚本使用原有接口 ----
    private static readonly FieldInfo CurrentHealthField = typeof(CharacterHealth).GetField("currentHp", BindingFlags.Instance | BindingFlags.NonPublic);
    // 最终属性由 CharacterStats 持有；控制台直接改写它的计算结果，再通过 CharacterHealth.UpdateStats 应用到生命组件。
    private static readonly FieldInfo StatsAttackField = typeof(CharacterStats).GetField("atk", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo StatsDefenseField = typeof(CharacterStats).GetField("def", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo StatsMaxHealthField = typeof(CharacterStats).GetField("maxHp", BindingFlags.Instance | BindingFlags.NonPublic);
    // 事件只能在 CharacterHealth 内部触发；控制台改血后通过它的编译器生成字段通知血条刷新。
    private static readonly FieldInfo HealthChangedField = typeof(CharacterHealth).GetField("OnHealthChange", BindingFlags.Instance | BindingFlags.NonPublic);

    // ---- 运行时生成的控件 ----
    private TMP_FontAsset font;
    private GameObject window;
    private TMP_Text statusText, messageText;
    private readonly List<Image> tabImages = new List<Image>();
    private readonly List<GameObject> pages = new List<GameObject>();
    private TMP_InputField amountInput, attackInput, defenseInput, maxHealthInput, levelInput;
    private TMP_InputField itemSearchInput, itemIdInput, itemCountInput;
    private RectTransform itemListContent;
    private Image itemDetailIcon;
    private TMP_Text itemDetailIconLetter, itemDetailName, itemDetailMeta, itemDetailHeld, itemDetailDesc;

    private class ItemRow
    {
        public int id;
        public Image background;
        public TMP_Text sub;
    }
    private readonly List<ItemRow> itemRows = new List<ItemRow>();

    // ---- 状态 ----
    private readonly List<Behaviour> pausedBehaviours = new List<Behaviour>();
    // 打开前的光标模式；控制台通过用户的 PlayerMgr.SetCursorState 显示光标，镜头由 CameraCursorLock 自动锁定。
    private bool previousCursorMode;
    private bool isOpen;
    private float refreshAt;
    private int currentTab;
    private CharacterHealth boundHealth;
    private bool itemEventBound;
    // 危险操作需要 3 秒内再点一次确认。
    private string pendingConfirm;
    private float confirmUntil;

    public bool IsOpen => isOpen;
    public int CurrentTab => currentTab;

    // ======================================================================
    // 生命周期
    // ======================================================================

    private void Awake()
    {
        RemoveLegacyChildren();
        font = fontOverride != null ? fontOverride : Resources.Load<TMP_FontAsset>("DevConsole/DevConsoleFont");
        if (font == null)
            font = TMP_Settings.defaultFontAsset;
        BuildUI();
    }

    private void Start()
    {
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

    private void OnDisable()
    {
        if (isOpen)
            SetOpen(false);
        UnbindItemEvents();
        boundHealth = null;
    }

    // 旧版控制台由编辑器工具生成在预制件里（Window / ToggleButton）。新版界面由代码生成，旧控件直接移除。
    private void RemoveLegacyChildren()
    {
        bool found = false;
        foreach (string legacy in new[] { "Window", "ToggleButton" })
        {
            Transform child = transform.Find(legacy);
            if (child == null)
                continue;
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
            found = true;
        }
        if (found)
            Debug.Log("DevConsole：已移除预制件中的旧版控件。可以在编辑模式执行 Tools/AstralEchoes/Dev Console/Remove Legacy Layout，把它们从 Canvas.prefab 中删掉。");
    }

    // ======================================================================
    // 打开、关闭、分页
    // ======================================================================

    public void Toggle() => SetOpen(!isOpen);
    public void Close() => SetOpen(false);

    public void SetOpen(bool value)
    {
        if (isOpen == value)
            return;
        isOpen = value;
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
            BindItemEvents();
            RebuildItemList();
            RefreshItemDetail();
            RefreshDisplay();
        }
        else
        {
            foreach (Behaviour behaviour in pausedBehaviours)
                if (behaviour != null)
                    behaviour.enabled = true;
            pausedBehaviours.Clear();
            UnbindItemEvents();
            pendingConfirm = null;
            PlayerMgr.Instance.SetCursorState(previousCursorMode);
        }
    }

    public void SelectTab(int index)
    {
        currentTab = Mathf.Clamp(index, 0, pages.Count - 1);
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(i == currentTab);
            tabImages[i].color = i == currentTab ? Accent : TabIdle;
        }
        pendingConfirm = null;
        if (currentTab == ItemTab && isOpen)
        {
            RebuildItemList();
            RefreshItemDetail();
        }
    }

    private void Pause(Behaviour behaviour)
    {
        if (behaviour == null || !behaviour.enabled)
            return;
        pausedBehaviours.Add(behaviour);
        behaviour.enabled = false;
    }

    // ======================================================================
    // 玩家绑定与状态显示
    // ======================================================================

    private bool BindPlayer()
    {
        if (targetPlayer == null)
            targetPlayer = PlayerMgr.Instance.player;
        boundHealth = targetPlayer != null ? targetPlayer.GetComponent<CharacterHealth>() : null;
        return targetPlayer != null && targetPlayer.Config != null && targetPlayer.Stats != null && boundHealth != null && boundHealth.MaxHp > 0;
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
        if (targetPlayer == null || targetPlayer.Config == null || boundHealth == null || targetPlayer.Stats == null)
        {
            statusText.text = "等待玩家初始化…";
            return;
        }
        CharacterStats stats = targetPlayer.Stats;
        PlayerSaveData save = PlayerMgr.Instance.saveData;
        string level = save != null ? $"Lv.{save.level}  经验 {save.exp}" : "等级未读取";
        statusText.text =
            $"{targetPlayer.name}    {level}    {(boundHealth.isDead ? "死亡" : "存活")}{MoneySummary()}\n" +
            $"生命 {boundHealth.CurrentHp} / {boundHealth.MaxHp}    攻击 {stats.Atk}    防御 {stats.Def}";
        SyncField(attackInput, stats.Atk);
        SyncField(defenseInput, stats.Def);
        SyncField(maxHealthInput, boundHealth.MaxHp);
        if (save != null)
            SyncField(levelInput, save.level);
    }

    // 状态栏附带显示货币类道具（itemType == money）的持有数量。
    private static string MoneySummary()
    {
        string text = "";
        foreach (ItemConfig config in AllItemConfigs())
            if (config.itemType == ItemType.money)
                text += $"    {config.itemName} {PlayerItemMgr.Instance.GetItemCount(config.itemID)}";
        return text;
    }

    private static void SyncField(TMP_InputField field, int value)
    {
        if (field != null && !field.isFocused && field.text != value.ToString())
            field.SetTextWithoutNotify(value.ToString());
    }

    private bool ConfirmTwice(string key, string prompt)
    {
        if (pendingConfirm == key && Time.unscaledTime <= confirmUntil)
        {
            pendingConfirm = null;
            return true;
        }
        pendingConfirm = key;
        confirmUntil = Time.unscaledTime + 3f;
        Say(prompt, true);
        return false;
    }

    // ======================================================================
    // 生命调试
    // ======================================================================

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
        var handler = HealthChangedField.GetValue(boundHealth) as UnityAction<int, int>;
        handler?.Invoke(boundHealth.CurrentHp, boundHealth.MaxHp);
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
        Say(useDefense ? $"已施加 {value} 点原始伤害，经过防御计算。" : $"已直接扣除 {value} 点生命。");
    }

    private void HealBy(int value)
    {
        if (!Ready()) return;
        if (boundHealth.isDead) { Say("玩家已死亡，请使用“复活并回满”。", true); return; }
        SetConsoleHealth((int)System.Math.Min((long)boundHealth.MaxHp, (long)boundHealth.CurrentHp + value));
        Say($"已恢复 {value} 点生命，最高不超过最大生命。");
    }

    public void DamageCustom() { if (ReadAmount(out int amount)) Damage(amount); }
    public void HealCustom() { if (ReadAmount(out int amount)) HealBy(amount); }
    public void TestDamage() { if (ReadAmount(out int amount)) Damage(amount, true); }

    public void FullHealth()
    {
        if (Ready()) HealBy(boundHealth.MaxHp);
    }

    public void KillPlayer()
    {
        if (!Ready()) return;
        if (boundHealth.isDead) { Say("玩家已经处于死亡状态。", true); return; }
        SetConsoleHealth(0);
        Say("玩家生命归零，已调用 Dead() 进入死亡流程。");
    }

    // 复活走玩家自己的 Respawn()；玩家未死亡时只回满，避免多发一次复活广播。
    public void RevivePlayer()
    {
        if (!Ready()) return;
        if (!boundHealth.isDead)
        {
            HealBy(boundHealth.MaxHp);
            Say("玩家未死亡，已恢复满生命。");
            return;
        }
        boundHealth.Respawn();
        RefreshDisplay();
        Say("已调用 Respawn()，玩家复活并恢复满生命。");
    }

    // ======================================================================
    // 属性与等级
    // ======================================================================

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
        Say("运行时属性已调整。");
    }

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
        Say("已按配置表和当前等级重新计算属性，并回满生命。");
    }

    // 等级调试：直接写入玩家存档的等级（经验清零），再调用用户的 PlayerLevelChanged 广播，
    // 由 PlayerObject 重新计算属性、GamePanel 刷新等级；随后保存，和获得经验后的存档行为一致。
    public void ApplyLevel()
    {
        if (!Ready()) return;
        if (!int.TryParse(levelInput.text, out int level))
        {
            Say("等级请输入整数。", true);
            return;
        }
        SetLevel(level);
    }

    public void ResetLevel()
    {
        if (!Ready()) return;
        SetLevel(targetPlayer.Config.initLv);
    }

    private void SetLevel(int level)
    {
        var config = targetPlayer.Config;
        int clamped = Mathf.Clamp(level, config.initLv, Mathf.Max(config.initLv, config.maxLv));
        PlayerSaveData save = PlayerMgr.Instance.saveData;
        save.level = clamped;
        save.exp = 0;
        PlayerMgr.Instance.PlayerLevelChanged();
        PlayerMgr.Instance.SavePlayerData();
        RefreshDisplay();
        string note = clamped != level ? $"（输入 {level} 超出 {config.initLv}～{config.maxLv}，已限制）" : "";
        Say($"等级已设为 Lv.{clamped}，经验清零并写入存档{note}。当前生命保持不变。");
    }

    // ======================================================================
    // 道具：从列表选择或直接输入物品 ID，调用 PlayerItemMgr / RewardMgr
    // ======================================================================

    private void BindItemEvents()
    {
        if (itemEventBound) return;
        PlayerItemMgr.Instance.OnItemCountChanged += OnItemCountChanged;
        itemEventBound = true;
    }

    private void UnbindItemEvents()
    {
        if (!itemEventBound) return;
        PlayerItemMgr.Instance.OnItemCountChanged -= OnItemCountChanged;
        itemEventBound = false;
    }

    // 订阅用户的数量变化事件：无论是控制台、奖励结算还是其他系统改了数量，列表和详情都会跟着刷新。
    private void OnItemCountChanged(int itemId, int newCount)
    {
        RefreshItemRows();
        RefreshItemDetail();
    }

    private static List<ItemConfig> AllItemConfigs()
    {
        ItemDatabase database = ConfigMgr.Instance.itemDatabase;
        var list = new List<ItemConfig>();
        if (database != null && database.items != null)
            foreach (ItemConfig config in database.items)
                if (config != null)
                    list.Add(config);
        return list;
    }

    private static string TypeName(ItemType type)
    {
        switch (type)
        {
            case ItemType.exp: return "经验";
            case ItemType.money: return "货币";
            case ItemType.consumable: return "消耗品";
            case ItemType.missionItem: return "任务物品";
            case ItemType.stuff: return "材料";
            default: return type.ToString();
        }
    }

    private bool TryGetInputItemId(out int id) => int.TryParse(itemIdInput.text, out id);

    private void RebuildItemList()
    {
        if (itemListContent == null) return;
        // 先移出再销毁：Destroy 要到帧末才生效，避免同一帧里新旧两套同名行并存。
        for (int i = itemListContent.childCount - 1; i >= 0; i--)
        {
            Transform child = itemListContent.GetChild(i);
            child.gameObject.SetActive(false);
            child.SetParent(null, false);
            Destroy(child.gameObject);
        }
        itemRows.Clear();
        string filter = itemSearchInput != null ? itemSearchInput.text.Trim() : "";
        foreach (ItemConfig config in AllItemConfigs())
        {
            if (filter.Length > 0 && !config.itemID.ToString().Contains(filter) &&
                (config.itemName == null || !config.itemName.Contains(filter)))
                continue;
            itemRows.Add(CreateItemRow(config));
        }
        if (itemRows.Count == 0)
        {
            TMP_Text empty = Label(itemListContent, "Empty", filter.Length > 0 ? "没有匹配的物品" : "ItemDatabase 为空", 0, 0, 280, 40, 16, Muted);
            empty.gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            empty.alignment = TextAlignmentOptions.Center;
        }
        RefreshItemRows();
    }

    private ItemRow CreateItemRow(ItemConfig config)
    {
        int id = config.itemID;
        GameObject go = ButtonBase(itemListContent, "ItemRow_" + id, 0, 0, 280, 56, RowIdle, () => SelectItem(id));
        go.AddComponent<LayoutElement>().preferredHeight = 56;
        CreateIcon(go.transform, config, 8, 8, 40, out _, out _);
        Label(go.transform, "Name", $"#{id}  {config.itemName}", 58, 5, 214, 26, 18, Color.white);
        TMP_Text sub = Label(go.transform, "Sub", "", 58, 30, 214, 22, 14, Muted);
        return new ItemRow { id = id, background = go.GetComponent<Image>(), sub = sub };
    }

    private void CreateIcon(Transform parent, ItemConfig config, float x, float y, float size, out Image image, out TMP_Text letter)
    {
        GameObject frame = Box(parent, "Icon", x, y, size, size, IconEmpty);
        image = frame.GetComponent<Image>();
        image.raycastTarget = false;
        letter = Label(frame.transform, "Letter", "", 0, 0, size, size, Mathf.RoundToInt(size * 0.5f), Muted);
        letter.alignment = TextAlignmentOptions.Center;
        ApplyIcon(image, letter, config);
    }

    // 配置里没有图标时显示名称首字，便于区分。
    private static void ApplyIcon(Image image, TMP_Text letter, ItemConfig config)
    {
        Sprite sprite = config != null ? config.itemIcon : null;
        image.sprite = sprite;
        image.color = sprite != null ? Color.white : IconEmpty;
        image.preserveAspect = true;
        if (config == null)
            letter.text = "?";
        else if (sprite != null || string.IsNullOrEmpty(config.itemName))
            letter.text = "";
        else
            letter.text = config.itemName.Substring(0, 1);
    }

    private void RefreshItemRows()
    {
        bool hasId = TryGetInputItemId(out int selected);
        foreach (ItemRow row in itemRows)
        {
            ItemConfig config = ConfigMgr.Instance.GetItemConfigByID(row.id);
            if (config == null) continue;
            string limit = config.maxCount > 0 ? $" / {config.maxCount}" : "";
            row.sub.text = config.itemType == ItemType.exp
                ? "经验 · 不进背包"
                : $"{TypeName(config.itemType)} · 持有 {PlayerItemMgr.Instance.GetItemCount(row.id)}{limit}";
            row.background.color = hasId && row.id == selected ? RowSelected : RowIdle;
        }
    }

    public void SelectItem(int id)
    {
        itemIdInput.SetTextWithoutNotify(id.ToString());
        RefreshItemRows();
        RefreshItemDetail();
    }

    private void RefreshItemDetail()
    {
        if (itemDetailName == null) return;
        if (!TryGetInputItemId(out int id))
        {
            ApplyIcon(itemDetailIcon, itemDetailIconLetter, null);
            itemDetailName.text = "未选择物品";
            itemDetailMeta.text = "从左侧列表选择，或直接输入物品 ID";
            itemDetailHeld.text = "";
            itemDetailDesc.text = "";
            return;
        }
        ItemConfig config = ConfigMgr.Instance.GetItemConfigByID(id);
        int held = PlayerItemMgr.Instance.GetItemCount(id);
        ApplyIcon(itemDetailIcon, itemDetailIconLetter, config);
        if (config == null)
        {
            itemDetailName.text = "未找到物品";
            itemDetailMeta.text = $"ItemDatabase 中没有 ID {id}";
            itemDetailHeld.text = held > 0 ? $"存档中持有 {held}（没有对应配置）" : "";
            itemDetailDesc.text = "不在配置表里的 ID 不能添加：背包格子读取不到它的配置。";
            return;
        }
        PlayerSaveData save = PlayerMgr.Instance.saveData;
        itemDetailName.text = config.itemName;
        itemDetailMeta.text = $"ID {config.itemID} · {TypeName(config.itemType)} · 上限 {(config.maxCount > 0 ? config.maxCount.ToString() : "无")}";
        itemDetailHeld.text = config.itemType == ItemType.exp
            ? $"当前 Lv.{save.level}  经验 {save.exp}"
            : $"当前持有 {held}";
        itemDetailDesc.text = config.itemDesc;
    }

    private bool ReadItemCount(out int count)
    {
        if (int.TryParse(itemCountInput.text, out count) && count > 0 && count <= 1000000)
            return true;
        Say("数量请输入 1～1000000 的整数。", true);
        return false;
    }

    private bool ReadItemInput(out ItemConfig config, out int count)
    {
        config = null;
        count = 0;
        if (!TryGetInputItemId(out int id))
        {
            Say("请先选择物品，或输入整数物品 ID。", true);
            return false;
        }
        config = ConfigMgr.Instance.GetItemConfigByID(id);
        if (config == null)
        {
            Say($"ItemDatabase 中没有 ID {id}，不能添加。", true);
            return false;
        }
        return ReadItemCount(out count);
    }

    // PlayerItemMgr 按 maxCount 截断，实际增加量少于请求数量时说明原因。
    private static string LimitNote(ItemConfig config, int before, int after, int requested)
    {
        if (after - before < requested && config.maxCount > 0 && after >= config.maxCount)
            return $" 达到上限 {config.maxCount}，实际增加 {after - before}，多出的被丢弃。";
        if (config.maxCount > 0 && after > config.maxCount)
            return $" 注意：持有数量超过了 maxCount {config.maxCount}。";
        return "";
    }

    public void AddSelectedItem()
    {
        if (!ReadItemInput(out ItemConfig config, out int count)) return;
        // 背包设计约定经验不进物品数据（记录 37），这里不把经验写进背包存档。
        if (config.itemType == ItemType.exp)
        {
            Say("经验不进背包。要加经验请点“按奖励结算”，它会走 RewardMgr → PlayerMgr.AddExp。", true);
            return;
        }
        int before = PlayerItemMgr.Instance.GetItemCount(config.itemID);
        if ((long)before + count > int.MaxValue)
        {
            Say($"持有 {before}，再加 {count} 会超出 int 上限，已拒绝。", true);
            return;
        }
        PlayerItemMgr.Instance.AddItem(config.itemID, count);
        int after = PlayerItemMgr.Instance.GetItemCount(config.itemID);
        Say($"已调用 AddItem({config.itemID}, {count})：{config.itemName} {before} → {after}，已存盘。{LimitNote(config, before, after, count)}");
        RefreshItemRows();
        RefreshItemDetail();
        RefreshDisplay();
    }

    // 扣除只要求 ID 合法，不要求配置存在，方便清理存档里没有配置的旧记录。
    public void RemoveSelectedItem()
    {
        if (!TryGetInputItemId(out int id)) { Say("请先选择物品，或输入整数物品 ID。", true); return; }
        if (!ReadItemCount(out int count)) return;
        ItemConfig config = ConfigMgr.Instance.GetItemConfigByID(id);
        string name = config != null ? config.itemName : $"ID {id}";
        int before = PlayerItemMgr.Instance.GetItemCount(id);
        bool removed = PlayerItemMgr.Instance.RemoveItem(id, count);
        int after = PlayerItemMgr.Instance.GetItemCount(id);
        if (removed)
            Say($"已调用 RemoveItem({id}, {count})：{name} {before} → {after}，已存盘。");
        else
            Say($"RemoveItem({id}, {count}) 返回 false：{name} 持有 {before}，数量不足或未持有。", true);
        RefreshItemRows();
        RefreshItemDetail();
        RefreshDisplay();
    }

    // 走用户的奖励结算入口，用来检查 RewardMgr 对各类型的处理是否到位。
    public void SettleSelectedItem()
    {
        if (!ReadItemInput(out ItemConfig config, out int count)) return;
        PlayerSaveData save = PlayerMgr.Instance.saveData;
        int beforeCount = PlayerItemMgr.Instance.GetItemCount(config.itemID);
        int beforeLevel = save.level, beforeExp = save.exp;
        RewardMgr.Instance.SettlementReward(new List<ItemRewardData> { new ItemRewardData { itemId = config.itemID, count = count } });
        int afterCount = PlayerItemMgr.Instance.GetItemCount(config.itemID);
        if (config.itemType == ItemType.exp)
            Say($"已结算 {count} 点经验：Lv.{beforeLevel} 经验 {beforeExp} → Lv.{save.level} 经验 {save.exp}。");
        else if (afterCount != beforeCount)
            Say($"已结算 {config.itemName} ×{count}：持有 {beforeCount} → {afterCount}。{LimitNote(config, beforeCount, afterCount, count)}");
        else if (config.maxCount > 0 && beforeCount >= config.maxCount)
            Say($"已结算 {config.itemName} ×{count}，但持有数量已在上限 {config.maxCount}，没有增加。", true);
        else
            Say($"已调用 SettlementReward，但 {config.itemName}（{TypeName(config.itemType)}）的持有数量没有变化，请检查 RewardMgr 对这个类型的处理。", true);
        RefreshDisplay();
        RefreshItemRows();
        RefreshItemDetail();
    }

    // ======================================================================
    // 存档
    // ======================================================================

    // 保存空进度并同步内存，防止旧任务数据再次被写回存档。
    public void ClearMissionSave()
    {
        if (!ConfirmTwice(nameof(ClearMissionSave), "再点一次“清空任务存档”确认（3 秒内）。")) return;
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

    // 对每条持有记录调用用户的 RemoveItem 扣到 0，由 PlayerItemMgr 自己删除记录、广播并存盘。
    public void ClearAllItems()
    {
        if (!ConfirmTwice(nameof(ClearAllItems), "再点一次“清空背包道具”确认（3 秒内）。")) return;
        var snapshot = new List<(int id, int count)>();
        foreach (ItemCountData data in PlayerItemMgr.Instance.GetAllItems())
            snapshot.Add((data.itemID, data.itemCount));
        int cleared = 0, failed = 0;
        foreach (var (id, count) in snapshot)
        {
            if (count > 0 && PlayerItemMgr.Instance.RemoveItem(id, count)) cleared++;
            else failed++;
        }
        string tail = failed > 0 ? $"；{failed} 条记录数量不大于 0 或扣除失败，仍留在存档里" : "";
        Say($"已清空 {cleared} 种道具{tail}。", failed > 0);
        RefreshItemRows();
        RefreshItemDetail();
        RefreshDisplay();
    }

    // ======================================================================
    // 界面生成
    // ======================================================================

    private void BuildUI()
    {
        GameObject launcher = Button(transform, "ConsoleLauncher", "管理员控制台 · F1", 0, 0, LauncherWidth, LauncherHeight, Accent, Toggle);
        RectTransform launcherRect = launcher.GetComponent<RectTransform>();
        launcherRect.anchorMin = launcherRect.anchorMax = launcherRect.pivot = Vector2.one;
        launcherRect.anchoredPosition = new Vector2(-EdgeMargin, -LauncherY);

        window = Box(transform, "ConsoleWindow", 0, 0, WindowWidth, WindowHeight, Background);
        // 先隐藏再生成子物体：输入框等组件在首次打开时才执行 OnEnable，此时引用已全部接好。
        window.SetActive(false);
        RectTransform windowRect = window.GetComponent<RectTransform>();
        windowRect.anchorMin = windowRect.anchorMax = windowRect.pivot = Vector2.one;
        windowRect.anchoredPosition = new Vector2(-EdgeMargin, -WindowTop);
        Transform w = window.transform;

        Box(w, "AccentLine", 0, 0, WindowWidth, 4, AccentBright);
        Label(w, "Title", "管理员控制台", 24, 14, 220, 36, 27, Color.white);
        Label(w, "Subtitle", "F1 开关 · Esc 关闭 · 只用于验证玩法接口", 250, 22, 560, 26, 16, Muted);
        Button(w, "CloseButton", "×", WindowWidth - 58, 12, 40, 38, new Color(0.22f, 0.27f, 0.35f), Close);

        Box(w, "StatusBackground", 16, 64, WindowWidth - 32, 66, PanelColor);
        statusText = Label(w, "Status", "等待玩家初始化…", 30, 70, WindowWidth - 60, 56, 19, Color.white);

        for (int i = 0; i < TabTitles.Length; i++)
        {
            int index = i;
            GameObject tab = Button(w, "Tab" + i, TabTitles[i], 16, PageY + i * 56, 136, 48, TabIdle, () => SelectTab(index));
            tabImages.Add(tab.GetComponent<Image>());
        }

        pages.Add(BuildHealthPage(Page(w, "HealthPage")));
        pages.Add(BuildStatsPage(Page(w, "StatsPage")));
        pages.Add(BuildItemPage(Page(w, "ItemPage")));
        pages.Add(BuildSavePage(Page(w, "SavePage")));

        Box(w, "MessageBackground", 16, 624, WindowWidth - 32, 60, PanelColor);
        messageText = Label(w, "Message", "选择左侧分页开始调试。数值输入后回车或移开焦点即生效。", 30, 630, WindowWidth - 60, 50, 16, new Color(0.55f, 0.87f, 0.9f));

        SelectTab(HealthTab);
    }

    private Transform Page(Transform parent, string name) => Rect(parent, name, PageX, PageY, PageWidth, PageHeight);

    private GameObject BuildHealthPage(Transform p)
    {
        Section(p, "HealthTitle", "生命调试", 0);
        float bw = (PageWidth - 30) / 4f;
        Button(p, "Damage10", "扣血 10", 0, 40, bw, 44, Danger, () => Damage(10));
        Button(p, "Damage25", "扣血 25", bw + 10, 40, bw, 44, Danger, () => Damage(25));
        Button(p, "Heal10", "回血 10", (bw + 10) * 2, 40, bw, 44, HealColor, () => HealBy(10));
        Button(p, "Heal25", "回血 25", (bw + 10) * 3, 40, bw, 44, HealColor, () => HealBy(25));

        amountInput = Input(p, "AmountInput", "10", 0, 96, 140, 44, TMP_InputField.ContentType.IntegerNumber);
        Button(p, "DamageCustom", "按数值扣血", 150, 96, 278, 44, Danger, DamageCustom);
        Button(p, "HealCustom", "按数值回血", 438, 96, 278, 44, HealColor, HealCustom);

        float tw = (PageWidth - 20) / 3f;
        Button(p, "FullHealth", "恢复满血", 0, 152, tw, 44, HealColor, FullHealth);
        Button(p, "KillPlayer", "杀死玩家", tw + 10, 152, tw, 44, new Color(0.64f, 0.16f, 0.20f), KillPlayer);
        Button(p, "RevivePlayer", "复活并回满", (tw + 10) * 2, 152, tw, 44, Accent, RevivePlayer);
        Button(p, "TestDamage", "按输入值造成伤害（经过防御）", 0, 208, PageWidth, 44, Neutral, TestDamage);

        Hint(p, "扣血 / 回血直接改写生命值；“造成伤害”调用 TakeDamage，会先减去防御。生命归零时调用 Dead()，复活调用 Respawn()。", 270);
        return p.gameObject;
    }

    private GameObject BuildStatsPage(Transform p)
    {
        Section(p, "AttributesTitle", "实时属性（只改运行时，不写 SO）", 0);
        attackInput = StatRow(p, "Attack", "攻击力", 40, "步长 1", () => Step(-1, 0, 0), () => Step(1, 0, 0));
        defenseInput = StatRow(p, "Defense", "防御力", 96, "步长 1", () => Step(0, -1, 0), () => Step(0, 1, 0));
        maxHealthInput = StatRow(p, "MaxHealth", "最大生命", 152, "步长 10", () => Step(0, 0, -10), () => Step(0, 0, 10));
        foreach (TMP_InputField field in new[] { attackInput, defenseInput, maxHealthInput })
            field.onEndEdit.AddListener(_ => ApplyAttributes());
        float hw = (PageWidth - 10) / 2f;
        Button(p, "ApplyAttributes", "应用属性", 0, 208, hw, 44, Accent, ApplyAttributes);
        Button(p, "ResetAttributes", "恢复 SO 初始值", hw + 10, 208, hw, 44, Neutral, ResetAttributes);

        Section(p, "LevelTitle", "等级（写入存档，经验清零）", 280);
        levelInput = Input(p, "LevelInput", "1", 0, 320, 140, 44, TMP_InputField.ContentType.IntegerNumber);
        Button(p, "ApplyLevel", "设置等级", 150, 320, 278, 44, Accent, ApplyLevel);
        Button(p, "ResetLevel", "重置为初始等级", 438, 320, 278, 44, Neutral, ResetLevel);
        Hint(p, "属性改动在升级、设置等级或恢复初始值时会按等级重新计算并被覆盖。等级超出 initLv～maxLv 时自动限制。", 378);
        return p.gameObject;
    }

    private TMP_InputField StatRow(Transform p, string key, string title, float y, string step, UnityAction down, UnityAction up)
    {
        Label(p, key + "Label", title, 0, y + 8, 120, 30, 19, Color.white);
        Button(p, key + "Down", "−", 130, y, 60, 44, Accent, down);
        TMP_InputField field = Input(p, key + "Input", "0", 200, y, 200, 44, TMP_InputField.ContentType.IntegerNumber);
        Button(p, key + "Up", "+", 410, y, 60, 44, Accent, up);
        Label(p, key + "Step", step, 486, y + 10, 200, 26, 15, Muted);
        return field;
    }

    private GameObject BuildItemPage(Transform p)
    {
        // 左侧：搜索 + 可滚动的物品列表
        itemSearchInput = Input(p, "ItemSearch", "", 0, 0, 300, 44, TMP_InputField.ContentType.Standard, "搜索名称或 ID");
        itemSearchInput.characterLimit = 20;
        itemSearchInput.onValueChanged.AddListener(_ => RebuildItemList());
        itemListContent = ScrollList(p, "ItemList", 0, 54, 300, PageHeight - 54);

        // 右侧：详情
        const float rx = 316, rw = PageWidth - rx;
        GameObject detail = Box(p, "ItemDetail", rx, 0, rw, 190, PanelColor);
        CreateIcon(detail.transform, null, 14, 14, 72, out itemDetailIcon, out itemDetailIconLetter);
        itemDetailName = Label(detail.transform, "DetailName", "", 100, 12, rw - 112, 32, 22, Color.white);
        itemDetailMeta = Label(detail.transform, "DetailMeta", "", 100, 48, rw - 112, 24, 15, Muted);
        itemDetailHeld = Label(detail.transform, "DetailHeld", "", 100, 74, rw - 112, 24, 16, AccentBright);
        itemDetailDesc = Label(detail.transform, "DetailDesc", "", 14, 102, rw - 28, 80, 15, new Color(0.82f, 0.86f, 0.92f));

        // 右侧：输入与操作
        Label(p, "ItemIdLabel", "物品 ID", rx, 198, 190, 24, 15, Muted);
        Label(p, "ItemCountLabel", "数量", rx + 200, 198, 200, 24, 15, Muted);
        itemIdInput = Input(p, "ItemIdInput", "", rx, 222, 190, 44, TMP_InputField.ContentType.IntegerNumber, "输入 ID");
        itemIdInput.onValueChanged.AddListener(_ => { RefreshItemRows(); RefreshItemDetail(); });
        itemCountInput = Input(p, "ItemCountInput", "1", rx + 200, 222, rw - 200, 44, TMP_InputField.ContentType.IntegerNumber);

        int[] quick = { 1, 10, 100, 999 };
        float qw = (rw - 30) / 4f;
        for (int i = 0; i < quick.Length; i++)
        {
            int value = quick[i];
            Button(p, "ItemQuick" + value, "×" + value, rx + i * (qw + 10), 276, qw, 36, Neutral, () => itemCountInput.text = value.ToString());
        }

        float hw = (rw - 10) / 2f;
        Button(p, "AddItem", "加入背包", rx, 322, hw, 46, HealColor, AddSelectedItem);
        Button(p, "RemoveItem", "扣除", rx + hw + 10, 322, hw, 46, Danger, RemoveSelectedItem);
        Button(p, "RewardItem", "按奖励结算（RewardMgr）", rx, 378, rw, 42, Neutral, SettleSelectedItem);
        Hint(p, "加入 / 扣除调用 PlayerItemMgr；奖励结算走 RewardMgr.SettlementReward。", 428, rx, rw);
        return p.gameObject;
    }

    private GameObject BuildSavePage(Transform p)
    {
        Section(p, "SaveTitle", "存档（需在 3 秒内点两次确认）", 0);
        Button(p, "ClearMissionSave", "清空任务存档", 0, 40, PageWidth, 48, Danger, ClearMissionSave);
        Hint(p, "保存空的 MissionData，并同步 MissionMgr 中的任务进度。", 96);
        Button(p, "ClearItems", "清空背包道具", 0, 140, PageWidth, 48, Danger, ClearAllItems);
        Hint(p, "对每条持有记录调用 PlayerItemMgr.RemoveItem 扣到 0，由管理器删除记录并存盘。", 196);
        return p.gameObject;
    }

    // ---- 控件工具 ----

    private void Section(Transform parent, string name, string title, float y) =>
        Label(parent, name, title, 0, y, PageWidth, 30, 22, AccentBright);

    private void Hint(Transform parent, string text, float y, float x = 0, float width = PageWidth) =>
        Label(parent, "Hint" + y, text, x, y, width, 44, 14, Muted);

    private static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    private static GameObject Box(Transform parent, string name, float x, float y, float width, float height, Color color)
    {
        var rect = Rect(parent, name, x, y, width, height);
        rect.gameObject.AddComponent<Image>().color = color;
        return rect.gameObject;
    }

    private TMP_Text Label(Transform parent, string name, string value, float x, float y, float width, float height, int size, Color color)
    {
        var rect = Rect(parent, name, x, y, width, height);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.richText = false;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        return text;
    }

    private static GameObject ButtonBase(Transform parent, string name, float x, float y, float width, float height, Color color, UnityAction action)
    {
        var go = Box(parent, name, x, y, width, height, color);
        var button = go.AddComponent<Button>();
        button.targetGraphic = go.GetComponent<Image>();
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        button.colors = colors;
        button.onClick.AddListener(action);
        return go;
    }

    private GameObject Button(Transform parent, string name, string title, float x, float y, float width, float height, Color color, UnityAction action)
    {
        GameObject go = ButtonBase(parent, name, x, y, width, height, color, action);
        var text = Label(go.transform, "Label", title, 4, 0, width - 8, height, 19, Color.white);
        text.alignment = TextAlignmentOptions.Center;
        return go;
    }

    private TMP_InputField Input(Transform parent, string name, string value, float x, float y, float width, float height,
        TMP_InputField.ContentType contentType, string placeholder = null)
    {
        var go = Box(parent, name, x, y, width, height, FieldColor);
        var input = go.AddComponent<TMP_InputField>();
        var area = Rect(go.transform, "TextArea", 10, 4, width - 20, height - 8);
        area.gameObject.AddComponent<RectMask2D>();
        var text = Label(area, "Text", "", 0, 0, width - 20, height - 8, 21, Color.white);
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.overflowMode = TextOverflowModes.Overflow;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        if (placeholder != null)
        {
            var hint = Label(area, "Placeholder", placeholder, 0, 0, width - 20, height - 8, 18, new Color(0.45f, 0.53f, 0.63f));
            hint.alignment = TextAlignmentOptions.MidlineLeft;
            input.placeholder = hint;
        }
        input.textViewport = area;
        input.textComponent = text as TextMeshProUGUI;
        input.targetGraphic = go.GetComponent<Image>();
        input.contentType = contentType;
        input.characterLimit = 8;
        input.text = value;
        input.caretColor = Color.white;
        input.customCaretColor = true;
        return input;
    }

    // 纵向滚动列表：Viewport 裁剪，Content 由 VerticalLayoutGroup 排列、ContentSizeFitter 撑高。
    private static RectTransform ScrollList(Transform parent, string name, float x, float y, float width, float height)
    {
        GameObject root = Box(parent, name, x, y, width, height, new Color(0.07f, 0.10f, 0.16f));
        var scroll = root.AddComponent<ScrollRect>();
        RectTransform viewport = Rect(root.transform, "Viewport", 0, 0, width, height);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = viewport.offsetMax = Vector2.zero;
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform content = Rect(viewport, "Content", 0, 0, width, 0);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 6;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30;
        return content;
    }
}
