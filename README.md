# 星渊回响 · AstralEchoes

基于Unity3D的ARPG游戏《星渊回响》设计与实现。Unity 6 / C#，二次元风格第三人称 ARPG 毕业设计原型。

当前已有移动与冲刺、基础对话、敌人战斗原型、任务面板、玩家 HUD 和调试控制台。测试任务已接入击杀计数、返回 NPC 交付和结算状态存档，实际发奖待接入。游戏范围为单一剧情探索模式，部分战斗节点加入三选一临时 Buff。

## 开发环境与入口

- Unity：6000.4.8f1；URP：17.4.0；Input System：1.19.0；Cinemachine：3.1.7。
- 当前场景：`Assets/Scenes/GMScene.unity`。
- 当前控制代码：`Assets/Scripts/Object/Player/PlayerObject.cs`；当前 Animator：`Assets/Resources/Animator/Cyrene.controller`。
- 旧 Testing 与配套 Editor 测试工具已清理；历史测试证据保留在开发文档，不能代替当前玩法验收。
- 靠近测试 NPC 后可通过交互提示按钮或 F 键开始对话。当前测试 NPC 通过配置取得任务 ID，按进度播放接取、进行中或交付对白。换工作区运行时需要恢复本地 NPC 配置、对白和 UI 资源，见 [资源补回说明](Docs/ASSET_RESTORE.md)。

## 文档

- [文档总入口](Docs/README.md)
- [当前开发状态](Docs/Development/STATUS.md)
- [问题处理与验证记录](Docs/Development/README.md)
- [世界观与主线剧情 V1 草案](Docs/Design/世界观与主线剧情-V1.md)
- [资源补回说明](Docs/ASSET_RESTORE.md)

本仓库不包含角色、贴图、动作素材和第三方插件。完整运行需补回本地资源及其原 `.meta`；仅克隆仓库不是完整可玩成品。历史测试不自动代表最新代码已经验收。

## 2026-10-28 开发快照

已保存按类型加载的角色SO配置表、生命组件初始化、球形命中原型与敌人世界空间血条。最新源码接通玩家/敌人初始化和初始血条刷新；历史NaN/Infinity已分析原因，尚未执行本次修复后的运行回归。自编PlayerConfig/EnemyConfig及meta随源码提交；模型、动作、贴图和插件继续留本地。

[用户思路与实现过程](Docs/Development/records/17-character-config-health-and-hit.md) · [本轮交付](Docs/Development/deliveries/2026-10-28.md)

## 2026-10-30 任务与调试 UI 快照

本轮保存任务 SO 定义、独立玩家进度、按 ID 接取及 JSON 保存/恢复入口、任务列表/详情/奖励预览、生命事件与玩家 HUD、启动预热和管理员控制台。任务展示与奖励预览不等于目标判定、完成和发奖闭环；最新验证范围见日志。

[任务设计与用户思路](Docs/Development/records/22-mission-system-design-and-prototype.md) · [本轮日志](Docs/Development/logs/2026-10-30-mission-and-ui.md) · [交付清单](Docs/Development/deliveries/2026-10-30.md)

## 2026-11-05 敌人原型快照

GMScene 已配置 NavMesh 和四个巡逻点。敌人脚本已接入巡逻、追击、脱战返回、攻击冷却和死亡停机。现有日志记录了双方命中与敌人血量归零。巡逻边界和完整脱战流程仍待专项回归。

[敌人原型与排错记录](Docs/Development/records/25-enemy-ai-and-combat.md)

## 2026-11-06 NPC 对话与任务结算快照

任务 `0` 配有击杀 `0` 号怪和与 `0` 号 NPC 对话两个阶段。源码已接入击杀推进、交付对白及结算存档，本机已有 `isFinished = true` 的存档。当前验证范围为源码、资源、存档快照和用户的字体显示反馈，完整流程复测与实际发奖待补。

[NPC 对话与两阶段任务记录](Docs/Development/records/26-npc-dialogue-and-mission-turn-in.md)

## 2026-11-08 缓存池与剧情文档归档

缓存池已保存容量配置、空闲对象复用和满额轮换实现，当前完成静态检查，实际对象接入和运行验证待补。世界观与短主线 V1 草案已归档，包含失忆主角、回响现象和观星塔故事；关卡、首领战及约 20 分钟的流程时长仍为制作设想。

[缓存池设计与使用约定](Docs/Development/records/27-pool-capacity-and-usage-contract.md) · [世界观与主线剧情草案](Docs/Design/世界观与主线剧情-V1.md)

## 2026-11-09 对话输入与完成后对白

对话显示和淡出时接入 `canInput` 与 InputAction 开关，保留面板关闭清理回调。任务完成后对白已新增，相关路径按 NPC 和任务 ID 拼接。当前已完成源码与 JSON 核对，额外挥刀修复效果、结束后移动及重复交互仍待运行验证。

[用户调整与验证范围](Docs/Development/records/28-dialogue-input-and-finished-dialogue.md)
