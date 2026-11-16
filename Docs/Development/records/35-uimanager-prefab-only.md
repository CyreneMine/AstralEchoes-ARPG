# UIManager 恢复统一由预制体创建

2026-11-16。代码提交为 `c166a09`，场景提交为 `d345388`。涉及 UIManager、GMScene，以及本地预制件 `Resources/UI/Canvas.prefab`。辅助工具为 Claude Code（下文写作 Claude）。对应 #4，也是[记录 29](29-console-support-code-removal.md)中撤回清单的第 4 项。

## 用户观察与目标

撤回清单第 4 项写的是"调试完成后整理 UIManager 的 Canvas 和面板创建方式"。用户要求 Claude 只说明需要修复什么，不提供思路，方案由用户自己设计。

## 来历（Claude 根据提交历史与记录核对）

- 10-15，用户写的 UIManager 只有一条路：Canvas 和所有面板都通过 `Resources.Load` 加载预制件后实例化。
- 10-29，Codex 按用户委托，直接在 GMScene 的 Canvas 里搭建了任务面板 UI，没有做成预制件（见[记录 19](19-mission-panel-ui.md)）。
- 10-30，Codex 做控制台和 HUD 时发现"场景里已经有 HUD，但 UIManager 总是另外创建一份 Canvas"，于是在 `ffd3bad` 中修改了 UIManager，让它优先接管场景里已有的 Canvas 和面板（见[记录 23](23-dev-console-and-player-hud.md)）。
- 11-10 撤回 Codex 的配套代码时，用户决定先保留 UIManager 的这部分改动，等调试结束后再处理。

由此可见，这一项不是用户原有设计的缺陷，而是清理辅助工具为迁就调试期临时场景而对框架代码做的修改。

## Claude 列出的现状问题

1. **两条创建路径并存**：同一个面板，有时来自场景，有时来自预制件，前后两次的来源可能不同。
2. **场景上的覆盖值只在第一次生效**：`HidePanel` 会销毁面板，下次打开改为从预制件创建。用户在场景里对 MissionPanel 做的调整、GamePanel 在场景里的覆盖值，第二次打开时都会丢失。
3. **嵌套的 GamePanel**：`Canvas.prefab` 里嵌套了一个 GamePanel。即使删掉场景里的 Canvas，GamePanel 仍然会走"接管"的路径。
4. **淡出期间重新打开会丢失面板**：面板淡出期间调用 `ShowPanel`，会拿到这个正在淡出的旧面板，淡出结束后它被销毁，面板就没了。
5. **跨场景可能出现两个 Canvas**：被接管的场景 Canvas 设置了 `DontDestroyOnLoad`，以后切换场景时，新场景里的 Canvas 和旧的会同时存在。
6. **HUD 每次都被销毁重建**。

## 用户提出的想法与决定

- **统一管理**：所有 UI 都由 UIMgr 统一管理，创建、隐藏、删除都走同一处。子控件由各面板自己的脚本管理，世界空间的 UI 跟随所属对象。用户指出，从项目现有代码也能看出这一点。
- **场景里的 Canvas 是临时的**：场景中的 Canvas 及其子对象都是调试期临时放的，用户已经删除。正式流程中，GamePanel 会在"开始场景 → 游戏场景"的过程中创建；DevConsole 只用于 GMScene，以后会删除，用户也考虑另做一个 GMScene 专用面板。
- **绘制顺序不需要分层**：设计上只会在一个主面板之上再打开功能面板。DialoguePanel 打开 OptionsPanel 这种情况，两者在屏幕上的位置本来就不重叠。
- **关闭时的处理**：按现有写法直接销毁，以后有需要再改成隐藏。
- **淡出窗口**：窗口只有约 0.2 秒，正常操作碰不到，暂不处理，只做记录。
- **判空**：不无脑判空，靠各环节逻辑衔接严密来避免空引用。
- **场景切换**：以后在开始界面完成选角，进入游戏场景时用代码生成对应角色。
- **实施**：用户先删掉 `ShowPanel` 里的接管代码；按 Claude 的提醒，同时移除了 `Canvas.prefab` 里嵌套的 GamePanel；随后删除构造函数里查找场景 Canvas 的代码。第一次删除时把实例化那一行也删掉了，Claude 审查后指出，用户又补了回来，并加了一个判空。

## 辅助分析与建议

- **企业常见做法**：用一个 UI 管理器统一负责面板的打开、关闭、缓存和销毁，用户的方向与此一致。Claude 起初提醒的几点中，"管理范围"和"绘制顺序"在用户说明设计前提后已不需要考虑，Claude 也承认这两点讲多了。
- **删除后的嵌套问题**：只删代码而不清理预制件，`Canvas.prefab` 里嵌套的 GamePanel 仍会生成，UIMgr 又会再建一个，界面上就会出现两个 HUD。
- **构造函数遗留**：构造函数里查找场景 Canvas 的代码，以后会把场景中现成的 Overlay Canvas 带进 `DontDestroyOnLoad`，所以建议一并删除。
- **误删的一行**：第一次删除时连实例化那一行也删了，`canvas` 为 null，构造函数会抛出异常。由于 `BaseManager` 用反射构造，此后每次访问 `UIManager.Instance` 都会重新抛出。
- **判空的效果**：构造函数里新加的判空条件永远成立，不会出错，但效果等同于没加。资源缺失属于"按设计必须存在"的情况，直接报错即可。
- **以后的时序**：GamePanel 的血条会在 `Start` 里读取玩家，所以进入游戏场景时要先生成玩家，再创建 GamePanel。

## 实际采纳与当前文件依据

- `UIManager` 的构造函数实例化 `UI/Canvas` 并调用 `DontDestroyOnLoad`；`ShowPanel` 只从 `Resources/UI/{类型名}` 的预制件创建面板；`HidePanel` 关闭时销毁面板。
- GMScene 删除了调试用的 Canvas（其中含 GamePanel，以及另放的 MissionPanel 实例），同时保存了用户调试时对玩家位置的微调。
- `Canvas.prefab` 只保留容器、EventSystem 和 DevConsole。这个文件在被忽略的 `Resources/UI/` 目录里，修改只保存在本地。
- 未处理：淡出窗口问题已转为 Issue；构造函数中的判空保留。

## 验证与当前状态

- **用户实测（11-16）**：HUD 正常显示，场景里只有一个 Canvas，任务面板打开、关闭后 HUD 能恢复，对话和选项正常，F1 能打开控制台。
- **静态核验**：Claude 审查了 UIManager 的三次修改，并核对了 `Canvas.prefab` 里已不再有嵌套的预制件。
- **未覆盖**：场景切换后的 Canvas 和面板（#17）；淡出期间重新打开同一面板的情况。

## 论文素材与证据入口

可以作为"UI 管理器统一创建与销毁面板"的设计说明，也可以作为"调试期的临时搭建不应反向改动框架"的过程案例。UI 管理器是通用做法，不应写成原创技术。

- [UIManager](../../../Assets/Scripts/Managers/UIManager.cs)、[BasePanel](../../../Assets/Scripts/Base/BasePanel.cs)、[GMScene](../../../Assets/Scenes/GMScene.unity)
