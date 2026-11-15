#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 在已打开的 GMScene 内搭建控件，不覆盖或重新创建用户的场景。
public static class DevConsoleSetup
{
    private const string ScenePath = "Assets/Scenes/GMScene.unity";
    private const string RequestPath = "Temp/DevConsoleSetup.request";
    private const string ReportDir = "Temp/DevConsoleReports";
    private const string StageKey = "AstralEchoes.DevConsole.VerifyStage";
    private static TMP_FontAsset font;
    private static int captureFrame;
    private static string resultJson;
    private static readonly Color Background = new Color(0.045f, 0.065f, 0.11f, 0.98f);
    private static readonly Color Accent = new Color(0.17f, 0.48f, 0.56f, 1);

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.delayCall += RunPending;
    }

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
            if (request.Contains("mission-reset"))
            {
                AddMissionResetButton();
                return;
            }
            if (request.Contains("font")) RefreshConsoleFont();
            else Build();
            SessionState.SetString(StageKey, "WaitingForPlayer");
            EditorApplication.EnterPlaymode();
        }
        catch (Exception error)
        {
            WriteReport("setup-error.txt", error.ToString());
            Debug.LogException(error);
        }
    }

    [MenuItem("Tools/AstralEchoes/Dev Console/Build in GMScene")]
    public static void Build()
    {
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        if (!scene.IsValid() || !scene.isLoaded || Application.isPlaying)
            throw new InvalidOperationException("请在编辑模式打开 GMScene 再搭建控制台。");

        Canvas canvas = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Canvas>(true))
            .FirstOrDefault(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay);
        if (canvas == null)
            throw new InvalidOperationException("没有找到场景中的屏幕空间 Canvas。");

        Transform existing = canvas.transform.Find("DevConsole");
        if (existing != null)
            throw new InvalidOperationException("DevConsole 已存在，为保留手动布局，本工具不会覆盖它。");

        font = GetConsoleFont();

        GameObject root = new GameObject("DevConsole", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(DevConsole));
        Undo.RegisterCreatedObjectUndo(root, "Create DevConsole");
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(canvas.transform, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
        Canvas overlay = root.GetComponent<Canvas>();
        overlay.overrideSorting = true;
        overlay.sortingOrder = 5000;
        DevConsole console = root.GetComponent<DevConsole>();

        GameObject launcher = Button(root.transform, "ToggleButton", "管理员控制台 · F1", 0, 0, 200, 42, Accent, console.Toggle);
        RectTransform launcherRect = launcher.GetComponent<RectTransform>();
        launcherRect.anchorMin = launcherRect.anchorMax = launcherRect.pivot = Vector2.one;
        launcherRect.anchoredPosition = new Vector2(-24, -18);

        GameObject window = Box(root.transform, "Window", 0, 0, 540, 874, Background);
        RectTransform windowRect = window.GetComponent<RectTransform>();
        windowRect.anchorMin = windowRect.anchorMax = windowRect.pivot = Vector2.one;
        windowRect.anchoredPosition = new Vector2(-24, -70);
        Box(window.transform, "AccentLine", 0, 0, 540, 4, new Color(0.25f, 0.9f, 0.86f));
        Label(window.transform, "Title", "管理员控制台", 24, 15, 410, 34, 27, Color.white);
        Label(window.transform, "Subtitle", "F1 开关 · Esc 关闭 · 角色与任务调试", 24, 53, 470, 26, 17, new Color(0.55f, 0.67f, 0.77f));
        Button(window.transform, "CloseButton", "×", 478, 14, 40, 38, new Color(0.22f, 0.27f, 0.35f), console.Close);
        Box(window.transform, "StatusBackground", 20, 92, 500, 104, new Color(0.09f, 0.13f, 0.20f));
        TMP_Text status = Label(window.transform, "Status", "等待玩家初始化…", 35, 100, 470, 88, 20, Color.white);
        Label(window.transform, "HealthTitle", "生命调试", 24, 210, 470, 28, 23, new Color(0.4f, 0.9f, 0.85f));

        Color damageColor = new Color(0.55f, 0.21f, 0.25f);
        Color healColor = new Color(0.15f, 0.4f, 0.32f);
        Button(window.transform, "Damage10", "扣血 10", 24, 247, 117, 42, damageColor, console.Damage10);
        Button(window.transform, "Damage25", "扣血 25", 149, 247, 117, 42, damageColor, console.Damage25);
        Button(window.transform, "Heal10", "回血 10", 274, 247, 117, 42, healColor, console.Heal10);
        Button(window.transform, "Heal25", "回血 25", 399, 247, 117, 42, healColor, console.Heal25);
        TMP_InputField amount = Input(window.transform, "AmountInput", "10", 24, 301, 115, 42);
        Button(window.transform, "DamageCustom", "按数值扣血", 149, 301, 178, 42, damageColor, console.DamageCustom);
        Button(window.transform, "HealCustom", "按数值回血", 337, 301, 179, 42, healColor, console.HealCustom);
        Button(window.transform, "FullHealth", "恢复满血", 24, 355, 157, 42, healColor, console.FullHealth);
        Button(window.transform, "KillPlayer", "杀死玩家", 191, 355, 157, 42, new Color(0.64f, 0.16f, 0.20f), console.KillPlayer);
        Button(window.transform, "RevivePlayer", "复活并回满", 358, 355, 158, 42, Accent, console.RevivePlayer);
        Button(window.transform, "TestDamage", "按输入值造成伤害（经过防御）", 24, 409, 492, 40, new Color(0.25f, 0.31f, 0.42f), console.TestDamage);

        Label(window.transform, "AttributesTitle", "实时属性", 24, 466, 470, 28, 23, new Color(0.4f, 0.9f, 0.85f));
        Label(window.transform, "AttackLabel", "攻击力", 24, 504, 150, 25, 19, Color.white);
        Label(window.transform, "DefenseLabel", "防御力", 191, 504, 150, 25, 19, Color.white);
        Label(window.transform, "MaxHealthLabel", "最大生命", 358, 504, 150, 25, 19, Color.white);
        TMP_InputField attack = Input(window.transform, "AttackInput", "0", 24, 535, 157, 42);
        TMP_InputField defense = Input(window.transform, "DefenseInput", "0", 191, 535, 157, 42);
        TMP_InputField maxHealth = Input(window.transform, "MaxHealthInput", "100", 358, 535, 158, 42);
        Button(window.transform, "AttackDown", "攻击 −1", 24, 590, 117, 36, Accent, console.AttackDown);
        Button(window.transform, "AttackUp", "攻击 +1", 149, 590, 117, 36, Accent, console.AttackUp);
        Button(window.transform, "DefenseDown", "防御 −1", 274, 590, 117, 36, Accent, console.DefenseDown);
        Button(window.transform, "DefenseUp", "防御 +1", 399, 590, 117, 36, Accent, console.DefenseUp);
        Button(window.transform, "MaxHealthDown", "生命上限 −10", 24, 638, 241, 36, Accent, console.MaxHealthDown);
        Button(window.transform, "MaxHealthUp", "生命上限 +10", 275, 638, 241, 36, Accent, console.MaxHealthUp);
        Button(window.transform, "ApplyAttributes", "应用属性", 24, 686, 241, 42, Accent, console.ApplyAttributes);
        Button(window.transform, "ResetAttributes", "恢复 SO 初始值", 275, 686, 241, 42, new Color(0.25f, 0.31f, 0.42f), console.ResetAttributes);
        Button(window.transform, "ClearMissionSave", "清空任务存档", 24, 742, 492, 42, new Color(0.55f, 0.21f, 0.25f), console.ClearMissionSave);
        TMP_Text message = Label(window.transform, "Message", "输入属性后回车或移开焦点即可生效。\n扣血直接减生命；伤害按钮经过防御计算。", 24, 796, 492, 62, 17, new Color(0.55f, 0.87f, 0.9f));

        var serialized = new SerializedObject(console);
        Set(serialized, "window", window);
        Set(serialized, "targetPlayer", scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<PlayerObject>(true)).FirstOrDefault());
        Set(serialized, "statusText", status);
        Set(serialized, "messageText", message);
        Set(serialized, "amountInput", amount);
        Set(serialized, "attackInput", attack);
        Set(serialized, "defenseInput", defense);
        Set(serialized, "maxHealthInput", maxHealth);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        int boundBars = 0;
        foreach (PlayerHealthBar bar in scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<PlayerHealthBar>(true)))
        {
            bar.healthBarFill = bar.GetComponentsInChildren<Image>(true).FirstOrDefault(image => image.name == "Fill");
            bar.txtHp = bar.GetComponentInChildren<TMP_Text>(true);
            EditorUtility.SetDirty(bar);
            boundBars++;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = root;
        int buttons = root.GetComponentsInChildren<Button>(true).Length;
        if (buttons < 18 || boundBars < 1)
            throw new InvalidOperationException("控制台按钮或玩家血条绑定数量异常。");
        WriteReport("scene-build.json", $"{{\"scene\":\"GMScene\",\"buttons\":{buttons},\"boundPlayerBars\":{boundBars},\"canvas\":\"{canvas.name}\",\"saved\":true}}");
        Debug.Log($"[DevConsole] 已在现有Canvas搭建控制台，{buttons}个按钮，{boundBars}条玩家血条引用已绑定。");
    }

    [MenuItem("Tools/AstralEchoes/Dev Console/Add Mission Reset Button")]
    public static void AddMissionResetButton()
    {
        if (Application.isPlaying) throw new InvalidOperationException("请在编辑模式添加按钮。");
        font = GetConsoleFont();
        const string prefabPath = "Assets/Resources/UI/Canvas.prefab";
        GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
        int prefabCount;
        try
        {
            var consoles = prefab.GetComponentsInChildren<DevConsole>(true);
            prefabCount = consoles.Length;
            foreach (var console in consoles) UpgradeMissionButton(console);
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }

        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        int sceneCount = 0;
        if (scene.IsValid() && scene.isLoaded)
        {
            foreach (var console in scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<DevConsole>(true)))
            {
                UpgradeMissionButton(console);
                sceneCount++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        WriteReport("mission-reset-button.json", $"{{\"prefabConsoles\":{prefabCount},\"sceneConsoles\":{sceneCount},\"saved\":true,\"saveDataExecuted\":false}}");
        Debug.Log("[DevConsole] 已添加清空任务存档按钮；未执行清空操作。");
    }

    private static void UpgradeMissionButton(DevConsole console)
    {
        var window = console.transform.Find("Window") as RectTransform;
        if (window == null) throw new InvalidOperationException("控制台 Window 不存在。");
        Undo.RecordObject(window, "Extend DevConsole Window");
        window.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(window.sizeDelta.y, 874));
        if (window.Find("ClearMissionSave") == null)
            Button(window, "ClearMissionSave", "清空任务存档", 24, 742, 492, 42,
                new Color(0.55f, 0.21f, 0.25f), console.ClearMissionSave);
        var message = window.Find("Message") as RectTransform;
        if (message != null)
        {
            Undo.RecordObject(message, "Move DevConsole Message");
            message.anchoredPosition = new Vector2(24, -796);
        }
        var subtitle = window.Find("Subtitle")?.GetComponent<TMP_Text>();
        if (subtitle != null && subtitle.text.Contains("仅修改运行时实例"))
        {
            Undo.RecordObject(subtitle, "Update DevConsole Subtitle");
            subtitle.text = "F1 开关 · Esc 关闭 · 角色与任务调试";
        }
        Canvas.ForceUpdateCanvases();
        var label = window.Find("ClearMissionSave/Label").GetComponent<TMP_Text>();
        label.ForceMeshUpdate(true);
        var button = window.Find("ClearMissionSave").GetComponent<Button>();
        if (button.onClick.GetPersistentEventCount() != 1 ||
            button.onClick.GetPersistentMethodName(0) != nameof(DevConsole.ClearMissionSave))
            throw new InvalidOperationException("清空任务存档按钮回调不正确。");
        if (PrefabUtility.IsPartOfPrefabInstance(window))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(window);
            if (message != null) PrefabUtility.RecordPrefabInstancePropertyModifications(message);
            if (subtitle != null) PrefabUtility.RecordPrefabInstancePropertyModifications(subtitle);
        }
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
            File.ReadAllText("Assets/Editor/DevConsoleSetup.cs") +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 /.-+×−_";
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

    [MenuItem("Tools/AstralEchoes/Dev Console/Refresh Console Font")]
    public static void RefreshConsoleFont()
    {
        var console = UnityEngine.Object.FindFirstObjectByType<DevConsole>();
        if (console == null || Application.isPlaying) throw new InvalidOperationException("请在编辑模式选择GMScene控制台。");
        font = GetConsoleFont();
        foreach (TMP_Text text in console.GetComponentsInChildren<TMP_Text>(true))
        {
            text.font = font;
            EditorUtility.SetDirty(text);
        }
        EditorSceneManager.MarkSceneDirty(console.gameObject.scene);
        EditorSceneManager.SaveScene(console.gameObject.scene);
    }

    private static void Set(SerializedObject target, string field, UnityEngine.Object value)
    {
        target.FindProperty(field).objectReferenceValue = value;
    }

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

    private static TMP_Text Label(Transform parent, string name, string value, float x, float y, float width, float height, int size, Color color)
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

    private static GameObject Button(Transform parent, string name, string title, float x, float y, float width, float height, Color color, UnityAction action)
    {
        var go = Box(parent, name, x, y, width, height, color);
        var button = go.AddComponent<Button>();
        button.targetGraphic = go.GetComponent<Image>();
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        button.colors = colors;
        var text = Label(go.transform, "Label", title, 4, 0, width - 8, height, 19, Color.white);
        text.alignment = TextAlignmentOptions.Center;
        UnityEventTools.AddPersistentListener(button.onClick, action);
        return go;
    }

    private static TMP_InputField Input(Transform parent, string name, string value, float x, float y, float width, float height)
    {
        var go = Box(parent, name, x, y, width, height, new Color(0.12f, 0.17f, 0.25f));
        var input = go.AddComponent<TMP_InputField>();
        var area = Rect(go.transform, "TextArea", 10, 4, width - 20, height - 8);
        area.gameObject.AddComponent<RectMask2D>();
        var text = Label(area, "Text", value, 0, 0, width - 20, height - 8, 22, Color.white);
        text.alignment = TextAlignmentOptions.MidlineLeft;
        input.textViewport = area;
        input.textComponent = text as TextMeshProUGUI;
        input.targetGraphic = go.GetComponent<Image>();
        input.contentType = TMP_InputField.ContentType.IntegerNumber;
        input.characterLimit = 8;
        input.text = value;
        input.caretColor = Color.white;
        input.customCaretColor = true;
        return input;
    }

    [MenuItem("Tools/AstralEchoes/Dev Console/Verify in Play Mode")]
    public static void Verify()
    {
        SessionState.SetString(StageKey, "WaitingForPlayer");
        if (!Application.isPlaying) EditorApplication.EnterPlaymode();
        else { EditorApplication.update -= VerifyTick; EditorApplication.update += VerifyTick; }
    }

    private static void VerifyTick()
    {
        if (!Application.isPlaying || EditorApplication.isCompiling) return;
        try
        {
            if (SessionState.GetString(StageKey, "") == "WaitingForPlayer")
            {
                var player = PlayerMgr.Instance.player;
                var console = UnityEngine.Object.FindFirstObjectByType<DevConsole>();
                CharacterHealth health = player != null ? player.GetComponent<CharacterHealth>() : null;
                if (player == null || health == null || health.MaxHp <= 0 || console == null || Time.frameCount < 20)
                    return;
                var template = ConfigMgr.Instance.GetCharacterData(player.Config.type, player.Config.id);
                int templateAttack = template.atk, templateDefense = template.def, templateMaxConfig = template.maxHp;
                // 属性按等级计算，基准取当前等级下 CharacterStats 的结果，而不是配置表的 1 级数值。
                int startAttack = player.Stats.Atk, templateMax = health.MaxHp;
                console.SetOpen(true);
                Click(console, "Damage10"); Require(health.CurrentHp == templateMax - 10, "直接扣血10");
                Click(console, "Heal10"); Require(health.CurrentHp == templateMax, "回血10");
                Click(console, "KillPlayer"); Require(health.CurrentHp == 0 && health.isDead, "杀死玩家");
                Click(console, "RevivePlayer"); Require(health.CurrentHp == templateMax && !health.isDead, "复活");
                Field(console, "AttackInput").SetTextWithoutNotify("7");
                Field(console, "DefenseInput").SetTextWithoutNotify("4");
                Field(console, "MaxHealthInput").SetTextWithoutNotify("80");
                Click(console, "ApplyAttributes");
                int defense = (int)typeof(CharacterHealth).GetField("def", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(health);
                Require(player.Stats.Atk == 7 && player.Stats.Def == 4 && defense == 4 && health.MaxHp == 80 && health.CurrentHp == 80, "实时属性应用并限制生命上限");
                Field(console, "AmountInput").SetTextWithoutNotify("14");
                Click(console, "TestDamage"); Require(health.CurrentHp == 70, "原始伤害14减防御4");
                Field(console, "AmountInput").SetTextWithoutNotify("5");
                Click(console, "HealCustom"); Require(health.CurrentHp == 75, "自定义回血");
                Click(console, "ResetAttributes");
                Require(health.CurrentHp == templateMax && player.Stats.Atk == startAttack, "恢复初始配置");
                Require(template.atk == templateAttack && template.def == templateDefense && template.maxHp == templateMaxConfig, "共享SO数据未被修改");
                console.Close(); Require(player.enabled && player.GetComponent<PlayerCombat>().enabled, "关闭控制台恢复玩家组件");
                console.SetOpen(true);
                resultJson = "{\"passed\":true,\"runtime\":true,\"checks\":[\"damage\",\"heal\",\"kill\",\"revive\",\"attributes\",\"defense\",\"reset\",\"shared-config-isolation\",\"close-input-restore\"],\"note\":\"Buttons invoked through serialized UnityEvents in actual Play Mode; HUD auto-binding was removed and is excluded. Pointer and keyboard require separate UI verification.\"}";
                ScreenCapture.CaptureScreenshot(ReportDir + "/DevConsole-PlayMode.png");
                captureFrame = Time.frameCount;
                SessionState.SetString(StageKey, "Capture");
            }
            else if (SessionState.GetString(StageKey, "") == "Capture" && Time.frameCount >= captureFrame + 12)
            {
                WriteReport("playmode-verification.json", resultJson);
                SessionState.SetString(StageKey, "Done");
                EditorApplication.update -= VerifyTick;
                EditorApplication.ExitPlaymode();
            }
        }
        catch (Exception error)
        {
            WriteReport("playmode-error.txt", error.ToString());
            SessionState.SetString(StageKey, "Failed");
            EditorApplication.update -= VerifyTick;
            Debug.LogException(error);
            EditorApplication.ExitPlaymode();
        }
    }

    private static TMP_InputField Field(DevConsole console, string name) => console.transform.Find("Window/" + name).GetComponent<TMP_InputField>();
    private static void Click(DevConsole console, string name) => console.transform.Find("Window/" + name).GetComponent<Button>().onClick.Invoke();
    private static void Require(bool condition, string message) { if (!condition) throw new Exception("DevConsole 验证失败：" + message); }
    private static void WriteReport(string name, string contents)
    {
        Directory.CreateDirectory(ReportDir);
        File.WriteAllText(Path.Combine(ReportDir, name), contents);
    }
}
#endif
