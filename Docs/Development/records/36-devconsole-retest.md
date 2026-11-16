# 控制台复测与自动检查扩充

2026-11-16。控制台修改提交为 `9209b36`，自动检查提交为 `9dbf9b7`，涉及 `DevConsole.cs` 和 `Assets/Editor/DevConsoleSetup.cs`。这两个文件都由 Claude Code（下文写作 Claude）维护，本轮没有改动用户的玩法脚本。对应 #5，也就是[记录 29](29-console-support-code-removal.md) 撤回清单里的第 5 项。

## 用户观察与目标

撤回清单的前四项完成后，最后一项是复测控制台：在 Unity 中验证扣血、回血、属性修改、复活、清空任务存档和界面开关。用户在 11-15 已明确，控制台由 Claude 负责，代码和 UI 都可以修改，只要不动用户自己的代码；控制台只是用来验证用户写的接口是否可行。

## 用户提出的想法

- 用户询问 Claude 能否直接操作 Unity 来复测，并在桌面应用里打开了"计算机使用"。但当前会话里没有出现计算机使用的工具，Claude 说明了这一点；还提醒即使能用，Unity 可能被归为 IDE，只允许点击、不能按键。之后用户选择 Claude 提出的方案：扩充自动检查，由用户点一次菜单，Claude 读取报告。
- 手动复测后，用户认为"控制台与任务面板、对话同时使用"这一项的问题容易解决，但没有必要：正式游戏中打不开控制台，它只用于调试。因此这一项按通过处理，不修改。

## 辅助分析与建议

**静态核对**：`Canvas.prefab` 中 21 个按钮的持久回调方法名在 `DevConsole` 里都还存在。

**发现的冲突**：控制台原来自己设置 `Cursor`，并直接禁用 `CinemachineInputAxisController`。这和用户写的光标模式（`PlayerMgr.SetCursorState`）、`CameraCursorLock` 是两套机制。控制台打开时，如果按下再松开 Alt，光标会被锁住、镜头恢复转动，控制台就无法点击。Claude 把控制台改为：

- 打开时调用用户的 `SetCursorState(true)`；
- 打开期间如果光标模式被退出，下一帧重新进入；
- 关闭时恢复打开前的光标模式；
- 移动和攻击仍然通过暂停玩家组件来禁止。

这样也消除了 #5 中记录的"控制台与 CameraCursorLock 同时开关镜头"的冲突。

**自动检查扩充**：`Tools → AstralEchoes → Dev Console → Verify in Play Mode` 由原来的 9 项、遇到失败就中断，改为逐项记录通过和失败，共 49 项。覆盖以下内容：

- 开关后的光标模式、镜头、玩家组件；
- 模拟松开 Alt 后能重新进入光标模式；
- 各生命按钮的数值和 `OnHealthChange` 的触发；
- 死亡广播、Animator 的 `IsDead`、怪物脱战，以及死亡时拒绝其他操作；
- 复活广播；存活时点复活不发广播；
- 应用属性、六个 ± 按钮与 `GetAtkValue` 是否同步；
- 恢复初始值后回到当前等级的数值；
- 共享 SO 的引用未被替换；
- 关闭后恢复原状。

**第一次运行结果**：48 项通过、1 项失败，原因都在检查脚本本身。

- 场景中名为 `EnemyObj` 的怪物，其 EnemyAI 组件被禁用，巡逻点数量为 0，是一个静止木桩。禁用的组件不会在 `OnEnable` 中订阅死亡事件，本来就不参与脱战。检查改为只统计启用中的 EnemyAI，并在报告中注明被禁用的数量。
- `ScreenCapture` 不会自动创建目录，截图保存失败。改为截图前先创建目录。

## 实际采纳与当前文件依据

- 控制台改为依赖用户接口：光标模式用 `PlayerMgr.SetCursorState` / `CurrentIsCursor`，死亡和复活用 `CharacterHealth.Dead` / `Respawn`，属性用 `CharacterHealth.UpdateStats`。
- 控制台通过反射访问的字段有：`CharacterHealth` 的 `currentHp`、`OnHealthChange`，`CharacterStats` 的 `atk`、`def`、`maxHp`。
- 未处理：控制台与任务面板、对话同时使用时的问题，用户决定不处理，具体现象未记录。

## 验证与当前状态

- **自动检查（11-16 第二次运行，用户触发）**：49 项通过，0 项失败。开始时为 4 级，攻击 8、防御 6、生命 130。截图中状态栏与 HUD 的等级、生命一致。报告和截图保存在 `Temp/DevConsoleReports/`，不进入仓库。
- **用户手动实测**：实际按键（F1、Alt、Esc、WASD）、修改攻击后打怪的伤害、清空任务存档，都正常。
- **已知未处理**：与其他面板同时使用，原因见上，属于用户决定。

## 论文素材与证据入口

可以说明调试工具如何通过公开接口和反射验证玩法接口而不侵入玩法代码，以及如何把手动清单改写为可重复运行的 Play Mode 自动检查。检查结果只代表当时的版本，不能当作最终验收。

- [DevConsole](../../../Assets/Scripts/UI/DevConsole.cs)、[DevConsoleSetup](../../../Assets/Editor/DevConsoleSetup.cs)
