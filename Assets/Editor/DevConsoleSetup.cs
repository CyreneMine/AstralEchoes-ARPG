#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// 控制台的编辑器工具（Claude Code 维护）。
// 控制台界面已改为运行时由 DevConsole 代码生成，这里只负责：把 DevConsole 物体放进 Canvas 预制件、
// 清理旧版预制件控件、准备字体，以及 Play Mode 自动检查。
public static class DevConsoleSetup
{
    private const string CanvasPrefabPath = "Assets/Resources/UI/Canvas.prefab";
    private const string RequestPath = "Temp/DevConsoleSetup.request";
    private const string ReportDir = "Temp/DevConsoleReports";
    private const string StageKey = "AstralEchoes.DevConsole.VerifyStage";
    private static readonly string[] LegacyChildren = { "Window", "ToggleButton" };

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.delayCall += RunPending;
    }

    // 编译完成后读取 Temp/DevConsoleSetup.request：内容含 migrate 时先清理旧控件，再进入 Play Mode 自动检查。
    private static void RunPending()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += RunPending;
            return;
        }
        string stage = SessionState.GetString(StageKey, "");
        if (Application.isPlaying && stage == "WaitingForPlayer")
        {
            EditorApplication.update -= VerifyTick;
            EditorApplication.update += VerifyTick;
            return;
        }
        if (!File.Exists(RequestPath) || Application.isPlaying)
            return;
        string request = File.ReadAllText(RequestPath);
        File.Delete(RequestPath);
        try
        {
            if (request.Contains("migrate"))
                RemoveLegacyLayout();
            if (request.Contains("verify"))
                Verify();
        }
        catch (Exception error)
        {
            WriteReport("setup-error.txt", error.ToString());
            Debug.LogException(error);
        }
    }

    // 资源恢复用：Canvas 预制件里没有控制台时，加一个挂 DevConsole 的空物体，界面由代码生成。
    [MenuItem("Tools/AstralEchoes/Dev Console/Add to Canvas Prefab")]
    public static void AddToCanvasPrefab()
    {
        if (Application.isPlaying) throw new InvalidOperationException("请在编辑模式执行。");
        GetConsoleFont();
        GameObject prefab = PrefabUtility.LoadPrefabContents(CanvasPrefabPath);
        try
        {
            if (prefab.GetComponentInChildren<DevConsole>(true) != null)
            {
                Debug.Log("[DevConsole] Canvas.prefab 里已经有 DevConsole，未做修改。");
                return;
            }
            // 不用 ??：编辑器里 GetComponent 找不到时返回的是 Unity 的"假 null"。
            Canvas rootCanvas = prefab.GetComponent<Canvas>();
            if (rootCanvas == null) rootCanvas = prefab.GetComponentInChildren<Canvas>(true);
            if (rootCanvas == null) throw new InvalidOperationException("Canvas.prefab 里没有 Canvas。");
            var root = new GameObject("DevConsole", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(DevConsole));
            var rect = root.GetComponent<RectTransform>();
            rect.SetParent(rootCanvas.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var overlay = root.GetComponent<Canvas>();
            overlay.overrideSorting = true;
            overlay.sortingOrder = 5000;
            PrefabUtility.SaveAsPrefabAsset(prefab, CanvasPrefabPath);
            Debug.Log("[DevConsole] 已在 Canvas.prefab 中加入 DevConsole。");
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
    }

    // 旧版由编辑器生成的 Window / ToggleButton 不再使用；运行时 DevConsole 也会自动移除它们，这里把预制件一并清理干净。
    [MenuItem("Tools/AstralEchoes/Dev Console/Remove Legacy Layout")]
    public static void RemoveLegacyLayout()
    {
        if (Application.isPlaying) throw new InvalidOperationException("请在编辑模式执行。");
        GameObject prefab = PrefabUtility.LoadPrefabContents(CanvasPrefabPath);
        int removed = 0, consoles = 0;
        try
        {
            foreach (DevConsole console in prefab.GetComponentsInChildren<DevConsole>(true))
            {
                consoles++;
                foreach (string name in LegacyChildren)
                {
                    Transform child = console.transform.Find(name);
                    if (child == null) continue;
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                    removed++;
                }
            }
            if (consoles == 0) throw new InvalidOperationException("Canvas.prefab 里没有找到 DevConsole，可先执行 Add to Canvas Prefab。");
            if (removed > 0) PrefabUtility.SaveAsPrefabAsset(prefab, CanvasPrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        WriteReport("remove-legacy.json", $"{{\"consoles\":{consoles},\"removedChildren\":{removed}}}");
        Debug.Log($"[DevConsole] 已从 Canvas.prefab 移除 {removed} 个旧版控件。");
    }

    private static TMP_FontAsset GetConsoleFont()
    {
        const string path = "Assets/Resources/DevConsole/DevConsoleFont.asset";
        var loaded = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (loaded != null && loaded.material != null && AssetDatabase.Contains(loaded.material)) return loaded;
        // 仅重建本工具刚创建、尚未完成保存的控制台字体资产。
        if (loaded != null) AssetDatabase.DeleteAsset(path);
        if (!AssetDatabase.IsValidFolder("Assets/Resources/DevConsole"))
            AssetDatabase.CreateFolder("Assets/Resources", "DevConsole");
        Font source = AssetDatabase.LoadAssetAtPath<Font>("Assets/ArtRes/Font/STHUPO.TTF");
        if (source == null) throw new InvalidOperationException("控制台中文字体源文件未找到。");
        var created = TMP_FontAsset.CreateFontAsset(source, 48, 5,
            UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048,
            AtlasPopulationMode.Dynamic, true);
        created.name = "DevConsoleFont";
        AssetDatabase.CreateAsset(created, path);
        string characters = File.ReadAllText("Assets/Scripts/UI/DevConsole.cs") +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 /.-+×−_#·";
        characters = new string(characters.Where(c => !char.IsControl(c)).Distinct().ToArray());
        created.TryAddCharacters(characters, out string missing);
        created.fallbackFontAssetTable = new List<TMP_FontAsset>();
        if (TMP_Settings.defaultFontAsset != null)
            created.fallbackFontAssetTable.Add(TMP_Settings.defaultFontAsset);
        AssetDatabase.AddObjectToAsset(created.material, created);
        foreach (var texture in created.atlasTextures)
            if (texture != null) AssetDatabase.AddObjectToAsset(texture, created);
        EditorUtility.SetDirty(created);
        AssetDatabase.SaveAssets();
        WriteReport("font-prewarm.json", "{\"requestedCharacters\":" + characters.Length + ",\"missing\":\"" + missing.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"}");
        return created;
    }

    [MenuItem("Tools/AstralEchoes/Dev Console/Verify in Play Mode")]
    public static void Verify()
    {
        SessionState.SetString(StageKey, "WaitingForPlayer");
        if (!Application.isPlaying) EditorApplication.EnterPlaymode();
        else { EditorApplication.update -= VerifyTick; EditorApplication.update += VerifyTick; }
    }

    // ---- Play Mode 自动检查（Claude 维护）：逐项记录通过/失败，不在第一处失败时中断 ----
    private static readonly List<string> checkLines = new List<string>();
    private static int passCount, failCount, stageFrame, captureIndex;
    private static int healthEventCount, stateEventCount, levelEventCount, itemEventCount;
    private static bool lastStateEvent;
    private static bool startCursorMode;
    private static float startMax, startAttack, startDefense;
    private static PlayerObject vPlayer;
    private static CharacterHealth vHealth;
    private static DevConsole vConsole;
    private static readonly FieldInfo HealthDefField = typeof(CharacterHealth).GetField("def", BindingFlags.Instance | BindingFlags.NonPublic);
    // 敌人（#42）和玩家（#45）改为状态机后，原来的 isReturning 等标记已不存在；状态机是私有字段，这里用反射读当前状态。
    private static readonly FieldInfo EnemyStateMachineField = typeof(EnemyAI).GetField("stateMachine", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo PlayerStateMachineField = typeof(PlayerController).GetField("stateMachine", BindingFlags.Instance | BindingFlags.NonPublic);
    private static IState CurrentStateOf(FieldInfo field, object owner) => field == null || owner == null ? null : (field.GetValue(owner) as StateMachine)?.CurrentState;
    private static string StateName(IState state) => state == null ? "null" : state.GetType().Name;
    private static readonly string[] CaptureNames = { "Health", "Stats", "Items", "Save", "Buff" };

    // ---- Buff 检查：只读 BuffMgr / TimerMgr / CharacterStats 的内部状态；
    // 唯一的写入是把一个计时器的剩余时间改成 0.05 秒，让真实的到期流程（TimerMgr → RemoveBuff → 减回加成）在检查里跑一遍。
    private static readonly FieldInfo BuffActiveField = typeof(BuffMgr).GetField("hasBuffs", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo BuffTimerIdsField = typeof(BuffMgr).GetField("timers", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo TimerMgrTimersField = typeof(TimerMgr).GetField("timers", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo StatsAtkPercentField = typeof(CharacterStats).GetField("atkBuffPercent", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo StatsDefPercentField = typeof(CharacterStats).GetField("defBuffPercent", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo StatsMaxHpPercentField = typeof(CharacterStats).GetField("maxHpBuffPercent", BindingFlags.Instance | BindingFlags.NonPublic);
    // 计时阶段用来到期的那个 Buff，以及它所在属性的白值。为 null 表示跳过计时阶段。
    private static BuffConfig buffProbe;
    private static float buffProbeBase, buffRemainingMark, buffStageTime;
    private static float buffSavedTimeScale = -1;
    private static int buffProbeTimerId, buffPoolMark;
    private static bool captureBuffAdded;
    // 三选一：onChoiceCountChange 可能是无参或带次数参数的事件，按实际类型订阅；无参时 lastChoiceEventValue 保持 int.MinValue。
    private const int RollSamples = 200;
    private static Delegate choiceHandler;
    private static int choiceEventCount, lastChoiceEventValue = int.MinValue;

    private static void OnLevelEvent() => levelEventCount++;
    // 记下最后一次广播的参数：HUD 血条只靠这次广播刷新，所以它必须等于最终数值。
    private static float lastEventHp, lastEventMax;
    private static void OnHealthEvent(float current, float max) { healthEventCount++; lastEventHp = current; lastEventMax = max; }
    // 属性和生命是 float，按小误差比较，避免浮点运算顺序不同造成的误判。
    private static bool Same(float a, float b) => Mathf.Abs(a - b) < 0.001f;
    private static float HealthDef() => (float)HealthDefField.GetValue(vHealth);
    private static void OnStateEvent(bool dead) { stateEventCount++; lastStateEvent = dead; }
    private static void OnItemEvent(int id, int count) => itemEventCount++;

    private static void Check(bool condition, string name, string detail = "")
    {
        if (condition) { passCount++; checkLines.Add("PASS  " + name); }
        else { failCount++; checkLines.Add("FAIL  " + name + (detail.Length > 0 ? "  —— " + detail : "")); }
    }

    private static void Note(string text) => checkLines.Add("NOTE  " + text);

    private static bool CameraInputEnabled()
    {
        var locks = UnityEngine.Object.FindObjectsByType<CameraCursorLock>(FindObjectsSortMode.None);
        foreach (var l in locks)
        {
            var axis = l.GetComponent<Unity.Cinemachine.CinemachineInputAxisController>();
            if (axis != null && axis.enabled) return true;
        }
        return false;
    }

    private static string Hp() => $"实际 {vHealth.CurrentHp}/{vHealth.MaxHp}，isDead={vHealth.isDead}";

    private static void Amount(int value) => Field("AmountInput").SetTextWithoutNotify(value.ToString());

    private static void Unsubscribe()
    {
        if (vHealth != null) vHealth.OnHealthChange -= OnHealthEvent;
        PlayerMgr.Instance.OnPlayerStateChange -= OnStateEvent;
        PlayerMgr.Instance.OnPlayerLevelChange -= OnLevelEvent;
        PlayerItemMgr.Instance.OnItemCountChanged -= OnItemEvent;
        UnsubscribeChoiceEvent();
    }

    private static void VerifyTick()
    {
        if (!Application.isPlaying || EditorApplication.isCompiling) return;
        string stage = SessionState.GetString(StageKey, "");
        try
        {
            if (stage == "WaitingForPlayer")
            {
                vPlayer = PlayerMgr.Instance.player;
                vConsole = UnityEngine.Object.FindFirstObjectByType<DevConsole>();
                vHealth = vPlayer != null ? vPlayer.GetComponent<CharacterHealth>() : null;
                if (vPlayer == null || vHealth == null || vHealth.MaxHp <= 0 || vConsole == null || vPlayer.Stats == null || Time.frameCount < 20)
                    return;
                checkLines.Clear(); passCount = failCount = 0; healthEventCount = stateEventCount = levelEventCount = itemEventCount = 0;
                buffProbe = null; captureBuffAdded = false; buffSavedTimeScale = -1;
                // 同一次 Play 里手动加过 Buff 或选择次数时先清空，否则后面的属性、等级和三选一检查会把它们算进去。
                if (MissingBuffFields().Length == 0 && ConfigMgr.Instance.buffDatabase != null &&
                    (ActiveBuffs().Count > 0 || BuffMgr.Instance.ChoiceCount != 0 || BuffMgr.Instance.RandomBuffs.Count > 0))
                {
                    Note($"开始时有 {ActiveBuffs().Count} 个生效中的 Buff、剩余选择次数 {BuffMgr.Instance.ChoiceCount}、候选 {BuffMgr.Instance.RandomBuffs.Count} 个，已调用 ClearAll 清空。");
                    BuffMgr.Instance.ClearAll();
                }
                Note($"开始：Lv.{PlayerMgr.Instance.saveData.level}，攻击 {vPlayer.Stats.Atk}，防御 {vPlayer.Stats.Def}，{Hp()}");
                if (vHealth.isDead || vHealth.CurrentHp != vHealth.MaxHp)
                    Note("开始时玩家不是满血存活状态，后续数值以控制台回满后的状态为准。");
                startMax = vHealth.MaxHp; startAttack = vPlayer.Stats.Atk; startDefense = vPlayer.Stats.Def;
                startCursorMode = PlayerMgr.Instance.CurrentIsCursor;
                vHealth.OnHealthChange += OnHealthEvent;
                PlayerMgr.Instance.OnPlayerStateChange += OnStateEvent;
                PlayerMgr.Instance.OnPlayerLevelChange += OnLevelEvent;

                // 0. 运行时生成的界面
                Check(vConsole.transform.Find("Window") == null && vConsole.transform.Find("ToggleButton") == null, "界面：旧版预制件控件已移除");
                Check(vConsole.transform.Find("ConsoleWindow") != null && vConsole.transform.Find("ConsoleLauncher") != null, "界面：运行时生成窗口与开关按钮");
                CheckHudOverlap("ConsoleLauncher", true);
                CheckHudOverlap("ConsoleWindow", false);

                // 1. 打开控制台
                vConsole.SetOpen(true);
                Check(vConsole.IsOpen, "打开：控制台窗口打开");
                Check(PlayerMgr.Instance.CurrentIsCursor, "打开：进入光标模式");
                Check(Cursor.lockState == CursorLockMode.None && Cursor.visible, "打开：光标解锁且可见", $"lockState={Cursor.lockState}，visible={Cursor.visible}");
                Check(!CameraInputEnabled(), "打开：镜头输入关闭（CameraCursorLock）");
                Check(!vPlayer.enabled, "打开：玩家移动组件已暂停");
                Check(!vPlayer.GetComponent<PlayerCombat>().enabled, "打开：玩家攻击组件已暂停");
                if (vHealth.isDead) Click("RevivePlayer");
                Click("FullHealth");

                // 2. 模拟松开 Alt：退出光标模式，等控制台下一帧重新进入
                PlayerMgr.Instance.SetCursorState(false);
                stageFrame = Time.frameCount;
                SessionState.SetString(StageKey, "AltReentry");
            }
            else if (stage == "AltReentry")
            {
                if (Time.frameCount < stageFrame + 3) return;
                Check(PlayerMgr.Instance.CurrentIsCursor, "打开期间光标模式被退出后，控制台自动重新进入");
                Check(!CameraInputEnabled(), "重新进入后镜头输入仍关闭");
                RunValueChecks();
                if (buffProbe != null) BeginBuffPause();
                else BeginCapture();
            }
            else if (stage == "BuffPause")
            {
                // timeScale 为 0 时 Time.deltaTime 为 0，TimerMgr 每帧减 0，剩余时间应保持不变。
                if (Time.frameCount < stageFrame + 10) return;
                float now = BuffMgr.Instance.GetBuffRemaining(buffProbe.id);
                Check(Same(now, buffRemainingMark), $"timeScale = 0 时 {buffProbe.buffName} 的剩余时间不变（10 帧）", $"{buffRemainingMark} → {now}");
                RestoreTimeScale();
                buffRemainingMark = now;
                buffStageTime = Time.realtimeSinceStartup;
                stageFrame = Time.frameCount;
                SessionState.SetString(StageKey, "BuffResume");
            }
            else if (stage == "BuffResume")
            {
                if (Time.frameCount < stageFrame + 5 || Time.realtimeSinceStartup < buffStageTime + 0.3f) return;
                float now = BuffMgr.Instance.GetBuffRemaining(buffProbe.id);
                Check(now < buffRemainingMark - 0.05f, "恢复 timeScale 后剩余时间继续减少", $"{buffRemainingMark} → {now}");
                // 不等满 20 秒：把这个 Buff 的计时器改成 0.05 秒，之后由 TimerMgr 正常到期、调用回调。
                Dictionary<int, int> ids = BuffTimerIds();
                Dictionary<int, TimeData> timers = TimerMgrTimersField.GetValue(TimerMgr.Instance) as Dictionary<int, TimeData>;
                if (ids == null || timers == null || !ids.TryGetValue(buffProbe.id, out buffProbeTimerId) || !timers.ContainsKey(buffProbeTimerId))
                {
                    Check(false, "到期检查：找到该 Buff 对应的计时器", "BuffMgr.timers 或 TimerMgr.timers 的类型变了，或者里面没有这个 Buff");
                    FinishBuffStages();
                    return;
                }
                timers[buffProbeTimerId].remaining = 0.05f;
                buffPoolMark = BuffMgr.Instance.poolCount;
                buffStageTime = Time.realtimeSinceStartup;
                stageFrame = Time.frameCount;
                SessionState.SetString(StageKey, "BuffExpire");
            }
            else if (stage == "BuffExpire")
            {
                if (Time.frameCount < stageFrame + 5 || Time.realtimeSinceStartup < buffStageTime + 0.3f) return;
                RunBuffExpireChecks();
                FinishBuffStages();
            }
            else if (stage == "Capture")
            {
                if (Time.frameCount < stageFrame + 8) return;
                if (captureIndex < CaptureNames.Length)
                {
                    // Buff 页截图时加上一个 Buff，画面里能看到生效中的状态；截完统一清空。
                    if (captureIndex == DevConsole.BuffTab) ShowBuffForCapture();
                    vConsole.SelectTab(captureIndex);
                    // ScreenCapture 不会自动创建目录。
                    ScreenCapture.CaptureScreenshot($"{ReportDir}/DevConsole-{captureIndex}-{CaptureNames[captureIndex]}.png");
                    captureIndex++;
                    stageFrame = Time.frameCount;
                    return;
                }
                if (captureBuffAdded)
                {
                    Click("ClearBuffs");
                    Check(ActiveBuffs().Count == 0 && BuffMgr.Instance.ChoiceCount == 0 && BuffMgr.Instance.RandomBuffs.Count == 0,
                        "截图后已清空截图用的 Buff、选择次数和候选",
                        $"生效 {ActiveBuffs().Count} 个，次数 {BuffMgr.Instance.ChoiceCount}，候选 {BuffMgr.Instance.RandomBuffs.Count} 个");
                    captureBuffAdded = false;
                }
                vConsole.SelectTab(DevConsole.HealthTab);
                vConsole.SetOpen(false);
                Unsubscribe();
                string summary = $"DevConsole Play Mode 检查：通过 {passCount}，失败 {failCount}（{DateTime.Now:yyyy-MM-dd HH:mm:ss}）";
                WriteReport("playmode-verification.txt", summary + "\n\n" + string.Join("\n", checkLines) + "\n");
                if (failCount == 0) Debug.Log(summary); else Debug.LogWarning(summary + "\n" + string.Join("\n", checkLines.Where(l => l.StartsWith("FAIL"))));
                SessionState.SetString(StageKey, failCount == 0 ? "Done" : "Failed");
                EditorApplication.update -= VerifyTick;
                EditorApplication.ExitPlaymode();
            }
        }
        catch (Exception error)
        {
            RestoreTimeScale();
            Unsubscribe();
            checkLines.Add("ERROR " + error);
            WriteReport("playmode-verification.txt", $"DevConsole Play Mode 检查中断：通过 {passCount}，失败 {failCount}\n\n" + string.Join("\n", checkLines) + "\n");
            WriteReport("playmode-error.txt", error.ToString());
            SessionState.SetString(StageKey, "Failed");
            EditorApplication.update -= VerifyTick;
            Debug.LogException(error);
            EditorApplication.ExitPlaymode();
        }
    }

    private static void RunValueChecks()
    {
        CheckLayout();

        float max = vHealth.MaxHp;
        float def = HealthDef();

        // 3. 生命按钮
        int events = healthEventCount;
        Click("Damage10"); Check(vHealth.CurrentHp == max - 10, "扣血 10", Hp());
        Check(healthEventCount > events, "扣血后触发 OnHealthChange（血条刷新）");
        Click("Heal10"); Check(vHealth.CurrentHp == max, "回血 10", Hp());
        Click("Damage25"); Check(vHealth.CurrentHp == max - 25, "扣血 25", Hp());
        Click("Heal25"); Check(vHealth.CurrentHp == max, "回血 25（不超过上限）", Hp());
        Amount(7); Click("DamageCustom"); Check(vHealth.CurrentHp == max - 7, "按数值扣血 7", Hp());
        Amount(3); Click("HealCustom"); Check(vHealth.CurrentHp == max - 4, "按数值回血 3", Hp());
        events = healthEventCount;
        Click("FullHealth"); Check(vHealth.CurrentHp == max, "恢复满血", Hp());
        Check(healthEventCount > events, "回血后触发 OnHealthChange");
        Amount(14); Click("TestDamage");
        float expected = max - Mathf.Max(14 - def, 0);
        Check(Same(vHealth.CurrentHp, expected), $"经过防御的伤害 14（防御 {def}）", Hp() + $"，期望 {expected}");
        Click("FullHealth");
        // 小数：生命改为 float 后，控制台可以输入小数
        Field("AmountInput").SetTextWithoutNotify("2.5"); Click("DamageCustom");
        Check(Same(vHealth.CurrentHp, max - 2.5f), "按数值扣血 2.5（小数）", Hp());
        Check(Text("Status").text.Contains("生命 " + (max - 2.5f).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + " /"),
            "状态栏按小数显示生命", Text("Status").text);
        Click("FullHealth");

        // 4. 死亡
        // 只检查启用中的 EnemyAI：被禁用的组件（例如场景里的静止木桩）不会在 OnEnable 订阅，本来就不参与脱战。
        EnemyAI[] allEnemies = UnityEngine.Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        EnemyAI[] enemies = allEnemies.Where(e => e.isActiveAndEnabled).ToArray();
        // 玩家死亡时，正在追击或出招的敌人切到返回状态，巡逻、等待、返回中的敌人保持原状态（EnemyAI.OnPlayerStateChange）。
        Dictionary<EnemyAI, IState> enemyStatesBefore = enemies.ToDictionary(e => e, e => CurrentStateOf(EnemyStateMachineField, e));
        PlayerController playerController = vPlayer.GetComponent<PlayerController>();
        int states = stateEventCount;
        Click("KillPlayer");
        Check(vHealth.CurrentHp == 0 && vHealth.isDead, "杀死玩家：生命归零且 isDead", Hp());
        Check(stateEventCount > states && lastStateEvent, "杀死玩家：发出死亡广播 OnPlayerStateChange(true)");
        // 直接取玩家物体上的 Animator，不依赖 PlayerObject 上的旧 animator 字段（#45 重构后可能删除）。
        Animator playerAnimator = vPlayer.GetComponent<Animator>();
        Check(playerAnimator != null && playerAnimator.GetBool("IsDead"), "杀死玩家：Animator 的 IsDead 为 true");
        if (playerController == null || PlayerStateMachineField == null) Note("玩家物体上没有 PlayerController，或找不到它的状态机字段，跳过玩家状态机检查。");
        else
        {
            IState playerState = CurrentStateOf(PlayerStateMachineField, playerController);
            Check(playerState == playerController.PlayerDeadState, "杀死玩家：玩家状态机切到死亡状态", $"实际 {StateName(playerState)}");
        }
        if (allEnemies.Length > enemies.Length) Note($"场景中有 {allEnemies.Length - enemies.Length} 个 EnemyAI 组件处于禁用状态，不参与脱战检查。");
        if (enemies.Length == 0) Note("场景中没有启用的 EnemyAI，跳过怪物脱战检查。");
        else if (EnemyStateMachineField == null) Note("找不到 EnemyAI 的状态机字段，跳过怪物脱战检查。");
        else
        {
            int engaged = 0;
            List<string> wrong = new List<string>();
            foreach (EnemyAI enemy in enemies)
            {
                IState before = enemyStatesBefore[enemy];
                bool wasEngaged = before == enemy.ChaseState || before == enemy.AttackState;
                if (wasEngaged) engaged++;
                IState expectedState = wasEngaged ? enemy.ReturnState : before;
                IState now = CurrentStateOf(EnemyStateMachineField, enemy);
                if (now != expectedState) wrong.Add($"{enemy.name}：{StateName(before)} → {StateName(now)}，期望 {StateName(expectedState)}");
            }
            if (engaged == 0) Note($"杀死玩家时没有正在追击或出招的敌人，只检查 {enemies.Length} 个敌人保持原状态。");
            Check(wrong.Count == 0, $"杀死玩家：{engaged} 个追击或出招中的敌人进入返回状态，其余 {enemies.Length - engaged} 个保持原状态", string.Join("；", wrong));
        }
        Click("Damage10"); Check(vHealth.CurrentHp == 0, "死亡时扣血被拒绝", Hp());
        Click("Heal10"); Check(vHealth.CurrentHp == 0 && vHealth.isDead, "死亡时回血被拒绝", Hp());
        Click("ResetAttributes"); Check(vHealth.isDead && vHealth.CurrentHp == 0, "死亡时恢复初始值被拒绝", Hp());

        // 5. 复活
        states = stateEventCount;
        Click("RevivePlayer");
        Check(vHealth.CurrentHp == max && !vHealth.isDead, "复活：回满且存活", Hp());
        Check(stateEventCount > states && !lastStateEvent, "复活：发出复活广播 OnPlayerStateChange(false)");
        Check(playerAnimator != null && !playerAnimator.GetBool("IsDead"), "复活：Animator 的 IsDead 为 false");
        if (playerController != null && PlayerStateMachineField != null)
        {
            IState playerState = CurrentStateOf(PlayerStateMachineField, playerController);
            Check(playerState == playerController.PlayerMovementState, "复活：玩家状态机回到移动状态", $"实际 {StateName(playerState)}");
        }
        Click("Damage10");
        states = stateEventCount;
        Click("RevivePlayer");
        Check(vHealth.CurrentHp == max && stateEventCount == states, "存活时点复活：只回满，不发复活广播", Hp());

        // 6. 属性
        events = healthEventCount;
        Field("AttackInput").SetTextWithoutNotify("7");
        Field("DefenseInput").SetTextWithoutNotify("4");
        Field("MaxHealthInput").SetTextWithoutNotify("80");
        Click("ApplyAttributes");
        float healthDef = HealthDef();
        Check(vPlayer.Stats.Atk == 7 && vPlayer.Stats.Def == 4, "应用属性：CharacterStats 为 7/4", $"实际 {vPlayer.Stats.Atk}/{vPlayer.Stats.Def}");
        Check(healthDef == 4 && vHealth.MaxHp == 80 && vHealth.CurrentHp == 80, "应用属性：生命组件防御 4、上限 80，满血按比例仍为 80", $"防御 {healthDef}，{Hp()}");
        Check(PlayerMgr.Instance.GetAtkValue() == 7, "应用属性：玩家攻击力（GetAtkValue）为 7", $"实际 {PlayerMgr.Instance.GetAtkValue()}");
        Check(healthEventCount > events, "应用属性后触发 OnHealthChange");
        Amount(14); Click("TestDamage"); Check(vHealth.CurrentHp == 70, "新防御下伤害 14 → 生命 70", Hp());
        Amount(5); Click("HealCustom"); Check(vHealth.CurrentHp == 75, "回血 5 → 生命 75", Hp());
        Click("AttackUp"); Check(vPlayer.Stats.Atk == 8, "攻击 +1", $"实际 {vPlayer.Stats.Atk}");
        Click("AttackDown"); Check(vPlayer.Stats.Atk == 7, "攻击 −1", $"实际 {vPlayer.Stats.Atk}");
        Click("DefenseUp"); Check(vPlayer.Stats.Def == 5 && HealthDef() == 5, "防御 +1（属性与生命组件同步）");
        Click("DefenseDown"); Check(vPlayer.Stats.Def == 4, "防御 −1", $"实际 {vPlayer.Stats.Def}");
        // 上限变化时当前生命按比例保持（#31）：75/80 → 上限 90 时为 84.375，再回到 80 时为 75。
        Click("MaxHealthUp"); Check(vHealth.MaxHp == 90 && Same(vHealth.CurrentHp, 84.375f), "生命上限 +10（75/80 按比例 → 84.375/90）", Hp());
        Click("MaxHealthDown"); Check(vHealth.MaxHp == 80 && Same(vHealth.CurrentHp, 75), "生命上限 −10（按比例回到 75/80）", Hp());
        Field("AttackInput").SetTextWithoutNotify("7.5");
        Field("DefenseInput").SetTextWithoutNotify("4.25");
        Click("ApplyAttributes");
        Check(Same(vPlayer.Stats.Atk, 7.5f) && Same(vPlayer.Stats.Def, 4.25f) && Same(HealthDef(), 4.25f),
            "应用小数属性：攻击 7.5、防御 4.25（属性与生命组件同步）", $"实际 {vPlayer.Stats.Atk}/{vPlayer.Stats.Def}，生命组件防御 {HealthDef()}");
        Check(Field("AttackInput").text == "7.5" && Text("Status").text.Contains("攻击 7.5"), "小数属性在输入框和状态栏正确显示",
            $"输入框「{Field("AttackInput").text}」，状态栏「{Text("Status").text}」");

        // 7. 恢复初始值
        Click("ResetAttributes");
        Check(vPlayer.Stats.Atk == startAttack && vPlayer.Stats.Def == startDefense, "恢复初始值：属性回到当前等级的数值", $"实际 {vPlayer.Stats.Atk}/{vPlayer.Stats.Def}，期望 {startAttack}/{startDefense}");
        Check(vHealth.MaxHp == startMax && vHealth.CurrentHp == startMax, "恢复初始值：生命上限回到当前等级并回满", Hp());
        Check(PlayerMgr.Instance.GetAtkValue() == startAttack, "恢复初始值：GetAtkValue 同步");
        var template = ConfigMgr.Instance.GetCharacterData(vPlayer.Config.type, vPlayer.Config.id);
        Check(ReferenceEquals(template, vPlayer.Config), "共享 SO：玩家仍引用配置表中的同一份数据（未被控制台替换）");

        // 8. 等级（会写入玩家存档，检查结束时恢复原等级和经验并再次保存）
        RunLevelChecks();

        // 9. 道具（会写入道具存档，检查结束时恢复原数量）
        RunItemChecks();

        // 10. Buff 三选一：选择次数、随机抽取、选中、ClearAll 清零（结束时清空，回到没有 Buff 的状态）
        RunBuffChoiceChecks();

        // 11. Buff 加成（同一帧内能检查的部分；暂停、恢复、到期和 ClearAll 在之后的几个阶段里跨帧检查）
        RunBuffChecks();

        // 12. 关闭
        vConsole.SetOpen(false);
        Check(!vConsole.IsOpen, "关闭：控制台窗口关闭");
        Check(vPlayer.enabled && vPlayer.GetComponent<PlayerCombat>().enabled, "关闭：玩家移动和攻击组件恢复");
        Check(PlayerMgr.Instance.CurrentIsCursor == startCursorMode, "关闭：光标模式恢复到打开前", $"打开前 {startCursorMode}，现在 {PlayerMgr.Instance.CurrentIsCursor}");
        Check(CameraInputEnabled() == !startCursorMode, "关闭：镜头输入随光标模式恢复");
        Note("未覆盖：实际按键（F1、Esc、Alt、WASD）和鼠标点击、打怪的实际伤害、清空任务存档、清空背包道具、与任务面板/背包面板/对话同时使用；" +
             "BuffPanel / BuffOption 与 GamePanel Buff 按钮的实际点击、打开背包时的真实暂停（这里直接把 timeScale 设为 0）。");
    }

    // 控制台的控件不应压住 HUD 上的按钮（例如右上角的背包按钮）。
    // 屏幕空间 Overlay 画布下，RectTransform 的世界坐标角点就是屏幕像素坐标。
    private static void CheckHudOverlap(string child, bool failOnOverlap)
    {
        var target = (RectTransform)vConsole.transform.Find(child);
        Rect area = ScreenRect(target);
        var hits = new List<string>();
        foreach (Button button in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            if (!button.isActiveAndEnabled || button.transform.IsChildOf(vConsole.transform)) continue;
            if (ScreenRect((RectTransform)button.transform).Overlaps(area)) hits.Add(button.name);
        }
        string name = child == "ConsoleLauncher" ? "开关按钮" : "展开后的窗口";
        if (failOnOverlap) Check(hits.Count == 0, $"布局：{name}不压住 HUD 按钮", "重叠：" + string.Join("，", hits));
        else Note(hits.Count == 0 ? $"布局：{name}不压住 HUD 按钮。" : $"布局：{name}压住了 HUD 按钮：{string.Join("，", hits)}。");
    }

    private static Rect ScreenRect(RectTransform rect)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
    }

    // 逐页检查固定文字是否被截断（省略号），用来发现布局放不下的问题。
    private static void CheckLayout()
    {
        var window = vConsole.transform.Find("ConsoleWindow");
        string[] pages = { "HealthPage", "StatsPage", "ItemPage", "SavePage", "BuffPage" };
        for (int i = 0; i < pages.Length; i++)
        {
            vConsole.SelectTab(i);
            Check(window.Find(pages[i]).gameObject.activeSelf && vConsole.CurrentTab == i, $"分页：切换到 {pages[i]}");
            Canvas.ForceUpdateCanvases();
            var truncated = new List<string>();
            foreach (TMP_Text text in window.GetComponentsInChildren<TMP_Text>(false))
            {
                // 状态栏与提示栏内容随操作变化，不参与检查。
                if (text.name == "Status" || text.name == "Message" || text.name == "Text" || text.name == "Placeholder") continue;
                text.ForceMeshUpdate(true);
                if (text.isTextTruncated) truncated.Add(text.transform.parent.name + "/" + text.name + "「" + text.text + "」");
            }
            Check(truncated.Count == 0, $"布局：{pages[i]} 的固定文字没有被截断", string.Join("，", truncated));
        }
        vConsole.SelectTab(DevConsole.HealthTab);
    }

    private static void RunLevelChecks()
    {
        var save = PlayerMgr.Instance.saveData;
        var cfg = vPlayer.Config;
        int origLevel = save.level, origExp = save.exp;
        Note($"等级检查前存档：Lv.{origLevel}，经验 {origExp}，检查结束后恢复。");
        try
        {
            int levelEvents = levelEventCount;
            Click("ResetLevel");
            Check(save.level == cfg.initLv && save.exp == 0, "重置为初始等级：等级回到 initLv 且经验清零", $"实际 Lv.{save.level}，经验 {save.exp}");
            Check(levelEventCount > levelEvents, "重置等级后广播 OnPlayerLevelChange");
            Check(Same(vPlayer.Stats.Atk, cfg.atk) && Same(vPlayer.Stats.Def, cfg.def) && Same(vHealth.MaxHp, cfg.maxHp),
                "重置等级后属性与生命上限回到初始等级", $"实际 攻击 {vPlayer.Stats.Atk}、防御 {vPlayer.Stats.Def}、{Hp()}");
            Check(vHealth.CurrentHp <= vHealth.MaxHp, "重置等级后当前生命不超过上限", Hp());

            int maxLevel = Mathf.Max(cfg.initLv, cfg.maxLv);
            int target = Mathf.Min(cfg.initLv + 2, maxLevel);
            TMP_InputField levelField = Field("LevelInput");
            // 等级变化时当前生命按比例保持（#31）：先扣 10 点得到非满血比例，设置等级后按新上限换算。
            Amount(10); Click("DamageCustom");
            float ratioBefore = vHealth.CurrentHp / vHealth.MaxHp;
            // 模拟手动操作：输入等级 → 输入框失去焦点 → 控制台定时刷新一次 → 再点按钮。
            levelField.text = target.ToString();
            RefreshConsole();
            Check(levelField.text == target.ToString(), "输入等级后控制台刷新不会把输入框改回旧等级", $"实际「{levelField.text}」，期望「{target}」");
            Click("ApplyLevel");
            float expectedAtk = cfg.atk + cfg.atkPerLv * (target - cfg.initLv);
            float expectedMax = cfg.maxHp + cfg.maxHpPerLv * (target - cfg.initLv);
            Check(save.level == target && Same(vPlayer.Stats.Atk, expectedAtk) && Same(vHealth.MaxHp, expectedMax),
                $"设置等级 {target}：属性按等级重新计算", $"实际 Lv.{save.level}、攻击 {vPlayer.Stats.Atk}、上限 {vHealth.MaxHp}，期望攻击 {expectedAtk}、上限 {expectedMax}");
            Check(Same(vHealth.CurrentHp, expectedMax * ratioBefore) && !vHealth.isDead,
                $"设置等级 {target}：当前生命按原比例换算到新上限", $"{Hp()}，期望当前生命 {expectedMax * ratioBefore}");
            Check(Same(lastEventHp, vHealth.CurrentHp) && Same(lastEventMax, vHealth.MaxHp),
                "设置等级后最后一次 OnHealthChange 广播的是最终生命（HUD 不会停在中间值）", $"广播 {lastEventHp}/{lastEventMax}，{Hp()}");

            int submitTarget = Mathf.Min(cfg.initLv + 1, maxLevel);
            levelField.text = submitTarget.ToString();
            RefreshConsole();
            levelField.onSubmit.Invoke(levelField.text);
            Check(save.level == submitTarget, $"等级输入框按回车即应用（Lv.{submitTarget}）", $"实际 Lv.{save.level}");

            levelField.text = (cfg.maxLv + 5).ToString();
            Click("ApplyLevel");
            Check(save.level == maxLevel, "超出最高等级的输入被限制到 maxLv", $"实际 Lv.{save.level}");
            Check(levelField.text == maxLevel.ToString(), "被限制后输入框显示实际生效的等级", $"实际「{levelField.text}」");
        }
        finally { RestoreLevel(origLevel, origExp); }
        Check(save.level == origLevel && vPlayer.Stats.Atk == startAttack && vHealth.MaxHp == startMax, "等级检查后已恢复原等级和属性", $"实际 Lv.{save.level}、攻击 {vPlayer.Stats.Atk}、{Hp()}");
    }

    private static void RestoreLevel(int level, int exp)
    {
        var save = PlayerMgr.Instance.saveData;
        save.level = level;
        save.exp = exp;
        PlayerMgr.Instance.PlayerLevelChanged();
        PlayerMgr.Instance.SavePlayerData();
        Click("FullHealth");
    }

    private static void RunItemChecks()
    {
        ItemDatabase db = ConfigMgr.Instance.itemDatabase;
        List<ItemConfig> configs = db != null && db.items != null ? db.items.Where(i => i != null).ToList() : new List<ItemConfig>();
        if (configs.Count == 0) { Note("ItemDatabase 为空，跳过道具检查。"); return; }

        vConsole.SelectTab(DevConsole.ItemTab);
        Transform content = vConsole.transform.Find("ConsoleWindow/ItemPage/ItemList/Viewport/Content");
        int rows = content.Cast<Transform>().Count(t => t.name.StartsWith("ItemRow_"));
        Check(rows == configs.Count, $"道具列表：显示配置表中的全部 {configs.Count} 个物品", $"实际 {rows} 行");

        PlayerItemMgr mgr = PlayerItemMgr.Instance;
        ItemConfig sample = configs.FirstOrDefault(i => i.itemType != ItemType.exp);
        ItemConfig expItem = configs.FirstOrDefault(i => i.itemType == ItemType.exp);
        if (sample == null) { Note("配置表里只有经验类物品，跳过加入背包检查。"); return; }

        int id = sample.itemID, orig = mgr.GetItemCount(id);
        Note($"道具检查前：{sample.itemName}（ID {id}）持有 {orig}，检查结束后恢复。");
        mgr.OnItemCountChanged += OnItemEvent;
        var save = PlayerMgr.Instance.saveData;
        int origLevel = save.level, origExp = save.exp;
        try
        {
            // 从列表选择
            Click("ItemRow_" + id);
            Check(Field("ItemIdInput").text == id.ToString(), "点击列表行后填入物品 ID", $"实际「{Field("ItemIdInput").text}」");
            Check(Text("DetailName").text == sample.itemName, "选择后详情显示物品名称", $"实际「{Text("DetailName").text}」");
            Field("ItemCountInput").SetTextWithoutNotify("3");
            int itemEvents = itemEventCount;
            Click("AddItem");
            Check(mgr.GetItemCount(id) == orig + 3, $"加入背包：{sample.itemName} +3（AddItem）", $"实际 {mgr.GetItemCount(id)}");
            Check(itemEventCount > itemEvents, "加入背包触发 OnItemCountChanged");
            Check(Text("Sub", content.Find("ItemRow_" + id)).text.Contains("持有 " + (orig + 3)), "列表中的持有数量随之刷新", Text("Sub", content.Find("ItemRow_" + id)).text);
            Click("RemoveItem");
            Check(mgr.GetItemCount(id) == orig, "扣除 3（RemoveItem）后回到原数量", $"实际 {mgr.GetItemCount(id)}");
            Field("ItemCountInput").SetTextWithoutNotify((orig + 5).ToString());
            Click("RemoveItem");
            Check(mgr.GetItemCount(id) == orig, "扣除数量超过持有时被拒绝，数量不变", $"实际 {mgr.GetItemCount(id)}");

            // 直接输入 ID（带通知，模拟键入）
            Field("ItemIdInput").text = "";
            Check(Text("DetailName").text == "未选择物品", "清空 ID 后详情回到未选择");
            Field("ItemIdInput").text = id.ToString();
            Check(Text("DetailName").text == sample.itemName, "直接输入 ID 后详情同步");
            Field("ItemCountInput").SetTextWithoutNotify("2");
            Click("AddItem");
            Check(mgr.GetItemCount(id) == orig + 2, "直接输入 ID 后加入背包 +2", $"实际 {mgr.GetItemCount(id)}");
            Click("RemoveItem");

            // 配置表外的 ID
            int missing = configs.Max(i => i.itemID) + 1000;
            Field("ItemIdInput").text = missing.ToString();
            Field("ItemCountInput").SetTextWithoutNotify("1");
            Click("AddItem");
            Check(mgr.GetItemCount(missing) == 0, $"配置表外的 ID {missing} 被拒绝，没有写入存档");
            Check(Text("DetailName").text == "未找到物品", "配置表外的 ID：详情提示未找到");

            // 按奖励结算（RewardMgr）：非经验物品进入背包，受 maxCount 限制。
            Field("ItemIdInput").text = id.ToString();
            Field("ItemCountInput").SetTextWithoutNotify("1");
            int beforeReward = mgr.GetItemCount(id);
            Click("RewardItem");
            int expectedReward = sample.maxCount > 0 ? Mathf.Min(beforeReward + 1, sample.maxCount) : beforeReward + 1;
            Check(mgr.GetItemCount(id) == expectedReward, $"按奖励结算 {sample.itemName} ×1：经 RewardMgr 进入背包",
                $"持有 {beforeReward} → {mgr.GetItemCount(id)}，期望 {expectedReward}");

            if (expItem == null) Note("配置表里没有经验类物品，跳过经验检查。");
            else
            {
                int expHeld = mgr.GetItemCount(expItem.itemID);
                Field("ItemIdInput").text = expItem.itemID.ToString();
                Click("AddItem");
                Check(mgr.GetItemCount(expItem.itemID) == expHeld, "经验类物品不能加入背包");
                int levelBefore = save.level, expBefore = save.exp;
                Click("RewardItem");
                Check(save.level != levelBefore || save.exp != expBefore, "按奖励结算经验 ×1：走 AddExp，经验或等级发生变化", $"Lv.{levelBefore} 经验 {expBefore} → Lv.{save.level} 经验 {save.exp}");
            }
        }
        finally
        {
            mgr.OnItemCountChanged -= OnItemEvent;
            int diff = mgr.GetItemCount(id) - orig;
            if (diff > 0) mgr.RemoveItem(id, diff);
            else if (diff < 0) mgr.AddItem(id, -diff);
            if (save.level != origLevel || save.exp != origExp) RestoreLevel(origLevel, origExp);
        }
        Check(mgr.GetItemCount(id) == orig && save.level == origLevel && save.exp == origExp, "道具检查后已恢复原持有数量与等级经验",
            $"持有 {mgr.GetItemCount(id)}（原 {orig}），Lv.{save.level} 经验 {save.exp}（原 Lv.{origLevel} 经验 {origExp}）");
        vConsole.SelectTab(DevConsole.HealthTab);
    }

    // ---- Buff 检查 ----

    private static string MissingBuffFields()
    {
        List<string> missing = new List<string>();
        if (BuffActiveField == null) missing.Add("BuffMgr.hasBuffs");
        if (BuffTimerIdsField == null) missing.Add("BuffMgr.timers");
        if (TimerMgrTimersField == null) missing.Add("TimerMgr.timers");
        if (StatsAtkPercentField == null) missing.Add("CharacterStats.atkBuffPercent");
        if (StatsDefPercentField == null) missing.Add("CharacterStats.defBuffPercent");
        if (StatsMaxHpPercentField == null) missing.Add("CharacterStats.maxHpBuffPercent");
        return string.Join("，", missing);
    }

    private static List<BuffConfig> BuffConfigs()
    {
        BuffDatabase db = ConfigMgr.Instance.buffDatabase;
        return db != null && db.buffs != null ? db.buffs.Where(b => b != null).ToList() : new List<BuffConfig>();
    }

    private static List<BuffConfig> ActiveBuffs() => BuffActiveField.GetValue(BuffMgr.Instance) as List<BuffConfig> ?? new List<BuffConfig>();
    private static bool BuffActive(int id) => ActiveBuffs().Any(b => b != null && b.id == id);
    private static Dictionary<int, int> BuffTimerIds() => BuffTimerIdsField.GetValue(BuffMgr.Instance) as Dictionary<int, int>;

    private static string BuffTypeName(BuffType type) =>
        type == BuffType.atk ? "攻击" : type == BuffType.def ? "防御" : type == BuffType.maxHp ? "生命上限" : type.ToString();

    private static float StatOf(BuffType type) =>
        type == BuffType.atk ? vPlayer.Stats.Atk : type == BuffType.def ? vPlayer.Stats.Def : vPlayer.Stats.MaxHp;

    private static float PercentOf(BuffType type)
    {
        FieldInfo field = type == BuffType.atk ? StatsAtkPercentField : type == BuffType.def ? StatsDefPercentField : StatsMaxHpPercentField;
        return (float)field.GetValue(vPlayer.Stats);
    }

    // 同一帧内的部分：添加、属性与生命组件同步、按比例保持、重复添加被拦截、Buff 生效时升级、按等级和 Buff 重算。
    // 结束时三个 Buff 仍在生效，留给之后的暂停 / 恢复 / 到期 / ClearAll 阶段。
    private static void RunBuffChecks()
    {
        buffProbe = null;
        List<BuffConfig> configs = BuffConfigs();
        if (configs.Count == 0) { Note("BuffDatabase 为空或未加载，跳过 Buff 检查。"); return; }
        string missing = MissingBuffFields();
        Check(missing.Length == 0, "Buff：控制台读取的内部字段都存在", "找不到 " + missing);
        if (missing.Length > 0) return;

        vConsole.SelectTab(DevConsole.BuffTab);
        Transform content = vConsole.transform.Find("ConsoleWindow/BuffPage/BuffList/Viewport/Content");
        int rows = content.Cast<Transform>().Count(t => t.name.StartsWith("BuffRow_"));
        Check(rows == configs.Count, $"Buff 列表：显示配置表中的全部 {configs.Count} 个 Buff", $"实际 {rows} 行");

        BuffMgr mgr = BuffMgr.Instance;
        Check(ActiveBuffs().Count == 0 && mgr.poolCount == configs.Count, "Buff：开始时没有生效中的 Buff，候选池为全部配置",
            $"生效 {ActiveBuffs().Count} 个，候选池 {mgr.poolCount} 个");
        int absentId = configs.Max(b => b.id) + 1000;
        Check(mgr.GetBuffRemaining(absentId) == 0, "未生效的 Buff：GetBuffRemaining 返回 0", $"实际 {mgr.GetBuffRemaining(absentId)}");

        BuffConfig atkBuff = configs.FirstOrDefault(b => b.buffType == BuffType.atk);
        BuffConfig defBuff = configs.FirstOrDefault(b => b.buffType == BuffType.def);
        BuffConfig hpBuff = configs.FirstOrDefault(b => b.buffType == BuffType.maxHp);

        // 先扣 10 点得到非满血比例，用来检查生命上限变化时当前生命按比例保持。
        Click("FullHealth");
        Amount(10); Click("DamageCustom");
        float baseAtk = vPlayer.Stats.Atk, baseDef = vPlayer.Stats.Def, baseMax = vHealth.MaxHp;
        int pool = mgr.poolCount;

        if (atkBuff == null) Note("配置里没有攻击类 Buff，跳过攻击加成检查。");
        else
        {
            float expected = baseAtk * (1 + atkBuff.percent);
            Click("BuffRow_" + atkBuff.id);
            Check(Same(vPlayer.Stats.Atk, expected) && Same(PlayerMgr.Instance.GetAtkValue(), expected),
                $"添加 {atkBuff.buffName}：攻击 {baseAtk} → {expected}（GetAtkValue 同步）", $"实际 {vPlayer.Stats.Atk}，GetAtkValue {PlayerMgr.Instance.GetAtkValue()}");
            Check(BuffActive(atkBuff.id) && mgr.poolCount == pool - 1, "添加后进入生效列表，并从候选池移除", $"候选池 {pool} → {mgr.poolCount}");
            pool = mgr.poolCount;
            float remaining = mgr.GetBuffRemaining(atkBuff.id);
            Check(remaining > 0 && remaining <= atkBuff.totalTime, $"添加后剩余时间在 0～{atkBuff.totalTime} 秒之间", $"实际 {remaining}");
            RefreshConsole();
            string sub = Text("Sub", content.Find("BuffRow_" + atkBuff.id)).text;
            Check(sub.Contains("生效中"), "Buff 列表：该行显示生效中和剩余时间", $"实际「{sub}」");
            int count = ActiveBuffs().Count;
            Click("BuffRow_" + atkBuff.id);
            Check(ActiveBuffs().Count == count && Same(vPlayer.Stats.Atk, expected), "重复点击生效中的 Buff：控制台拦截，状态不变",
                $"生效 {ActiveBuffs().Count} 个，攻击 {vPlayer.Stats.Atk}");
        }

        if (defBuff == null) Note("配置里没有防御类 Buff，跳过防御加成检查。");
        else
        {
            float expected = baseDef * (1 + defBuff.percent);
            Click("BuffRow_" + defBuff.id);
            Check(Same(vPlayer.Stats.Def, expected) && Same(HealthDef(), expected), $"添加 {defBuff.buffName}：防御 {baseDef} → {expected}（生命组件同步）",
                $"实际 {vPlayer.Stats.Def}，生命组件 {HealthDef()}");
        }

        if (hpBuff == null) Note("配置里没有生命上限类 Buff，跳过生命上限加成检查。");
        else
        {
            float expectedMax = baseMax * (1 + hpBuff.percent);
            float ratio = vHealth.CurrentHp / vHealth.MaxHp;
            Click("BuffRow_" + hpBuff.id);
            Check(Same(vHealth.MaxHp, expectedMax) && Same(vHealth.CurrentHp, expectedMax * ratio),
                $"添加 {hpBuff.buffName}：生命上限 {baseMax} → {expectedMax}，当前生命按比例保持", Hp() + $"，期望当前 {expectedMax * ratio}");
            Check(Same(lastEventHp, vHealth.CurrentHp) && Same(lastEventMax, vHealth.MaxHp),
                "加生命上限后最后一次 OnHealthChange 广播的是最终生命（HUD 同步）", $"广播 {lastEventHp}/{lastEventMax}，{Hp()}");
        }

        RefreshConsole();
        string summary = Text("BuffCheck").text;
        Check(summary.StartsWith("加成与"), "Buff 页：CharacterStats 的加成与 BuffMgr 生效列表一致", $"实际「{summary}」");

        // 升级时 Recalculate 会从配置重新算白值，加成必须每次都乘进去，否则升级会用白值覆盖 Buff（#16）。
        PlayerSaveData save = PlayerMgr.Instance.saveData;
        CharacterConfig.CharacterData cfg = vPlayer.Config;
        if (atkBuff != null && save.level < cfg.maxLv)
        {
            int origLevel = save.level, origExp = save.exp;
            try
            {
                Field("LevelInput").text = (origLevel + 1).ToString();
                Click("ApplyLevel");
                float newBase = cfg.atk + cfg.atkPerLv * (origLevel + 1 - cfg.initLv);
                Check(Same(vPlayer.Stats.Atk, newBase * (1 + atkBuff.percent)),
                    $"Buff 生效时升到 Lv.{origLevel + 1}：攻击 = 新白值 {newBase} × (1 + {atkBuff.percent})", $"实际 {vPlayer.Stats.Atk}");
            }
            finally { RestoreLevel(origLevel, origExp); }
            Check(Same(vPlayer.Stats.Atk, baseAtk * (1 + atkBuff.percent)), "恢复原等级后攻击回到原白值 × (1 + 加成)", $"实际 {vPlayer.Stats.Atk}");
        }
        else if (atkBuff != null) Note("当前已是最高等级，跳过“Buff 生效时升级”检查。");

        Click("ResetAttributes");
        bool kept = (atkBuff == null || Same(vPlayer.Stats.Atk, baseAtk * (1 + atkBuff.percent)))
                    && (defBuff == null || Same(vPlayer.Stats.Def, baseDef * (1 + defBuff.percent)))
                    && (hpBuff == null || Same(vHealth.MaxHp, baseMax * (1 + hpBuff.percent)));
        Check(kept && Same(vHealth.CurrentHp, vHealth.MaxHp), "按等级和 Buff 重算：保留生效中的 Buff 加成，生命回满",
            $"攻击 {vPlayer.Stats.Atk}，防御 {vPlayer.Stats.Def}，{Hp()}");

        buffProbe = atkBuff ?? defBuff ?? hpBuff;
        buffProbeBase = buffProbe == atkBuff ? baseAtk : buffProbe == defBuff ? baseDef : baseMax;
        vConsole.SelectTab(DevConsole.HealthTab);
    }

    private static void BeginBuffPause()
    {
        buffRemainingMark = BuffMgr.Instance.GetBuffRemaining(buffProbe.id);
        buffSavedTimeScale = Time.timeScale;
        Time.timeScale = 0;
        stageFrame = Time.frameCount;
        SessionState.SetString(StageKey, "BuffPause");
    }

    private static void RestoreTimeScale()
    {
        if (buffSavedTimeScale < 0) return;
        Time.timeScale = buffSavedTimeScale > 0 ? buffSavedTimeScale : 1;
        buffSavedTimeScale = -1;
    }

    private static void RunBuffExpireChecks()
    {
        BuffMgr mgr = BuffMgr.Instance;
        BuffType type = buffProbe.buffType;
        Check(!BuffActive(buffProbe.id) && mgr.poolCount == buffPoolMark + 1, $"{buffProbe.buffName} 到期：移出生效列表并回到候选池",
            $"仍在生效 {BuffActive(buffProbe.id)}，候选池 {buffPoolMark} → {mgr.poolCount}");
        float remaining = mgr.GetBuffRemaining(buffProbe.id);
        float timerRemaining = TimerMgr.Instance.GetRemainingByIndex(buffProbeTimerId);
        Check(remaining == 0 && timerRemaining == -1, "到期后 GetBuffRemaining 为 0，计时器已从 TimerMgr 删除",
            $"GetBuffRemaining {remaining}，GetRemainingByIndex {timerRemaining}");
        float stillListed = ActiveBuffs().Where(b => b != null && b.buffType == type).Sum(b => b.percent);
        float expected = buffProbeBase * (1 + stillListed);
        Check(Same(StatOf(type), expected) && Same(PercentOf(type), stillListed), $"到期后{BuffTypeName(type)}减回加成：应为 {expected}",
            $"实际 {StatOf(type)}，CharacterStats 加成 {PercentOf(type)}");
        if (type == BuffType.atk)
            Check(Same(PlayerMgr.Instance.GetAtkValue(), expected), "到期后 GetAtkValue 同步", $"实际 {PlayerMgr.Instance.GetAtkValue()}");

        // ClearAll：取消剩下的计时、清零加成、恢复候选池；生命上限变回时当前生命按比例保持。
        Dictionary<int, int> ids = BuffTimerIds();
        List<int> cleared = ids != null ? ids.Values.ToList() : new List<int>();
        float ratio = vHealth.CurrentHp / vHealth.MaxHp;
        int total = BuffConfigs().Count;
        Click("ClearBuffs");
        Dictionary<int, int> idsAfter = BuffTimerIds();
        Check(ActiveBuffs().Count == 0 && (idsAfter == null || idsAfter.Count == 0) && mgr.poolCount == total,
            "ClearAll：生效列表和计时记录清空，候选池恢复为全部", $"生效 {ActiveBuffs().Count} 个，候选池 {mgr.poolCount}/{total}");
        Check(cleared.All(t => TimerMgr.Instance.GetRemainingByIndex(t) == -1), $"ClearAll：{cleared.Count} 个计时器已从 TimerMgr 取消");
        Check(Same(PercentOf(BuffType.atk), 0) && Same(PercentOf(BuffType.def), 0) && Same(PercentOf(BuffType.maxHp), 0),
            "ClearAll：CharacterStats 的三项加成清零", $"{PercentOf(BuffType.atk)}/{PercentOf(BuffType.def)}/{PercentOf(BuffType.maxHp)}");
        Check(Same(vPlayer.Stats.Atk, startAttack) && Same(vPlayer.Stats.Def, startDefense) && Same(HealthDef(), startDefense) && Same(vHealth.MaxHp, startMax),
            "ClearAll：攻击、防御、生命上限回到白值（生命组件同步）",
            $"攻击 {vPlayer.Stats.Atk}/{startAttack}，防御 {vPlayer.Stats.Def}/{startDefense}，生命组件防御 {HealthDef()}，上限 {vHealth.MaxHp}/{startMax}");
        Check(Same(vHealth.CurrentHp, vHealth.MaxHp * ratio), "ClearAll：生命上限变回时当前生命按比例保持", Hp() + $"，期望当前 {vHealth.MaxHp * ratio}");
    }

    private static void FinishBuffStages()
    {
        RestoreTimeScale();
        if (MissingBuffFields().Length == 0 && ActiveBuffs().Count > 0) Click("ClearBuffs");
        if (!vHealth.isDead) Click("FullHealth");
        buffProbe = null;
        BeginCapture();
    }

    private static void BeginCapture()
    {
        // 依次截取各个分页，截图时保持控制台打开
        vConsole.SetOpen(true);
        Directory.CreateDirectory(ReportDir);
        captureIndex = 0;
        stageFrame = Time.frameCount;
        SessionState.SetString(StageKey, "Capture");
    }

    private static void ShowBuffForCapture()
    {
        if (MissingBuffFields().Length > 0) return;
        List<BuffConfig> configs = BuffConfigs();
        if (configs.Count == 0 || BuffActive(configs[0].id)) return;
        Click("BuffRow_" + configs[0].id);
        // 再加两次选择次数并抽一次，截图里能看到三选一区域的候选。
        Click("BuffChoiceAdd");
        Click("BuffChoiceAdd");
        Click("BuffRoll");
        captureBuffAdded = true;
    }

    // ---- 三选一检查 ----

    private static void OnChoiceEventNoArg() { choiceEventCount++; lastChoiceEventValue = int.MinValue; }
    private static void OnChoiceEventWithCount(int count) { choiceEventCount++; lastChoiceEventValue = count; }

    // 按事件的实际类型订阅，BuffMgr 改用无参或带次数参数的写法时这里都能用。
    private static bool SubscribeChoiceEvent()
    {
        UnsubscribeChoiceEvent();
        EventInfo info = typeof(BuffMgr).GetEvent("onChoiceCountChange", BindingFlags.Instance | BindingFlags.Public);
        if (info == null) return false;
        Type type = info.EventHandlerType;
        if (type == typeof(UnityEngine.Events.UnityAction)) choiceHandler = new UnityEngine.Events.UnityAction(OnChoiceEventNoArg);
        else if (type == typeof(UnityEngine.Events.UnityAction<int>)) choiceHandler = new UnityEngine.Events.UnityAction<int>(OnChoiceEventWithCount);
        else if (type == typeof(Action)) choiceHandler = new Action(OnChoiceEventNoArg);
        else if (type == typeof(Action<int>)) choiceHandler = new Action<int>(OnChoiceEventWithCount);
        else return false;
        info.AddEventHandler(BuffMgr.Instance, choiceHandler);
        return true;
    }

    private static void UnsubscribeChoiceEvent()
    {
        if (choiceHandler == null) return;
        EventInfo info = typeof(BuffMgr).GetEvent("onChoiceCountChange", BindingFlags.Instance | BindingFlags.Public);
        if (info != null && ConfigMgr.Instance.buffDatabase != null) info.RemoveEventHandler(BuffMgr.Instance, choiceHandler);
        choiceHandler = null;
    }

    // 通过控制台按钮调用后检查事件。先订阅的对象（例如 GamePanel）在 Invoke 时抛异常，后面的订阅者收不到，
    // 控制台会把异常写进提示栏，这里把它作为失败原因报告出来。
    private static void CheckChoiceEvent(int before, int expected, string call)
    {
        if (choiceHandler == null) return;
        string message = Text("Message").text;
        if (message.Contains("抛出异常"))
        {
            Check(false, $"{call} 后触发 onChoiceCountChange", "调用时抛出异常：" + message);
            return;
        }
        bool withCount = choiceHandler is UnityEngine.Events.UnityAction<int> || choiceHandler is Action<int>;
        bool valueOk = !withCount || lastChoiceEventValue == expected;
        Check(choiceEventCount == before + 1 && valueOk, $"{call} 后触发一次 onChoiceCountChange" + (withCount ? $"，参数为 {expected}" : ""),
            $"触发 {choiceEventCount - before} 次，参数 {(withCount ? lastChoiceEventValue.ToString() : "无")}");
    }

    private static string Ids(List<BuffConfig> list) => "[" + string.Join(",", list.Select(b => b == null ? "null" : b.id.ToString())) + "]";

    // 连续抽 RollSamples 次：每次数量为 min(3, 候选池)、互不重复、都不是生效中的 Buff；候选池里的每个 Buff 都至少抽到一次。
    private static void RunRollStatistics(BuffMgr mgr)
    {
        int pool = mgr.poolCount, expected = Mathf.Min(3, pool), bad = 0;
        string firstBad = "";
        HashSet<int> seen = new HashSet<int>();
        for (int i = 0; i < RollSamples; i++)
        {
            mgr.StartRandomBuff();
            List<BuffConfig> picked = mgr.RandomBuffs;
            bool ok = picked.Count == expected && picked.All(b => b != null && !BuffActive(b.id)) && picked.Select(b => b.id).Distinct().Count() == picked.Count;
            if (!ok) { bad++; if (firstBad.Length == 0) firstBad = Ids(picked); }
            foreach (BuffConfig b in picked)
                if (b != null) seen.Add(b.id);
        }
        Check(bad == 0, $"抽取 {RollSamples} 次：每次 {expected} 个、互不重复、都来自候选池", $"{bad} 次不符合，第一次为 {firstBad}");
        Check(seen.Count == pool, $"抽取 {RollSamples} 次：候选池里的 {pool} 个 Buff 都被抽到过", $"只抽到 {seen.Count} 个");
    }

    // 从没有 Buff、次数为 0 的状态开始，最后用 ClearAll 回到这个状态，留给后面的加成检查。
    private static void RunBuffChoiceChecks()
    {
        List<BuffConfig> configs = BuffConfigs();
        // 配置为空或字段缺失由 RunBuffChecks 报告。
        if (configs.Count == 0 || MissingBuffFields().Length > 0) return;
        vConsole.SelectTab(DevConsole.BuffTab);
        BuffMgr mgr = BuffMgr.Instance;
        choiceEventCount = 0; lastChoiceEventValue = int.MinValue;
        Check(SubscribeChoiceEvent(), "三选一：订阅 BuffMgr.onChoiceCountChange", "找不到这个公开事件，或类型不是 UnityAction / UnityAction<int>");
        try
        {
            Check(mgr.ChoiceCount == 0 && mgr.RandomBuffs.Count == 0 && ActiveBuffs().Count == 0, "三选一：开始时剩余选择次数为 0，没有候选和生效中的 Buff",
                $"次数 {mgr.ChoiceCount}，候选 {Ids(mgr.RandomBuffs)}，生效 {ActiveBuffs().Count} 个");

            // 次数为 0：可以抽，但选中被控制台拦截（游戏里此时 Buff 按钮隐藏，面板打不开）
            Click("BuffRoll");
            int expected = Mathf.Min(3, mgr.poolCount);
            Check(mgr.RandomBuffs.Count == expected, $"抽一次候选：抽出 {expected} 个（候选池 {mgr.poolCount} 个）", $"实际 {Ids(mgr.RandomBuffs)}");
            Click("BuffPick0");
            Check(mgr.ChoiceCount == 0 && ActiveBuffs().Count == 0, "次数为 0 时点候选：控制台不调用 SelectBuff，状态不变",
                $"次数 {mgr.ChoiceCount}，生效 {ActiveBuffs().Count} 个");

            // 加次数
            int events = choiceEventCount;
            Click("BuffChoiceAdd");
            Check(mgr.ChoiceCount == 1, "次数 +1（AddChoiceCount）：0 → 1", $"实际 {mgr.ChoiceCount}");
            CheckChoiceEvent(events, 1, "AddChoiceCount");
            Click("BuffChoiceAdd");
            Check(mgr.ChoiceCount == 2, "再加一次：1 → 2", $"实际 {mgr.ChoiceCount}");

            RunRollStatistics(mgr);

            // 选中第 1 个候选
            Click("BuffRoll");
            RefreshConsole();
            if (mgr.RandomBuffs.Count == 0 || mgr.RandomBuffs[0] == null)
            {
                Check(false, "选中检查：抽出了至少 1 个候选", $"候选 {Ids(mgr.RandomBuffs)}");
                return;
            }
            BuffConfig pick = mgr.RandomBuffs[0];
            string shown = Text("PickName", Find("BuffPick0")).text;
            Check(shown == pick.buffName, "候选按钮显示本次候选的名称", $"显示「{shown}」，候选为 {pick.buffName}");
            int pool = mgr.poolCount;
            float percentBefore = PercentOf(pick.buffType);
            events = choiceEventCount;
            Click("BuffPick0");
            Check(BuffActive(pick.id) && mgr.ChoiceCount == 1 && mgr.poolCount == pool - 1,
                $"选中第 1 个（{pick.buffName}，SelectBuff）：生效，次数 2 → 1，候选池 {pool} → {pool - 1}",
                $"生效 {BuffActive(pick.id)}，次数 {mgr.ChoiceCount}，候选池 {mgr.poolCount}");
            CheckChoiceEvent(events, 1, "SelectBuff");
            Check(Same(PercentOf(pick.buffType), percentBefore + pick.percent), $"选中后{BuffTypeName(pick.buffType)}加成增加 {pick.percent}",
                $"{percentBefore} → {PercentOf(pick.buffType)}");
            Check(mgr.RandomBuffs.All(b => b != null && !BuffActive(b.id)), "选中后的候选里没有生效中的 Buff", $"候选 {Ids(mgr.RandomBuffs)}");
            Note($"选中后候选 {Ids(mgr.RandomBuffs)}：选中后是否重新抽取、抽几个由 BuffMgr.SelectBuff 决定，这里不做要求。");

            // 不在本次候选里的 id：直接调用 SelectBuff，应该什么都不做
            int count = mgr.ChoiceCount, activeCount = ActiveBuffs().Count;
            if (!mgr.RandomBuffs.Contains(pick))
            {
                events = choiceEventCount;
                mgr.SelectBuff(pick.id);
                Check(mgr.ChoiceCount == count && ActiveBuffs().Count == activeCount && choiceEventCount == events,
                    $"SelectBuff 传入不在候选里的 id（已生效的 {pick.buffName}）：不扣次数、不重复添加、不发事件",
                    $"次数 {count} → {mgr.ChoiceCount}，生效 {activeCount} → {ActiveBuffs().Count}");
            }
            int absentId = configs.Max(b => b.id) + 1000;
            events = choiceEventCount;
            mgr.SelectBuff(absentId);
            Check(mgr.ChoiceCount == count && ActiveBuffs().Count == activeCount && choiceEventCount == events,
                $"SelectBuff 传入配置表外的 id {absentId}：不扣次数、状态不变", $"次数 {count} → {mgr.ChoiceCount}，生效 {activeCount} → {ActiveBuffs().Count}");

            // 左侧列表直接 AddBuff：不扣次数；已经直接添加的候选再点选中会被控制台拦截
            BuffConfig direct = mgr.RandomBuffs.FirstOrDefault(b => b != null);
            if (direct == null) Note("选中后没有候选，跳过“直接添加不扣次数”检查。");
            else
            {
                int slot = mgr.RandomBuffs.IndexOf(direct);
                Click("BuffRow_" + direct.id);
                Check(BuffActive(direct.id) && mgr.ChoiceCount == count, $"左侧列表直接添加 {direct.buffName}（AddBuff）：生效，不扣选择次数",
                    $"生效 {BuffActive(direct.id)}，次数 {count} → {mgr.ChoiceCount}");
                activeCount = ActiveBuffs().Count;
                Click("BuffPick" + slot);
                Check(mgr.ChoiceCount == count && ActiveBuffs().Count == activeCount, "已直接添加的候选再点选中：控制台拦截，不扣次数、不重复添加",
                    $"次数 {count} → {mgr.ChoiceCount}，生效 {activeCount} → {ActiveBuffs().Count}");
            }

            // 候选池为空：全部 Buff 生效后再抽，应该抽出 0 个，次数保留
            foreach (BuffConfig b in configs)
                if (!BuffActive(b.id)) Click("BuffRow_" + b.id);
            count = mgr.ChoiceCount;
            Check(mgr.poolCount == 0, "全部 Buff 生效后候选池为空", $"候选池 {mgr.poolCount} 个");
            Click("BuffRoll");
            Check(mgr.RandomBuffs.Count == 0 && mgr.ChoiceCount == count, "候选池为空时抽取：0 个候选，次数保留",
                $"候选 {Ids(mgr.RandomBuffs)}，次数 {count} → {mgr.ChoiceCount}");
            RefreshConsole();
            string emptySub = Text("PickSub", Find("BuffPick0")).text;
            Check(emptySub == "候选池为空", "候选按钮显示候选池为空", $"实际「{emptySub}」");

            // ClearAll：次数清零、候选清空，并发出次数事件
            events = choiceEventCount;
            Click("ClearBuffs");
            Check(mgr.ChoiceCount == 0 && mgr.RandomBuffs.Count == 0 && ActiveBuffs().Count == 0 && mgr.poolCount == configs.Count,
                "ClearAll：次数清零、候选清空、生效列表清空、候选池恢复为全部",
                $"次数 {mgr.ChoiceCount}，候选 {Ids(mgr.RandomBuffs)}，生效 {ActiveBuffs().Count} 个，候选池 {mgr.poolCount}/{configs.Count}");
            CheckChoiceEvent(events, 0, "ClearAll");
        }
        finally
        {
            UnsubscribeChoiceEvent();
            if (ActiveBuffs().Count > 0 || mgr.ChoiceCount != 0 || mgr.RandomBuffs.Count > 0)
                Click("ClearBuffs");
            vConsole.SelectTab(DevConsole.HealthTab);
        }
    }

    private static Transform Find(string name, Transform root = null)
    {
        root = root != null ? root : vConsole.transform;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        throw new Exception("找不到控制台控件 " + name);
    }

    private static TMP_InputField Field(string name) => Find(name).GetComponent<TMP_InputField>();
    // 手动执行一次控制台的定时刷新（DevConsole.Update 每 0.15 秒调用一次）。
    private static readonly MethodInfo RefreshDisplayMethod = typeof(DevConsole).GetMethod("RefreshDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
    private static void RefreshConsole() => RefreshDisplayMethod.Invoke(vConsole, null);
    private static TMP_Text Text(string name, Transform root = null) => Find(name, root).GetComponent<TMP_Text>();
    private static void Click(string name) => Find(name).GetComponent<Button>().onClick.Invoke();

    private static void WriteReport(string name, string contents)
    {
        Directory.CreateDirectory(ReportDir);
        File.WriteAllText(Path.Combine(ReportDir, name), contents);
    }
}
#endif
