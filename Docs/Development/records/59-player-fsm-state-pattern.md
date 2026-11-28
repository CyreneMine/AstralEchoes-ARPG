# 玩家改为状态模式的状态机（移动、闪避、攻击、死亡）

日期：2026-11-28 至 11-29 凌晨。工程版本：基于 `0e9119b`，提交为 `1edd0d6`（玩家状态机，用户编写）、`6e2e93b`（控制台适配，Claude Code 修改）、`68dc0a6`（玩家 Animator 与 GMScene）。

协助工具：Claude Code。

相关 Issue：#45（本次，第一步：重构）、#46（第二步：受击、闪避无敌、打断与冲刺接跑步）、#42（敌人用的同一套 `IState` / `StateMachine`，见[记录 56](56-enemy-fsm-state-pattern.md)）。

分工：
- 用户编写和设置，Claude Code 只解释和审查：`PlayerController` 与四个状态类、`PlayerObject` 和 `PlayerCombat` 的删减、冲刺按计时结束和保留冲刺速度、攻击状态的退出判断、死亡状态的接入、玩家 Animator 的连段优化和三段攻击的 `Attack` 标签、删除 `StartQuick` / `EndQuick` 方法和对应的动画事件。
- Claude Code：分析和审查各版代码、按用户要求把 Attack 层搬进 Base Layer（只搬运，不加新内容）、按用户要求删除 19 个动作副本上的 `SwitchSocket` 事件、适配控制台和自动检查、更新 #45 / #46、写本记录和同步文档。

## 用户观察与目标

- 玩家的移动、冲刺、攻击状态都写在 `PlayerObject` 里，用 `isAttacking`、`isQuicking`、`dashTimer`、`isDead` 组合判断。用户原话："你可以看到我的玩家代码写的相当乱其实 PlayerObject里面"，希望像怪物那样做成状态机，当天就改（当次聊天）。
- 冲刺后会原地顿一下再开始跑，想做成类似原神那样冲刺后直接进入奔跑。这一项按讨论留到 #46。
- 做攻击状态时问"该在什么时候切回移动状态"：玩家可以一直狂点左键，给一个固定时间退出又"感觉也怪怪的"（当次聊天）。

## 用户提出的想法

按讨论顺序：

1. 玩家也做成和怪物一样的状态机，并且把控制逻辑拆成单独的组件（Claude Code 原来建议当天先把状态机放在 `PlayerObject` 里）。
2. 移动代码一段一段地搬："我故意保留的 我一点点拆 相当于重新做一下 之前做的太乱了"。
3. 问先转向再移动和先移动再转向有没有区别（"原本 a→b→a→b 变成 b→a→b→a"）。听完只差一帧的说明后仍然改了，"虽然看不出来但是 留个心眼没错"；读输入挪到 Update 开头。
4. 想到"如果被打断了，执行 Exit 里面的内容，把参数都调成 0"；又问能不能在退出移动状态时判断下一个状态是不是冲刺。
5. 对 `GetTargetSpeed` 的理解："取初速，在冲刺结束后检查是否按了方向键和 shift，然后给对应的 3 种值"。
6. 冲刺先用动画事件 `EndQuick` 结束。对"事件可能到不了"的分析，用户指出"闪避期间算无敌的，不会被打断"；之后改为按 `dashDuration` 计时，再删掉 `EndQuick`。
7. 冲刺结束后**保留冲刺速度**，由移动状态平滑降速。用户看了两种交接的效果说明后说"这个其实我觉得效果更好"，取代原来交接 `GetTargetSpeed()` 的方案。
8. 冲刺键的判断放在移动状态 Update 开头并直接 return。用户认为挪到最后"会有一个问题，就是操作延迟"。
9. 攻击的退出参考原神："在前期可以给一个可以进入闪避的时间，然后再给一个相对后期的可以正常进入移动状态的时间"，相当于闪避可以取消一部分后摇。
10. 自己调整玩家 Animator：三段连段改为按键立即切换，`AttackEnd` 事件放到每段判定帧之后，每段播完直接回待机。之后让 Claude Code 把 Attack 层的内容搬到 Base Layer："你只搬运 不新加内容……这是纯体力活"。
11. 自己发现攻击状态里的 `return` 让退出判断执行不到（"我刚刚停止你的思考 去改就是发现了这个问题"）；改用标签判断时，Tag 已经加了但没保存。
12. 认为攻击状态里 `canInput`、光标、死亡三个检查多余："玩家如果进入无法接受输入的状态，一定会在外部执行更改状态"，只有按住 Alt 显示光标时会出问题，"换个地方就好了"。
13. 冲刺中死亡后速度残留，选择在死亡状态的 Enter 里清零。
14. 动作上有很多 `SwitchSocket` 事件："这是素材自带的 我不知道有什么用"，让 Claude Code 全删。

## 辅助分析与建议（Claude Code）

### 1. 组件拆分和每帧一次移动

- 控制组件要挂在 Animator 所在的物体上：动画事件会发给这个物体上的所有脚本，没有接收方会报错，同名方法会都被调用。同一物体上各组件 `Awake` 的顺序不固定，`Awake` 里只取本物体的组件。
- 每帧只调用一次 `CharacterController.Move`：各状态通过 `SetHorizontalVelocity` 设置水平速度，控制组件最后叠加重力统一移动。`isGrounded` 只反映最近一次 `Move` 的结果，分开调用会让重力判断出错。

### 2. 进入、退出和转换动作

- 只和某一对状态相关的逻辑（冲刺结束交接速度）写在决定切换的那一行之前，不在 Exit 里判断下一个状态。这样被别的状态打断时不会带着旧速度。
- `MoveWeight` 是动画曲线驱动的参数，每帧重新读取、脚本改不了，Exit 里不用处理。

### 3. 冲刺：动画事件和计时

- 依据 `S_QuickShift_F.fbx.meta`（`EndQuick` 在 20.8%，片段 30 帧）和 `Cyrene.controller`：动画事件只表示"动画播到了这一帧"，不知道状态机现在在哪个状态。冲刺动画被打断时状态会出不来；有了死亡状态后，事件还可能把角色从死亡状态拉回移动。敌人出招用 3 秒超时兜底，是同一类问题。
- 用户改成计时后、`EndQuick` 删掉之前，审查发现两者并存的问题：计时 0.2 秒结束、事件约 0.208 秒到（按 30 帧/秒推算），计时先到时，事件会再切一次移动状态；`StateMachine` 切到同一状态会重新执行 Exit / Enter，把刚交接的速度清零，冲刺后的效果时有时无。建议删掉事件，`StateMachine` 不加同状态拦截（#46 里怪物重复受击要靠重新进入）。
- 第一版移动状态在 `ChangeState` 之后没有 return，同一帧的 `Move` 把冲刺速度从 10 拉到 9.5（不按方向为 9.25，按 60 帧/秒推算），冲刺速度会随帧率变化，`Velocity` 也被改回 1.9。用户随后加了 return。
- 对想法 8：两种写法都在按下的那一帧调用 `ChangeState`、设置 `Quick`，冲刺位移都从下一帧开始，差别只在按下那一帧有没有水平位移，不存在多一帧的操作延迟。用户保留 return。

### 4. 冲刺后保留冲刺速度

- `Velocity` 会超过 1（最高 2）。Blend Tree 阈值是 0 / 0.5 / 1，超过 1 用跑步动作，目前也没有过渡条件用到 `Velocity`，所以 `Clamp01` 不是必须的。这一点更正了 #46 原来"改成 `dashSpeed` 必须加 `Clamp01`"的说法。
- 冲刺速度写在闪避的 Enter 里，冲刺被死亡打断时 `currentSpeed` 停在 10，复活后会在起身过程中滑出约 1.1 米（按减速度 45 推算）。用户选择在死亡状态的 Enter 里清零。

### 5. 重构中间状态的提醒

原来 `PlayerObject.Update` 开头的 `if (isAttacking || health.isDead) return;` 不再起作用后，攻击中、死亡后移动状态照样运行。依据各动作 `.meta`：`MoveWeight` 只有冲刺动作有曲线，其余动作回到默认值 1，所以攻击中、死亡后都能走动、转身和冲刺。做完攻击和死亡状态后这些都消失了。

### 6. 攻击什么时候退出：取消窗口

- 常见做法：动作游戏把一招按进度分成前摇、判定、后摇，给每招配"从多少进度起可以被什么取消"的窗口（frame data / cancel window），闪避一般最早、移动最晚，常配输入缓冲。Unreal 常用 AnimNotifyState 标区间；Unity 没有区间型动画事件，常见的是拿动画进度对照每招的数据，或用成对的动画事件。
- 攻击状态的出口：窗口内按攻击留在攻击状态、闪避窗口内按冲刺、移动窗口内有方向输入、后摇自然播完。
- 建议窗口时间用数据配置并读动画进度，理由：攻击动作的 `.meta` 不入库；接下一段的过渡有 0.25 秒交叉淡化，Unity 过渡期间旧片段的事件照样触发；事件不知道状态机的状态。移动窗口不能早于 `AttackEnd`，否则按着方向键连点时还没接上下一段就被移动取消。
- Attack 层是覆盖模式、权重 1、没有遮罩，代码取消后这一层还会盖住冲刺和走路，所以 Animator 要跟着离开攻击动作；推荐按 #46 把攻击并进 Base Layer。

### 7. 攻击状态卡住的原因

- 第一版：`AttackEnd` 之后 `Attacking` 为 false，每帧进入 `if (!Attacking)` 后在块尾 return，后面的退出判断永远执行不到。
- 用户删掉 return 后用 `GetCurrentAnimatorStateInfo(0).normalizedTime >= 1` 仍不行：刚进攻击时 Animator 还在从 Blend Tree 过渡，过渡期间当前状态是起点 Blend Tree，循环状态的整数部分是循环次数，站过一轮待机就大于 1；淡出时 S_Attack2 的进度约停在 0.999，当前状态换成 Blend Tree 后要等待机动作播满一轮（约 6 秒）。
- 建议给三段攻击加标签，按"先看到攻击开始，再看到它离开"判断。用户的写法把退出判断放在 `if (!Attacking)` 里，`AttackEnd` 只有在攻击真的播起来后才会触发，作用和"先看到开始"相同，比建议的写法更简单。

### 8. 输入检查放在哪

对想法 12 基本同意，补充两点：对话并不会切换玩家状态，但对话打开时 `CloseInput()` 会关掉整套输入，所以 `canInput` 确实多余；死亡要等死亡状态接上才由外部切换。光标检查建议放在 `KeyDetection` 里，算出 `AttackPressed`，只有攻击需要排除光标模式，因为它和点 UI 共用鼠标左键。

### 9. Animator 合并后的变化

- 只有 Null 需要对应：它在 Attack 层表示"不播动作、露出下层"，搬到 Base Layer 后对应 Blend Tree，"Null → S_Attack1"和三条"回 Null"都改为接 Blend Tree，其余条件、退出时间、过渡时长和打断设置不变。合并前的文件已备份并逐行对比。
- 攻击只能从 Blend Tree 开始：冲刺收尾时按攻击，动画要等回到 Blend Tree 才出来，最多晚约 0.2 秒（推算）。#46 定过的 S_Quickshift_F → S_Attack1 过渡按用户"不新加内容"的要求没有加。
- 冲刺和死亡的 Any State 过渡和攻击在同一层，出刀时死亡会立刻播 S_Die。

### 10. SwitchSocket 和控制台

- `SwitchSocket` 由素材包的 `Character_Weapon_Controller` 接收，用来在背上和手上的挂点之间切换武器（`To_Hand_R_Socket` 即挂到右手）。玩家身上没有这个组件，原来 `PlayerObject.SwitchSocket` 是空方法；用户删掉这个方法后，S_Idle 每循环一次都会报 "has no receiver"。只删了 `Resources/Animation` 下项目使用的 19 个副本，素材包原版 36 个留给它自己的演示预制体。
- 控制台：恢复初始值不再调用已删除的 `SetIsAttacking`；自动检查改为直接取 Animator 组件。第一次自动检查在第 36 项中断：怪物脱战检查读的 `EnemyAI.isReturning` 在敌人改为状态机（#42）后已不存在，原来"所有怪物都返回"的判断也和现在"只有追击、出招中的返回"的规则不一致。改为读敌人状态机，并新增玩家状态机死亡、复活两项。

## 实际采纳与用户调整

- `PlayerController`（挂在 Animator 所在物体）：持有 `StateMachine` 和四个状态；移动参数 `walkSpeed` 2.5、`runSpeed` 5、`acceleration` 30、`deceleration` 45、`rotationSpeed` 720、`dashSpeed` 10、`dashDuration` 0.2、`gravity` −9.81；`KeyDetection` 每帧读输入（`MoveInput`、`HasInputMove`、`IsRunning`、`AttackPressed`，后者排除光标模式）；`GetTargetSpeed()` 返回 0 / 2.5 / 5；`SetHorizontalVelocity` 加重力后每帧一次 `Move`；在 `OnEnable` / `OnDisable` 订阅 `OnPlayerStateChange`，死亡切到死亡状态、复活回到移动状态；接收 `AttackEnd` 事件并把 `Attacking` 设为 false。
- `PlayerMovementState`：先判断冲刺键（切到闪避并 return）和 `AttackPressed`（切到攻击并 return），再转向、移动、写 `Velocity`；Exit 清零 `currentSpeed` 和 `Velocity`。移动仍乘 `MoveWeight`，第二步再删。
- `PlayerDodgingState`：Enter 设计时 `dashDuration`、`currentSpeed = dashSpeed`、触发 `Quick`；Update 沿当前朝向施加速度、不转向，计时到 0 切回移动，速度带入移动状态后平滑降速。
- `PlayerAttackState`：Enter 调用 `PlayerCombat.Attack()`；`Attacking` 为 false（`AttackEnd` 之后）时，按攻击接下一段、按冲刺切到闪避，当前 Animator 状态不带 `Attack` 标签时回到移动。没有 Exit 清理，第二步加受击、死亡打断时再补。
- `PlayerDeadState`：Enter 清零 `currentSpeed`、设置 `IsDead`；Exit 取消 `IsDead`。
- `PlayerObject` 只保留属性、等级、Buff 和 `InitPlayer`；`PlayerCombat` 只保留 `OnAttackCheck` 和设置触发器的 `Attack()`。`SetIsAttacking`、旧 `HandleMovement`、`Quick` / `StartQuick` / `Dash`、`SwitchSocket` 都删除。`PlayerObject` 里还剩 `animator`、`controller`、`dashDirection`、`moveWeight` 四个没用到的字段（后两个有"从未使用"的编译警告）。
- 玩家 Animator：只有 Base Layer。Blend Tree 按 `Atk` 进 S_Attack1，三段之间按 `Atk` 立即切换（不等退出时间），每段 0.82～0.88 处回 Blend Tree；三段攻击带 `Attack` 标签；S_Put 只剩 Any State 的 `PutWeapon` 入口（代码里没有设置）；删除了未用的 `New Bool` 参数。`AttackEnd` 事件位于三段攻击的 0.33 / 0.29 / 0.41 处和 S_Put 的 0.19 处（本地 `.meta`）。
- 取消窗口：本步只做了"`AttackEnd` 之后可以闪避"，移动取消窗口和"窗口用数据还是动画事件"还没定，归入 #46。

## 验证与当前状态

- **用户实测**（当次聊天报告，无截图和日志）：
  - 11-28：冲刺距离、冲刺后减速每次一致，冲刺中不能转向（"测试没问题了"）；攻击挥完能回到移动状态（"测试没问题了，可以正常回到移动状态"）。
  - 11-29：#45 的验收项全部通过（"测试全部通过"）：三段连段；按住 Alt、对话中点左键不出刀，对话结束不多砍一刀；冲刺中按攻击不出刀、结束后能攻击；移动中、出刀时、冲刺中被控制台杀死后复活都能正常移动、攻击、冲刺，冲刺中被杀的那次复活后不滑；被追击时死亡，怪物返回。
- **运行证据**：控制台 Play Mode 自动检查 148/148 通过（11-29 00:12），包括新增的"杀死玩家后玩家状态机切到死亡状态""复活后回到移动状态"。这次杀死玩家时场上那只怪在巡逻，"追击中的怪物返回"由上面的用户实测覆盖。
- **静态核验**：各版代码由 Claude Code 逐版审查；Animator 合并后校验了文件内引用，和备份逐行对比；19 个 `.meta` 和备份对比，只少了 `SwitchSocket` 一段，其余动画事件（`AttackEnd` 4 条、`OnAttackCheck` 4 条、`OnAttackEnd` 1 条）都在。
- **已知、留到 #46**：冲刺后顿一下；冲刺收尾按攻击最多晚约 0.2 秒（推算，未单独测）；出刀后、判定帧前死亡时，过渡到 S_Die 的 0.25 秒里旧攻击的 `OnAttackCheck` 可能仍会触发（静态分析）；攻击状态的 Exit 清理；移动取消窗口。
- **没有提交的本地改动**：`EnemyAnimator.controller` 一个节点挪了位置；GMScene 里关闭 AI 的千人律者 (1) 多了 `patrolPoints` 清空的覆盖，用途没有确认，没有带入提交。

## 论文素材与证据入口

- 可以说明的设计取舍：角色数据和控制拆成两个组件；状态模式里进入、退出、转换三类动作的分工；用动画事件还是计时结束一个玩法状态，以及事件"不知道当前状态"的问题；冲刺后保留速度和交接目标速度的手感取舍；攻击的取消窗口和 Animator 与代码状态的同步。
- 代码：`Assets/Scripts/Object/Player/PlayerController.cs`、`Assets/Scripts/Object/Player/State/`、`Assets/Scripts/FSM/StateMachine.cs`；Animator：`Assets/Resources/Animator/Cyrene.controller`；自动检查：`Assets/Editor/DevConsoleSetup.cs`。
- 动画事件在本地 `.meta` 里，不入库，清单见 [ASSET_RESTORE](../../ASSET_RESTORE.md)。
