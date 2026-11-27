# 变更记录

按日期倒序记录每次交付的内容、对应提交和验证范围，每条一两句话，细节放在链接的开发记录里。"待回归"表示还没有在 Play Mode 里重新验证。

## 2026-11-28

- **敌人攻击状态（#42 第二步）**（`8d075ec`）：用户完成，Claude Code 讲解和审查。追击进入出招距离后切到攻击状态：停下导航、出招前转向玩家；出招期间不切状态、不转向，收招动画事件 `OnAttackEnd` 清掉 `IsAttacking`，3 秒超时兜底。脱战起点改为只在巡逻、等待发现玩家时记录，攻击回追击不再重置；Animator 的 `velocity` 参数改在 `EnemyAI.Update` 每帧同步。用户实测四项通过：出招时走开会挥完再追、前摇后退打空、站着不动每 2 秒出招且先转向、出招中不跟着转。[记录 58](Docs/Development/records/58-enemy-attack-state-and-ranges.md)
- **攻击距离的三个数**（`e97fdee`，预制体为本地文件）：按用户的方案排成停止距离 1.3 < 出招距离 1.4 ≤ 命中半径 1.4（`EnemyAIConfig.attackRange` 原为 2）。Claude Code 推算玩家站着不动时实际能被打中的距离约 1.62 米，并指出出招距离是全局一份、命中半径按敌人配置，新开 #44，用户计划次日修改。[记录 58](Docs/Development/records/58-enemy-attack-state-and-ranges.md)
- **配置按 id 查找与任务目标名**（`eaf29fe`、`e97fdee`）：用户完成。`GetCharacterData` 原来按下标取，敌人表两项 id 都是 0 却"刚好对上"；改为按 id 匹配，角色配置新增 `name`。敌人表新增千人律者（id 1）及奖励，任务 0 的战斗目标改为千人律者；任务面板显示敌人名，对话目标改为按 id 查 NPC。改为按 id 后奖励和任务计数没有单独复测。[记录 58](Docs/Development/records/58-enemy-attack-state-and-ranges.md)
- **敌人 Animator 与场景**（`4508e5b`）：用户修改。敌人 Animator 改为待机、跑步、攻击三个状态（僵尸动作，动作文件为本地素材）；GMScene 放入两只千人律者（第二只关闭 AI 当木桩），停用原来的胶囊敌人，玩家 CharacterController 的场景覆盖改为高 1.46、半径 0.26。千人律者预制体、FBX 和僵尸动作不入库，已登记 ASSET_RESTORE。[记录 58](Docs/Development/records/58-enemy-attack-state-and-ranges.md)

## 2026-11-27

- **敌人人形模型转换（本地素材，仅文档入库）**：Antigravity 通过 Blender 5.2 + `mmd_tools` 将模之屋素材 `千人律者1.0.pmx` 转换为 Unity 可用 FBX，输出到 `Assets/Resources/Prefab/千人律者/`（含 4 张贴图与 `使用规则.txt`，已登记 ASSET_RESTORE）。处理了双套腿骨与辅助骨清理（合并 `下半身` 到 `Hips`，保留 63 根骨骼并改为 Unity 标准人形骨名）、材质重建为 Principled BSDF，并剥离全不透明贴图的冗余 Alpha 通道。仅完成 Blender 回导与正面渲染核验、Unity 生成 `.meta`；Humanoid 映射、Scene 显示与预制体接入待验证（#43）。[记录 57](Docs/Development/records/57-enemy-model-pmx-to-fbx.md)
- **敌人 AI 改为状态模式的状态机**（`ed3be12`）：用户完成，Claude Code 讲解和审查。新增通用的 `IState` / `StateMachine`；巡逻、等待、追击、返回、死亡各一个状态类，原来的 bool 组合删除。玩家死亡或复活时改为只让追击中的怪物返回（用户决定，原来是所有怪物）；`CharacterHealth` 新增 `OnDeath`，怪物由它进入死亡状态。用户实测巡逻、两种脱战和返回、死亡流程正常。复用入口 `EnemyObject.Spawn` / `EnemyAI.ResetAI` 已写、未接入（#26）；攻击状态是 #42 第二步。[记录 56](Docs/Development/records/56-enemy-fsm-state-pattern.md)
- **场景与配置**（`567d5c9`、`25b17bf`）：用户修改。GMScene 中敌人的 NavMeshAgent 停止距离改为 1.4；任务 0 的奖励新增星屑药剂 ×5。[记录 56](Docs/Development/records/56-enemy-fsm-state-pattern.md)
- **任务奖励格显示道具图标**（`1800b4a`）：用户完成，Claude Code 审查。`RewardSlot` 有图标时只显示图标，没有图标时关闭 Image、显示名称；删掉了会把图标撑到原图大小（512×512）的 `SetNativeSize()`。本地 `RewardSlot` 预制体由 Claude Code 调整：Icon 固定 60×60、Simple + Preserve Aspect，NameText 移到和 Icon 同级。编译通过；推送后用户实测确认没问题，关闭 #41。[记录 55](Docs/Development/records/55-reward-slot-icon.md)

## 2026-11-26

- **Buff 三选一：随机抽取与选择次数**（`79bc439`）：用户完成，Claude Code 审查。`StartRandomBuff` 按重抽法从候选池抽 min(3, 候选池) 个；`AddChoiceCount` / `SelectBuff` 管理剩余选择次数，`AddBuff` 不再扣次数；`onChoiceCountChange` 带次数参数；`ClearAll` 同时清零次数和候选。次数为 0 由 UI 入口把关。控制台自动检查 146/146 通过。[记录 52](Docs/Development/records/52-buff-choice-random-and-panel.md)
- **Buff 三选一面板**（`79bc439`、`ab7e11e`）：预制体和 9 张界面图片由 Claude Code 按用户给的参考图生成（`Assets/Editor/BuffPanelSetup.cs`）；`BuffPanel`、`BuffOption` 脚本和 GamePanel 的 Buff 按钮由用户完成，Claude Code 审查。审查中修复了 GamePanel 没有退订次数事件（实测报过 `MissingReferenceException`）、新建时没有读取当前次数、候选池为空时打开面板会让 GamePanel 丢失。按用户决定，“你已经足够强了”的提示改用以后的通用浮动文字。用户实测 4 个流程通过。[记录 52](Docs/Development/records/52-buff-choice-random-and-panel.md)
- **面板淡入淡出期间禁止点击**（`5703c28`）：用户自己加的，`BasePanel` 淡入淡出期间设置 `CanvasGroup.interactable = false`。没有专门测试。[记录 52](Docs/Development/records/52-buff-choice-random-and-panel.md)
- **控制台三选一区**（`d2988af`）：Claude Code 修改。Buff 页可以加次数、抽候选、点候选调用 `SelectBuff`；自动检查新增三选一 23 项，共 146 项全部通过。[记录 52](Docs/Development/records/52-buff-choice-random-and-panel.md)
- **道具表新增 5 个道具**（`2c00b4d`）：ID 3～7 为大瓶星屑药剂、回响残片、刻名钥匙、维修绞索、星图残页，名称、类型、上限和描述由 Claude Code 按用户要求编写，并绑定新图标。只有静态核验和编辑器导入日志，背包显示未测（#39）。[记录 53](Docs/Development/records/53-ai-generated-assets-and-item-config.md)
- **生图素材导入**（图标不入库）：Claude Code 编写提示词清单并逐批给出筛选建议，Codex 生成 61 张候选。选中的 Buff 图标 3 张、道具图标 5 张由 Claude Code 处理后导入本地 `Resources/Icon`，在 ASSET_RESTORE 登记来源；主视觉、特效贴图、概念图和守桥人立绘只保存在本地素材目录。[记录 53](Docs/Development/records/53-ai-generated-assets-and-item-config.md)、[记录 54](Docs/Development/records/54-ai-2d-asset-generation.md)
- **剧情设定补充**（只有文档）：用户确定守桥人为年龄不明的男性老者；区域概念图之间的空间关系对不上，按 Claude Code 的建议统一为"村口 → 旧桥 → 桥尾台阶 → 塔前庭院 → 观星塔"，写入剧情设计文档。[记录 53](Docs/Development/records/53-ai-generated-assets-and-item-config.md)
- **公开前整理仓库**（只有文档）：用户准备投简历，打算把仓库公开，让 Claude Code 先整理。README 改为面向面试官的首页，新增可演示内容、架构图、一次击杀的数据流、代码导读、AI 分工表和素材说明，原有的功能状态表、环境和运行步骤保留；模拟面试记录 `Docs/Interview` 移到私有的面试准备仓库。没有改代码；仓库可见性由用户自己修改。[记录 51](Docs/Development/records/51-pre-public-repo-cleanup.md)

## 2026-11-25

- **Buff 核心：配置、计时与属性加成**（`968b003`、`6b08720`）：用户完成，Claude Code 审查。`BuffDatabase`（SO）配置 Buff；`BuffMgr` 添加时计时、移出候选池，到期减回加成、回到候选池，`ClearAll` 全部清除；加成百分比存在 `CharacterStats`，按白值 × (1 + 加成) 重算，升级时保留 Buff。测试配置星锋、星盾、星息由 Claude Code 按用户要求填写。控制台自动检查 123/123，用户实测背包打开时暂停、20 秒自然到期。次数、抽取和面板未实现（#16）。[记录 50](Docs/Development/records/50-buff-core-timer-and-stats.md)
- **MonoMgr 与 TimerMgr**（`edcc01c`）：用户提出并完成。MonoMgr 把 Update 转发给普通单例；TimerMgr 按 id 计时，受 timeScale 影响，先收集到期 id 再回调。[记录 50](Docs/Development/records/50-buff-core-timer-and-stats.md)、#36
- **控制台 Buff 页**（`d8cb9e7`）：Claude Code 修改。点击添加 Buff、显示剩余时间和公式核对、清空；“恢复初始值”改为调用 `UpdateStats`，不再因 `Init` 丢掉加成；自动检查新增 30 项。[记录 50](Docs/Development/records/50-buff-core-timer-and-stats.md)
- **Buff 设计调整**（只有文档）：用户决定入口改为 GamePanel 按钮、到期的 Buff 回到候选池、剩余次数随保存点保存。[记录 48 补充](Docs/Development/records/48-temporary-buff-design.md)

## 2026-11-24

- **属性改 float，生命上限按比例保持**（`505ebc6`）：用户完成。攻击、防御、生命改为 float；`UpdateStats` 先算比例，再换上限、设当前生命，最后广播一次。排查"打怪后生命有时变 0"：原因是 `AddExp` 没升级也会广播，按比例设生命时用的是默认比例 0 或旧比例。用户改为在 `LevelUp` 里广播。敌人血条按比例填充，玩家血量文字向上取整。控制台自动检查 93/93 通过，关闭 #31；真实打怪升级、HUD、敌人血条和读档的实机回归见 #33。[记录 49](Docs/Development/records/49-float-stats-and-hp-ratio.md)
- **控制台适配 float**（`40447ef`）：Claude Code 修改。反射写入和事件类型改为 float，数值支持小数；修复等级输入在定时刷新时被改回旧等级（11-16 起就存在）；提示和自动检查改为按比例保持，新增"设置等级后按比例保持"和"最后一次广播等于最终生命"两项检查。[记录 49](Docs/Development/records/49-float-stats-and-hp-ratio.md)

## 2026-11-23

- **三选一临时 Buff 设计定稿**（只有文档）：用户设计、Claude Code 评估。只在主线副本内获得，获得时强制三选一，限时，按白值百分比加攻击、防御、生命上限；由管理器决定选项、面板只负责显示。拆出前置 #31（属性改 float、生命上限按百分比保持）和 #32（副本保存点、复活次数与重置），Buff 定稿写在 #16 的评论里。剧情草案同步改为限时。没有改代码。[记录 48](Docs/Development/records/48-temporary-buff-design.md)

## 2026-11-22

- **README 加入 AI 辅助说明**（只有文档）：开头用一句话说明玩法代码由作者编写，Codex 与 Claude Code 只用于代码审查、调试与编辑器工具、文档和仓库管理，避免把 GitHub 贡献者列表里的 `claude` 误读为参与了玩法开发。

- **背包回归与扣除事件顺序**（`f83d9e1`）：#29 四项用户复测通过；扣到 0 后详情区不切换，原因是 `RemoveItem` 先广播后删除记录，用户改为删除、存盘后再广播，并撤掉了界面侧的临时补丁。关闭 #29。[记录 47](Docs/Development/records/47-bag-regression-and-remove-event-order.md)
- **控制台复测**（只有文档）：开关按钮下移后自动检查 84 项全部通过，包括开关按钮不压住 HUD 按钮、奖励结算金币进背包；用户手动确认控制台关闭时背包按钮可正常点击，关闭 #30。[记录 46](Docs/Development/records/46-devconsole-tabs-and-item-debug.md)
- **控制台重构为分页界面并加入道具调试**（`7269797`、`b3c303d`）：控制台改为运行时由代码生成，分生命、属性·等级、道具、存档四页；道具页可从列表选择或直接输入 ID，调用 `PlayerItemMgr` 加减或经 `RewardMgr` 结算；新增清空背包道具，清空按钮需两次确认。用户在 11-21 版本上运行自动检查 82 项全部通过；之后开关按钮下移避开背包按钮，并跟进上限截断与背包自刷新，这部分只做了编译，待复测（#30）。[记录 46](Docs/Development/records/46-devconsole-tabs-and-item-debug.md)
- **道具上限、奖励进背包与任务发奖**（`be4eb71`、`5a511b4`）：用户补上 `maxCount` 截断（-1 不截断）、`RewardMgr` 非经验类型进背包、`RewardSlot` 读道具名，并完成任务结算发奖（先标记完成再发奖）。用户实测上限、打怪掉落、任务奖励到账与不重复发奖均通过，关闭 #13。销毁按钮不做，使用消耗品延后。[记录 45](Docs/Development/records/45-item-cap-reward-and-mission-settlement.md)
- **用户实测登记：背包详情区**（只有文档）：默认显示第一个格子、点击切换、清空后为空，三项用户自测通过，没有截图或日志。[记录 44](Docs/Development/records/44-bag-detail-panel.md)
- **背包详情区**（`09cd4ec`）：用户完成点击格子显示物品名称、数量、描述和图标，默认显示第一个格子，空背包时清空。排查了"数据正常但不显示"：详情区继承了 BasePanel，被自动添加的 CanvasGroup 淡出到透明，改为 MonoBehaviour 后解决。只做了编译，待 Play Mode 验证。[记录 44](Docs/Development/records/44-bag-detail-panel.md)

## 2026-11-21

- **背包显示持有道具**（`6834c6d`、`ee723ed`）：用户完成格子生成、金币栏显示、订阅数量变化事件整表刷新并在销毁时取消订阅；道具表金币类型改为 money，新增 2 号星屑药剂。只做了编译，背包开着时的刷新待 Play Mode 验证；上限、选中详情、使用按钮未做。[记录 43](Docs/Development/records/43-bag-slot-display-and-refresh.md)
- **星屑药剂图标定稿**（仅文档入库）：用户选定 A 版青蓝圆肚瓶，作为计划道具 ID `2` 的本地图标。设计取舍和验证范围见 [记录 42](Docs/Development/records/42-stardust-potion-icon.md)。
- **金币图标定稿**（仅文档入库）：用户选定 A 版观星币，透明底 PNG 已保存到本地资源目录。界面绑定和显示验证列在 [记录 41](Docs/Development/records/41-gold-coin-icon.md) 与 #25。

## 2026-11-18

- **用户实测登记：移动、对话与两阶段任务**（只有文档）：用户在 `aeda051` 上按检查点自测移动与冲刺、对话输入与完成后对白、任务 0 的完整流程，全部通过，关闭 #10、#8、#7。Shift 先冲刺、冲刺后转入奔跑按用户说明记为设计意图。没有截图或日志，只有用户确认。[记录 40](Docs/Development/records/40-user-playtest-movement-dialogue-mission.md)

## 2026-11-17

- **控制台等级调试与敌人测试攻击力**（`4bff8ce`、`1af6688`）：按用户 11-16 的建议，控制台加入"设置等级 / 重置为初始等级"，写入存档并调用用户的 `PlayerLevelChanged`；Play Mode 自动检查 56 项全部通过（11-16 运行）。敌人攻击力按用户确认提交为测试值 50。[记录 39](Docs/Development/records/39-devconsole-level-controls.md)

## 2026-11-16

- **背包与持有道具开始实施**（`be23363`、`e3e0ec4`）：用户编写 `PlayerItemMgr`（读档、增删查、每次变化存盘、广播新数量、数量为 0 时删除记录）和 BagPanel、BagSlot 脚本，GamePanel 新增背包按钮。Claude Code 按参考图拼接背包预制件并提供生成工具，修复了关闭按钮文字没有开启射线检测导致点不到的问题。只做了编译和静态检查，关闭按钮待复测；上限、货币、格子生成与详情还没做。[记录 38](Docs/Development/records/38-inventory-ui-and-item-mgr-start.md)
- **背包与发奖设计定稿**（只有文档）：用户指出要先做背包再做发奖，并完成了设计：新建玩家持有道具管理器负责数据和存档，背包面板只负责显示；只存持有的物品；`maxCount` 为 -1 表示无上限，货币单独显示。结论和参考类图写在 #25，尚未实施，没有代码改动。[记录 37](Docs/Development/records/37-inventory-and-reward-design.md)
- **控制台复测**（`9209b36`、`9dbf9b7`）：控制台改用玩家的光标模式，消除与 CameraCursorLock 的冲突；Play Mode 自动检查扩充到 49 项并全部通过，用户手动复测按键、伤害、清空任务存档正常。撤回清单五项至此全部完成。[记录 36](Docs/Development/records/36-devconsole-retest.md)
- **UIManager 恢复统一由预制体创建**（`c166a09`、`d345388`）：删除 10-30 为适配调试场景加入的两段接管逻辑，GMScene 删除调试用的 Canvas 和 MissionPanel，界面全部由 UIManager 在运行时创建，关闭时销毁。用户实测通过。[记录 35](Docs/Development/records/35-uimanager-prefab-only.md)

## 2026-11-15

- **模拟面试记录**：新增 Docs/Interview（11-26 已移到私有的面试准备仓库，见当天条目），针对当天的死亡流程、奖励与升级提交出了 6 题，保留用户回答和点评；记录 34 追加用户补充：主流程在 Codex 阶段就已设计，属性计算的改法是用户自己实现的。
- **怪物奖励、升级与属性层**（`c42cf15`、`84042d5`）：新增道具表、怪物奖励表和统一发奖方法；杀怪获得经验，升级后由 CharacterStats 重新计算攻击、防御、生命上限并更新生命组件；等级显示与玩家存档。用户实测通过，满级未测。[记录 34](Docs/Development/records/34-reward-level-and-stats.md)
- **控制台属性适配**（`accde67`）：属性改为读写 CharacterStats，显示等级。

- **玩家最小死亡流程**（`a22e86e`、`c11d394`）：血量归零后广播死亡，锁住移动、闪避、攻击，播放 S_Die；怪物在玩家死亡和复活时都脱战；复活后回满并刷新血条。用户实测确认了死亡和复活，怪物脱战待复测。[记录 33](Docs/Development/records/33-minimal-death-flow.md)
- **控制台适配**（`8117965`）：改为调用玩家的 `Dead()`、`Respawn()`，改血后刷新血条。控制台从这次起由 Claude 维护，不改动玩法脚本。

## 2026-11-14

- **Alt 光标模式与任务面板暂停**（`2b95693`、`41c9683`）：PlayerMgr 统一管理光标模式并通过事件通知；按住 Alt 或对话时不出刀，CameraCursorLock 锁住镜头；任务面板打开时暂停，UI 淡入淡出不受暂停影响。只做了静态审查和编译，最终版本待回归。[记录 32](Docs/Development/records/32-cursor-mode-camera-and-panel-pause.md)
- **文档与仓库管理**（`e284f9f`、`1672b06` 和随后的文档提交）：README 改为项目首页和功能状态表；新增本文件；STATUS 压缩为当前状态，原全文归档到 [history](Docs/Development/history/STATUS-2026-10-15_2026-11-14.md)；资源补回说明改为清单表；自编的 NPC 配置和 NPC 对白改为入库；待办迁到 GitHub Issues。协作规则改为 Codex 与 Claude Code 共用。
- **场景接线**：一起提交 GMScene 中 GamePanel 实例的 `playerHealthBar` 引用和 TestDialogue 的序列化字段，这两处是接血条时 Unity 保存下来的，经用户确认。
- **Issue 决定**：#12 演示用的重复配置只在本地存在过，已关闭；#20 用户决定暂时继续使用 LitJson。
- **玩家血条刷新**（`e305f25`、`3b6f590`）：血条在显示时拉取当前血量，并订阅 `OnHealthChange` 事件，修复了面板重建后显示满血、面板隐藏时受伤报空引用两个问题。只做了静态审查和编译，待回归。[记录 31](Docs/Development/records/31-player-health-bar-refresh.md)
- **协作规则**（`328bf7d`）：增加"提交推送默认流程"，CLAUDE.md 导入 AGENTS.md。

## 2026-11-11

- **玩家成长原型**（`3369c24`）：在 SO 里配置成长参数，新增 PlayerGrowth 和 PlayerSaveData（等级、经验），攻击力按等级计算。升级和存档恢复没有运行验证。[记录 30](Docs/Development/records/30-player-growth-and-save-design.md)
- **撤回控制台配套代码**（`9722ca3`、`a535623`）：恢复角色初始化和原血条实现，列出需要用户接回的待办。[记录 29](Docs/Development/records/29-console-support-code-removal.md)

## 2026-11-09

- **对话输入控制和完成后对白**（`bd2696a`、`32172ad`、`d366cf3`）：对话期间禁止攻击，淡出后恢复输入；新增任务完成后的对白；交付对白按当前阶段加载。待回归。[记录 28](Docs/Development/records/28-dialogue-input-and-finished-dialogue.md)

## 2026-11-08

- **缓存池**（`1834220`）：增加容量配置和满额轮换，还没有接入方。[记录 27](Docs/Development/records/27-pool-capacity-and-usage-contract.md)
- **剧情草案**（`0ac49a0`）：归档世界观与主线剧情 V1。[草案](Docs/Design/世界观与主线剧情-V1.md)

## 2026-11-06

- **NPC 对话与两阶段任务**（`c42e414`、`038f334`、`e02c2b3`）：击杀推进任务阶段、按进度播放对白、交付时结算存档。本机存档已写入 `isFinished`，完整流程待复测，实际发奖未接入。[记录 26](Docs/Development/records/26-npc-dialogue-and-mission-turn-in.md)

## 2026-11-05

- **敌人原型**（`3d628f8`、`5efe3b0`、`8e4b812`）：NavMesh 巡逻、追击、脱战返回、攻击冷却、死亡停机。编辑器日志记录了双方命中。[记录 25](Docs/Development/records/25-enemy-ai-and-combat.md)

## 2026-10-30

- **任务系统和调试 UI**（`9378e3f`、`ffd3bad`、`0111d2b`、`0759a89`）：任务 SO、玩家任务进度 JSON、任务面板、生命 HUD、交互提示预热、管理员控制台。[记录 22–24](Docs/Development/records/22-mission-system-design-and-prototype.md) · [交付清单](Docs/Development/deliveries/2026-10-30.md)

## 2026-10-26 至 10-28

- **配置、生命值与血条**（`d45389e` 至 `ac1be45`）：统一管理器单例、NPC 距离交互、对象池、角色属性 SO、生命组件、世界空间血条和球形命中原型。[记录 15–18](Docs/Development/records/17-character-config-health-and-hit.md) · [交付清单](Docs/Development/deliveries/2026-10-28.md)

## 2026-10-16

- **基础对话**（`f98e78f` 至 `6fce39e`）：线性对话推进和动态选项，用户测试确认本轮流程完成。[记录 14](Docs/Development/records/14-dialogue-system-implementation.md)

## 2026-10-15

- **首次入库与阶段整理**（`d09e52d` 至 `d6811ce`）：建立源码与配置基线；移动平滑、冲刺权重、武器挂点；清理旧测试脚本；加入攻击状态接口、UI 和对话骨架；场景重命名为 GMScene。[阶段总结](Docs/Development/records/12-stage-summary-20261015.md)
