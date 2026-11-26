#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 由 Claude Code 维护：搭建三选一 Buff 面板的静态布局（BuffPanel、BuffOption 两个预制件）和配套图片，
// 不含任何 Buff 数据或选择逻辑；面板脚本由用户编写后挂到预制件上，再绑定引用。
// 结构参考 Warframe 双衍王境"选择王令"界面：顶部标题、中间一排拱形卡片。按用户要求去掉了下方刷新按钮和右侧列表，
// 卡片最多 3 张、自动横向排列，右上角加关闭按钮。图形和配色都是这里用代码画的，不使用原作素材。
// 候选为 0 时的"你已经足够强了"不放在面板里，用户打算以后做一个通用的浮动文字面板来提示（11-26 决定）。
// Resources/UI 不进仓库，所以图片也由本脚本生成，克隆后执行菜单即可恢复。坐标按 1920×1080 设计。
public static class BuffPanelSetup
{
    private const string RequestPath = "Temp/BuffPanelSetup.request";
    private const string ReportDir = "Temp/BuffPanelReports";
    private const string PanelPath = "Assets/Resources/UI/BuffPanel.prefab";
    private const string OptionPath = "Assets/Resources/UI/BuffOption.prefab";
    private const string SpriteDir = "Assets/Resources/UI/Sprites/BuffPanel";

    private static readonly Vector2 CardSize = new Vector2(340, 560);
    private const float CardSpacing = 70;

    private static readonly Color ScreenDim = new Color(0.02f, 0.03f, 0.07f, 0.80f);
    private static readonly Color Gold = new Color(0.86f, 0.74f, 0.50f, 1f);
    private static readonly Color GoldSoft = new Color(0.86f, 0.74f, 0.50f, 0.65f);
    private static readonly Color Navy = new Color(0.07f, 0.10f, 0.19f, 0.94f);
    private static readonly Color TitleColor = new Color(0.98f, 0.94f, 0.84f, 1f);
    private static readonly Color DescColor = new Color(0.84f, 0.88f, 0.95f, 1f);
    private static readonly Color MutedColor = new Color(0.62f, 0.68f, 0.78f, 1f);
    private static readonly Color StarColor = new Color(0.62f, 0.90f, 1f, 1f);

    private static TMP_FontAsset font;
    private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

    [InitializeOnLoadMethod]
    private static void Initialize() => EditorApplication.delayCall += RunPending;

    // 编译完成后读取 Temp/BuffPanelSetup.request：含 build 时生成（含 overwrite 时覆盖已有预制件和图片），之后都渲染预览。
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
            if (request.Contains("build")) Build(request.Contains("overwrite"));
            RenderPreviews();
        }
        catch (Exception error)
        {
            Directory.CreateDirectory(ReportDir);
            File.WriteAllText(ReportDir + "/setup-error.txt", error.ToString());
            Debug.LogException(error);
        }
    }

    [MenuItem("Tools/AstralEchoes/Buff UI/Build BuffPanel Prefabs")]
    public static void BuildFromMenu() { Build(false); RenderPreviews(); }

    [MenuItem("Tools/AstralEchoes/Buff UI/Render Preview")]
    public static void RenderFromMenu() => RenderPreviews();

    public static void Build(bool overwrite)
    {
        if (Application.isPlaying) throw new InvalidOperationException("请在编辑模式下搭建。");
        LoadFont();
        GenerateSprites(overwrite);
        LoadSprites();
        // 预制件挂上面板脚本、绑定引用之后，重新生成会丢掉这些设置，所以默认不覆盖。
        if (!overwrite && (File.Exists(PanelPath) || File.Exists(OptionPath)))
        {
            RemoveEmptyHint();
            Debug.Log("[BuffPanelSetup] BuffPanel 或 BuffOption 预制件已存在，为保留手动调整，没有覆盖。");
            return;
        }
        BuildOption();
        BuildPanel();
        AssetDatabase.SaveAssets();
        Debug.Log("[BuffPanelSetup] 已生成 BuffPanel 与 BuffOption 预制件。");
    }

    private static void LoadFont()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/DevConsole/DevConsoleFont.asset");
        if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/ArtRes/Font/STHUPO SDF 1.asset");
        if (font == null) throw new InvalidOperationException("没有找到工程已有的中文字体。");
    }

    // ======================================================================
    // 图片：按 2 倍分辨率用有向距离场画白色图形，颜色在 Image 上着色
    // ======================================================================

    private static readonly string[] SpriteNames =
        { "CardBody", "CardBorder", "Emblem", "Star", "Divider", "TitlePlate", "TitleBorder", "Disk", "Ring" };

    private static string SpritePath(string name) => $"{SpriteDir}/Buff{name}.png";

    private static void GenerateSprites(bool overwrite)
    {
        Directory.CreateDirectory(SpriteDir);
        bool wrote = false;
        foreach (string name in SpriteNames)
        {
            string path = SpritePath(name);
            if (!overwrite && File.Exists(path)) continue;
            DrawSprite(name, path);
            wrote = true;
        }
        if (!wrote) return;
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string name in SpriteNames)
            ConfigureSprite(SpritePath(name));
    }

    private static void ConfigureSprite(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) throw new InvalidOperationException("图片没有导入：" + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static void LoadSprites()
    {
        sprites.Clear();
        foreach (string name in SpriteNames)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath(name));
            if (sprite == null) throw new InvalidOperationException("图片没有按 Sprite 导入：" + SpritePath(name));
            sprites[name] = sprite;
        }
    }

    private static void DrawSprite(string name, string path)
    {
        switch (name)
        {
            // 拱形卡片：上半圆 + 下方圆角矩形。底板带上亮下暗的渐变，边框是一粗一细两道线。
            case "CardBody":
                Draw(path, 680, 1120, (x, y) => Gray(Mathf.Lerp(1f, 0.72f, y / 1120f), Cover(ArchSdf(x, y, 680, 1120, 72) + 10)));
                break;
            case "CardBorder":
                Draw(path, 680, 1120, (x, y) =>
                {
                    float d = ArchSdf(x, y, 680, 1120, 72);
                    return White(Mathf.Max(Band(d, 0, 8), Band(d, 18, 3)));
                });
                break;
            // 菱形徽记：外框、半透明底、内框。
            case "Emblem":
                Draw(path, 256, 256, (x, y) =>
                {
                    float d = DiamondSdf(x - 128, y - 128, 120);
                    return White(Mathf.Max(Mathf.Max(Band(d, 0, 8), Cover(d + 8) * 0.3f), Band(d, 26, 4) * 0.9f));
                });
                break;
            // 默认图标：四芒星叠一个小的斜向四芒星。BuffConfig 还没有图标字段，先用它占位。
            case "Star":
                Vector2[] big = StarPoints(76, 15, 0f), small = StarPoints(42, 12, Mathf.PI / 4f);
                Draw(path, 160, 160, (x, y) => White(Mathf.Max(Cover(PolygonSdf(x - 80, y - 80, big)), Cover(PolygonSdf(x - 80, y - 80, small)))));
                break;
            // 分隔线：两端渐隐的横线，中间一个菱形，两侧各一个小菱形。
            case "Divider":
                Draw(path, 520, 40, (x, y) =>
                {
                    float fade = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(x - 260) / 260f), 0.8f);
                    float line = Mathf.Clamp01(2f - Mathf.Abs(y - 20)) * fade;
                    float center = Cover(DiamondSdf(x - 260, y - 20, 13));
                    float sides = Mathf.Max(Cover(DiamondSdf(x - 214, y - 20, 6)), Cover(DiamondSdf(x - 306, y - 20, 6)));
                    return White(Mathf.Max(line, Mathf.Max(center, sides)));
                });
                break;
            // 标题牌：两端尖的长六边形。
            case "TitlePlate":
                Draw(path, 1360, 192, (x, y) => Gray(Mathf.Lerp(1f, 0.8f, y / 192f), Cover(PlateSdf(x, y, 1360, 192) + 8)));
                break;
            case "TitleBorder":
                Draw(path, 1360, 192, (x, y) =>
                {
                    float d = PlateSdf(x, y, 1360, 192);
                    return White(Mathf.Max(Band(d, 0, 6), Band(d, 16, 2.5f)));
                });
                break;
            case "Disk":
                Draw(path, 144, 144, (x, y) => White(Cover(Length(x - 72, y - 72) - 62)));
                break;
            case "Ring":
                Draw(path, 144, 144, (x, y) => White(Band(Length(x - 72, y - 72) - 68, 0, 5)));
                break;
            default:
                throw new InvalidOperationException("未知图片：" + name);
        }
    }

    // pixel(x, y)：x、y 是距左上角的像素中心坐标。
    private static void Draw(string path, int width, int height, Func<float, float, Color> pixel)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var data = new Color32[width * height];
        for (int row = 0; row < height; row++)
        {
            float y = height - 1 - row + 0.5f; // 纹理第 0 行在底部
            for (int col = 0; col < width; col++)
                data[row * width + col] = pixel(col + 0.5f, y);
        }
        tex.SetPixels32(data);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
    }

    private static Color White(float alpha) => new Color(1, 1, 1, alpha);
    private static Color Gray(float value, float alpha) => new Color(value, value, value, alpha);
    private static float Length(float x, float y) => Mathf.Sqrt(x * x + y * y);

    // d 为有向距离（像素，内部为负）。Cover 是填充的覆盖率，Band 是从边缘向内 from 处开始、宽 thickness 的线。
    private static float Cover(float d) => Mathf.Clamp01(0.5f - d);
    private static float Band(float d, float from, float thickness) =>
        Mathf.Clamp01(Mathf.Min(0.5f - (d + from), 0.5f + (d + from + thickness)));

    private static float RoundBoxSdf(float x, float y, float cx, float cy, float hx, float hy, float radius)
    {
        float qx = Mathf.Abs(x - cx) - (hx - radius);
        float qy = Mathf.Abs(y - cy) - (hy - radius);
        return Length(Mathf.Max(qx, 0), Mathf.Max(qy, 0)) + Mathf.Min(Mathf.Max(qx, qy), 0) - radius;
    }

    // 上半圆（半径为宽的一半）与下方圆角矩形的并集。矩形从圆心上方 radius 处开始，把它上面的圆角藏进半圆里，侧边与圆弧相切。
    private static float ArchSdf(float x, float y, float width, float height, float bottomRadius)
    {
        float r = width * 0.5f;
        float disk = Length(x - r, y - r) - r;
        float top = r - bottomRadius;
        float box = RoundBoxSdf(x, y, r, (top + height) * 0.5f, r, (height - top) * 0.5f, bottomRadius);
        return Mathf.Min(disk, box);
    }

    // 旋转 45° 的正方形，halfDiagonal 为中心到顶点的距离。
    private static float DiamondSdf(float x, float y, float halfDiagonal) =>
        (Mathf.Abs(x) + Mathf.Abs(y) - halfDiagonal) / 1.41421356f;

    // 上下平、左右尖的长六边形，四周留 10 像素边距。
    private static float PlateSdf(float x, float y, float width, float height)
    {
        float hw = width * 0.5f - 10, hh = height * 0.5f - 10, tip = 70;
        float px = Mathf.Abs(x - width * 0.5f), py = Mathf.Abs(y - height * 0.5f);
        float slope = tip / hh;
        return Mathf.Max(py - hh, (px + py * slope - hw) / Mathf.Sqrt(1 + slope * slope));
    }

    // 四芒星的 8 个顶点：尖角在 outer 半径上，凹点在 inner 半径上，rotation 为第一个尖角的方向。
    private static Vector2[] StarPoints(float outer, float inner, float rotation)
    {
        var points = new Vector2[8];
        for (int i = 0; i < 8; i++)
        {
            float angle = rotation + i * Mathf.PI / 4f;
            float radius = i % 2 == 0 ? outer : inner;
            points[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }
        return points;
    }

    // 任意简单多边形的有向距离：到各边的最近距离，内外由穿越次数决定。
    private static float PolygonSdf(float px, float py, Vector2[] v)
    {
        var p = new Vector2(px, py);
        float d = Vector2.Dot(p - v[0], p - v[0]);
        float sign = 1;
        for (int i = 0, j = v.Length - 1; i < v.Length; j = i, i++)
        {
            Vector2 e = v[j] - v[i];
            Vector2 w = p - v[i];
            Vector2 b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
            d = Mathf.Min(d, Vector2.Dot(b, b));
            bool c1 = p.y >= v[i].y, c2 = p.y < v[j].y, c3 = e.x * w.y > e.y * w.x;
            if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) sign = -sign;
        }
        return sign * Mathf.Sqrt(d);
    }

    // ======================================================================
    // BuffOption：一张拱形卡片
    // ======================================================================

    private static void BuildOption()
    {
        var root = new GameObject("BuffOption", typeof(RectTransform), typeof(Image), typeof(Button));
        try
        {
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = CardSize;
            // 根物体是卡片底板，接收点击；边框画在它上面，悬停时由按钮提亮。
            var body = root.GetComponent<Image>();
            body.sprite = sprites["CardBody"];
            body.color = Navy;

            var border = NewImage(rt, "Border", sprites["CardBorder"], Gold);
            Stretch(border.rectTransform);

            var emblem = NewImage(rt, "Emblem", sprites["Emblem"], Gold);
            TopCenter(emblem.rectTransform, 64, 128, 128);
            var icon = NewImage(emblem.rectTransform, "Icon", sprites["Star"], StarColor);
            icon.preserveAspect = true;
            Center(icon.rectTransform, 0, 0, 80, 80);

            var nameText = NewText(rt, "NameText", "Buff 名称", 36, TitleColor, TextAlignmentOptions.Center);
            TopCenter(nameText.rectTransform, 214, 300, 50);

            var divider = NewImage(rt, "Divider", sprites["Divider"], GoldSoft);
            TopCenter(divider.rectTransform, 272, 260, 20);

            var desc = NewText(rt, "DescText", "Buff 描述", 24, DescColor, TextAlignmentOptions.Top);
            TopCenter(desc.rectTransform, 306, 272, 176);
            desc.textWrappingMode = TextWrappingModes.Normal;
            desc.lineSpacing = 20;

            var footer = NewImage(rt, "FooterEmblem", sprites["Emblem"], GoldSoft);
            TopCenter(footer.rectTransform, 494, 40, 40);

            SetupButton(root.GetComponent<Button>(), border);
            PrefabUtility.SaveAsPrefabAsset(root, OptionPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    // ======================================================================
    // BuffPanel：全屏遮罩 + 标题 + 卡片列表 + 空列表提示 + 关闭按钮
    // ======================================================================

    private static void BuildPanel()
    {
        var root = new GameObject("BuffPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        try
        {
            var rt = root.GetComponent<RectTransform>();
            Stretch(rt);
            root.GetComponent<Image>().color = ScreenDim; // 同时挡住面板下方的点击

            // 标题区：上下各一条分隔线，中间是标题牌
            var titleArea = NewRect(rt, "TitleArea");
            TopCenter(titleArea, 40, 680, 150);
            var topDivider = NewImage(titleArea, "TopDivider", sprites["Divider"], GoldSoft);
            TopCenter(topDivider.rectTransform, 0, 360, 20);
            var plate = NewImage(titleArea, "TitlePlate", sprites["TitlePlate"], Navy);
            TopCenter(plate.rectTransform, 27, 680, 96);
            var plateBorder = NewImage(plate.rectTransform, "TitleBorder", sprites["TitleBorder"], Gold);
            Stretch(plateBorder.rectTransform);
            var title = NewText(plate.rectTransform, "TitleText", "选择一道星辉", 40, TitleColor, TextAlignmentOptions.Center);
            Stretch(title.rectTransform);
            var bottomDivider = NewImage(titleArea, "BottomDivider", sprites["Divider"], GoldSoft);
            TopCenter(bottomDivider.rectTransform, 130, 360, 20);

            var chance = NewText(rt, "ChanceText", "剩余选择次数 1", 24, MutedColor, TextAlignmentOptions.Center);
            TopCenter(chance.rectTransform, 196, 400, 34);

            // 卡片列表：选项预制体放在这里，HorizontalLayoutGroup 按数量居中排列（最多 3 张）
            var list = NewRect(rt, "OptionList");
            Center(list, 0, -50, 3 * CardSize.x + 2 * CardSpacing + 80, CardSize.y + 40);
            var layout = list.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = CardSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // 右上角关闭按钮
            var close = NewImage(rt, "CloseButton", sprites["Disk"], Navy);
            close.raycastTarget = true; // 按钮靠这张图接收点击
            TopRight(close.rectTransform, 56, 48, 72, 72);
            var ring = NewImage(close.rectTransform, "Ring", sprites["Ring"], Gold);
            Stretch(ring.rectTransform);
            var closeText = NewText(close.rectTransform, "CloseText", "×", 48, TitleColor, TextAlignmentOptions.Center);
            Stretch(closeText.rectTransform);
            SetupButton(close.gameObject.AddComponent<Button>(), ring);

            PrefabUtility.SaveAsPrefabAsset(root, PanelPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    // 第一版预制件里有 EmptyHint（候选为 0 时的提示）。只删掉这一个物体，预制件上的其他手动调整保留。
    private static void RemoveEmptyHint()
    {
        if (!File.Exists(PanelPath)) return;
        GameObject prefab = PrefabUtility.LoadPrefabContents(PanelPath);
        try
        {
            Transform hint = prefab.transform.Find("EmptyHint");
            if (hint == null) return;
            UnityEngine.Object.DestroyImmediate(hint.gameObject);
            PrefabUtility.SaveAsPrefabAsset(prefab, PanelPath);
            Debug.Log("[BuffPanelSetup] 已从 BuffPanel 预制件删除 EmptyHint。");
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
    }

    // 面板打开时 timeScale 为 0，Selectable 的颜色过渡本身不受 timeScale 影响。
    // 选中状态用普通颜色：点过的卡片不会一直保持高亮。
    private static void SetupButton(Button button, Graphic target)
    {
        button.targetGraphic = target;
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.62f, 0.62f, 0.62f, 1f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
        colors.colorMultiplier = 1;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }

    // ======================================================================
    // 预览：在隐藏的预览场景里渲染 3 张、2 张、1 张三种情况，不改动当前场景
    // ======================================================================

    public static void RenderPreviews()
    {
        LoadFont();
        var panelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PanelPath);
        var optionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(OptionPath);
        if (panelPrefab == null || optionPrefab == null) throw new InvalidOperationException("预制件不存在，先生成。");

        var samples = new List<(string name, string desc)>();
        var database = AssetDatabase.LoadAssetAtPath<BuffDatabase>("Assets/Resources/GameData/Buff/BuffDatabase.asset");
        if (database != null && database.buffs != null)
            foreach (BuffConfig config in database.buffs)
                if (config != null) samples.Add((config.buffName, config.buffDesc));
        while (samples.Count < 3) samples.Add(("示例 Buff", "这里显示 Buff 的描述，文字超过一行时自动换行。"));

        var report = new List<string>();
        RenderPreview(panelPrefab, optionPrefab, samples.GetRange(0, 3), "BuffPanel-preview-3.png", report);
        RenderPreview(panelPrefab, optionPrefab, samples.GetRange(0, 2), "BuffPanel-preview-2.png", report);
        RenderPreview(panelPrefab, optionPrefab, samples.GetRange(0, 1), "BuffPanel-preview-1.png", report);
        File.WriteAllText(ReportDir + "/report.txt", $"{DateTime.Now:O}\n" + string.Join("\n", report) + "\n");
        Debug.Log("[BuffPanelSetup] 预览已输出到 " + ReportDir);
    }

    private static void RenderPreview(GameObject panelPrefab, GameObject optionPrefab, List<(string name, string desc)> cards, string file, List<string> report)
    {
        Scene scene = EditorSceneManager.NewPreviewScene();
        var rtex = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        Camera cam = null;
        try
        {
            var camGo = new GameObject("PreviewCamera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(camGo, scene);
            cam = camGo.GetComponent<Camera>();
            cam.scene = scene;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.30f, 0.36f, 0.42f, 1f); // 代替游戏画面
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
            Transform list = panel.transform.Find("OptionList");
            foreach ((string name, string desc) in cards)
            {
                var card = (GameObject)UnityEngine.Object.Instantiate(optionPrefab, list, false);
                card.transform.Find("NameText").GetComponent<TMP_Text>().text = name;
                card.transform.Find("DescText").GetComponent<TMP_Text>().text = desc;
            }
            panel.transform.Find("ChanceText").GetComponent<TMP_Text>().text = "剩余选择次数 1";

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)list);
            Canvas.ForceUpdateCanvases();
            var overflow = new List<string>();
            foreach (TMP_Text text in panel.GetComponentsInChildren<TMP_Text>(false))
            {
                text.ForceMeshUpdate(true, true);
                if (text.isTextOverflowing || text.isTextTruncated) overflow.Add(text.name + "「" + text.text + "」");
            }
            Canvas.ForceUpdateCanvases();
            cam.Render();

            var prev = RenderTexture.active;
            RenderTexture.active = rtex;
            var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            Directory.CreateDirectory(ReportDir);
            File.WriteAllBytes(ReportDir + "/" + file, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            var positions = new List<string>();
            foreach (Transform card in list)
                positions.Add(((RectTransform)card).anchoredPosition.x.ToString("F0"));
            report.Add($"{file}: 卡片 {cards.Count} 张，横向位置 [{string.Join(", ", positions)}]，" +
                       (overflow.Count == 0 ? "文字都放得下" : "放不下：" + string.Join("，", overflow)));
        }
        finally
        {
            if (cam != null) cam.targetTexture = null; // 先解除绑定，避免释放 RenderTexture 时报错
            rtex.Release();
            UnityEngine.Object.DestroyImmediate(rtex);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    // ======================================================================
    // 工具方法
    // ======================================================================

    private static RectTransform NewRect(Transform parent, string name)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    private static Image NewImage(Transform parent, string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
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

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static void TopCenter(RectTransform rt, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -y);
        rt.sizeDelta = new Vector2(w, h);
    }

    // right：右边缘距父物体右侧的距离
    private static void TopRight(RectTransform rt, float right, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-right, -y);
        rt.sizeDelta = new Vector2(w, h);
    }

    private static void Center(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }
}
#endif
