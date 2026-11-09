# 对话输入权限、淡出回调与完成后对白

2026-11-09，GMScene，NPC 对话与玩家攻击输入。

## 用户观察与目标

用户反馈对话结束后角色会额外挥刀，尝试把恢复输入推迟到面板淡出结束，修改后出现新的交互问题。用户随后改用统一的操作标志控制攻击入口，并希望为任务已经完成的 NPC 增加一段单独的对白。

## 用户提出的想法

- 用户认为恢复输入应在面板隐藏完成后执行，尝试在 `HideMe()` 中给回调赋值，再传给基类。
- 用户追问匿名函数内部 `callback?.Invoke()` 的作用，并理解它会继续执行 UIManager 原先传入的面板清理逻辑。
- 用户原先把禁用 InputAction 理解为对应脚本也停止处理输入。讨论后改用 `PlayerMgr.canInput` 表达玩家是否允许操作，在攻击脚本入口直接检查。
- 用户询问对话时如何停止鼠标转动视角。Codex 根据 GMScene 指出 `FreeLook Camera` 上的 `CinemachineInputAxisController` 为输入入口，讨论了禁用该组件再恢复的接法。
- 用户提出完成后对白命名为 `Mission_0_Finished`，授权 Codex 创建对应 JSON。
- 用户随后要求检查哪些 TODO 已完成。Codex 根据当前源码指出接取任务和 NPC 名称显示的两条说明已过时，并建议将固定的交付阶段后缀改为当前阶段索引。用户独立完成路径修改后再次要求核对，随后移除这三条 TODO。

## 辅助分析与建议

- `UIManager.HidePanel()` 传给 `HideMe()` 的回调负责销毁面板并移除字典登记。使用 `callback = ...` 会替换原有内容，造成面板已淡出但仍登记为打开。建议用追加回调，或在包装函数内继续调用原回调。
- InputAction 的 `Disable()` 会停用动作，MonoBehaviour 的 `Update()` 仍会运行。`WasPressedThisFrame()` 在当前帧已经记录按下后，即使动作随后被禁用，仍可能保持为真。Codex 提出同时检查 `attack.enabled`，用户最终采用更明确的 `canInput` 入口判断。
- 用户引入 `canInput` 后，旧的 `CloseInput()` 曾仍会执行，而关闭时遗漏 `OpenInput()`。建议把输入系统与操作标志一起恢复。最新源码已移到对话显示时关闭输入，淡出后恢复。
- 恢复输入的时机和释放面板的职责都要保留。当前建议围绕用户选定的原型方案，实际触发帧与动画结果留待运行复测。

## 实际采纳与用户调整

| 环节 | 当前已保存代码 |
|---|---|
| 玩家操作权限 | `PlayerMgr` 新增默认值为 `true` 的 `canInput`。`PlayerCombat.Update()` 为 `false` 时立即返回。 |
| 打开对话 | `DialoguePanel.ShowMe()` 调用 `CloseInput()`，解锁鼠标并设置 `canInput = false`。关闭输入从原来的数据初始化位置移到面板显示入口。 |
| 结束对话 | `HideMe()` 用 `callback += ...` 追加操作，淡出完成后设置 `canInput = true`、调用 `OpenInput()`、锁定鼠标。UIManager 原有清理回调得以保留。 |
| NPC 选择任务 | `TestDialogue` 新增 Inspector 字段 `currenMissionId`，作为 `npcData.missionIds` 的下标，得到实际任务 ID 后拼接接取路径并传入 DialogueMgr。 |
| 对白选择 | `DialogueMgr` 复用一次取得的 `currentMissionData`，进行中、交付及完成后路径都使用 NPC ID 和任务 ID。用户后续将交付对白后缀改为 `Stage_{currentMissionData.currentStageIndex}`，按当前阶段索引加载。 |
| 完成后对白 | 完成后分支在当前为末阶段且 `status = true` 时加载 `Finished`。该分支目前未直接检查 `isFinished`，其表现待完整任务回归核对。 |

Codex 按用户指定名字创建 `Assets/StreamingAssets/Dialogue/Npc/0/Mission_0_Finished.json`。内容为测试引导员确认先前敌人已清理完毕，并感谢玩家。文件包含一条普通对白，`options` 为空。

## 验证与当前状态

- **用户反馈**。用户报告原先额外挥刀以及修改后出现交互问题，随后提交新的 `canInput` 思路供核对。本轮尚无最终修复通过的运行声明。
- **静态核验**。已核对输入标志、InputAction 关闭和恢复、回调追加，以及完成后对白的加载路径。NPC 当前按 Inspector 指定列表下标选任务，自动依次发任务属于后续工作。
- **JSON 核验**。完成后对白文件已通过 JSON 解析与字段检查，包含一条有名字、有正文且无选项的对白。内容不包含再次接受任务的操作。
- **运行回归待补**。依次测试打开对话、翻页及选择选项、关闭时是否额外攻击、淡出后是否能移动和主动攻击、再次互动是否正常。任务完成后再找 NPC 时应能播放完成后对白。
- **镜头输入讨论**。本轮核对到 Cinemachine 镜头输入组件，但当前玩家与对话脚本未新增该组件引用，关闭镜头输入的实际接线和效果待用户实施验证。
- **归档与交付**。前一轮讨论只整理文档并保留工作区改动。本轮用户已要求检查文档后推送，按输入控制、任务对白和开发记录拆分提交。源码及文档纳入仓库，完成后对白 JSON 继续遵循现有忽略规则保留本地，恢复要求见资源说明。
- **本轮复核**。重新核对 ShowMe、HideMe、BasePanel 淡出回调及 PlayerCombat 输入入口，确认上述文件记录与当前磁盘版本一致。当前测试任务的两阶段对白路径有对应本地 JSON，解析与结构检查通过。本轮未进入 Unity Play Mode 或执行构建。
- **TODO 整理后复核**。当前阶段索引为 `1` 时仍加载已有的 `Mission_0_Stage_1.json`。核对用户已移除接取任务、NPC 名称显示及固定交付路径的三条过时 TODO；前两处只清理注释，运行逻辑保留。其他阶段索引需要准备对应 JSON，尚未新增多阶段对白的运行测试。

## 文件依据

- [玩家输入和权限](../../../Assets/Scripts/Managers/PlayerMgr.cs)、[玩家攻击入口](../../../Assets/Scripts/Object/Player/PlayerCombat.cs)
- [对话面板显示与淡出](../../../Assets/Scripts/UI/GamePanel/DialoguePanel.cs)、[面板回调执行](../../../Assets/Scripts/Base/BasePanel.cs)与 [管理器清理](../../../Assets/Scripts/Managers/UIManager.cs)
- [NPC 互动入口](../../../Assets/Scripts/Object/NPC/TestDialogue.cs)、[对白选择](../../../Assets/Scripts/Managers/DialogueMgr.cs)与 [资源恢复说明](../../ASSET_RESTORE.md)
- [Unity InputAction API](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/api/UnityEngine.InputSystem.InputAction.html)说明 `WasPressedThisFrame()` 的帧记录和动作启用状态行为。
