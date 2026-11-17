# 控制台等级调试与敌人测试攻击力

日期：2026-11-16 实施和验证，2026-11-17 提交。工程版本：`6cf95db`。

协助工具：Claude Code。控制台由 Claude Code 维护（见[记录 29](29-console-support-code-removal.md)），只适配用户的接口，不改玩法脚本。

相关 Issue：#6（玩家成长：升级、满级、新档与存档恢复验证。等级调试可以用来快速切换等级，但本记录不包含 #6 要求的验证）。

## 用户观察与目标

11-16 控制台复测（[记录 36](36-devconsole-retest.md)）结束后，用户提出：

> 我建议在控制台多加一个功能 重来重置玩家等级到初始等级 或者干脆改成可以修改等级

用户的目标是调试升级相关功能时，不用每次靠杀怪攒经验，也能把等级调回初始值。

## 用户提出的想法

- 二选一：把玩家等级重置为初始等级，或者干脆做成可以修改等级。
- 实现完成后，用户在 Unity 里运行了自动检查，当时决定"先不提交"。
- 11-17 用户确认这两处改动正常提交：控制台等级调试，以及 `EnemyConfig.asset`。

## 辅助分析与建议（Claude Code）

- 两个需求一起做：
  - 输入框加"设置等级"：输入值超出 `initLv`～`maxLv` 时自动限制在范围内，并提示用户。
  - "重置为初始等级"：直接回到配置中的 `initLv`。
- 两个按钮的处理流程相同：
  1. 修改 `PlayerMgr.saveData.level`，经验清零。
  2. 调用用户写的 `PlayerMgr.PlayerLevelChanged()`，由用户的 `PlayerObject` 重新计算属性、更新生命组件，GamePanel 刷新等级显示。
  3. 调用用户写的 `SavePlayerData()` 写入存档，和杀怪升级后的存档行为一致。
- 改动范围只在 Claude Code 维护的 `DevConsole.cs` 和 `DevConsoleSetup.cs`，没有修改用户脚本：
  - `DevConsole` 新增 `levelInput`、`ApplyLevel`、`ResetLevel`，状态同步时显示存档等级。
  - `DevConsoleSetup` 新增菜单 **Tools → AstralEchoes → Dev Console → Add Level Controls**，把等级控件补进本地 `Canvas.prefab`，并把提示文字下移、窗口拉长。
  - Play Mode 自动检查新增等级项。检查期间会临时修改存档，结束后恢复原来的等级和经验并重新保存。
- 当前生命遵循用户定的规则（升级时当前血量不变），只会被限制在新的上限以内。
- 等级改变后，属性按新等级重新计算，之前在控制台改过的攻击和防御会被覆盖。

## 敌人测试攻击力

`EnemyConfig.asset` 中敌人 0 的攻击力从 5 改为 50，这是用户测试死亡流程时设置的测试值。11-16 的会话里曾几次说明"测试用的 50，暂不提交"，11-17 用户确认按当前值提交。

同一文件里还多了 `initLv`、`maxLv`、`initExpPerLv`、`atkPerLv`、`defPerLv`、`maxHpPerLv`、`expPerLv` 七个字段，值都是 0。这是 11-15 `CharacterConfig` 新增成长字段后，Unity 重新序列化写进去的，不是手动修改。敌人当前等级为 0，这些成长字段不影响敌人属性。

注意：提交后，仓库里敌人的攻击力就是 50，玩家会受到比原来高得多的伤害。之后如果要恢复正常数值，需要单独修改并记录。

## 当前文件依据

| 文件 | 内容 |
|---|---|
| [DevConsole](../../../Assets/Scripts/UI/DevConsole.cs) | `ApplyLevel`、`ResetLevel`、`SetLevel`（限制范围、经验清零、广播、存档、刷新显示） |
| [DevConsoleSetup](../../../Assets/Editor/DevConsoleSetup.cs) | `AddLevelControls` 菜单；自动检查中的等级项，以及检查后恢复存档 |
| [EnemyConfig.asset](../../../Assets/Resources/GameData/Character/EnemyConfig.asset) | 敌人 0：`atk: 50`；成长字段均为 0 |
| 本地 `Resources/UI/Canvas.prefab` | 已通过 Add Level Controls 加入等级控件，不入库。登记见 [ASSET_RESTORE](../../ASSET_RESTORE.md) |

## 验证与当前状态

- **自动检查（11-16，用户在 Unity 中运行，Claude Code 读取报告）**：56 项全部通过，0 项失败。其中新增 7 项等级检查：
  - 重置为初始等级后，等级回到 `initLv`、经验清零、广播 `OnPlayerLevelChange`；
  - 攻击、防御、生命上限回到初始值；当前生命不超过新上限；
  - 设置为 3 级时，属性按等级计算正确；
  - 超出最高等级的输入被限制到 `maxLv`。
- **存档恢复**：检查前存档为 `Lv.4，经验 100`，检查后存档文件仍是 `{"level":4,"exp":100}`。
- **界面**：截图中"清空任务存档"下方多了等级调试一行，1080p 下完整显示，没有超出屏幕。
- **代码时间**：两个脚本最后修改于 11-16 16:35，自动检查在此之后运行，之后没有再改动。
- **未做**：用户没有单独手动点击等级按钮。当时的报告位于 `Temp/DevConsoleReports`，现在 Temp 目录已被清理，依据来自当时会话中读取报告的记录。
- **敌人攻击 50**：只是数值配置，没有专门测试。

## 论文素材与证据入口

可作为"测试与调试工具"的例子：用调试控制台快速切换等级，用自动检查验证属性随等级重新计算。攻击 50 只是测试数值，不能作为游戏平衡数据写入论文。
