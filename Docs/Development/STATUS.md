# 当前开发状态

更新日期：2026-11-14。本文件只写**当前**状态，每次提交时直接覆盖对应行，不按日期追加。按日期的变化写在 [CHANGELOG](../../CHANGELOG.md)，过程细节写在 [records](README.md)，旧版全文在 [history](history/STATUS-2026-10-15_2026-11-14.md)。

正式题目：基于Unity3D的ARPG游戏《星渊回响》设计与实现。只做一个剧情探索模式，三选一临时 Buff 只用于部分战斗节点。

## 验证口径

- **用户声明**：用户报告已经完成或测试过，没有独立复现。
- **静态核验**：读过源码或资源文件，或者编译通过。
- **运行证据**：有 Play Mode、编辑器日志或用户实测记录，并注明日期和版本。
- 历史运行证据只适用于当时的版本，代码后来改过的模块要标"待回归"。

## 模块状态

| 模块 | 当前实现 | 验证状态 | 依据 |
|---|---|---|---|
| 移动与冲刺 | `PlayerObject`：currentSpeed 标量、1D Velocity 混合、MoveWeight 曲线、冲刺期间禁止转向 | 静态核验 + 用户声明（10-15）；当前版本没有完整试玩记录 | [记录 9–10](records/10-user-design-and-implementation.md) |
| 普攻与命中 | `PlayerCombat.OnAttackCheck` 球形检测，伤害取自 `PlayerMgr.GetAtkValue` | 运行证据（11-05 编辑器日志，双方命中） | [记录 25](records/25-enemy-ai-and-combat.md) |
| 受击与死亡 | `CharacterHealth.TakeDamage` / `Dead`；死亡后禁止操作的逻辑已在 11-10 撤回 | 静态核验；死亡后的操作限制待用户接回 | [记录 29](records/29-console-support-code-removal.md) |
| 敌人 AI | NavMesh 巡逻（4 个点）、追击、脱战返回、攻击冷却、死亡停机 | 运行证据（11-05）；巡逻边界和完整脱战流程待回归 | [记录 25](records/25-enemy-ai-and-combat.md) |
| 属性、等级与经验 | CharacterData 成长参数、PlayerGrowth、PlayerSaveData（level、exp） | 静态核验 + 编译（11-11）；升级、满级、存档恢复、生命和防御成长都没有运行 | [记录 30](records/30-player-growth-and-save-design.md) |
| NPC 对话 | 动态选项；按 NPC 和任务阶段选对白（接取、进行中、交付、完成后）；对话期间关闭输入 | 基础对话有用户测试确认（10-16）；完成后对白和输入控制待回归 | [记录 14](records/14-dialogue-system-implementation.md)、[28](records/28-dialogue-input-and-finished-dialogue.md) |
| 任务 | 任务 0 分两阶段：击杀 0 号怪，再与 0 号 NPC 对话；击杀推进、交付结算、进度存档 | 本机存档已写入 `isFinished = true`；完整流程待复测；实际发奖未接入 | [记录 26](records/26-npc-dialogue-and-mission-turn-in.md) |
| 玩家 HUD | 血条在 `Start` 拉取血量并订阅 `OnHealthChange`，在 `OnDestroy` 取消订阅 | 静态核验 + 编译（11-14）；Play Mode 待验证 | [记录 31](records/31-player-health-bar-refresh.md) |
| 存档 | MissionData JSON 写入 persistentDataPath；`SavePlayerData` 还没有调用方 | 任务存档有本机文件；玩家存档未运行 | [记录 22](records/22-mission-system-design-and-prototype.md)、[30](records/30-player-growth-and-save-design.md) |
| 对象池 | PoolData / PoolObj，容量配置、满额轮换 | 静态核验；源码中没有调用方 | [记录 27](records/27-pool-capacity-and-usage-contract.md) |
| 调试控制台 | DevConsole 通过反射读写字段，不再依赖玩法脚本的接口 | 编译通过（11-10）；联动效果待复测 | [记录 29](records/29-console-support-code-removal.md) |
| 角色与衣物 | Cyrene_Blender4.2，8 组 BoneCloth，面部覆盖材质开启裁剪 | 运行证据（10-15，396 次采样）；跑步、闪避时的穿插和性能未测 | [记录 8](records/08-blender42-bonecloth.md)、[21](records/21-face-overlay-material-set.md) |
| 技能、场景切换、剧情推进、临时 Buff、音效 | 未开始 | — | [剧情草案](../Design/世界观与主线剧情-V1.md) |
| 发布构建 | 没有发布过构建 | — | — |

## 待办

具体待办统一记在 [GitHub Issues](https://github.com/CyreneMine/AstralEchoes-ARPG/issues)，按标签区分：`待验证`、`待接回`、`功能`、`question`、`bug`、`文档`。本文件不再单独维护待办列表。2026-11-14 迁移时共 20 条：待验证 #1、#5–#11，待接回 #2–#4，需要用户决定 #12、#20，计划功能 #13–#19。

## 工程环境

Unity 6000.4.8f1 / URP 17.4.0 / Input System 1.19.0 / Cinemachine 3.1.7。本地工程位于 `G:\Unity\UnityProject\AstralEchoes`，入口场景为 `Assets/Scenes/GMScene.unity`。仓库不含素材和第三方插件，恢复方法见 [ASSET_RESTORE](../ASSET_RESTORE.md)。
