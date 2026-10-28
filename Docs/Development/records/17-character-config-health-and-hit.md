# 角色配置管理、生命值与血条更新排查

日期：2026-10-28。当前实现是战斗与属性的原型，不等于完整战斗系统或最终验收。

## 用户观察与目标

用户发现玩家攻击力不是预期值，随后要求按 ID 动态取得 SO 数据，不依靠逐个绑定；希望一个管理器管理玩家、敌人等多个配置文件，并使敌人受伤后正确更新头顶血条。后续强调：架构建议应解释实际工程职责与适用条件，不把临时方案当成唯一选择。

## 用户提出的想法与调整过程

1. 用户认为正式项目可以根据角色 ID 动态加载 SO，进一步提出在 SO 里增加玩家/敌人/NPC 标识。
2. 初版尝试先读取 config.type、config.id，再拼路径加载；用户要求改为真正动态获取，不手动绑定 SO。
3. 用户自行将单条 SO 改为包含 List<CharacterData> 的配置表；PlayerConfig.asset、EnemyConfig.asset 分文件编辑，但都属于 CharacterConfig 类型。
4. 用户指出列表下标可严格对应 ID。Codex 解释独立 ID 与下标的不同后，保留用户方案：当前 id 就是表内下标，不强制改为按角色 ID 的字典。
5. 用户希望后续增加 NPC 时自动建立容器，不再手写 playerList、enemyList、npcList。实际由用户选择 Enum.GetValues 遍历类型，并将加载的 SO 存入 Dictionary<CharacterType, CharacterConfig>；每个 SO 自己持有列表。
6. 用户选择暂时移除尚未制作的 NPC 枚举，后续有对应表再增加，当前固定 Player=0、Enemy=1。
7. 用户用 Inspector 截图指出表内已经有数据；助手此前读取磁盘看到空列表。随后重新读取确认两张表均已保存第0条数据，撤销“表仍为空”的旧判断；不把该情况写成用户未填写数据。

## 辅助分析与建议

早期 CharacterObject.Awake 写死加载 Enemy_Test，玩家调用 base.Awake 后也使用了敌人的 atk=1；这是选择错误配置，与最终扣除防御后的伤害不是同一概念。建议将查找条件与加载结果分开。

角色行为、配置查询、生成初始化分别承担职责；ConfigMgr 负责加载与查询，角色接收一条 CharacterData，CharacterHealth 持有独立的当前血量与死亡状态。加载模式本轮采用用户的枚举预加载，不是助手先前提出的按需加载。NPC 不在本轮实现范围。多 SO 文件与多 C# 类型是不同概念；并未要求所有角色必须合并一个文件。

血条故障排查读取了现有 Editor.log，发现“当前剩余血量NaN”和“Infinity”。HealthBarController 对 hp/maxHp 计算比例，但当时 CharacterObject.Init 虽定义了 health.Init，角色却没有实际调用 Init，maxHp 仍为0。场景的 hpController 与 imgFill 引用已连接，问题首先在初始化链路，不是引用未拖。建议角色进入战斗前完成配置与生命初始化，并刷新初始血条。

## 实际采纳与当前文件依据

- `Assets/Scripts/Data/Config/CharacterConfig.cs`：包含枚举、List<CharacterData> 和 id/atk/def/maxHp/type。
- `Assets/Scripts/Managers/ConfigMgr.cs`：私有构造函数按枚举加载 GameData/Character/{type}Config，保存表引用；GetCharacterData(type,id) 取表内下标。
- `Assets/Scripts/Base/CharacterObject.cs`：Init(id,type) 查询管理器，再调用 health.Init(config.maxHp,config.def)。Awake 获取或添加生命组件。
- `Assets/Scripts/Object/Player/PlayerObject.cs`：最新源码在 base.Awake 后调用 Init(0,Player)，注释明确属于测试固定选择。
- `Assets/Scripts/Object/Enemy/EnemyObject.cs`：最新源码在 base.Awake 后调用 Init(0,Enemy)，随后 UpdateHealthBar；TakeDamage 后刷新血条。此前未调用的诊断是历史状态，不能当作本次提交后的现状。
- `Assets/Scripts/Object/Common/CharacterHealth.cs`：当前生命值、最大生命值只读接口与受伤计算；最终伤害为 max(damage-def,0)。
- `Assets/Scripts/Object/Player/PlayerCombat.cs`：OnAttackCheck 使用前方球形 OverlapSphere 检测 Enemy Layer，并传入玩家配置中的 atk；并未据此完成通用连击伤害、每挥击去重或敌人AI。
- `Assets/Scripts/UI/GamePanel/HealthBarController.cs`：将浮点 hp/maxHp 赋给 Fill.fillAmount。
- `Assets/Resources/GameData/Character/PlayerConfig.asset`：第0条 id=0、atk=2、def=3、maxHp=100、type=Player。
- `Assets/Resources/GameData/Character/EnemyConfig.asset`：第0条 id=0、atk=1、def=1、maxHp=10、type=Enemy。

## 验证与当前状态

| 证据 | 能支持的结论 | 不能支持的结论 |
|---|---|---|
| 用户 Inspector 截图及后续文件复读 | 两张表各含第0条配置 | 不能替代初始化运行验收 |
| 现有 Editor.log 的 NaN/Infinity 与调用栈 | 历史血条链路确实产生过非有限比例 | 不表示修复后仍存在，也不表示修复已通过 |
| 本次最新源码 | 玩家/敌人初始化调用及初始血条刷新已接通 | 尚未重新运行测试或构建 |

仅作预期推导：当前玩家atk=2、敌人def=1时，一次独立命中应扣1点，敌人从10降到9，比例0.9。这不是本次实测结果；动画事件、多Collider或重复回调会影响实际命中次数，应另外验证。

本轮保留的边界：ConfigMgr 尚未校验加载失败和下标范围；HealthBarController 尚未对 maxHp<=0 防护；命中代码直接获取 EnemyObject，未做空值处理与重复命中去重；测试 ID 仍在 Awake 写死，尚无动态生成器注入流程。均记录为待完善，不在提交整理时擅自修改。

## 论文素材与证据入口

可用于解释数据驱动配置、共享静态配置与独立运行状态、列表下标约定、按类型建立缓存及生命周期故障定位。贡献归属：表结构、按下标查询、分SO文件、枚举遍历预加载和暂删NPC由用户提出或选择；助手提供职责分析、路径/空引用诊断及初始化建议；具体实现由用户保存。本轮只归档与Git操作。

[静态证据](../evidence/config-health-20261028.json) · [开发日志](../logs/2026-10-28-config-health.md)
