# 敌人攻击状态、三个攻击距离与配置按 id 查找（状态机第二步）

日期：2026-11-27 晚至 11-28 凌晨。工程版本：基于 `2eff993`，提交为 `8d075ec`（敌人攻击状态等，用户编写）、`eaf29fe`（配置按 id 查找、任务面板目标名，用户编写）、`e97fdee`（配置表，用户修改）、`4508e5b`（敌人 Animator 与 GMScene，用户修改）。

协助工具：Claude Code。

相关 Issue：#42（敌人 AI 状态模式，本次是第二步攻击状态）、#43（千人律者模型接入）、#44（出招距离按敌人配置，本次新开）、#26（怪物复用入口）。第一步见[记录 56](56-enemy-fsm-state-pattern.md)，千人律者模型转换见[记录 57](57-enemy-model-pmx-to-fbx.md)。

分工：
- 用户编写和设置，Claude Code 只解释和审查：攻击状态 `EnemyAttackState`、`EnemyAI` 的速度参数同步和 `StartChase`、`EnemyCombat` 的出招标记和超时、`ConfigMgr.GetCharacterData` 改为按 id 查找、`CharacterData.name`、`MissionItem` 的目标名、全部配置表数值、敌人 Animator 状态机、攻击动画的两个动画事件、千人律者预制体的停止距离、场景里的木桩。
- Claude Code：分析问题、审查各版代码、推算命中范围、开 #44、写本记录和同步文档。

## 用户观察与目标

- 换上千人律者模型后，用 `owner.Agent.velocity.magnitude` 驱动 Animator 的 `velocity` 参数，发现怪物停下时这个值不为 0（当次聊天）。
- 完成 #42 第二步：单独的攻击状态。
- 测试了 SO 配置和任务系统的联动，报告"一切正常"（当次聊天）。

## 用户提出的想法

按讨论顺序：

1. 问 `velocity.magnitude` 停下时为什么不为 0。
2. 给角色配置加 `name` 字段，任务面板按 id 显示敌人名字。
3. Claude Code 指出配置表两项 id 都是 0 之后，用户自己找到原因："GetCharacterData 这个方法我最后返回的是按下标来的，刚好误打误撞撞上了"，改为按 id 查找。
4. 配置表的 id、奖励表和任务目标，用户说早已在 Inspector 里改好，只是没保存（Unity 的 SO 改动要 Save Project 才写入文件）。
5. 收招：用户提出"在这个动画播放完成的时候触发一个事件来通知"，担心这样做麻烦了。
6. 攻击距离：用户先问"按到底不是应该取上限吗""那相等不是才是最好的吗"；之后提出自己的方案："让 AIconfig 的攻击距离和停止距离略小于 EnemyConfig 的攻击距离，这样停下来攻击一定在攻击范围里面"，最终采用这个方案。
7. 对 Claude Code 指出的"`EnemyAIConfig` 全局共用、`atkRange` 按敌人配置"的问题，用户决定次日修改，让开 Issue（#44）。
8. 场景里第二只千人律者关闭 EnemyAI，用户说明"是留着当木桩的"。

## 辅助分析与建议（Claude Code）

### 1. 速度参数停下时不为 0

依据 `EnemyPatrolState.cs`、`EnemyWaitState.cs`：

- 主要原因是 `SetFloat` 只写在巡逻状态的 Update 里。切到等待以后没人再写，Animator 参数停在巡逻最后一帧的值；当时停止距离是 1.4，切换那一刻怪物还在走。
- 次要原因：`isStopped = true` 后 agent 按 `acceleration` 减速几帧，接近 0 时还会是很小的浮点数，Animator 的条件要用阈值。
- 建议：速度这类连续参数属于表现层，放到 `EnemyAI.Update` 里、`stateMachine.Update()` 之后每帧同步；用实际速度 `velocity` 而不是 `desiredVelocity`（后者停下时立刻为 0，会出现"已经播待机、身体还在滑"）。`StringToHash` 和阻尼是可选项。

### 2. 第一版攻击状态的审查

- **配置表 id**：`EnemyConfig.asset` 里测试敌人和千人律者的 `id` 都是 0，而 `GetCharacterData` 用 `characterDatas[id]` 按下标取，千人律者（`enemyId` 1）只是因为排在第 2 位才取对。奖励表只有 `EnemyId: 0`、任务 `targetId` 是 0，打死千人律者会报"未找到怪物的奖励列表"，任务也不计数。
  用户改为按 id 查找后，磁盘上的文件仍是两个 `id: 0`（修改时间早于代码），按新查法千人律者会取到 null，在 `CharacterObject.Init` 里报空引用；用户保存后，文件变为 id 0、1，奖励表加了 `EnemyId: 1`，任务 `targetId` 改为 1。
- **脱战起点**：`EnemyChaseState.Enter` 记录 `startPos`，攻击 → 追击也会进入，每次都把起点重置到当前位置，玩家可以把怪物一直往外引。建议起点放在 `EnemyAI` 上，只在巡逻或等待发现玩家时记录（`StartChase`），攻击回追击不重置。
- **任务面板**：目标名在 `switch` 之前查敌人表，对话、交互任务的 `targetId` 是 NPC 或区域的 id，NPC id 超过敌人表长度时会越界。建议移进战斗分支；对话分支的 `npcConfigs[targetId]` 同样按下标取，建议改用已有的 `GetNpcConfig`。
- **攻击状态还差的四件事**：停下导航、朝向玩家、判断距离和命中范围对齐、退出时留余量或者挥完再退。

### 3. 三个攻击距离

依据 `EnemyCombat.OnAttackCheck`（`OverlapSphere` 以怪物脚底为球心、`atkRange` 为半径）、`EnemyAI.CanAttackPlayer`（中心点距离 < `EnemyAIConfig.attackRange`）、千人律者预制体的 NavMeshAgent 和场景里玩家的 CharacterController：

- **约束**：停止距离 < 出招距离 ≤ 实际能打中的距离。停止距离不小于出招距离时，导航先停下，怪物贴着玩家不出手；出招距离超过实际命中范围时，玩家站着不动也会被打空。
- **实际能打中的距离**：`OverlapSphere` 检查的是碰撞体相交，不是玩家中心点。按场景里玩家 CharacterController（半径 0.26、高 1.46、中心离地 0.81）和球心在脚底推算，水平距离约 √(1.66² − 0.34²) ≈ 1.62 米；前提是两边脚底在同一地面上。
- **两处更正**：Claude Code 先建议把出招距离调到 1.6，算出 1.62 后改为建议 1.5；又更正了"导航停下后一定能进入攻击状态"的说法：攻击状态的 Enter 会当场停下，所以怪物停在出招距离附近出招，停止距离只是保底。
- **为什么不取上限或相等**：两边量法不同（中心点距离和碰撞体相交），换算有误差且方向未知，取相等就是容错为 0。给出备选：判断也用与命中参数相同的 `Physics.CheckSphere`，两边就是同一个检测。
- **用户问"企业里面也是这么做的吗"**：回答行业没有统一规定，并列出三类常见做法：按碰撞体边缘算距离（举了 Unreal 行为树 Move To 的 Reach Test Includes Agent Radius / Goal Radius 选项，凭记忆，当时联网工具报错，没能核实）、攻击配置数据驱动（出招距离和判定放在同一份配置里，出招距离略小）、AI 直接用判定形状决定能否出招。
- **用户方案的补充**：除了两者都小于命中半径，停止距离还要小于出招距离；另外 `EnemyAIConfig` 只有一份、所有敌人共用，而 `atkRange` 按敌人配置，以后命中范围不同的敌人对不上，建议出招距离从各敌人的 `atkRange` 往回扣或放进 `EnemyConfig`（#44）。

### 4. 出招必须挥完

- 没有这一步时，玩家在前摇时退出范围，攻击状态当帧切回追击，导航恢复移动，而 Animator 的攻击过渡有 Exit Time，还要播到 0.8125 才切走，结果是边挥刀边往前滑；命中帧 0.527 时怪物可能已经滑近，玩家退开了也会挨打；在边界上还会来回切状态、一顿一顿。以上为按代码和 Animator 设置的推断，未单独实测。
- 用户的动画事件方案就是常见做法，和命中判定 `OnAttackCheck` 是同一套机制。建议用 bool 标记而不是 C# 事件：攻击状态每帧都在检查，关心的是"还在不在出招"这个持续状态，用事件要在 Enter 订阅、Exit 退订，还可能漏掉；适合用事件的是多个互不相关的系统都要知道某件事的场合（如死亡）。
- 建议事件放在 Exit Time 之前，出招期间不转向，并加超时兜底。审查第一版时指出缺超时：现在敌人没有死亡和受击动画，攻击动画总能播完，不会卡；以后挥刀被打断、收招事件不触发，标记会一直为 true，复活或从对象池复用后一进攻击状态就会卡死。

## 实际采纳与用户调整

- **速度参数**：`EnemyAI.Update` 在 `stateMachine.Update()` 后执行 `animator.SetFloat("velocity", agent.velocity.magnitude)`；没有用 `StringToHash` 和阻尼。
- **攻击状态 `EnemyAttackState`**：
  - Enter：`isStopped = true`，`velocity = Vector3.zero`。
  - Update：`Combat.IsAttacking` 为 true 时直接 return；否则先转向玩家（`RotateTowards`，每秒 360°），在攻击距离内调用 `TryAttack`，否则切回追击。
  - Exit：`isStopped = false`。
  - 追击状态进入攻击距离时改为切到攻击状态；玩家死亡或复活时，追击中和攻击中的怪物都切到返回。
- **脱战起点**：`EnemyAI.StartChase()` 记录 `ChaseStartPos` 再切追击，巡逻和等待发现玩家时调用；追击状态删掉了自己的 `startPos`。
- **出招标记**：`EnemyCombat` 的 `isAttacking` 为私有字段，对外只读属性 `IsAttacking` 同时判断时间上限 `maxAttackTime`（3 秒）；`TryAttack` 触发 `fire` 时设为 true 并记录开始时间，动画事件 `OnAttackEnd` 设回 false。
- **攻击距离（用户方案）**：千人律者预制体的停止距离 1.3（本地预制体，不入库，见 ASSET_RESTORE）< `EnemyAIConfig.attackRange` 1.4（原为 2）≤ 千人律者 `atkRange` 1.4。出招距离和命中半径取相等在这里可以：判断用的是中心点距离，中心点不到 1.4 就已经在命中球里面。未采用 `CheckSphere` 备选；出招距离按敌人配置留到 #44。
- **配置与查找**：
  - `CharacterData` 新增 `name`；`GetCharacterData` 按 `id` 循环匹配，找不到时输出"数据源为空"并返回 null。
  - 敌人表：测试敌人（id 0）补名称；新增千人律者（id 1，攻击 50、防御 1、生命 10、冷却 2、`atkRange` 1.4）。玩家表补名称，字段顺序随序列化重排，数值不变。
  - 奖励表新增 `EnemyId: 1`（道具 0 × 50）；任务 0 第一段战斗目标 `targetId` 0 → 1。
  - `MissionItem`：战斗任务显示"击败{敌人名}"，对话任务用 `GetNpcConfig` 取 NPC 名。
- **敌人 Animator（`EnemyAnimator.controller`，用户设计）**：待机 `Zombie_Idle_03`、跑步 `Run_Forward_InPlace`、攻击 `Attack`（`Zombie_Attack_Forward01`）三个状态，参数 `fire`（Trigger）和 `velocity`（Float）；待机与跑步按 `velocity` 切换，`fire` 进入攻击，攻击回待机的过渡 Exit Time 0.8125、时长 0.25。
- **攻击动画事件**（`Zombie_Attack_Forward01.fbx.meta`，本地不入库）：`OnAttackCheck` 在 0.527，`OnAttackEnd` 在 0.780，比 Exit Time 早。
- **场景（`GMScene.unity`）**：放入两只千人律者实例，第二只"千人律者 (1)"关闭 EnemyAI 当木桩；原来的两个胶囊敌人 `EnemyObj` 实例停用；玩家 CharacterController 的场景覆盖为高 1.46、半径 0.26、中心 0.81（本次对话没有讨论这项修改，按当前场景一并提交）。
- **模型位置**：千人律者 FBX 现位于 `Assets/ArtRes/Character/千人律者/`，导入类型为 Humanoid（`animationType: 3`）；迁移过程不在本次对话中。

## 验证与当前状态

- **用户实测（11-28，当次聊天报告"测试全部通过"，无截图日志）**：用能动的那只千人律者测了四项：出招时走开，怪物原地挥完再追；前摇时后退，这一刀打空；站着不动，每 2 秒出招一次且先转向；出招中绕到侧面，怪物不跟着转。
- **用户报告（11-27）**：SO 配置和任务系统联动正常。当时查找还按下标；改为按 id 查找后，奖励发放和任务计数没有单独复测。
- **静态核验**：
  - 配置文件已写入磁盘：敌人表 id 0、1，奖励表含 `EnemyId: 1`，任务 `targetId` 为 1。
  - 编辑器日志中改到一半时的编译错误之后，最后一次编译和程序集重载成功。
  - 两个动画事件的时间点都早于攻击过渡的 Exit Time。
- **未测**：超时兜底（可选项，没有删事件测试）；速度参数修改后等待状态降到 0（用户没有单独报告）。
- **后续**：#44（出招距离按敌人配置，用户计划次日修改）；#26（复用入口未接）；#43（Humanoid 映射和渲染细节未单独确认）。

## 论文素材与证据入口

- **攻击距离的约束与推导**：停止距离、出招距离、命中范围三者的顺序，中心点距离和碰撞体相交的差异，以及"判断与命中共用同一检测"的取舍，可用于说明敌人 AI 的参数设计和验证方法。
- **出招必须挥完**：代码状态机负责决策，动画事件只报告时机（命中帧、收招帧），加超时兜底防止事件丢失；可和记录 56 的状态模式一起说明攻击状态的设计。
- **配置按 id 查找**：按下标查表"刚好对上"掩盖了配置错误，改为按 id 后错误会直接暴露。
- **证据**：`Assets/Scripts/Object/Enemy/State/EnemyAttackState.cs`、`EnemyAI.cs`、`EnemyCombat.cs`、`Managers/ConfigMgr.cs`、`UI/GamePanel/MissionItem.cs`、`Resources/GameData/` 下的配置表、`Resources/Animator/Enemy/EnemyAnimator.controller`；本地依赖见 [ASSET_RESTORE.md](../../ASSET_RESTORE.md)。
