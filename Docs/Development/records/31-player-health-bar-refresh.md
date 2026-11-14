# 玩家血条刷新：显示时拉取与生命事件

2026-11-14，基于提交 328bf7d 之后的未提交工作区。涉及 CharacterHealth、CharacterObject、PlayerHealth、PlayerHealthBar、GamePanel，以及 MissionPanel 和 UIManager 的面板生命周期。

## 用户观察与目标

用户按 [撤回记录](29-console-support-code-removal.md) 的问题清单顺序处理，先做第 1 项“接回玩家血条刷新”。用户要求 Claude 先读相关代码，不提前给出做法；由用户自己实现，完成后再请 Claude 审查。

目标：玩家血条在初始化和受伤后显示真实血量，并修正 `currentHp / maxHp` 两个 int 相除导致的比例截断。

## 用户提出的想法

按讨论顺序记录：

1. Canvas 以后会动态生成，用户询问如何在不靠 Inspector 挂载的情况下，在代码里找到 GamePanel 下的 PlayerHealthBar。
2. 用户询问玩家当前血量一般放在哪个脚本，最初猜测是 PlayerMgr。
3. 第一版实现：在 GamePanel 里用 `[SerializeField] PlayerHealthBar playerHealthBar` 持有血条，新增 `UpdateHpBar`；在 `PlayerHealth.TakeDamage` 里调用 `UIManager.Instance.GetPanel<GamePanel>().UpdateHpBar(CurrentHp, MaxHp)`；将血条比例改为 `(float)currentHp / (float)maxHp`。
4. 审查指出面板重建后显示默认值。用户分析：面板每次动态生成，没法在面板自身保存数值。随后提出两种候选：存在 Mgr 里，但用户认为 Mgr 不适合保存这类数据；或者面板隐藏时把自己交给触发隐藏的对象，对方再显示它时负责初始化。用户自己认为这两种都不理想，请求标准思路。
5. 用户在 CharacterObject 中新增 `public CharacterHealth Health => health;`，并询问如何让它只读。
6. 用户在 `PlayerHealthBar.Start()` 里通过 `PlayerMgr.Instance.player.Health` 读取当前血量并刷新。第一版另外用了两个临时字段中转，后来删除。
7. 对面板隐藏期间的空引用问题，用户表示不熟悉事件订阅，要求在不改动项目代码的前提下演示。
8. 用户询问为什么不用 Unity 的 `UnityAction`。
9. 用户在子类 `PlayerHealth` 里调用基类事件的 `Invoke` 时遇到编译错误，截图询问原因。
10. 用户猜测，不取消订阅会导致下次重复订阅，同一个方法被执行多次。
11. 最终实现：`CharacterHealth` 声明 `public event UnityAction<int,int> OnHealthChange`，在基类 `TakeDamage` 末尾触发；PlayerHealthBar 在 `Start` 里拉取一次并订阅，在 `OnDestroy` 里判空后取消订阅；PlayerHealth 中直接调用 UI 的那一行改为注释。

## 辅助分析与建议

Claude 依据当次磁盘源码进行分析，涉及 PlayerHealthBar、CharacterHealth、PlayerHealth、CharacterObject、PlayerObject、GamePanel、UIManager、MissionPanel、EnemyCombat、DevConsole，以及 `GamePanel.prefab` 中的序列化引用。

**查找动态生成的子物体。** 同一预制体内部的序列化引用，在 `Instantiate` 后会自动指向新实例，可以在 GamePanel 预制体里拖好；会断开的是场景物体对预制体子物体的引用。外部通过 `UIManager.GetPanel<GamePanel>()` 访问面板。不建议用 `GameObject.Find` 或 `transform.Find` 按名称或路径查找。当时已提出用事件解耦作为推荐方向。

**当前血量的归属。** 实时状态属于角色身上的生命组件，不放在全局 Mgr；需要跨场景时，在保存时把数值写入存档快照。行业没有统一标准，MVC 的 Model 类和 ECS 的 Component 都是同一原则的不同实现。结论：项目保持放在 `CharacterHealth`，不需要迁移。

**第一次审查发现的两个问题。**

| 问题 | 依据 | 后果 |
|---|---|---|
| 面板重建后显示默认值 | MissionPanel 显示时 `HidePanel<GamePanel>()` 销毁面板，关闭时 `ShowPanel<GamePanel>()` 从 Resources 重新生成；预制体默认是 `100/100`、填充 1 | 受伤后打开再关闭任务面板，血条显示满血，要等下次受伤才更新 |
| 面板隐藏期间受伤报空引用 | 面板淡出后从字典移除，`GetPanel` 返回 null；代码中没有设置 `timeScale` 暂停，敌人仍会攻击 | `PlayerHealth.TakeDamage` 抛出 `NullReferenceException`，打断敌人的 `OnAttackCheck` |

**问题 1 的标准思路。** UI 不保存权威数据：显示时向数据来源拉取一次（pull），之后由数据变化时推送（push）。存到 Mgr 会产生两份需要同步的数据；把面板交给触发者会让面板之间互相耦合。另外提出 HUD 常驻、只隐藏不销毁，作为第 4 项整理 UIManager 时的候选，并说明它不能替代“显示时拉取”。

**只读属性。** `=> health` 本身就是只读属性，限制的是引用不能被替换，不是对象本身不能被修改。若要更严格，可以把 `isDead` 改成私有 set 的属性，或对外暴露只读接口；改字段名时要同步 DevConsole 的反射映射。

**问题 2 的方案。** 推荐由生命组件发 C# 事件、UI 订阅，并强调 `+=` 和 `-=` 要在成对的生命周期里出现；备选是调用前判空，并说明对 Unity 对象应使用 `!= null`，不要用 `?.`。

**演示。** 按用户要求，Claude 在工程外编写了独立演示 `G:\毕设\工作记录\事件订阅演示_20261114\`，包含 DemoHealth、DemoHealthBar、HealthEventDemo 三个脚本，演示拉取与订阅、销毁后不报错、忘记取消订阅的后果。演示已用 Unity 6000.4.8f1 自带的编译器编译通过，未在 Play Mode 运行，也未放入工程。

**UnityAction 与 UnityEvent。** `UnityAction` 和 `Action` 都是委托类型，配合 `event` 关键字使用时效果相同。真正需要区分的是 C# `event` 和 `UnityEvent`。后者的优势是在 Inspector 里绑定，但玩家与动态生成的面板之间本来就无法互相拖拽引用，所以选用 C# 事件。

**子类不能触发事件。** `event` 只能在声明它的类里触发。建议在基类改动血量的位置触发；同时指出原调用缺少两个参数，以及 PlayerHealth 中直接调用 UI 的那一行应删除，否则会重复刷新，空引用也仍然存在。

**未取消订阅的后果。** 用户的猜测在“同一个对象反复订阅”时成立，例如在 `OnEnable` 订阅却不在 `OnDisable` 取消。按项目当前“销毁后重建”的流程，主要后果是：调用已销毁的旧血条时访问 Image 或 TMP 抛出 `MissingReferenceException`；按订阅顺序，后订阅的新血条收不到通知；异常会传回 `TakeDamage` 和敌人攻击判定；玩家的事件一直引用旧对象，造成内存泄漏。

**最后一轮建议。** 在 `Start` 里缓存生命组件引用，`OnDestroy` 对同一对象取消订阅，避免以后切换角色或 `ClearPlayerMgr` 置空时出错。

## 实际采纳与用户调整

| 内容 | 采纳情况 |
|---|---|
| GamePanel 序列化持有 PlayerHealthBar | 用户采用，`GamePanel.prefab` 中已指定引用 |
| 比例改为浮点除法 | 用户采用 |
| 显示时拉取 | 用户采用，写在 `PlayerHealthBar.Start()`，通过自己新增的 `CharacterObject.Health` 读取 |
| 生命事件 | 用户采用 C# 事件，委托类型选 `UnityAction<int,int>`（与 `BasePanel.HideMe(UnityAction)` 保持一致），名称 `OnHealthChange`，在基类 `TakeDamage` 末尾、死亡处理之后触发 |
| 订阅生命周期 | 用户采用 `Start` 订阅、`OnDestroy` 取消订阅，并在取消前判断 `PlayerMgr.Instance.player.Health != null` |
| 删除 PlayerHealth 中直接调用 UI 的代码 | 用户改为注释保留 |
| 缓存生命组件引用 | 未采用，属于历史建议；按当前流程不影响结果 |
| HUD 常驻、只读接口、`isDead` 改属性、零值防护 | 未采用 |

本轮游戏代码均由用户编写。Claude 没有修改工程内的代码、场景或预制体，只编写了工程外的演示和本文档。

10 月 30 日 Codex 曾为控制台添加过生命事件和 HUD 订阅，已在 11 月 10 日按用户要求撤回（见 [记录 29](29-console-support-code-removal.md)）。本轮的 `OnHealthChange` 和订阅代码是用户在了解原理后自己重新实现的，不是恢复当时的实现。

## 验证与当前状态

- **用户完成声明**：用户分三次报告完成，Claude 每次都审查了当时的工作区代码。
- **静态核验**：最终版本中，受伤只通过事件刷新血条，PlayerHealth 不再依赖 UIManager；`Start`/`OnDestroy` 成对订阅与取消；比例使用浮点除法。`GamePanel.UpdateHpBar` 和 `playerHealthBar` 字段目前没有调用方。
- **编译**：用 Unity 生成的 Assembly-CSharp 编译参数和编辑器自带的 C# 编译器独立编译主脚本，输出放在系统临时目录，无编译错误，有现有 LitJson 的警告。未编译 Editor 程序集。
- **实际运行**：尚未在 Play Mode 验证。建议的回归步骤：
  1. 受伤后血条是否变化；
  2. 打开任务面板，等 GamePanel 消失后受伤，Console 是否没有报错；
  3. 关闭任务面板后，血条是否显示真实血量；
  4. 再次受伤，新血条是否更新。
- **未覆盖的范围**：
  - `maxHp` 为 0 时的保护没有做，按现有配置和控制台限制不会出现；
  - 回血逻辑在游戏中还不存在；
  - 升级改变上限属于第 3 项；
  - 控制台通过反射修改字段，不会触发事件，留到第 5 项复测时处理。
- **提交状态**：改动未提交。`Assets/Resources/UI/` 被 `.gitignore` 忽略，与之前“UI 预制件留在本地”的处理一致，因此 `GamePanel.prefab` 中的血条引用不在 Git 中。`PlayerConfig.asset` 中另有一条重复的 `id: 0` 配置，用途待用户确认，与本项无关。

### 补充：提交状态（2026-11-14 文档整理时追加）

上面“改动未提交”是写入时的状态。同日代码已经作为 `e305f25`、本记录作为 `3b6f590` 推送到 main。另外，GMScene 里 GamePanel 实例对 `playerHealthBar` 的场景覆盖当时没有一起提交，留在工作区待用户确认。Play Mode 验证仍未进行，已转为 GitHub Issue 跟踪。用户确认上述场景覆盖随 11-14 的文档整理一起提交。

上文提到的 `PlayerConfig.asset` 中重复的 `id: 0`，用户说明是之前给导师检查进度时临时演示用的，可以删除。核对时磁盘文件和历次提交中都只有一条，这条演示数据没有进入过仓库，#12 已关闭。

## 论文素材与证据入口

可以作为“UI 与游戏数据解耦”的案例：从 UI 主动查找并调用，到 UI 显示时拉取、生命组件通过事件推送；说明动态面板生命周期带来的两个具体缺陷，以及订阅、取消订阅与对象销毁的关系。事件和观察者模式是通用机制，不应写成原创技术。目前没有运行数据，不能写成已测试通过。

- [生命组件与事件](../../../Assets/Scripts/Object/Common/CharacterHealth.cs)、[角色 Health 属性](../../../Assets/Scripts/Base/CharacterObject.cs)、[玩家生命](../../../Assets/Scripts/Object/Player/PlayerHealth.cs)
- [玩家血条](../../../Assets/Scripts/UI/GamePanel/PlayerHealthBar.cs)、[游戏面板](../../../Assets/Scripts/UI/GamePanel/GamePanel.cs)、[任务面板生命周期](../../../Assets/Scripts/UI/GamePanel/MissionPanel.cs)、[UIManager](../../../Assets/Scripts/Managers/UIManager.cs)
