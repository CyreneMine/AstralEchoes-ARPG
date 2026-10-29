# 基础任务系统：用户设计讨论与当前实现

记录日期：2026-10-30，覆盖 10 月 29 日晚至 30 日收尾。Unity 6000.4.8f1；场景 GMScene。用户表示本轮主要工作为任务系统及少量优化，并要求参考“设计星渊回响程序架构”对话归档。

## 用户目标与最初想法

用户最初限定为：在原测试 NPC 对话中接取任务，保存接取数据，在任务面板查看名称、简介、ID、奖励和主线/支线。用户要求界面参考《崩坏：星穹铁道》的列表与详情分区，只保留主线/支线页签、一个追踪按钮，使用默认 UGUI 资源；界面由 Codex 搭建，任务逻辑由用户编写。

用户随后提出一条任务可能包含“先对话、去某处战斗或交互、再对话结束”的流程，并询问是否需要为每条任务单写一个类、额外保存下一阶段 ID。讨论逐步采用一条任务包含顺序阶段列表的方案；分支跳转、多个同时目标和失败重试没有纳入本轮。

## 配置、玩家进度与存档的分离

用户询问 SO 与 JSON 的分工，最终采用 SO 保存任务定义、普通 C# 对象保存玩家进度、JsonMgr 负责持久化。Codex 建议通用逻辑解释目标类型与参数，而非每条普通任务一个类。

当前实际类型与命名如下，早期讨论对两个存档类的命名曾与最终代码相反，以本表为准：

| 文件或类型 | 当前职责与字段 |
|---|---|
| `Assets/Scripts/Data/Config/MissionConfig.cs` | SO 表，包含 `List<MissionData> missionDatas` |
| `MissionData` | `missionName`、`missionDescription`、`missionId`、`stages`、`itemRewards` |
| `MissionStage` | `missionType`、`targetId`、`targetCount` |
| `MissionType` | `Battle`、`Dialogue`、`Interaction`，表示阶段目标行为 |
| `ItemRewardData` | `itemId`、`count` |
| `Assets/Scripts/Data/Save/MissionProgressData.cs` | 全部任务进度容器，初始化 `List<MissionSaveData> missions` |
| `Assets/Scripts/Data/Save/MissionSaveData.cs` | 单条玩家记录：ID、当前阶段索引、当前计数、bool 状态 |

用户提出“奖励字典的 key 是道具、value 是数量”。Codex 根据 Unity SO 的默认序列化与 Inspector 编辑需求，建议奖励条目列表；用户实际采用 `List<ItemRewardData> itemRewards`。用户还担心要求数量只对战斗有用，因此阶段使用通用 `targetCount`，对话/交互可解释为完成次数。

用户自行修改的阶段存档最终为普通 C# 类，未继承 MonoBehaviour。配置中的玩家当前阶段字段已移除。当前默认使用 LitJson；存档类没有 Serializable 标记，因此不能据此宣称切换 JsonUtility 也已通过验证。

## 阶段表示方式与查询结果复用

用户曾提出：若平铺配置中相邻行具有相同任务 ID，就可以认为是同一任务的不同阶段。Codex 后续明确承认这一约定本身可行；当时的问题是配置已包含 `stages`，推进代码却采用平铺规则。最终保留一个 `MissionData` 包含多个阶段的结构，不重复配置任务名称和奖励。

用户提出的下一阶段条件 `stages.Count - 1 > currentStageIndex` 与“当前索引加一小于总数”等价，最终保留用户写法。任务 ID 与配置行号、玩家进度列表下标分别处理，不能互换。

讨论中 Codex 最初建议推进方法再次查找数据，用户指出这会重复遍历。随后建议复用已找到的对象，以 `out` 返回进度与配置，替代管理器共享的 `currentIndex`、`hasNextStage` 字段。用户询问 out 是否会改 SO，以及是否必须 new；解释为返回已有玩家记录的引用，未找到时赋 null，修改玩家记录不等于修改 SO 或自动写 JSON。

最终 `GetAcceptMission(int, out MissionSaveData, out MissionData)` 的约定是：找到玩家记录则返回 accepted=true；只有还有下一阶段时，才给配置输出赋值；`UpdateStage` 收到非 null 配置才递增索引并清零计数。用户明确解释这是有意的条件控制，Codex 撤回了把这一约定直接认作推进错误的判断。本轮保留该实现；null 仍不能独自区分“最后阶段”和“配置缺失”，完成分支待后续处理。

## 当前代码实际接通的流程

1. `ConfigMgr` 在构造时加载 `GameData/Mission/MissionConfig`，通过 `GetMissionData(id)` 查表；角色表字段改名为 `characterConfigs`。
2. `MissionMgr` 的私有构造函数读取 `MissionData`。`AcceptMission` 为配置中存在且未接取的任务创建记录，阶段/计数为 0、状态 false，加入后保存整个进度容器并 break。未匹配配置的 ID 不创建记录。
3. `DialoguePanel.AcceptMission` 设置接受标志；`EndDialogue` 在标志成立时调用 `MissionMgr.Instance.AcceptMission(0)`。ID=0 是用户明确保留的测试入口。
4. `GamePanel.btnMission` 打开任务面板并关闭玩家输入；Left Alt 临时释放鼠标。`MissionPanel` 显示时隐藏 GamePanel，关闭时恢复 GamePanel，由其 ShowMe 重新启用输入。
5. `MissionPanel.Init` 遍历玩家已接任务，加载 `UI/MissionItem`，并把右侧目标、描述和奖励容器传入条目的 Init。条目选择后按当前阶段刷新详情，清除旧奖励并加载 `UI/RewardSlot`。
6. 奖励显示目前把物品名固定为“金币”，数量来自配置；区域名固定为 GMScene。这是显示占位，没有背包入账或发奖代码。

当前自编 MissionConfig 测试数据只有任务 ID=0、名称“测试任务”、一段战斗目标（目标 ID=0、数量1）、奖励物品 ID=0、数量100。它只能作为当前接取与显示的数据，不是已完成的多阶段测试用例。

## 绑定与显示问题的实际处理

用户询问能否把场景组件一键同步到预制件。讨论确认条目内部标题、位置、按钮可保存在 MissionItem Prefab，右侧详情区属于父面板，应在动态生成后传入，而非保存外部对象引用。

架构对话中出现过三个实际问题：动态加载的 MissionItem Prefab 未挂组件；RewardSlot 文本未绑定；MissionPanel 的旧字段 `Reward` 与新字段 `reward` 名称不同导致引用失效。收尾时静态核查独立 Prefab 已有对应组件及内部引用，父面板的目标、描述、奖励、Content 和关闭按钮均非空；条目的外部详情字段仍为空，由 Init 传入，符合现有实现。

用户又询问奖励不显示。原因是原来在 Init 生成奖励，点击条目时只清空而不重建。用户采纳将奖励循环移进点击回调，并用 `SetParent(reward.transform, false)` 挂接。当前文件已保存这一修改。

曾因使用 `Mission/MissionData` 而出现父目录不存在异常，Codex 建议 JsonMgr 写前创建目录。当前文件采用无子目录的 `MissionData`，读写路径一致；JsonMgr 没有加入创建目录代码。这是最终代码与先前建议的区别，不能写为“通用子目录保存已实现”。

## 用户后续想法与尚未采用的建议

用户提出在 NPC SO 中配置任务 ID，之后用对应对话/选项把 ID 传给管理器。该数据驱动方向保留为后续方案，当前仍固定传 0，没有 NPC 任务配置或上下文传参实现。

主线/支线虽然已有 UI 页签，但当前 MissionData 没有任务分类字段，代码也没有筛选。不要把 `MissionType` 的三个目标类型写成主支线分类。

## 验证与剩余事项

本轮收尾核对了源码、SO 内容、Prefab 序列化引用和最近讨论，没有重新运行完整任务流程，也没有生成独立构建。用户声明本日主要任务系统工作已收尾，但没有提供“重启恢复、多阶段、完成发奖全部通过”的逐项验收结论。

静态确认的边界：

- 重复调用 AcceptMission 对已接任务会尝试推进；当前尚无目标达成校验。
- `UpdateStage` 尚未保存推进后的 JSON；目标事件计数、完成状态写入和实际发奖尚未接入。
- 战斗目标文本仍用 `nowMS.ToString()`，没有目标名称解析；MissionPanel 生成条目仍为 `SetParent(parent)`，后续应检查缩放。
- 未验证主支线筛选、追踪按钮、实际定位、多个任务切换和重启恢复。

下一步应以本轮已有“接取→保存→面板查看”为基础实际回归，再接“匹配事件→累计数量→推进并保存→完成”的流程。上述为待办，不是本轮新增实现。

## 论文素材与证据入口

可用于论文阐述配置与运行状态分离、阶段列表、查询结果复用、UI 依赖传入及持久化边界。应把用户提出的平铺阶段、性能疑问、奖励映射、NPC 配置想法与 Codex 建议分别署明为开发过程。代码是用户编写并调整的原型，不能将 Codex 的未来设计建议当作已完成成果。

[初始界面搭建](19-mission-panel-ui.md) · [任务存档清空按钮](20-devconsole-mission-reset.md) · [本轮日志](../logs/2026-10-30-mission-and-ui.md) · [收尾证据](../evidence/mission-ui-20261030.json)
