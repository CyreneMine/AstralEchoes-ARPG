#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 仅用于编辑器搭建静态控件；没有任务、分类、追踪或开关面板逻辑。
public static class MissionPanelSetup
{
    private const string ScenePath = "Assets/Scenes/GMScene.unity";
    private const string RequestPath = "Temp/MissionPanelSetup.request";
    private const string ReportPath = "Temp/MissionPanelReports";
    private static TMP_FontAsset font;
    private static Sprite uiSprite;
    private static Sprite background;

    [InitializeOnLoadMethod]
    private static void Initialize() => EditorApplication.delayCall += RunPending;

    private static void RunPending()
    {
        if (!File.Exists(RequestPath)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += RunPending;
            return;
        }
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(RequestPath);
        try { Build(); }
        catch (Exception error)
        {
            Directory.CreateDirectory(ReportPath);
            File.WriteAllText(ReportPath + "/setup-error.txt", error.ToString());
            Debug.LogException(error);
        }
    }

    [MenuItem("Tools/AstralEchoes/Mission UI/Build Empty MissionPanel")]
    public static void Build()
    {
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        if (!scene.IsValid() || !scene.isLoaded || Application.isPlaying)
            throw new InvalidOperationException("请在编辑模式打开 GMScene。");
        var panels = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<RectTransform>(true))
            .Where(t => t.name == "MissionPanel").ToArray();
        if (panels.Length != 1) throw new InvalidOperationException("需要唯一的 MissionPanel。");
        RectTransform root = panels[0];
        if (root.childCount != 0) throw new InvalidOperationException("MissionPanel 已有子物体，不覆盖手动布局。");
        if (root.GetComponentInParent<Canvas>() == null) throw new InvalidOperationException("MissionPanel 不在 Canvas 下。");

        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/DevConsole/DevConsoleFont.asset");
        if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/ArtRes/Font/STHUPO SDF 1.asset");
        if (font == null) throw new InvalidOperationException("没有找到工程已有的中文字体。");
        uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        int undo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Build MissionPanel UI");
        Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Build MissionPanel UI");
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.pivot = new Vector2(.5f, .5f);
        root.offsetMin = root.offsetMax = Vector2.zero;
        root.localScale = Vector3.one;
        root.SetAsLastSibling();
        root.gameObject.SetActive(true);
        Image rootImage = root.GetComponent<Image>();
        if (rootImage == null) rootImage = Undo.AddComponent<Image>(root.gameObject);
        rootImage.color = Color.white;
        rootImage.sprite = null;
        rootImage.raycastTarget = true;
        // 高于已有调试控制台，只影响这个任务界面的显示顺序。
        Canvas layer = root.GetComponent<Canvas>();
        if (layer == null) layer = Undo.AddComponent<Canvas>(root.gameObject);
        layer.overrideSorting = true;
        layer.sortingOrder = 6000;
        if (root.GetComponent<GraphicRaycaster>() == null) Undo.AddComponent<GraphicRaycaster>(root.gameObject);

        RectTransform header = Rect(root, "Header");
        Anchor(header, new Vector2(0, 1), Vector2.one, new Vector2(64, -130), new Vector2(-64, -30));
        Label(header, "TitleText", "任务", 0, 4, 260, 43, 36, true);
        Label(header, "SubtitleText", "全部任务", 0, 54, 260, 32, 24);
        RectTransform tabs = Rect(header, "CategoryTabs");
        tabs.anchorMin = tabs.anchorMax = new Vector2(.5f, 1);
        tabs.pivot = new Vector2(.5f, 1);
        tabs.anchoredPosition = new Vector2(0, -16);
        tabs.sizeDelta = new Vector2(388, 60);
        Button(tabs, "MainQuestTab", "主线", 0, 0, 184, 60);
        Button(tabs, "SideQuestTab", "支线", 204, 0, 184, 60);
        var close = Button(header, "CloseButton", "×", 0, 0, 60, 60);
        close.anchorMin = close.anchorMax = close.pivot = Vector2.one;
        close.anchoredPosition = new Vector2(0, -12);

        RectTransform body = Rect(root, "Body");
        Anchor(body, Vector2.zero, Vector2.one, new Vector2(64, 54), new Vector2(-64, -156));
        var listPanel = Panel(body, "MissionListPanel");
        Anchor(listPanel, Vector2.zero, new Vector2(.285f, 1), Vector2.zero, new Vector2(-14, 0));
        Label(listPanel, "ListTitleText", "任务列表", 22, 16, 360, 38, 27, true);
        BuildList(listPanel);

        var detail = Panel(body, "MissionDetailPanel");
        Anchor(detail, new Vector2(.30f, 0), Vector2.one, Vector2.zero, Vector2.zero);
        var description = Rect(detail, "DescriptionArea");
        Anchor(description, Vector2.zero, Vector2.one, new Vector2(32, 284), new Vector2(-32, -24));
        var location = Label(description, "LocationText", "区域名称 / 地点名称", 0, 0, 600, 32, 23);
        StretchWidth(location.rectTransform, 0, 0, 32);
        var title = Label(description, "MissionTitleText", "任务名称", 0, 48, 700, 56, 36, true);
        StretchWidth(title.rectTransform, 0, 48, 56);
        var objective = Panel(description, "ObjectiveRow");
        Anchor(objective, new Vector2(0, 1), Vector2.one, new Vector2(0, -191), new Vector2(0, -127));
        var objectiveText = Label(objective, "ObjectiveText", "与目标人物对话（0/1）", 20, 10, 800, 44, 26);
        StretchWidth(objectiveText.rectTransform, 20, 10, 44, 20);
        var desc = Label(description, "DescriptionText", "这里显示当前任务的描述。\n\n任务的背景、目标和提示可以显示在这里。", 0, 222, 900, 240, 25);
        Anchor(desc.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -222));
        desc.alignment = TextAlignmentOptions.TopLeft;

        var rewards = Rect(detail, "RewardPreview");
        Anchor(rewards, Vector2.zero, new Vector2(1, 0), new Vector2(32, 104), new Vector2(-32, 252));
        Label(rewards, "RewardTitleText", "奖励预览", 0, 0, 350, 36, 26, true);
        string[] rewardNames = { "经验", "金币", "道具" };
        string[] rewardAmounts = { "100", "500", "1" };
        for (int i = 0; i < rewardNames.Length; i++)
        {
            var slot = Panel(rewards, "RewardSlot" + (i + 1));
            Fixed(slot, i * 118, 47, 102, 98);
            var icon = ImageRect(slot, "Icon", uiSprite);
            Fixed(icon, 14, 10, 74, 45);
            var rewardName = Label(icon, "NameText", rewardNames[i], 0, 0, 74, 45, 20);
            rewardName.alignment = TextAlignmentOptions.Center;
            var amount = Label(slot, "AmountText", rewardAmounts[i], 0, 59, 102, 29, 22);
            amount.alignment = TextAlignmentOptions.Center;
        }
        var footer = Rect(detail, "Footer");
        Anchor(footer, Vector2.zero, new Vector2(1, 0), new Vector2(32, 22), new Vector2(-32, 86));
        var track = Button(footer, "TrackButton", "追踪", 0, 0, 272, 64);
        track.anchorMin = track.anchorMax = track.pivot = Vector2.one;
        track.anchoredPosition = Vector2.zero;

        Canvas.ForceUpdateCanvases();
        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true)) text.ForceMeshUpdate(true);
        Validate(root);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("场景保存失败。");
        Undo.CollapseUndoOperations(undo);
        Selection.activeGameObject = root.gameObject;
        EditorGUIUtility.PingObject(root.gameObject);
        Directory.CreateDirectory(ReportPath);
        File.WriteAllText(ReportPath + "/build-report.json", JsonUtility.ToJson(new BuildReport
        {
            scene = ScenePath, panel = "Canvas/MissionPanel", taskItems = 8, rewards = 3,
            buttons = root.GetComponentsInChildren<Button>(true).Length,
            saved = true, businessLogicAdded = false, buttonPersistentListeners = 0,
            font = AssetDatabase.GetAssetPath(font),
            canvasSize = ((RectTransform)root.GetComponentInParent<Canvas>().transform).rect.size.ToString()
        }, true));
        Debug.Log("MissionPanel 静态UI已保存：主线/支线、任务列表、详情、3个奖励占位、追踪按钮。所有按钮 OnClick 留空。");
    }

    private static void BuildList(RectTransform parent)
    {
        var scroll = Rect(parent, "MissionScrollView");
        Anchor(scroll, Vector2.zero, Vector2.one, new Vector2(16, 16), new Vector2(-16, -68));
        var viewport = ImageRect(scroll, "Viewport", background);
        Anchor(viewport, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-22, 0));
        viewport.gameObject.AddComponent<RectMask2D>();
        var content = Rect(viewport, "Content");
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(.5f, 1);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12;
        layout.padding = new RectOffset(4, 4, 4, 4);
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        for (int i = 0; i < 8; i++)
        {
            var item = Button(content, "MissionItem_" + (i + 1).ToString("00"), null, 0, 0, 400, 112);
            var element = item.gameObject.AddComponent<LayoutElement>();
            element.minHeight = element.preferredHeight = 112;
            var name = Label(item, "TitleText", "任务名称 " + (i + 1).ToString("00"), 24, 17, 340, 40, 25, i == 0);
            StretchWidth(name.rectTransform, 24, 17, 40, 18);
            var area = Label(item, "LocationText", "区域名称 / 地点名称", 24, 62, 340, 30, 19);
            StretchWidth(area.rectTransform, 24, 62, 30, 18);
        }
        var barRect = ImageRect(scroll, "VerticalScrollbar", background);
        Anchor(barRect, new Vector2(1, 0), Vector2.one, new Vector2(-16, 0), Vector2.zero);
        var bar = barRect.gameObject.AddComponent<Scrollbar>();
        var slide = Rect(barRect, "SlidingArea");
        Anchor(slide, Vector2.zero, Vector2.one, new Vector2(2, 2), new Vector2(-2, -2));
        var handle = ImageRect(slide, "Handle", uiSprite);
        Anchor(handle, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        bar.handleRect = handle;
        bar.targetGraphic = handle.GetComponent<Image>();
        bar.direction = Scrollbar.Direction.BottomToTop;
        var view = scroll.gameObject.AddComponent<ScrollRect>();
        view.viewport = viewport;
        view.content = content;
        view.horizontal = false;
        view.vertical = true;
        view.movementType = ScrollRect.MovementType.Clamped;
        view.scrollSensitivity = 36;
        view.verticalScrollbar = bar;
        view.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
    }

    private static RectTransform Rect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create Mission UI");
        go.layer = LayerMask.NameToLayer("UI");
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        return rect;
    }
    private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 insetMin, Vector2 insetMax)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = new Vector2(.5f, .5f);
        rect.offsetMin = insetMin;
        rect.offsetMax = insetMax;
    }
    private static void Fixed(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }
    private static void StretchWidth(RectTransform rect, float left, float top, float height, float right = 0)
        => Anchor(rect, new Vector2(0, 1), Vector2.one, new Vector2(left, -top - height), new Vector2(-right, -top));
    private static RectTransform ImageRect(Transform parent, string name, Sprite sprite)
    {
        var rect = Rect(parent, name);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = Color.white;
        return rect;
    }
    private static RectTransform Panel(Transform parent, string name) => ImageRect(parent, name, background);
    private static TMP_Text Label(Transform parent, string name, string value, float x, float y, float width, float height, int size, bool bold = false)
    {
        var rect = Rect(parent, name);
        Fixed(rect, x, y, width, height);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.color = Color.black;
        text.fontSize = size;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.raycastTarget = false;
        text.richText = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        return text;
    }
    private static RectTransform Button(Transform parent, string name, string title, float x, float y, float width, float height)
    {
        var rect = ImageRect(parent, name, uiSprite);
        Fixed(rect, x, y, width, height);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();
        if (title != null)
        {
            var text = Label(rect, "Label", title, 0, 0, width, height, 26);
            Anchor(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(8, 4), new Vector2(-8, -4));
            text.alignment = TextAlignmentOptions.Center;
        }
        return rect;
    }
    private static void Validate(RectTransform root)
    {
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
            if (button.onClick.GetPersistentEventCount() != 0) throw new InvalidOperationException("按钮不应绑定逻辑。");
        var view = root.GetComponentInChildren<ScrollRect>();
        if (view == null || view.viewport == null || view.content == null || view.verticalScrollbar == null)
            throw new InvalidOperationException("列表滚动组件引用不完整。");
        foreach (Component component in root.GetComponentsInChildren<Component>(true))
            if (component == null) throw new InvalidOperationException("UI内存在缺失的组件。");
    }
    [Serializable]
    private class BuildReport
    {
        public string scene, panel, font, canvasSize;
        public int taskItems, rewards, buttons, buttonPersistentListeners;
        public bool saved, businessLogicAdded;
    }
}
#endif
