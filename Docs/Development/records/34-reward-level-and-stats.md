# 怪物奖励、升级与属性层

2026-11-15。玩法代码提交为 `c42cf15`，控制台为 `accde67`，配置为 `84042d5`。涉及 ItemDatabase、EnemyRewardDatabase、ItemRewardData、RewardMgr、PlayerGrowth、PlayerMgr、JsonMgr、ConfigMgr、CharacterStats、CharacterObject、CharacterHealth、PlayerObject、EnemyHealth、GamePanel、DevConsole。辅助工具为 Claude Code（下文写作 Claude）。对应 #3，同时推进了 #6、#13、#25 的一部分。

## 用户观察与目标

#3 的原始描述是：升级后把生命上限和防御应用到生命组件，并决定当前血量怎么调整。

开工前读代码发现，`AddExp` 和 `SavePlayerData` 都没有调用方，`LevelUp` 先加等级、后扣经验，所以游戏里实际上还不会升级。用户在推进过程中逐步加了几个目标：

- 在界面上显示等级；
- 怪物死亡后给经验；
- 道具和奖励的结构要能支撑以后的背包。

## 用户提出的想法

按时间顺序：

1. 询问玩家数据由 PlayerMgr 管理是否合适、能否方便其他地方实时读取。随后说明问的其实是等级和经验，因为要在界面上显示等级。
2. 想写一个无参数事件，写成了 `UnityAction<Null>`，询问正确写法，以及这个场景适不适合无参数事件。
3. 询问怪物的经验值是否应该放进 SO。接着提出能不能把所有道具，包括以后的武器、装备，都放进同一套结构里。之后决定背包一定要做，武器装备暂不确定，并请 Claude 开了 #25。
4. 先问怪物经验能否直接写进 `CharacterConfig`，随后要求"一步到位"，按正式做法来做。
5. 写 `SettlementReward` 时发现：方法知道道具是经验，却不知道该给多少，于是提问。
6. 询问属性层应该写成普通类还是 SO，以及类名怎么起。
7. **指出 Claude 的遗漏**：Claude 只给出了 `Recalculate(int level)`，用户指出这样设计根本不知道每级的增量，也没有玩家或敌人的 id。
8. 实现过程中多次修改。每一轮由 Claude 审查，用户自己修改，见下文"实际采纳"。
9. **用户自己察觉到不对**：把"订阅玩家升级、读取玩家属性"写进 `CharacterHealth` 基类后，用户觉得"好怪"，指出这样怪物也会执行。Claude 一开始以为用户问的是 `CharacterObject` 里的属性计算，用户随后澄清，问的是生命组件这部分。
10. 询问为什么同一次升级会把属性计算两次。
11. 实测：杀怪升级、存档重进、界面、控制台属性修改都正常。

## 辅助分析与建议

**数据分类（Claude）**

- 配置：SO；
- 成长进度（等级、经验）：存档数据，由管理器持有；
- 最终属性：角色身上的属性层；
- 战斗状态：生命组件。

等级和经验由 PlayerMgr 持有是合适的。但外部不应该能直接改 `saveData`，界面应该"显示时先读一次，之后订阅变化"。

**无参数事件（Claude）**

写成 `UnityAction`，不带泛型参数。等级只有一个来源，订阅者随时可以自己去读，所以适合无参数。下面几种情况才需要参数：事件来自多个对象、需要旧值、需要逐级处理。另外建议在 `AddExp` 全部处理完之后只触发一次。

**奖励与道具（Claude）**

- 奖励数值放在配置里。
- 企业项目常见的做法是把结构分成三层：道具定义、奖励条目（`itemId` + `count`）、背包。
- 经验作为道具表里的一种类型，这样任务和怪物可以共用同一个发奖方法。
- 怪物奖励单独建一张表，不放进玩家和敌人共用的 `CharacterData`。
- 敌人死亡适合改成事件，但现在只有两个接收方，直接调用也可以，等出现第三个接收方时再改。
- 背包的范围：开题报告写的是"不照搬商城、复杂背包"，简单背包与此一致。
- 结算奖励时不知道数量，是因为方法的参数用的是 `ItemConfig`（道具定义），而数量在奖励条目里。

**属性层（Claude）**

- 运行时属性写成普通 C# 类，不用 SO。SO 是共享资产，而且在编辑器里修改会被保存，这一点和 AGENTS.md 中"SO 只存配置"的原则一致。
- 类名建议 `CharacterStats`。不建议以 `Attribute` 结尾，因为 C# 里这类名字约定用于特性。
- 用户指出遗漏后，Claude 补充：构造时传入角色自己的配置数据；查找配置这一步 `CharacterObject.Init` 已经用 id 做过，属性类本身不需要 id；等级作为参数传入，不在属性类里再存一份。
- `GetLevel()` 是虚方法，由子类提供等级，属于模板方法模式。

**逐轮审查中发现并由用户修复的问题（Claude）**

| 问题 | 后果 |
|---|---|
| `ItemRewardData` 里重复存了类型 | 可能和道具表不一致；任务奖励里原有的金币会因为默认值被当作经验发放 |
| `ItemDatabase` 类写在 `ItemConfig.cs` 文件里 | Unity 要求类名与文件名一致，资产丢失了脚本引用 |
| 初始等级改为 1 后，经验需求和攻击力公式没有同步修改，`LevelUp` 的顺序也不对 | 按每只怪 50 经验推算，经验会变成 −50 |
| `PlayerSaveData` 的字段初始化表达式访问了 `PlayerMgr.Instance` | `BaseManager` 在构造完成后才赋值实例，导致无限递归，栈溢出 |
| 等级变化时调用 `GamePanel.Init()` | 按钮监听会重复添加；GamePanel 不存在时报空引用 |
| `JsonMgr` 不会自动创建子目录；存档文件名多了一个 `.json` | 保存失败；文件名变成 `0.json.json` |
| 怪物奖励字段从 `ItemIDs` 改名为 `rewards` | Unity 按字段名读取，已填的数据被读成空列表，杀怪不加经验。Claude 当初建议改名时没有提醒这一点，用户实测没有效果后，Claude 根据存档目录排查出原因。用户在 Inspector 里重新填写并保存 |
| `CharacterStats` 在 `Awake` 中用尚未赋值的配置创建，并且从未计算 | 生命上限和防御都是 0，玩家和怪物都会一击即死 |
| 生命组件在基类里订阅玩家升级、读取玩家属性 | 玩家升级后，所有怪物的上限和防御都会被改成玩家的数值。建议：生命组件只接收参数，由同时拥有属性和生命组件的 `PlayerObject` 订阅并调用 |
| PlayerMgr 和 PlayerObject 各保留了一次重新计算 | 每次升级计算两次 |

**控制台（Claude 修改，`accde67`）**

属性挪进 `CharacterStats` 后，控制台原来"复制 config"的做法会出现"界面上的数字变了，实际伤害没变"。改为：

- 读取属性；
- 用反射改写 `CharacterStats` 的计算结果，再调用用户写的 `CharacterHealth.UpdateStats`；
- 状态栏显示等级；
- `DevConsoleSetup` 自检的基准改为当前等级下的属性。

控制台修改的属性在下次升级或点击"恢复初始值"时会被重新计算覆盖。

## 实际采纳与用户调整

| 内容 | 结果 |
|---|---|
| 道具表 | `ItemDatabase`（`ItemConfig`：id、名称、描述、图标、最大数量、类型），类型为 exp、money、consumable、missionItem、stuff；资产中 0 为经验，1 为金币 |
| 奖励 | `ItemRewardData` 移到单独文件；`EnemyRewardDatabase` 按敌人 id 存放奖励列表；`RewardMgr.SettlementReward` 按道具 id 查找类型后分发，查不到时报错并跳过该条，经验交给 `AddExp`，其他类型暂未处理 |
| 敌人死亡 | `EnemyHealth.Dead` 先结算奖励，再上报任务；仍是直接调用，没有改成事件 |
| 成长 | 经验需求为 (等级 − initLv) × expPerLv + initExpPerLv；先扣经验再升级；有满级判断；`AddExp` 末尾广播一次 `OnPlayerLevelChange` 并存档 |
| 存档 | 新档判断用 `level <= 0`，此时取配置的 initLv（检测存档是否存在的写法未采用）；文件为 `Character/Player/{id}.json`；`JsonMgr` 自动创建目录 |
| 属性 | `CharacterStats(config)` + `Recalculate(level)`，结果为 Atk、Def、MaxHp；`CharacterObject.Init` 创建并计算属性后初始化生命组件；`GetLevel()` 默认返回 initLv，PlayerObject 重写为存档中的等级 |
| 升级应用 | `PlayerObject` 订阅等级变化（Awake / OnDestroy），重新计算属性后调用 `CharacterHealth.UpdateStats(maxHp, def)`。当前血量保持不变，只限制在新上限以内，然后触发 `OnHealthChange` |
| 界面 | GamePanel 在 Init 中拉取等级，并在 OnEnable / OnDisable 中订阅、取消订阅 |
| 未采用或留待以后 | 属性变化事件（目前只有生命组件一个使用方）；敌人攻击仍直接读 `config.atk`，没有经过属性层；敌人死亡事件；道具按 id 查找改用字典；背包、金币发放、任务发奖（#13、#25） |

## 验证与当前状态

- **用户实测（11-15）**：杀两只怪从 1 级升到 2 级，经验归零；存档目录生成了 `Character/Player/0.json`，内容为 `{"level":2,"exp":0}`（Claude 读取文件核对过）；退出后重进为 2 级；界面上的等级、血条上限、控制台属性修改都正常。
- **静态核验**：Claude 逐轮审查；使用 Unity 的编译参数独立编译了主脚本程序集和 Editor 程序集，没有错误。
- **未覆盖**：
  - 满级行为；
  - 删除存档后的新档流程；
  - 连续升级多级；
  - 任务奖励发放。
- **未提交**：`EnemyConfig.asset` 中的攻击力 50 是用户的测试值；GMScene 中的玩家位置和 Canvas 调整是调试用的。
- **小遗留**：
  - `PlayerMgr` 中的 `using System.Data;` 和 `CharacterHealth` 中的 `using System;` 没有用到；
  - `OnPlayerLevelChange` 在每次获得经验时都会触发，不只是升级时。

## 论文素材与证据入口

可以作为以下几个主题的设计案例：

- 配置、成长进度、运行时属性、战斗状态四层数据的划分；
- 道具定义与奖励条目的分离；
- 用模板方法让玩家和敌人共用初始化流程；
- 通用组件只接收参数、由拥有者负责调用的依赖方向；
- 序列化字段改名会丢失数据，以及单例构造期间访问自身引发的递归。

观察者模式、模板方法、SO 配置都是通用技术，不应写成原创。

- [CharacterStats](../../../Assets/Scripts/Base/CharacterStats.cs)、[CharacterObject](../../../Assets/Scripts/Base/CharacterObject.cs)、[CharacterHealth](../../../Assets/Scripts/Object/Common/CharacterHealth.cs)、[PlayerObject](../../../Assets/Scripts/Object/Player/PlayerObject.cs)
- [PlayerGrowth](../../../Assets/Scripts/Object/Player/PlayerGrowth.cs)、[PlayerMgr](../../../Assets/Scripts/Managers/PlayerMgr.cs)、[JsonMgr](../../../Assets/Scripts/Json/JsonMgr.cs)
- [RewardMgr](../../../Assets/Scripts/Managers/RewardMgr.cs)、[ItemDatabase](../../../Assets/Scripts/Data/Config/ItemDatabase.cs)、[EnemyRewardDatabase](../../../Assets/Scripts/Data/Config/EnemyRewardDatabase.cs)、[EnemyHealth](../../../Assets/Scripts/Object/Enemy/EnemyHealth.cs)
- [DevConsole](../../../Assets/Scripts/UI/DevConsole.cs)
