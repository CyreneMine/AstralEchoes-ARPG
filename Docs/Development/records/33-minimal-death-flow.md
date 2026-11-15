# 玩家最小死亡流程与控制台适配

2026-11-15。玩法代码提交为 `a22e86e`，控制台为 `8117965`，Animator 为 `c11d394`。涉及 PlayerMgr、CharacterHealth、PlayerHealth、PlayerObject、PlayerCombat、EnemyAI、Cyrene.controller 和 DevConsole。辅助工具为 Claude Code（下文写作 Claude）。对应 #2，前一天的讨论见[记录 32](32-cursor-mode-camera-and-panel-pause.md)。

## 用户观察与目标

按 11-14 的讨论，#2 不再只是"死亡后禁止移动、闪避和攻击"，而是改为"最小死亡流程"：

- 血量归零后进入死亡状态，锁住操作，播放死亡动画；
- 怪物停止追击；
- 复活时恢复以上状态。复活暂时由控制台触发。

实现过程中，用户又发现一个问题：复活的瞬间，怪物立刻攻击，玩家马上掉血。

## 用户提出的想法

按时间顺序：

1. **第一版**：
   - PlayerMgr 新增事件 `OnPlayerStateChange(bool)`（true 表示死亡）和方法 `PlayerStateChange`。`PlayerHealth.Dead` 调用这个方法广播死亡，并新增 `Respawn`。
   - PlayerObject 通过订阅事件维护一个 `isLive` 标记。PlayerCombat 直接读取 `Health.isDead`。EnemyAI 既订阅事件，又每帧检查玩家是否死亡。
   - Animator 新增 `IsDead` 参数和 `S_Die` 状态。
   - 用户自己把 DevConsole 的"复活"改为调用 `Respawn()`。
   - 为了能通过 `player.Health` 调到 `Respawn`，把 `CharacterObject.health` 的类型改成了 `PlayerHealth`。
   - 为方便测试，把敌人攻击力临时调到 50。
2. **第一次审查后**：
   - 把 `CharacterObject` 改回 `CharacterHealth`；
   - 把 `Respawn` 移到基类，作为公开虚方法；
   - 把设置死亡动画的代码移到 `PlayerObject.ChangeState`，同时重置 `isAttacking`；
   - PlayerObject 改为直接读取 `health.isDead`，删掉了 `isLive` 和调试输出。
3. **第二次修改**：
   - 修正 `Heal` 里的 `Clamp`；
   - `Respawn` 中补上血条刷新；
   - `PlayerHealth` 重写 `Respawn` 来广播复活，和 `Dead` 对称；
   - EnemyAI 考虑到以后接入缓存池，打算按 OnEnable/OnDisable 配对订阅。但第一次写成了在 `Awake` 订阅、`OnEnable` 取消，用户随后自己更正。
4. **第三次修改**：EnemyAI 改为 OnEnable 订阅、OnDisable 取消；`Heal` 自己触发 `OnHealthChange`，`Respawn` 不再重复通知。用户确认死亡、复活、出刀时死亡、控制台杀死这几项实测都正确。
5. **复活后立刻挨打**：用户先自己尝试在事件里设置 `isWaiting = arg0` 和 `waitUntil`，没有效果，于是询问原因。
6. **用户自己的方案**：死亡和复活时都调用 `LoseTarget()`，让怪物脱战；同时去掉 `Update` 开头"玩家死亡就停下"的判断，让怪物在玩家死亡期间也能走回巡逻点。审查后，又在 `CheckPlayer` 开头加上玩家死亡检查，并补充了注释"死亡和复活都脱战"。
7. **控制台归属**：用户说明，工程里只有控制台是 Codex 写的，其余都是用户自己写的。控制台原本只是用来调试用户写好的接口，结果当时连带改动了用户的代码，才出现了记录 29 的五项待办。今后控制台由 Claude 负责，代码和 UI 都可以随意修改，但不能动用户原有的代码。

## 辅助分析与建议

**第一次审查（Claude）**

1. **敌人被加上了 PlayerHealth**：`CharacterObject` 是玩家和敌人的共同基类。敌人预制体上挂的是 `EnemyHealth`，找不到 `PlayerHealth` 时会自动再加一个。后果有三：
   - 怪物被打死时会调用 `PlayerHealth.Dead`，被当成玩家死亡广播出去；
   - 原来的 `EnemyHealth` 一直不扣血，任务的击杀上报失效；
   - EnemyAI 取到的是 `EnemyHealth`，怪物"死"了还会继续追击。

   建议：基类保持通用类型，子类自己暴露具体类型。
2. **出刀时死亡，复活后不能动**：攻击被 S_Die 打断，`AttackEnd` 动画事件不会触发，`isAttacking` 一直是 true。
3. **`Heal` 有两个问题**：不刷新血条，也不限制上限。
4. **EnemyAI 只订阅、不取消订阅**。
5. **观察者模式的取舍**：自己的代码本来就每帧运行，直接读取 `health.isDead` 即可；事件适合"发生时只做一次"的事情，例如设置动画、重置攻击状态、以后的死亡界面、敌人在复活时恢复。玩家动画应由 PlayerObject 自己处理，不该由 PlayerMgr 去设置。另外，`isLive` 这个名字和实际含义相反。

**第二次审查（Claude）**

- `Mathf.Clamp(amount, 0, MaxHp)` 的返回值被丢弃了，而且限制的是 `amount` 而不是血量；
- `Respawn` 的广播写在基类里，敌人以后复活时也会被当成玩家复活；
- 列出了控制台需要跟进的三处。

**第三次审查（Claude）**

Unity 先执行 `Awake` 再执行 `OnEnable`，所以 EnemyAI 刚订阅就被取消了，复活时追击中的怪物会一直停着。缓存池对应的配对应该是 OnEnable/OnDisable。

**复活立刻挨打的原因（Claude）**

- `EnemyCombat` 的冷却以上次攻击时间为准。玩家死亡期间 EnemyAI 不调用 `TryAttack`，冷却照常计时，等玩家原地复活时早已结束，怪物又正好在攻击范围内。
- 用户的修改没起作用有两个原因：一是复活时 `arg0` 为 false，`isWaiting` 被设成了 false；二是 `Update` 先调用 `CheckPlayer` 再检查 `isWaiting`，攻击发生在等待判断之前。
- 企业项目常见的做法有三种，具体参数各不相同：玩家死亡时让怪物脱战、复活后短暂无敌、在存档点或传送点复活。结合现有代码，Claude 推荐复用 `LoseTarget()`。

**对用户方案的审查（Claude）**

去掉 `Update` 里的玩家死亡判断以后，怪物回到巡逻点会把 `isReturning` 改回 false。如果尸体在侦测范围内，怪物会重新追过去，对着尸体出刀。建议把玩家死亡检查放到 `CheckPlayer` 开头。

**一处判断错误（Claude）**

Claude 起初说侦测分支里的 `if (isReturning) return;` 是多余的，随后自己更正：同一帧里如果因为 `currentWay > maxRange` 调用了 `LoseTarget()`，而玩家仍在侦测范围内，就是靠这一行阻止怪物刚脱战又立刻重新追击。所以这一行要保留。在这种情况下，它也让本帧跳过后面的攻击判断。

**控制台（Claude 修改，仅改 `DevConsole.cs`）**

| 位置 | 改动 |
|---|---|
| `SetConsoleHealth` | 血量归零且尚未死亡时，调用用户的 `Dead()`；改血后通知血条 |
| `NotifyHealthChanged` | 新增。事件只能在声明它的类里触发，所以通过反射读取 `CharacterHealth.OnHealthChange` 这个由编译器生成的字段来调用；字段不存在时打印警告 |
| `KillPlayer` | 删掉手动重置攻击状态和速度的代码，改为走用户的死亡流程；已经死亡时给出提示 |
| `RevivePlayer` | 死亡时调用 `Respawn()`（沿用用户的改法）；未死亡时只回满血，避免多发一次复活广播 |
| `ResetAttributes` | 死亡时拒绝执行（`Init` 会绕过复活广播）；执行后刷新血条 |

`DevConsoleSetup` 里自动检查的条件在修改后仍然成立。控制台用反射读写的字段名现在有 `config`、`currentHp`、`maxHp`、`def` 和 `OnHealthChange`。

## 实际采纳与用户调整

| 内容 | 结果 |
|---|---|
| 死亡广播 | PlayerMgr `OnPlayerStateChange(bool)`，由 `PlayerHealth.Dead/Respawn` 调用 `PlayerStateChange` |
| 玩家响应 | `PlayerObject.ChangeState`：设置 `IsDead` 动画并重置 `isAttacking`；移动和闪避读取 `health.isDead`；PlayerCombat 读取 `Health.isDead` |
| 回血与复活 | `CharacterHealth.Heal`（protected，限制上限并触发 `OnHealthChange`）；`Respawn` 为 public virtual，`PlayerHealth` 重写后广播 |
| 怪物响应 | 用户自己的方案：死亡和复活都 `LoseTarget()`；`CheckPlayer` 在玩家死亡时跳过侦测和攻击；OnEnable/OnDisable 配对 |
| Animator | Any State 在 `IsDead` 为真时进入 `S_Die`（不允许转换到自身）；`S_Die` 在 `IsDead` 为假时切到 `S_Take` |
| 未采用 | 复活后重置攻击冷却、复活无敌、只让追击中的怪物脱战、拆分死亡/复活两个事件或修改参数命名 |
| 留待以后 | `PlayerMgr.PlayerStateChange` 中的 TODO：死亡界面 |

## 验证与当前状态

**用户实测**
- 第三次修改后，用户确认死亡、复活、出刀时死亡、控制台杀死这些情况都正确。

**用户声明**
- 发现"复活后立刻挨打"后，改为让怪物脱战，并补上了 `CheckPlayer` 的检查。最终版本没有单独报告复测结果。

**静态核验**
- Claude 逐轮审查了以上代码；
- 使用 Unity 的编译参数独立编译主脚本，没有错误；
- 控制台修改时，Editor 程序集也编译通过。

**Claude 未进行 Play Mode 测试。**

**未提交**
- `EnemyConfig.asset` 中攻击力 50 是用户的测试值，用户会自己改回；
- GMScene 中的玩家位置和 Canvas 改动是调试用的，用户之后会删除场景里的 Canvas。

**已知影响**
- `LoseTarget()` 对场景里所有怪物都生效，没有在追击的怪物也会被设为"返回中"，表现上几乎看不出来。
- 复活目前是原地复活，由控制台触发；死亡界面、传送点、复活无敌都还没做。

## 论文素材与证据入口

可以作为"角色状态 + 事件通知"的设计案例：

- 死亡作为角色的状态，由状态本身锁住操作，而不是依赖 UI；
- 每帧读取和订阅事件各自适合的场景；
- 基类与子类的类型选择对共用组件的影响；
- 订阅生命周期与缓存池的关系；
- 攻击冷却在玩家死亡期间照常计时，带来的"复活立刻挨打"问题；
- 调试工具通过反射适配接口，而不侵入玩法代码。

观察者模式和反射都是通用技术，不应写成原创技术；没有运行数据的部分不能写成已测试通过。

- [PlayerMgr](../../../Assets/Scripts/Managers/PlayerMgr.cs)、[CharacterHealth](../../../Assets/Scripts/Object/Common/CharacterHealth.cs)、[PlayerHealth](../../../Assets/Scripts/Object/Player/PlayerHealth.cs)
- [PlayerObject](../../../Assets/Scripts/Object/Player/PlayerObject.cs)、[PlayerCombat](../../../Assets/Scripts/Object/Player/PlayerCombat.cs)、[EnemyAI](../../../Assets/Scripts/Object/Enemy/EnemyAI.cs)、[EnemyCombat](../../../Assets/Scripts/Object/Enemy/EnemyCombat.cs)
- [DevConsole](../../../Assets/Scripts/UI/DevConsole.cs)、[Cyrene.controller](../../../Assets/Resources/Animator/Cyrene.controller)
