#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 由 Claude Code 维护：只搭建背包界面的静态布局（BagPanel、BagSlot 两个预制件），不含任何背包数据或显示逻辑。
// 布局按 1920×1080 参考图测量，坐标均为"距左上角"的像素。
public static class BagPanelSetup
{
    private const string RequestPath = "Temp/BagPanelSetup.request";
    private const string ReportDir = "Temp/BagPanelReports";
    private const string PanelPath = "Assets/Resources/UI/BagPanel.prefab";
    private const string SlotPath = "Assets/Resources/UI/BagSlot.prefab";

    // 物品格子：9 列，116×140，间距 9（参考图实测）
    private const int Columns = 9;
    private static readonly Vector2 SlotSize = new Vector2(116, 140);
    private static readonly Vector2 SlotSpacing = new Vector2(9, 9);
    private const int GridPadding = 4; // 给选中框留出空间，避免被遮罩裁掉

    private static readonly Color ScreenDim = new Color(0.05f, 0.06f, 0.09f, 0.90f);
    private static readonly Color TopBarColor = new Color(0f, 0f, 0f, 0.35f);
    private static readonly Color SlotBg = new Color(0.33f, 0.30f, 0.56f, 1f);
    private static readonly Color CountBarColor = new Color(0.11f, 0.12f, 0.16f, 0.96f);
    private static readonly Color Accent = new Color(0.72f, 0.62f, 0.98f, 1f);
    private static readonly Color SubText = new Color(0.78f, 0.80f, 0.84f, 1f);

    private static TMP_FontAsset font;
    private static Sprite rounded;

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
        string request = File.ReadAllText(RequestPath);
        File.Delete(RequestPath);
        try
        {
            if (request.Contains("build")) BuildPrefabs(request.Contains("overwrite"));
            RenderPreview();
        }
        catch (Exception error)
        {
            Directory.CreateDirectory(ReportDir);
            File.WriteAllText(ReportDir + "/setup-error.txt", error.ToString());
            Debug.LogException(error);
        }
    }

    [MenuItem("Tools/AstralEchoes/Bag UI/Build BagPanel Prefabs")]
    public static void BuildFromMenu() { BuildPrefabs(false); RenderPreview(); }

    [MenuItem("Tools/AstralEchoes/Bag UI/Render Preview")]
    public static void RenderFromMenu() => RenderPreview();

    private static void LoadShared()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/DevConsole/DevConsoleFont.asset");
        if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/ArtRes/Font/STHUPO SDF 1.asset");
        if (font == null) throw new InvalidOperationException("没有找到工程已有的中文字体。");
        rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    public static void BuildPrefabs(bool overwrite)
    {
        if (Application.isPlaying) throw new InvalidOperationException("请在编辑模式下搭建。");
        if (!overwrite && (File.Exists(PanelPath) || File.Exists(SlotPath)))
            throw new InvalidOperationException("BagPanel 或 BagSlot 预制件已存在，为保留手动调整，不覆盖。");
        LoadShared();
        BuildSlot();
        BuildPanel();
        AssetDatabase.SaveAssets();
        Debug.Log("[BagPanelSetup] 已生成 BagPanel 与 BagSlot 预制件。");
    }

    // ---------- BagSlot ----------
    private static void BuildSlot()
    {
        var root = new GameObject("BagSlot", typeof(RectTransform));
        try
        {
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = SlotSize;

            // 选中框：比格子各边大 3 像素，放在最底层，默认隐藏
            var frame = NewImage(rt, "SelectFrame", Color.white, rounded);
            Stretch(frame.rectTransform, -3, -3, -3, -3);
            frame.raycastTarget = false;
            frame.gameObject.SetActive(false);

            var bg = NewImage(rt, "Bg", SlotBg, rounded);
            Stretch(bg.rectTransform, 0, 0, 0, 0);

            var icon = NewImage(rt, "Icon", Color.white, null);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            TopCenter(icon.rectTransform, 0, 8, 96, 96);

            var bar = NewImage(rt, "CountBar", CountBarColor, null);
            bar.raycastTarget = false;
            var barRt = bar.rectTransform;
            barRt.anchorMin = new Vector2(0, 0);
            barRt.anchorMax = new Vector2(1, 0);
            barRt.pivot = new Vector2(0.5f, 0);
            barRt.anchoredPosition = Vector2.zero;
            barRt.sizeDelta = new Vector2(0, 30);

            var count = NewText(barRt, "CountText", "0", 22, Color.white, TextAlignmentOptions.Center);
            Stretch(count.rectTransform, 0, 0, 0, 0);

            var button = root.AddComponent<Button>();
            button.targetGraphic = bg;
            PrefabUtility.SaveAsPrefabAsset(root, SlotPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    // ---------- BagPanel ----------
    private static void BuildPanel()
    {
        var root = new GameObject("BagPanel", typeof(RectTransform), typeof(CanvasGroup));
        try
        {
            var rt = root.GetComponent<RectTransform>();
            Stretch(rt, 0, 0, 0, 0);
            var dim = root.AddComponent<Image>();
            dim.color = ScreenDim;

            // 顶栏
            var top = NewImage(rt, "TopBar", TopBarColor, null);
            var topRt = top.rectTransform;
            topRt.anchorMin = new Vector2(0, 1);
            topRt.anchorMax = new Vector2(1, 1);
            topRt.pivot = new Vector2(0.5f, 1);
            topRt.anchoredPosition = Vector2.zero;
            topRt.sizeDelta = new Vector2(0, 104);

            var titleIcon = NewImage(topRt, "TitleIcon", new Color(0.86f, 0.76f, 0.52f), rounded);
            TopLeft(titleIcon.rectTransform, 50, 34, 42, 48);
            var title = NewText(topRt, "TitleText", "背包", 20, SubText, TextAlignmentOptions.MidlineLeft);
            TopLeft(title.rectTransform, 106, 30, 240, 28);
            var subtitle = NewText(topRt, "SubtitleText", "全部物品", 28, Color.white, TextAlignmentOptions.MidlineLeft);
            TopLeft(subtitle.rectTransform, 106, 56, 360, 38);

            // 金币（参考图中的信用点位置）
            var money = NewImage(topRt, "MoneyBar", new Color(0f, 0f, 0f, 0.45f), rounded);
            TopRight(money.rectTransform, 300, 48, 180, 36);
            var moneyIcon = NewImage(money.rectTransform, "MoneyIcon", new Color(0.95f, 0.80f, 0.35f), rounded);
            moneyIcon.raycastTarget = false;
            var miRt = moneyIcon.rectTransform;
            miRt.anchorMin = miRt.anchorMax = new Vector2(0, 0.5f);
            miRt.pivot = new Vector2(0, 0.5f);
            miRt.anchoredPosition = new Vector2(6, 0);
            miRt.sizeDelta = new Vector2(30, 24);
            var moneyText = NewText(money.rectTransform, "MoneyText", "0", 22, Color.white, TextAlignmentOptions.MidlineLeft);
            Stretch(moneyText.rectTransform, 46, 0, 10, 0);

            var close = NewText(topRt, "CloseButton", "×", 56, Color.white, TextAlignmentOptions.Center);
            TopRight(close.rectTransform, 35, 38, 52, 52);
            close.raycastTarget = true; // 按钮本身就是这段文字，必须能接收射线，否则点不到
            var closeBtn = close.gameObject.AddComponent<Button>();
            closeBtn.targetGraphic = close;

            // 物品列表：ScrollRect + 自动排列的 Content
            var scrollGo = new GameObject("ItemScrollView", typeof(RectTransform), typeof(ScrollRect));
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.SetParent(rt, false);
            float gridWidth = Columns * SlotSize.x + (Columns - 1) * SlotSpacing.x + GridPadding * 2;
            TopLeft(scrollRt, 128 - GridPadding, 127 - GridPadding, gridWidth, 811);

            var viewport = NewImage(scrollRt, "Viewport", new Color(0, 0, 0, 0), null);
            Stretch(viewport.rectTransform, 0, 0, 0, 0);
            viewport.gameObject.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            var contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.SetParent(viewport.rectTransform, false);
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0, 1);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = Vector2.zero;
            var grid = contentGo.GetComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(GridPadding, GridPadding, GridPadding, GridPadding);
            grid.cellSize = SlotSize;
            grid.spacing = SlotSpacing;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Columns;
            var fitter = contentGo.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 竖向滚动条，位置按参考图放在列表右侧
            var barGo = new GameObject("ItemScrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            var barRt = barGo.GetComponent<RectTransform>();
            barRt.SetParent(rt, false);
            TopLeft(barRt, 1350, 130, 6, 785);
            barGo.GetComponent<Image>().color = new Color(1, 1, 1, 0.12f);
            var handleArea = new GameObject("SlidingArea", typeof(RectTransform)).GetComponent<RectTransform>();
            handleArea.SetParent(barRt, false);
            Stretch(handleArea, 0, 0, 0, 0);
            var handle = NewImage(handleArea, "Handle", new Color(1, 1, 1, 0.6f), null);
            Stretch(handle.rectTransform, 0, 0, 0, 0);
            var scrollbar = barGo.GetComponent<Scrollbar>();
            scrollbar.handleRect = handle.rectTransform;
            scrollbar.targetGraphic = handle;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = viewport.rectTransform;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

            // 销毁按钮（参考图"销毁"按钮的位置；功能未定，先占位）
            var destroy = NewImage(rt, "DestroyButton", new Color(0.36f, 0.37f, 0.42f), rounded);
            TopLeft(destroy.rectTransform, 1168, 967, 194, 46);
            var destroyText = NewText(destroy.rectTransform, "DestroyText", "销毁", 22, Color.white, TextAlignmentOptions.Center);
            Stretch(destroyText.rectTransform, 0, 0, 0, 0);
            destroy.gameObject.AddComponent<Button>().targetGraphic = destroy;

            // 右侧详情
            var detail = new GameObject("DetailPanel", typeof(RectTransform)).GetComponent<RectTransform>();
            detail.SetParent(rt, false);
            TopRight(detail, 77, 128, 441, 800);

            var name = NewText(detail, "ItemNameText", "物品名称", 28, Accent, TextAlignmentOptions.MidlineLeft);
            TopLeft(name.rectTransform, 0, 0, 441, 40);
            var line = NewImage(detail, "NameLine", Accent, null);
            TopLeft(line.rectTransform, 0, 38, 441, 3);

            var iconFrame = NewImage(detail, "IconFrame", new Color(0.42f, 0.36f, 0.68f), null);
            TopLeft(iconFrame.rectTransform, 0, 66, 441, 162);
            var bigIcon = NewImage(iconFrame.rectTransform, "ItemIcon", Color.white, null);
            bigIcon.preserveAspect = true;
            TopLeft(bigIcon.rectTransform, 240, 11, 140, 140);
            var detailCount = NewText(iconFrame.rectTransform, "CountText", "× 0", 34, Color.white, TextAlignmentOptions.BottomLeft);
            TopLeft(detailCount.rectTransform, 16, 108, 220, 44);

            var desc = NewText(detail, "DescText", "物品描述", 22, Color.white, TextAlignmentOptions.TopLeft);
            TopLeft(desc.rectTransform, 0, 242, 441, 500);
            desc.textWrappingMode = TextWrappingModes.Normal;
            desc.lineSpacing = 12;

            // 使用按钮：详情区右下，只在消耗品时显示（由面板脚本控制显隐）
            var use = NewImage(detail, "UseButton", new Color(0.92f, 0.92f, 0.92f), rounded);
            TopLeft(use.rectTransform, 50, 762, 341, 56);
            var useText = NewText(use.rectTransform, "UseText", "使用", 24, new Color(0.12f, 0.12f, 0.14f), TextAlignmentOptions.Center);
            Stretch(useText.rectTransform, 0, 0, 0, 0);
            use.gameObject.AddComponent<Button>().targetGraphic = use;

            PrefabUtility.SaveAsPrefabAsset(root, PanelPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    // ---------- 预览：在隐藏的预览场景里渲染一张 1920×1080 的图，不改动当前场景 ----------
    public static void RenderPreview()
    {
        LoadShared();
        var panelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PanelPath);
        var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPath);
        if (panelPrefab == null || slotPrefab == null) throw new InvalidOperationException("预制件不存在，先生成。");

        Scene scene = EditorSceneManager.NewPreviewScene();
        var rtex = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        try
        {
            var camGo = new GameObject("PreviewCamera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(camGo, scene);
            var cam = camGo.GetComponent<Camera>();
            cam.scene = scene;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.30f, 0.34f, 0.40f, 1f); // 代替游戏画面
            cam.orthographic = true;
            cam.targetTexture = rtex;

            var canvasGo = new GameObject("PreviewCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            SceneManager.MoveGameObjectToScene(canvasGo, scene);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 10;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0;

            var panel = (GameObject)UnityEngine.Object.Instantiate(panelPrefab, canvasGo.transform, false);
            var content = panel.transform.Find("ItemScrollView/Viewport/Content");
            int[] samples = { 982, 8328, 5773, 4176, 4020, 287, 378, 1121, 311, 25, 10, 5, 45, 10, 40, 1, 10, 1,
                              947, 21, 5, 1, 15, 1, 5, 1, 6, 45, 5, 5, 24, 42, 60, 13, 78, 1, 44, 414, 472, 422, 256, 40 };
            for (int i = 0; i < samples.Length; i++)
            {
                var slot = (GameObject)UnityEngine.Object.Instantiate(slotPrefab, content, false);
                slot.transform.Find("CountBar/CountText").GetComponent<TMP_Text>().text = samples[i].ToString();
                var icon = slot.transform.Find("Icon").GetComponent<Image>();
                icon.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                if (i == 0) slot.transform.Find("SelectFrame").gameObject.SetActive(true);
            }
            panel.transform.Find("TopBar/MoneyBar/MoneyText").GetComponent<TMP_Text>().text = "161709";
            var detail = panel.transform.Find("DetailPanel");
            detail.Find("ItemNameText").GetComponent<TMP_Text>().text = "示例物品";
            detail.Find("IconFrame/CountText").GetComponent<TMP_Text>().text = "× 982";
            detail.Find("IconFrame/ItemIcon").GetComponent<Image>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            detail.Find("DescText").GetComponent<TMP_Text>().text = "这里显示物品描述，文字超过一行时自动换行。预览中的格子、数量和金币都是示例数据。";

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);
            Canvas.ForceUpdateCanvases();
            foreach (var t in panel.GetComponentsInChildren<TMP_Text>(true)) t.ForceMeshUpdate(true, true);
            Canvas.ForceUpdateCanvases();
            cam.Render();

            var prev = RenderTexture.active;
            RenderTexture.active = rtex;
            var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            cam.targetTexture = null; // 先解除绑定，避免释放 RenderTexture 时报错
            Directory.CreateDirectory(ReportDir);
            File.WriteAllBytes(ReportDir + "/BagPanel-preview.png", tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            var contentRt = (RectTransform)content;
            File.WriteAllText(ReportDir + "/report.json",
                $"{{\"slots\":{content.childCount},\"contentHeight\":{contentRt.rect.height:F0},\"time\":\"{DateTime.Now:O}\"}}");
            Debug.Log("[BagPanelSetup] 预览已输出到 " + ReportDir + "/BagPanel-preview.png");
        }
        finally
        {
            cam_cleanup(rtex);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    private static void cam_cleanup(RenderTexture rtex)
    {
        if (rtex != null) { rtex.Release(); UnityEngine.Object.DestroyImmediate(rtex); }
    }

    // ---------- 工具方法 ----------
    private static Image NewImage(Transform parent, string name, Color color, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.sprite = sprite;
        if (sprite != null) image.type = Image.Type.Sliced;
        return image;
    }

    private static TextMeshProUGUI NewText(Transform parent, string name, string text, float size, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = false;
        return t;
    }

    private static void Stretch(RectTransform rt, float left, float top, float right, float bottom)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    private static void TopLeft(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
    }

    // right：右边缘距父物体右侧的距离
    private static void TopRight(RectTransform rt, float right, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-right, -y);
        rt.sizeDelta = new Vector2(w, h);
    }

    private static void TopCenter(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
    }
}
#endif
