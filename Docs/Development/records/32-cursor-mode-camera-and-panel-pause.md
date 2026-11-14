# Alt 光标模式、镜头锁定与任务面板暂停

2026-11-14，代码提交 `2b95693`，场景提交 `41c9683`。涉及 PlayerMgr、GamePanel、PlayerCombat、DialoguePanel、MissionPanel、BasePanel、Main 和新增的 CameraCursorLock。辅助工具为 Claude Code（下文写作 Claude）。

## 用户观察与目标

1. **死亡流程的讨论**：准备处理 #2“死亡后禁止移动、闪避和攻击”时，用户质疑这一项是否有意义，由此引出“面板打开时游戏是否暂停”的问题。
2. **左键误触发攻击**：按住 Alt 显示光标后点击 UI 上的任务按钮，左键每次都会让角色出一刀。
3. **镜头跟着转**：按住 Alt 移动鼠标时，镜头仍然会转动。用户在回复中确认了这一现象。

目标：按住 Alt 时只操作 UI，不出刀、不转镜头；打开任务面板期间不会被敌人打死。

## 用户提出的想法

按讨论顺序记录：

1. **对 #2 的质疑**：用户认为，血量归零后一般会显示死亡界面，或者直接传送回存档点。死亡界面本来就要切断输入，传送时直接开始加载，所以不需要专门限制操作。
2. **反驳 Claude 的例子**：Claude 举了“死后打开又关掉任务面板，GamePanel 会重新打开输入”的例子。用户指出，这是因为功能还没做完，做完以后死亡时就打不开其他面板了。
3. **担心 timeScale 卡住淡入**：讨论“打开面板时暂停游戏”时，用户问 `timeScale = 0` 会不会让面板的淡入卡在半路。
4. **实现暂停**：用户把 BasePanel 的淡入淡出改为不受缩放的时间，在 MissionPanel 显示时设置 `Time.timeScale = 0`，关闭时用 `+=` 在回调里恢复为 1，并报告实测可行。
5. **左键误触发的第一个想法**：用户最初想在按住 Alt 时关闭鼠标监听，随即自己否定了：关掉鼠标监听，光标也动不了。用户把它修正为只关闭判定攻击的那部分鼠标监听。
6. **选择写法 B**：用户选择在使用处检查状态的写法 B，并问镜头转动能否一起解决。
7. **对事件的疑问**：用户先问“参数是新状态”是什么意思，是不是应该是返回值；随后请 Claude 结合自己的代码解释事件有什么用。理解后，用户总结为：需要在光标变化时做操作的一方，自己订阅 `OnCursorChanged`，在订阅的方法里写自己的操作。
8. **第一版实现**：用 `CurrentIsCursor => Cursor.visible` 作为状态，GamePanel 中按下和松开 Alt 时传入的 true/false 写反了，结果很奇怪。
9. **第一次按 Alt 无效**：参数改回来以后，第一次按 Alt 没有效果。用户先在 Main 里补了 `Cursor.visible = false`，但没有解决；改成 PlayerMgr 自己的私有字段 `isCursor` 后恢复正常。之后用户追问为什么只在 Main 里设置不行。
10. **攻击检查**：在 `PlayerCombat.Update` 中加入 `CurrentIsCursor` 判断，用户报告 4 步自测正常。
11. **改用面板关闭时退出光标模式**：Claude 根据代码推断，“按住 Alt 点任务按钮、在面板里松开”这条路径会让光标模式残留。用户没有采用 Claude 建议的“打开面板前退出”，而是在 MissionPanel 关闭的回调里调用 `SetCursorState(false)`。
12. **修复对话光标**：Claude 提示光标默认隐藏后，对话时可能看不到光标。用户实测确认后，将 DialoguePanel 显示时改为 `SetCursorState(true)`；审查后又将结束回调改为 `SetCursorState(false)`。
13. **实现 CameraCursorLock**：用户按 Claude 的讲解编写了这个脚本，审查后挂到 FreeLook Camera 上，并保存了场景。
14. **请求多提点**：用户表示事件订阅以前用得少，这次帮助很大，请 Claude 在后续开发中多提醒适合使用的地方。

## 辅助分析与建议

Claude 依据当次源码和 GMScene 序列化内容进行分析。

**#2 的意义。** Claude 认为，死亡在正式项目中通常作为角色的一个状态，由这个状态负责锁住玩法输入，死亡界面和传送是后续环节。理由有三点：

- 血量归零到界面出现之间有一段时间，例如死亡动画；
- 当前项目完全没有死亡流程：`PlayerObject` 不检查死亡状态，`EnemyAI` 也不检查玩家是否死亡；
- 当前项目有多处代码在开关输入，`GamePanel.ShowMe()` 和 `DialoguePanel` 结束时会把整个输入资源打开，最后关闭的面板会重新打开输入。

用户提出“做完后死了就打不开面板”后，Claude 补充了“死亡时面板已经开着”的情况：游戏没有暂停，开着任务面板或对话时可能被打死。并给出三种可以组合的方案：死亡时强制关闭所有面板、打开面板时暂停游戏、由死亡状态自己锁住操作。

建议把 #2 重新表述为“玩家死亡流程的最小版本”。#2 本身尚未实施。

**timeScale 与淡入。** BasePanel 原来使用 `Time.deltaTime`，暂停后淡入会停在透明，淡出的回调也永远不会执行。通常的做法是 UI 使用不受缩放的时间。同时提醒：`timeScale` 是全局开关，会影响物理、Animator 和 `Time.time`，并且有多个来源同时暂停时如何恢复的问题。另一种方案是只暂停敌人。

**左键误触发。** 推荐把按住 Alt 视为“光标模式”，在这个模式下关闭会冲突的玩法动作。另一种方案是用 `EventSystem.current.IsPointerOverGameObject()` 判断指针是否在 UI 上，更适合光标一直显示的游戏。实现上给出两种写法：

- **写法 A**：按住 Alt 时关闭 `Player.Attack`。缺点是 `GamePanel.ShowMe()` 等处整体 `Enable()` 输入时，会把它重新打开。
- **写法 B**：攻击时检查一个明确的光标模式标记，不直接拿 `Cursor.lockState` 当作状态。

**镜头。** GMScene 中 FreeLook Camera 的 `CinemachineInputAxisController` 直接引用 `InputSystem_Actions.inputactions` 资源里的 Look 动作，并开启了 `AutoEnableInputs`。这和 PlayerMgr 里 `new InputSystem_Actions()` 创建的不是同一个实例，所以写法 B 和关闭 PlayerMgr 的输入都管不到镜头。

Claude 建议由 PlayerMgr 保存唯一的光标模式状态，自己的代码在使用处读取，无法修改的第三方组件订阅变化通知后自行开关。状态放在 PlayerMgr，因为它是全局输入状态，不属于某个角色；不放在 GamePanel，因为面板会被销毁重建。同时提醒，离开 HUD 时要退出光标模式。

**事件的作用。** Claude 结合用户代码说明：

- 攻击每帧都会运行，读取状态就够了；
- 镜头组件不是自己的代码，需要有人来通知它；
- 如果由 `SetCursorState` 直接开关镜头，PlayerMgr 跨场景存在而镜头随场景销毁，会出现和血条第一版相同的依赖问题；
- 事件数据通过参数从发送方传给订阅方，不用返回值。

**排查实现问题。**

- true/false 写反时，Claude 推算了逐步现象。
- `Cursor.visible` 默认是 true，导致第一次按 Alt 被判断为“值没变”。建议改用私有字段。
- 只在 Main 里设置 visible 不起作用，Claude 推测是编辑器在焦点变化时重置了光标，但没有实际验证。结论是：不要用引擎或系统的状态充当游戏逻辑的状态。

**审查发现的问题。**

- 光标默认隐藏后，对话只解锁不显示光标，可能导致选项按钮看不到光标（用户已确认并修复）。
- 对话结束时没有退出光标模式，会导致结束后无法攻击、镜头不能转（已修复）。
- CameraCursorLock 没有保存进场景（已修复）。

**CameraCursorLock 的结构。** Claude 讲解了这个脚本的结构，并给出了示意代码：

- Awake 中获取组件；
- OnEnable 中先拉取一次状态，再订阅；
- OnDisable 中取消订阅；
- 处理方法设置 `enabled = !isCursor`。

同时提醒，要关闭的是 Cinemachine 组件，而不是自己的脚本或整个物体。

**观察者模式。** 应用户提问，Claude 介绍了企业项目中常见的几种事件形式：

- C# event；
- UnityEvent；
- 全局事件中心；
- ScriptableObject 事件频道；
- UniRx/R3 等响应式库。

也说明了事件的代价：流程难追踪、容易忘记取消订阅、执行顺序不可靠、可能连锁触发，以及什么情况下更适合直接调用或每帧读取。行业里没有统一选择。

## 实际采纳与用户调整

| 内容 | 采纳情况 |
|---|---|
| UI 使用不受缩放的时间 | 用户采用，BasePanel 两处改为 `Time.unscaledDeltaTime` |
| 打开任务面板时暂停 | 用户自己选择并实现：显示时 `timeScale = 0`，关闭淡出回调中恢复为 1 |
| 写法 B | 用户采用，`PlayerCombat.Update` 中检查 `canInput` 和 `CurrentIsCursor` |
| 光标模式状态 | 由 PlayerMgr 的私有字段 `isCursor` 保存，只读属性 `CurrentIsCursor`，事件 `OnCursorChanged`（`UnityAction<bool>`），方法 `SetCursorState` 统一设置光标并触发事件 |
| 镜头订阅 | 用户按讲解编写 `CameraCursorLock`（字段名 `ciac`），挂在 FreeLook Camera 上 |
| 退出光标模式的时机 | 用户的设计：在 MissionPanel 关闭回调中退出，而不是 Claude 建议的打开前退出。这样任务面板打开期间一直处于光标模式，镜头保持锁定，比 Claude 的建议更合适 |
| 对话进入光标模式 | 用户采用：显示时 `SetCursorState(true)`，结束回调中 `SetCursorState(false)` |
| Main 开局隐藏光标 | 用户添加 `Cursor.visible = false` |
| 写法 A、`IsPointerOverGameObject`、只暂停敌人、统一管理暂停、死亡流程实现 | 未采用或尚未实施 |

`GamePanel` 中留有一段注释掉的 `OpenCloseMouseInput`，这是用户考虑过把镜头开关放在 GamePanel 的痕迹。Claude 建议放在镜头物体上，用户采纳了。

## 验证与当前状态

**用户声明**
- 任务面板暂停“实测可以实现”。
- 攻击的 4 步自测正常。不过按用户报告时的代码，“在面板里松开 Alt”这一步应当会让光标模式残留；之后改为在关闭面板时退出，用户没有单独报告改后的复测。
- 第一次按 Alt 现已正常。
- 对话看不到光标的问题已修复。
- 镜头部分已完成。

**静态核验**
- 最终代码中，所有改变光标模式的位置都通过 `SetCursorState`：GamePanel 处理 Alt，MissionPanel 关闭时、DialoguePanel 显示和结束时也都走这个方法。
- CameraCursorLock 中，OnEnable/OnDisable 订阅与取消成对，并先拉取一次状态。
- 任务面板只有带淡出的关闭路径，恢复回调一定会执行。
- GMScene 中，FreeLook Camera 已挂载 CameraCursorLock。

**编译**：使用 Unity 生成的 Assembly-CSharp 编译参数，以及编辑器自带的编译器独立编译主脚本，没有编译错误。

**Claude 未进行 Play Mode 测试。** 最终版本建议的回归步骤：

1. 按住 Alt 点击空白处，不出刀；移动鼠标，镜头不转。松开后两者都恢复。
2. 按住 Alt 点任务按钮，在面板里松开 Alt，然后关闭面板。不碰 Alt，直接左键能出刀、镜头能转。
3. 打开任务面板期间被敌人攻击范围覆盖，关闭面板前不会受伤。
4. 对话期间能看到光标，可以点击选项。对话结束后不碰 Alt，能直接攻击、转动镜头。

**已知问题**（转为 Issue 跟踪）：

- 对话期间如果按下又松开 Alt，GamePanel 会调用 `SetCursorState(false)`，对话中途光标会被锁住。
- 暂停关闭时直接写回 `timeScale = 1`，以后有多个来源需要暂停时会互相覆盖。
- 以后如果给任务面板加快捷键，不在光标模式下打开时会看不到光标。
- DevConsole 和 CameraCursorLock 都会开关镜头组件，交错使用时可能冲突。

## 论文素材与证据入口

可以和[记录 31](31-player-health-bar-refresh.md)一起作为观察者模式解耦的案例：

- 一个状态来源，自己的代码在使用处读取，第三方组件订阅通知；
- 引擎状态（`Cursor.visible`）和游戏逻辑状态要分开；
- 暂停与 UI 时间的关系；
- 多个来源同时开关输入或暂停带来的冲突。

事件、观察者模式和 timeScale 暂停都是通用机制，不应写成原创技术。目前没有运行数据，不能写成已测试通过。

- [PlayerMgr](../../../Assets/Scripts/Managers/PlayerMgr.cs)、[CameraCursorLock](../../../Assets/Scripts/Object/Common/CameraCursorLock.cs)、[PlayerCombat](../../../Assets/Scripts/Object/Player/PlayerCombat.cs)
- [GamePanel](../../../Assets/Scripts/UI/GamePanel/GamePanel.cs)、[MissionPanel](../../../Assets/Scripts/UI/GamePanel/MissionPanel.cs)、[DialoguePanel](../../../Assets/Scripts/UI/GamePanel/DialoguePanel.cs)、[BasePanel](../../../Assets/Scripts/Base/BasePanel.cs)、[Main](../../../Assets/Scripts/Main.cs)
- [GMScene](../../../Assets/Scenes/GMScene.unity)：FreeLook Camera 上的 CameraCursorLock
