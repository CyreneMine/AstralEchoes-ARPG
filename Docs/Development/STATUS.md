# 当前开发状态

更新日期：2026-11-16。本文件只写**当前**状态，每次提交时直接覆盖对应行，不按日期追加。按日期的变化写在 [CHANGELOG](../../CHANGELOG.md)，过程细节写在 [records](README.md)，旧版全文在 [history](history/STATUS-2026-10-15_2026-11-14.md)。

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
| 普攻与命中 | `PlayerCombat.OnAttackCheck` 球形检测，伤害取自 `PlayerMgr.GetAtkValue`（11-15 起读 `CharacterStats.Atk`）；敌人攻击仍直接读配置 | 运行证据（11-05 编辑器日志，双方命中）；11-15 用户实测控制台改攻击后伤害随之变化 | [记录 25](records/25-enemy-ai-and-combat.md)、[34](records/34-reward-level-and-stats.md) |
| 受击与死亡 | 最小死亡流程：`PlayerHealth.Dead/Respawn` 通过 PlayerMgr 的 `OnPlayerStateChange` 广播；死亡时不移动、不闪避、不攻击，播放 S_Die 并重置攻击状态；`Heal` 限制上限并刷新血条；复活暂由控制台触发 | 用户实测（11-15）：死亡、复活、出刀时死亡、控制台杀死都正确；之后加入的怪物脱战尚待复测；死亡界面和传送点未做 | [记录 33](records/33-minimal-death-flow.md) |
| 敌人 AI | NavMesh 巡逻（4 个点）、追击、脱战返回、攻击冷却、死亡停机；玩家死亡和复活时都脱战，玩家死亡期间不侦测、不攻击 | 运行证据（11-05）；巡逻边界和完整脱战流程待回归；11-15 的玩家死亡脱战只做了静态核验 | [记录 25](records/25-enemy-ai-and-combat.md)、[33](records/33-minimal-death-flow.md) |
| 属性、等级与经验 | `CharacterStats` 按（等级 − initLv）计算攻击、防御、生命上限，玩家和敌人共用；PlayerGrowth 先扣经验再升级、处理满级；升级后 PlayerObject 重新计算属性并更新生命组件（当前血量不变）；怪物死亡经 RewardMgr 发放经验 | 用户实测（11-15）：1→2 级、经验归零、属性和血条上限更新、存档重进；满级、连续多级、新档流程未测 | [记录 30](records/30-player-growth-and-save-design.md)、[34](records/34-reward-level-and-stats.md) |
| 道具与奖励 | 道具表 ItemDatabase（经验、金币）、怪物奖励表 EnemyRewardDatabase、`ItemRewardData` 共用、`RewardMgr.SettlementReward` 按类型分发；目前只处理经验 | 用户实测：杀怪获得经验；金币、背包、任务发奖未接入。11-16 背包与发奖的设计已定稿；同日开始实施：`PlayerItemMgr` 增删查、存盘、广播新数量已写，背包面板预制件已拼好，可以打开和关闭。上限、货币、格子生成与详情尚未实现。只做了编译和静态检查，关闭按钮修复待复测 | [记录 34](records/34-reward-level-and-stats.md)、[37](records/37-inventory-and-reward-design.md)、[38](records/38-inventory-ui-and-item-mgr-start.md)、#25 |
| NPC 对话 | 动态选项；按 NPC 和任务阶段选对白（接取、进行中、交付、完成后）；对话期间关闭输入 | 基础对话有用户测试确认（10-16）；完成后对白和输入控制待回归 | [记录 14](records/14-dialogue-system-implementation.md)、[28](records/28-dialogue-input-and-finished-dialogue.md) |
| 任务 | 任务 0 分两阶段：击杀 0 号怪，再与 0 号 NPC 对话；击杀推进、交付结算、进度存档 | 本机存档已写入 `isFinished = true`；完整流程待复测；实际发奖未接入 | [记录 26](records/26-npc-dialogue-and-mission-turn-in.md) |
| 玩家 HUD | 血条在 `Start` 拉取血量并订阅 `OnHealthChange`，在 `OnDestroy` 取消订阅；GamePanel 显示等级并订阅 `OnPlayerLevelChange` | 用户实测（11-15）：升级后等级和血条上限更新；面板重建与隐藏期间受伤的专项回归仍未记录 | [记录 31](records/31-player-health-bar-refresh.md)、[34](records/34-reward-level-and-stats.md) |
| UI 管理 | UIManager 构造时实例化 `UI/Canvas`，面板统一从 `Resources/UI` 预制件创建，关闭时销毁；场景中不预放 UI；子控件由各面板脚本管理 | 用户实测（11-16）：HUD、任务面板往返、对话选项、控制台正常；淡出约 0.2 秒内重开同一面板会丢失面板，已知未处理 | [记录 35](records/35-uimanager-prefab-only.md) |
| 光标模式与面板暂停 | PlayerMgr 保存光标模式并触发 `OnCursorChanged`；按住 Alt、对话期间进入光标模式；光标模式下 PlayerCombat 不攻击，CameraCursorLock 关闭镜头输入；任务面板打开时 `timeScale = 0`，BasePanel 淡入淡出改用不受缩放的时间 | 静态核验 + 编译（11-14）；用户声明暂停、攻击拦截、首次按 Alt 和对话光标正常，最终版本待回归 | [记录 32](records/32-cursor-mode-camera-and-panel-pause.md) |
| 存档 | MissionData 与玩家存档 `Character/Player/{id}.json` 写入 persistentDataPath，JsonMgr 自动创建子目录；获得经验后保存玩家存档；新档等级取配置 initLv | 任务存档有本机文件；玩家存档用户实测读写正常（11-15，`{"level":2,"exp":0}`） | [记录 22](records/22-mission-system-design-and-prototype.md)、[34](records/34-reward-level-and-stats.md) |
| 对象池 | PoolData / PoolObj，容量配置、满额轮换 | 静态核验；源码中没有调用方 | [记录 27](records/27-pool-capacity-and-usage-contract.md) |
| 调试控制台 | 由 Claude 维护，只适配用户接口：打开时进入玩家的光标模式；生命归零调用 `Dead()`，复活调用 `Respawn()`，改血后通过反射触发 `OnHealthChange`；属性读写 `CharacterStats` 并经 `CharacterHealth.UpdateStats` 应用；Editor 自动检查 49 项 | 运行证据（11-16）：自动检查 49/49 通过；用户手动实测按键、打怪伤害、清空任务存档正常；与其他面板同时使用有问题，仅调试用，用户决定不处理 | [记录 29](records/29-console-support-code-removal.md)、[33](records/33-minimal-death-flow.md)、[34](records/34-reward-level-and-stats.md)、[36](records/36-devconsole-retest.md) |
| 角色与衣物 | Cyrene_Blender4.2，8 组 BoneCloth，面部覆盖材质开启裁剪 | 运行证据（10-15，396 次采样）；跑步、闪避时的穿插和性能未测 | [记录 8](records/08-blender42-bonecloth.md)、[21](records/21-face-overlay-material-set.md) |
| 技能、场景切换、剧情推进、临时 Buff、音效 | 未开始 | — | [剧情草案](../Design/世界观与主线剧情-V1.md) |
| 发布构建 | 没有发布过构建 | — | — |

## 待办

具体待办统一记在 [GitHub Issues](https://github.com/CyreneMine/AstralEchoes-ARPG/issues)，按标签区分：`待验证`、`待接回`、`功能`、`question`、`bug`、`文档`。本文件不再单独维护待办列表。2026-11-14 迁移时共 20 条：待验证 #1、#5–#11，待接回 #2–#4，需要用户决定 #12、#20，计划功能 #13–#19。

## 工程环境

Unity 6000.4.8f1 / URP 17.4.0 / Input System 1.19.0 / Cinemachine 3.1.7。本地工程位于 `G:\Unity\UnityProject\AstralEchoes`，入口场景为 `Assets/Scenes/GMScene.unity`。仓库不含素材和第三方插件，恢复方法见 [ASSET_RESTORE](../ASSET_RESTORE.md)。
