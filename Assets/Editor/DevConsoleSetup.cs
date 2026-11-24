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
    private static readonly FieldInfo EnemyReturningField = typeof(EnemyAI).GetField("isReturning", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly string[] CaptureNames = { "Health", "Stats", "Items", "Save" };

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
                // 依次截取四个分页，截图时保持控制台打开
                vConsole.SetOpen(true);
                Directory.CreateDirectory(ReportDir);
                captureIndex = 0;
                stageFrame = Time.frameCount;
                SessionState.SetString(StageKey, "Capture");
            }
            else if (stage == "Capture")
            {
                if (Time.frameCount < stageFrame + 8) return;
                if (captureIndex < CaptureNames.Length)
                {
                    vConsole.SelectTab(captureIndex);
                    // ScreenCapture 不会自动创建目录。
                    ScreenCapture.CaptureScreenshot($"{ReportDir}/DevConsole-{captureIndex}-{CaptureNames[captureIndex]}.png");
                    captureIndex++;
                    stageFrame = Time.frameCount;
                    return;
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
        int states = stateEventCount;
        Click("KillPlayer");
        Check(vHealth.CurrentHp == 0 && vHealth.isDead, "杀死玩家：生命归零且 isDead", Hp());
        Check(stateEventCount > states && lastStateEvent, "杀死玩家：发出死亡广播 OnPlayerStateChange(true)");
        Check(vPlayer.animator != null && vPlayer.animator.GetBool("IsDead"), "杀死玩家：Animator 的 IsDead 为 true");
        // 只检查启用中的 EnemyAI：被禁用的组件（例如场景里的静止木桩）不会在 OnEnable 订阅，本来就不参与脱战。
        var allEnemies = UnityEngine.Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        var enemies = allEnemies.Where(e => e.isActiveAndEnabled).ToArray();
        if (allEnemies.Length > enemies.Length) Note($"场景中有 {allEnemies.Length - enemies.Length} 个 EnemyAI 组件处于禁用状态，不参与脱战检查。");
        if (enemies.Length == 0) Note("场景中没有启用的 EnemyAI，跳过怪物脱战检查。");
        else Check(enemies.All(e => (bool)EnemyReturningField.GetValue(e)), $"杀死玩家：{enemies.Length} 个启用中的 EnemyAI 都进入返回状态（脱战）");
        Click("Damage10"); Check(vHealth.CurrentHp == 0, "死亡时扣血被拒绝", Hp());
        Click("Heal10"); Check(vHealth.CurrentHp == 0 && vHealth.isDead, "死亡时回血被拒绝", Hp());
        Click("ResetAttributes"); Check(vHealth.isDead && vHealth.CurrentHp == 0, "死亡时恢复初始值被拒绝", Hp());

        // 5. 复活
        states = stateEventCount;
        Click("RevivePlayer");
        Check(vHealth.CurrentHp == max && !vHealth.isDead, "复活：回满且存活", Hp());
        Check(stateEventCount > states && !lastStateEvent, "复活：发出复活广播 OnPlayerStateChange(false)");
        Check(vPlayer.animator != null && !vPlayer.animator.GetBool("IsDead"), "复活：Animator 的 IsDead 为 false");
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

        // 10. 关闭
        vConsole.SetOpen(false);
        Check(!vConsole.IsOpen, "关闭：控制台窗口关闭");
        Check(vPlayer.enabled && vPlayer.GetComponent<PlayerCombat>().enabled, "关闭：玩家移动和攻击组件恢复");
        Check(PlayerMgr.Instance.CurrentIsCursor == startCursorMode, "关闭：光标模式恢复到打开前", $"打开前 {startCursorMode}，现在 {PlayerMgr.Instance.CurrentIsCursor}");
        Check(CameraInputEnabled() == !startCursorMode, "关闭：镜头输入随光标模式恢复");
        Note("未覆盖：实际按键（F1、Esc、Alt、WASD）和鼠标点击、打怪的实际伤害、清空任务存档、清空背包道具、与任务面板/背包面板/对话同时使用。");
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
        string[] pages = { "HealthPage", "StatsPage", "ItemPage", "SavePage" };
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
