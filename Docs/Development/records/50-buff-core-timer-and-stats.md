# Buff 核心：MonoMgr / TimerMgr、BuffMgr 与属性加成

日期：2026-11-25。工程版本：基于 `c245bb0`，提交为 `edcc01c`（MonoMgr、TimerMgr）、`968b003`（Buff 核心代码）、`6b08720`（Buff 配置）、`d8cb9e7`（控制台）。

协助工具：Claude Code。

相关 Issue：#16（三选一临时 Buff）、#36（MonoMgr 与 TimerMgr）、#32（保存点，需要 Buff 的剩余时间）。设计调整见[记录 48](48-temporary-buff-design.md)末尾的“补充”。

玩法代码由用户编写，包括 `MonoMgr`、`TimerMgr`、`BuffDatabase`（含 `BuffConfig`、`BuffType`）、`BuffMgr`、`ConfigMgr`、`CharacterStats`、`PlayerObject`、`PlayerMgr`。Claude Code 只负责审查，修改的只有 `DevConsole.cs`、`DevConsoleSetup.cs`；`BuffDatabase.asset` 里的 3 个测试 Buff 是按用户要求由 Claude Code 填写的。

## 用户观察与目标

- 按计划表，11-25～11-26 完成 #16 的 Buff 核心：管理器、面板、随机三选一、计时、总值计算。本次完成计时、配置、管理器和总值计算；剩余选择次数、随机抽取和面板留到 11-26。
- 开工前，用户提出入口改成按钮，并新增 MonoMgr、TimerMgr，后来技能 CD 也要复用（设计调整见记录 48 的补充、#16 评论、#36）。

## 用户提出的想法

1. **为什么还要 TimerMgr**：用户问：“我都有单例mono了为什么还需要其他管理器获得时间 直接用mono单例的Time不就好了吗”。听完说明后，用户决定“那我还是做 TimerMgr”。
2. **计时放在哪里**：用户一开始卡在“timerMgr没法计时那不是本来的计时还是需要再monomgr里面完成吗”，之后改为 TimerMgr 在构造函数里订阅 `MonoMgr.OnUpdate`。
3. **遍历中增删计时器**：Claude Code 指出 `foreach` 遍历 Dictionary 时增删元素会抛异常。用户自己推出了处理方法（当次聊天原话）：“假如我后续有一个buff再结束的时候 又续上了一个新的buff 就会导致异常 所以应该记录要执行的所有下标 然后再另一个循环里面一次全部执行”。
4. **暂停时是否计时**：用户决定 Buff 和 CD 都要受 `timeScale` 影响：“比如战斗过程中打开背包界面 此时cd应该暂停而不是继续恢复”。所以 TimerMgr 只用 `Time.deltaTime`，不做不受暂停影响的计时。
5. **配置**：用户提出用 SO 配置所有 Buff；每个 Buff 的时长写在自己的配置里；一个 Buff 只加一项属性，多属性的 Buff 先不考虑。
6. **测试数据**：用户让 Claude Code 设计 3 个测试 Buff，名字、介绍、数值都填好。
7. **保存计时器 id**：用户先写成 `Dictionary<int, float>`，存“计时器 id → 加入时的剩余时间”，问“第5点可以像我这样写吗”。
8. **候选池（用户自己加的）**：新增 `remainingBuffs`，添加时移出，到期加回；`ClearAll` 时恢复为全部配置。
9. **到期回到候选池**：Claude Code 说“生效中的 Buff 和已选列表要分开”，用户回复“不需要 如果一个buff到期了就会回归可选列表”。用户还说明了提示的条件：“所有buff都已经被选择并且都没有过期的时候才显示 你已经足够强了”。
10. **属性加成放在哪里**：用户先在 `PlayerMgr` 放了三个绿值字段，后来移到 `CharacterStats`。第一版按“当前属性 × 百分比”存绝对值，听完问题后改成累加百分比。
11. **清除时的刷新**：Claude Code 说 `CharacterStats` 不应调用 `PlayerMgr`，之后又建议在 `PlayerObject` 上加清除方法、在里面刷新。用户一开始理解成两条建议互相矛盾：“你上面不是说 这样这个类就依赖这个管理器了不能这么写吗”。看了依赖方向的说明后，用户说：“我还以为你让我直接恢复注释……这下我看懂了 是换个地方”。

## 辅助分析与建议（Claude Code）

### TimerMgr

- 说明了 MonoMgr 和 TimerMgr 的分工：MonoMgr 只转发 Update；TimerMgr 负责按 id 管理计时、取消和查询剩余时间。Buff 到期、全灭清除、#32 存档、图标倒计时都要用到这些功能。
- 审查了几轮。最终版本：先收集到期的 id，再在第二个循环里删除计时器、调用回调；用 `TryGetValue` 跳过已被别的回调删除的计时器；先删除再调用回调；id 只增不减；查询不到时返回 -1（用户写的约定）。

### BuffMgr 审查中发现的问题（都已由用户修复）

| 问题 | 后果 |
|---|---|
| `hasBuff` 没有初始化 | 第一次 `AddBuff` 就报空引用 |
| 查不到 id 时没有 `return` | 把 null 加进列表，随后报空引用 |
| `RemoveAt(id)` 把 Buff id 当成下标 | 只选星息（id 2）时越界；选两个时删错 |
| 回调里 `RemoveTimer(id)` 传的是 Buff id | 计时器在回调前已经删除，这行多余；还会误删 id 碰巧相同的其他计时器（例如先选星息、再选星锋，星锋到期时删掉星息的计时器） |

### 计时器 id 的保存方式

用户的 `Dictionary<int, float>` 有三个问题：
- 存的剩余时间是加入那一刻的快照，之后不会更新。
- 从 Buff 找不到它的计时器。
- 到期后没有从字典里删除。

建议改成“Buff id → 计时器 id”，剩余时间每次去 TimerMgr 查。用户采纳了。

### 候选池计数

`remainingCount` 和 `remainingBuffs.Count` 是同一个数存了两份。建议删掉这个字段，改成只读属性，名字避开以后的“剩余选择次数”。用户采纳，命名为 `poolCount`。

### 属性加成

- 方案对比：给出三种接入方式（`Recalculate` 加参数、`CharacterStats` 持有加成字段、在外部算好再传入），并说明企业项目里常见的是属性对象持有一组修饰项（Modifier），由 Buff 和装备负责增删。现在只有百分比加成，三个字段就够用。用户选了第二种。
- 绝对值的问题：按当前属性乘出绝对值存下来，升级后加成仍按旧白值计算（Lv.2→3 时攻击应为 10.5，实际是 10）；用 `=` 赋值，同类型的 Buff 会互相覆盖。建议改成累加百分比，在 `Recalculate` 里乘。
- 依赖方向：管理器和对象可以调用数据类，数据类不要反过来调用管理器。所以刷新放在 `PlayerObject.AddBuff` / `ClearBuff` 里，`CharacterStats` 只改自己的字段。
- 减回的位置：用户第一次把减回加成写在计时器回调里。建议挪进 `RemoveBuff`，这样从哪里调用移除都会还原属性。
- 清除的顺序：`ClearBuff` 第一次写成先刷新、再清零，全灭后属性仍带着加成，用户调换了顺序。

### 被撤回或修正的分析

- 早上评估按钮入口时，Claude Code 说次数规则和原设计冲突、初始化时读取次数有问题、面板打开时还能点到 GamePanel。用户逐条反驳后核实，三条都不成立：全屏面板会在 `ShowMe` 里隐藏 GamePanel。这三条已撤回。
- 建议 6“生效中的 Buff 和已选列表分开”被用户否定，设计改为到期回到候选池。

### 提醒过、用户还没处理的

- `AddBuff` 没有判断重复添加，重复调用会在 `timers.Add` 抛异常，而且这时列表和计时器已经改了一半。正常流程里生效中的 Buff 不会出现在候选里，加不加由用户决定。
- `RemoveBuff` 如果不是由计时器到期调用，它不会取消对应的计时器。旧计时器之后到期时，会把重新添加的同一个 Buff 提前移除。现在只有到期回调会调用它，所以不会出问题。以后要单独提前移除时，在 `RemoveBuff` 里先调用 `RemoveTimer` 就行。
- 小问题：`GetBuffRemaining` 的参数仍叫 `currentIndex`；`PlayerMgr` 留了一行空的绿值注释；`CharacterStats` 有一个没用到的 `using System;`。

### 控制台（Claude Code 修改）

- 新增 Buff 分页：
  - 左侧列出配置中的 Buff，点击一行调用 `AddBuff`。
  - 右侧显示候选池数量、生效中的剩余时间和 `timeScale`；攻击、防御、生命上限各显示“白值 × (1 + 加成) = 公式值”和实际值。白值按 `Recalculate` 的公式单独算一遍，加成和生效列表对不上时标红。
  - “清空全部 Buff”调用 `ClearAll`。
  - 生效列表和加成百分比通过反射只读。
- 重复点击生效中的 Buff 时由控制台拦截，避免触发上面说的异常。
- 属性页的“恢复 SO 初始值”改为“按等级和 Buff 重算”，改用 `PlayerObject.UpdateStats`。原来调用 `Init` 会新建 `CharacterStats`，加成随之丢失，之后 Buff 到期减回时加成会变成 -50%。
- 自动检查新增 30 项，见下文。为了不等满 20 秒，到期检查把这个 Buff 的计时器剩余时间改成 0.05 秒；之后的到期、回调、减回加成走的都是真实代码。

## 实际采纳与用户调整

最终代码：

- **MonoMgr**（`SingletonMono`）：`event UnityAction OnUpdate`，在 `Update` 里调用。
- **TimerMgr**（`BaseManager`）：`Dictionary<int, TimeData>` 加 `removeList`；`AddTimer(remaining, action)` 返回 id；`RemoveTimer(id)`；`GetRemainingByIndex(id)` 查询不到时返回 -1；按 `Time.deltaTime` 计时。
- **BuffDatabase**：`List<BuffConfig>`。`BuffConfig` 字段为 id、buffName、buffDesc、percent、totalTime、buffType。`BuffType` 有 atk、def、maxHp 三种。`ConfigMgr` 从 `Resources/GameData/Buff/BuffDatabase` 加载，提供 `GetBuffConfigById` 和 `GetBuffConfigs`。
- **BuffMgr**：
  - 字段：`hasBuffs`（生效列表）、`remainingBuffs`（候选池，复制自配置）、`timers`（Buff id → 计时器 id）、`poolCount`。
  - `AddBuff(id)`：加入生效列表、移出候选池、`AddTimer`（到期时调用 `RemoveBuff`）、调用 `player.AddBuff(type, +percent)`。
  - `RemoveBuff(id)`：`player.AddBuff(type, -percent)`，移出生效列表、回到候选池、删除计时器 id 记录。
  - `GetBuffRemaining(id)`：不在生效中时返回 0。
  - `ClearAll()`：逐个 `RemoveTimer`，清空列表和字典，候选池恢复为全部配置，调用 `player.ClearBuff()`。
- **CharacterStats**：`atkBuffPercent` 等三个字段，`AddBuff(type, value)` 累加，`ClearPlayerBuff()` 清零；`Recalculate` 为 (白值 + 每级成长 × 等级差) × (1 + 加成)。
- **PlayerObject**：`AddBuff` 和 `ClearBuff` 都是先改 `Stats`，再 `UpdateStats()`。
- **测试 Buff**：星锋（攻击 +50%，20 秒）、星盾（防御 +50%，25 秒）、星息（生命上限 +50%，30 秒）。名字和描述只是测试用的占位内容。

未采用：“计时器 id → 剩余时间快照”的字典；在 `PlayerMgr` 里放绿值；按绝对值存加成；在 `CharacterStats` 里刷新玩家；减回只写在计时器回调里；单独的已选列表。

## 验证与当前状态

- **静态核验**：19:24 用 Unity 自带的编译器（`DotNetSdkRoslyn/csc.dll`）按 Unity 生成的编译参数，在临时目录分别编译运行时和编辑器程序集，都没有错误。新增的警告只有原来就有的过时 API 提示。
- **运行证据（自动检查）**：用户在 11-25 19:32:14 运行 Verify in Play Mode，结果 **123 项通过、0 项失败**（原来 93 项，新增 30 项）。Buff 部分实测：
  - 开始时 Lv.2，攻击 6、防御 4、生命 110/110。
  - 加星锋，攻击 6 → 9，`GetAtkValue` 同步；加星盾，防御 4 → 6，生命组件同步；加星息，生命上限 110 → 165，当前生命按比例保持，HUD 最后收到的广播就是最终值。
  - 重复点击被拦截，状态不变。
  - Buff 生效时升到 Lv.3，攻击 = 7 × 1.5；恢复等级后回到 6 × 1.5。
  - “按等级和 Buff 重算”后加成仍然保留。
  - `timeScale = 0` 时 10 帧内剩余时间不变；恢复后继续减少。
  - 星锋到期：移出生效列表、回到候选池、计时器被删除（查询返回 -1）、攻击减回 6。
  - `ClearAll`：取消剩下的 2 个计时器，三项加成清零，属性回到白值，生命按比例保持。
  - Buff 页截图里布局和数值显示正常。
- **用户实测（当次聊天报告“验证通过”）**：
  - 加 Buff 后打开背包等几秒再关，剩余时间只扣了背包关闭期间的时间。
  - 等满 20 秒，星锋自然到期。
- **只有静态核验**：回调里增删计时器（TimerMgr 的两个循环和 `TryGetValue`）。
- **未实现**：剩余选择次数及其事件、随机抽 3 个、Buff 面板和 GamePanel 按钮、候选为 0 时的提示和关闭按钮（#16）；保存点存档和恢复 Buff（#32）；副本结束时调用 `ClearAll`（等副本流程）。

## 论文素材与证据入口

- **把 Update 转发给普通类**：MonoMgr 用事件把 Update 分发给非 MonoBehaviour 的管理器（观察者模式）。TimerMgr 用 id 管理计时、到期回调、取消和查询，Buff 和以后的技能 CD 共用。
- **遍历中修改集合**：先收集到期的 id，再统一删除和回调。这个做法是用户根据异常原因自己推出来的。
- **配置和运行时状态分开**：SO 只存配置，生效列表、计时器 id 和加成放在运行时对象里；控制台和管理器都不改写 SO。
- **加成每次重算，不存快照**：存的是百分比，每次重算时用当前白值乘，所以升级、叠加、移除都能得到正确结果。可以用“升级后 10 和 10.5”的例子说明。
- **依赖方向**：数据类不反过来调用管理器，刷新由持有数据的对象负责。
- **验证方法**：自动检查通过反射把计时缩短，在几秒内跑完真实的到期流程；暂停和自然到期由用户手动补测。
- 证据：提交 `edcc01c`、`968b003`、`6b08720`、`d8cb9e7`；本地自动检查报告 `Temp/DevConsoleReports/playmode-verification.txt` 和 `DevConsole-4-Buff.png`（不进仓库）；#16、#36 的评论；当次聊天记录。
