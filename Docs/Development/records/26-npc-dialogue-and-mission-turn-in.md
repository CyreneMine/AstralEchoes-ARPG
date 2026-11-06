# NPC 对话与两阶段任务结算

2026-11-06，GMScene，测试 NPC `0` 与任务 `0`。

## 用户观察与目标

用户在敌人战斗和死亡原型之后开始接任务结算，希望跑通接取、杀怪、回指定 NPC 对话的流程。开发中遇到击杀进度与结算状态混用、对白播放后任务未完成，以及新对白部分汉字显示为方框的问题。

## 用户提出的想法

- 怪物 ID 由 `EnemyObject` 统一提供，攻击和击杀上报读取同一个 ID。任务存档使用 `missionId` 找配置，用 `currentStageIndex` 找当前目标。
- 在任务配置中新增最后的对话阶段。`missionType` 为 `Dialogue` 时，用户明确把 `targetId` 解释为 NPC ID。交付对白由对应阶段触发。
- 用户考虑同一 NPC 发多条任务以及玩家没打完就回来两种情况，并提出按 NPC ID、任务和状态约定 JSON 文件名，通过代码拼路径，省去逐条录入对白路径。
- 用户接受原型在打开交付对白时完成任务，最后一句对白作为结算反馈。以后做正式奖励时可沿用这个明确的时机。

## 辅助分析与建议

- 区分阶段目标数 `targetCount` 和阶段总数 `stages.Count`。跳过一条已经达标的任务时用 `continue`，让同一次击杀仍能检查后面的任务。先改进度，再保存。
- 用 `status` 表示当前阶段达标，用 `isFinished` 表示整个任务已结算。任务面板按后者筛选，再选中第一条实际创建的可见任务。
- 战斗阶段达标后推进阶段，切换时重置计数和 `status`。NPC 根据当前阶段类型选对白。战斗分支需要匹配阶段类型，否则同为 `0` 的怪物 ID 和 NPC ID 会使第二次击杀误完成对话目标。
- 对话曾考虑为每条任务手填 `acceptDialogueKey`、为每段阶段手填 `dialogueKey`。用户选择命名约定后，任务归属通过每个 NPC 自己的 `missionIds` 表达。对话打开时携带本次任务 ID，避免根据对白文字或文件名反查任务。
- 字体问题通过编辑器日志定位。TMP 报告 `STHUPO SDF 1` 和备选字体未找到「清」「完」「败」等字，替换为 `□`。JSON 内容和原始 `STHUPO.TTF` 的字形覆盖已核对。图集空间不足是排查时的推断，给出的建议是开启多图集。

## 实际采纳与用户调整

| 环节 | 已保存实现 |
|---|---|
| NPC 配置 | `NpcData` 包含 `id`、`displayName`、`missionIds`。本地 SO 的序列化文件仍留有旧 `defaultDialogue`，当前源码已移除该字段。测试脚本按 NPC ID 查配置，取 `missionIds[0]` 作为单任务入口。 |
| 接取和进行中对白 | 接取对白使用拼出的 `Npc/0/Mission_0_Accept`，对话面板保存任务 ID，选择接受后使用它接任务。进行中对白暂用固定的 `Mission_0_InProgress` 路径。 |
| 击杀与切换阶段 | `EnemyHealth.Dead()` 上报 `Battle` 和敌人 ID。战斗分支匹配当前阶段的类型和目标 ID，击杀达标后把任务 `0` 从阶段 `0` 切到阶段 `1` 并保存。 |
| 交付 | 当前阶段为对话且尚未达标时，加载 `Mission_0_Stage_1`。打开该对白后立即上报 NPC ID，最后阶段达标时设置 `isFinished` 并保存。 |
| 任务面板 | 跳过 `isFinished` 的任务，自动展示第一条可见任务的标题、目标、描述和奖励预览。配置奖励是物品 `0`、数量 `100`，`FinishMission()` 里的实际发放仍是待办。 |

## 验证与当前状态

- **用户运行反馈**。用户确认修正后对白显示恢复正常。本轮记录没有确认最终采用了哪项字体设置，磁盘上的 `STHUPO SDF 1` 仍显示多图集关闭。
- **静态核验**。确认任务 `0` 的两个阶段依次为 `Battle`、`Dialogue`，目标均为 `0`，数量均为 `1`。三份对白 JSON 均可解析，进行中和交付文件各包含一句对白，均无选项。
- **本机存档证据**。2026-11-06 18:36:45 写入的 `MissionData.json` 中，任务 `0` 保存为 `currentStageIndex = 1`、`currentCount = 0`、`status = true`、`isFinished = true`。当前对话分支直接更新达标状态，所以该阶段的 `currentCount` 仍是 `0`。这份存档证明完成状态已写入，整条流程仍待从新存档重新验证。
- **当前单任务边界**。交付上报只传 NPC ID，`MissionMgr` 的对话分支遍历已接任务并比较目标 ID。现有示例的怪物与 NPC 都使用 ID `0`。若后续新增任务，需要按本次任务 ID 和当前阶段类型限定结算，避免误更新目标 ID 相同的其他阶段。详情面板目前也把 NPC ID 当作列表下标，配置中只有 ID 为 `0` 的一项。
- **下次处理**。已结算后再次互动时，NPC 先打开对话面板，随后没有可播内容，可能显示空框。先决定对白再打开面板。然后复测接取后提前回来、击杀切阶段、交付后重进。支持同一 NPC 多个任务时，交付结算需按本次任务 ID 和当前对话阶段限定范围。

## 文件依据

- [NPC 配置结构](../../../Assets/Scripts/Data/Config/NpcConfig.cs)与 [场景互动入口](../../../Assets/Scripts/Object/NPC/TestDialogue.cs)
- [任务配置](../../../Assets/Resources/GameData/Mission/MissionConfig.asset)与 [任务存档结构](../../../Assets/Scripts/Data/Save/MissionSaveData.cs)
- [任务计数与结算](../../../Assets/Scripts/Managers/MissionMgr.cs)、[对白选择](../../../Assets/Scripts/Managers/DialogueMgr.cs)与 [对话面板](../../../Assets/Scripts/UI/GamePanel/DialoguePanel.cs)
- [任务列表和默认详情](../../../Assets/Scripts/UI/GamePanel/MissionPanel.cs)与 [本地资源恢复清单](../../ASSET_RESTORE.md)
