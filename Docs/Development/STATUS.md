# 当前开发状态

核验日期为 2026-11-05。正式题目为基于Unity3D的ARPG游戏《星渊回响》设计与实现。

## 2026-11-05 敌人原型进展

- GMScene 中的 EnemyAI 实例配置了四个巡逻点及场景 NavMesh。源码已接入到点等待、追击、按距离脱战、返回巡逻和死亡停机。
- 攻击由 `fire` 触发测试状态，`TestAtk` 的动画事件调用范围判定。现有编辑器日志记录了敌人命中玩家、玩家命中敌人及敌人血量归零。
- 死亡入口已预留任务与奖励扩展位置。经验、金币、任务进度联动仍待接入。
- GamePanel 在任务面板存在时跳过 Alt 光标处理，保留基础面板更新。当前仅核对源码，切换时序待运行回归。

[敌人原型、日志证据与待回归项](records/25-enemy-ai-and-combat.md)

## 2026-10-30 当前状态（优先于下方历史快照）

- 用户编写的基础任务原型已保存：SO定义/顺序阶段/奖励、独立玩家进度、MissionMgr读写根目录MissionData、对话接受后固定任务0、动态任务列表/详情/奖励预览及面板开关。
- 当前测试配置为任务0、一个战斗阶段、数量1、金币显示数量100。主线/支线只有界面页签，数据分类和筛选未实现；任务完成、目标事件累计、推进后保存和实际发奖待接入。NPC SO传任务ID是用户提出的后续方案。
- 玩家HUD订阅生命事件；角色运行时属性从共享SO复制；DevConsole提供生命/属性操作和清空任务存档。旧20按钮10项Play Mode检查曾通过，键鼠操作及新清空按钮未完成实际执行验证。
- 用户在NPC Start里Show/Hide交互提示预热，并确认首次卡顿改善，未取得新的性能对照数据。敌人血条重复日志已移除。
- Blender4.2面部两个覆盖材质已开启裁剪，Scene观察黑块消失；与旧Mat_Cats材质分开记录，素材修复保留本地。
- 本次收尾仅静态核对与Git交付，不新增Play Mode、构建或任务闭环验收。五批提交及远端核验以交付记录为准。

[任务过程](records/22-mission-system-design-and-prototype.md) · [生命/HUD与控制台](records/23-dev-console-and-player-hud.md) · [启动预热](records/24-first-interaction-ui-warmup.md) · [本轮交付](deliveries/2026-10-30.md)

## 历史基线（2026-10-16整理）

- 本地工程：`E:\UnityProjects\AstralEchoes`；Unity 6000.4.8f1 / URP 17.4.0 / Input System 1.19.0 / Cinemachine 3.1.7。
- 当前场景为 `Assets/Scenes/GMScene.unity`，从 SampleScene 重命名且 GUID 保留；构建入口已同步。
- 当前角色使用本地 Cyrene_Blender4.2 预制件；控制脚本位于 `Assets/Scripts/Object/Player/`，控制器为 Cyrene.controller。
- 已有移动/冲刺与攻击原型；本轮基础对话流程已由用户运行测试确认完成，剩余小漏洞留待下一工作日。任务接取、实际分支路由、生命值和游戏主入口仍未完成，不扩大本轮完成范围。
- 旧 Testing 和对应 Editor 工具已移除。文中早期“尚未迁移”“尚未提交”等说明只代表各记录当时状态，不是最新基线。
- 最新入口：[对话实现与测试确认](records/14-dialogue-system-implementation.md)、[本轮开发日志](logs/2026-10-16-dialogue.md)。此前 [阶段总结](records/12-stage-summary-20261015.md)保留为当时快照。

## 历史实现与证据（按原阶段保留）

| 内容 | 已核实内容 | 验证范围与限制 |
|---|---|---|
| 英文工程路径 | 从中文目录搬迁，清单及核心文件哈希一致 | 用户重新启动后确认 QuickInstaller 字符串转换异常消失；不推断所有 Unicode 路径必然失败 |
| 旧 PMX 透明叠层 | 四个衣裙叠层改为透明混合，用户确认外观改善 | 旧工程历史，当前工程已采用 CATS/FBX |
| 旧 Avatar 映射 | 移除误映射到刘海的 LeftEye、RightEye、Jaw | 旧 FBX 的三条可选映射修复；不套用于当前 CATS 模型 |
| 临时 WASD 原型 | `CyreneMovementTest`、Idle 与二维混合树 | 旧测试对象七阶段实际 Play Mode；当前 `PlayerObject` 未复用该测试结论 |
| 身体左右偏移修正 | 动作副本继承原 AnimationClipSettings，只开启 loopTime | 隔离采样：前进骨盆横向范围约 1.0332m → 0.0870m，与源动作一致；不等于完整试玩验收 |
| 当前移动/闪避代码 | `PlayerObject` 含输入、加减速、重力、XSpeed/YSpeed、Quick 触发及定时位移；源动作 meta 有 StartQuick/EndQuick 事件 | 本次为静态核验，未重新 Play 测试；Shift 同时用于跑步和 Quick、转向及事件链等需后续实际验收 |
| 衣物物理 | 六组 BoneCloth，240 根衣物骨骼，11 个碰撞体 | 配置时 Play Mode 537 次采样，六组有效且运行，固定根漂移为 0；复杂动作穿插和性能待验证 |
| 面部黑块 | `Mat_Cats/顏2+` 恢复默认 Alpha=0，启用 `_UseAlphaClipping` 属性及关键字 | Unity 重导入后现场观察黑块消失；未重新关闭启动验证；资源文件本次不上传 |

## 2026-10-15 后续：Cyrene_Blender4.2 可切换配饰版本

已为场景中的 Cyrene_Blender4.2 配置独立 BoneCloth 预制体，8 组、296 根衣物骨骼、11 个碰撞体，保留 64 个独立网格。396 次实际 Play Mode 采样通过，配饰开关通过，原始资源哈希与结构签名一致。详见 [配置记录](records/08-blender42-bonecloth.md)。该模型尚未绑定运行控制器；PlayerObject 和相机控制仍在 CATS 模型，本次没有迁移。此补充尚未 Git 提交或推送。

## 待完成或待验证

- 当前角色移动、摄像机与闪避的完整人工试玩和回归记录。整理期间用户仍在编辑 PlayerObject 的摄像机方向计算；本提交是保存时的开发中快照，不声明镜头相对移动已完成。
- 跑步、闪避、攻击时衣物穿插、碰撞调参与性能采样。
- 普攻连击、技能、受击与死亡、敌人行为、属性与伤害、NPC 对话、任务、场景推进、临时 Buff、UI、音频和存档闭环；未取得本次完整实现或验收证据。
- 最终发布构建与功能测试。不能以素材导入、代码存在或历史测试代替最终版本验收。

游戏范围为单一剧情探索模式，仅部分战斗节点加入三选一临时 Buff，不扩展为独立 Roguelite 模式。

## 2026-10-15 后续：移动与冲刺衔接排查

最新保存代码已采用 currentSpeed 标量并沿角色朝向移动，Animator 参数更新为 Velocity。磁盘 Cyrene.controller 仍为 XSpeed/YSpeed 二维混合树，需核对用户未保存的 Inspector 修改。冲刺位移计时结束后仍恢复普通移动，已提出以 isQuicking 锁住整个冲刺状态，并在 OnStateExit 解锁的方案；本次未修改玩法代码、Animator 或动作资源，也未运行验证。详见 [处理过程](records/09-movement-and-dash-transitions.md)。当前已保存场景显示 PlayerObject 绑定 Cyrene_Blender4.2，旧“控制仍在 CATS”说明只对应前一阶段。

## 2026-10-15 最新：用户完成 MoveWeight 并补充转向锁定

用户报告已完成修改。静态核实 Cyrene.controller 已保存为 1D/Velocity（Idle/Walk/Run=0/0.5/1）；MoveWeight Float 默认值为 1；冲刺片段显式曲线为 0，普通片段通过参数默认值 1 提供恢复侧权重；代码读取并乘到普通水平位移，转向条件含 !isQuicking。实际使用变量名为 MoveWeight，未使用新 Layer。原 StartQuick/EndQuick 事件机制保留，未采用先前候选 QuickExitBehaviour 方案。本轮未运行验证，不将用户完成声明等同于全面试玩验收。用户想法及建议/采纳过程见 [实施确认](records/10-user-design-and-implementation.md)。此前未保存/待实施说明仅为早期状态。本次没有 Git 提交推送。

## 2026-10-15 后续：剑模型外观替换与右手挂点

用户提出复用原 SwordGirl 预制件的武器系统。检查发现原系统已有约束容器、三个挂点和 SwitchSocket 事件；换剑应保留外层约束，只更换子物体外观。之后发现 Hand_R_Socket 仍绑定复制的原素材 hand_r，Cyrene 的 Avatar 右手实际为手首.R。用户按建议重新挂接后确认“这下好了”；最新静态文件显示挂点父引用已改到 Cyrene 模型。该结果属于用户确认与静态核验，未独立运行采样。原武器控制器尚未绑定、PlayerObject.SwitchSocket 为空，收放剑切换和辅助轨迹适配仍待完成。记录见 [挂点处理记录](records/11-weapon-prefab-and-hand-socket.md)。本次只整理文档，不改玩法或资源，不提交推送。

## 本轮整理：攻击原型、UI/对话骨架与清理

详情见 [阶段总结](records/12-stage-summary-20261015.md)与 [动画/后摇/编译清理](records/13-animation-stock-dash-and-cleanup.md)。最新冲刺 Exit Time=0.3，EndQuick 导入事件已前移，过渡时长仍约0.317秒；不宣称手感已验收。角色脚本、场景入口与历史基线的差异已更新。第三方 LitJson 与所有素材继续只留本地，恢复说明已补充。

## 2026-10-16 收工：对话测试确认

用户确认本轮对话功能测试完成，剩余小漏洞暂未具体列出，计划下一工作日处理。已保存 ShowMe 登记、列表加载、动态选项、同帧输入保护、索引/判空/末尾保护、SetParent(canvas,false) 与 F 键测试入口。本轮收工只归档和 Git 操作，没有再修改玩法或重复运行测试。指定 Test1_1.json 作为自编测试数据提交，UI 预制件、模型与第三方插件继续本地保留。

## 2026-10-28 当前补充：配置管理、命中与敌人血条

本节优先于上文历史快照。玩家/敌人配置已改成各一份 CharacterConfig SO，内部使用 List<CharacterData>，以表内下标作为当前角色ID；ConfigMgr 根据 Player/Enemy 枚举预加载并保存引用。最新玩家与敌人 Awake 在 base.Awake 后调用 Init(0,type)，生命组件由该方法初始化，敌人初始及受伤后刷新血条。

OnAttackCheck 已有球形命中查询与伤害传递，Enemy Layer 与 World Space Canvas 引用已保存。这仍是基础原型：无本次修复后运行验收，没有据此宣称完整战斗、动态生成器、NPC AI、Buff或任务闭环完成。当前角色数据为玩家2攻击/3防御/100生命，敌人1攻击/1防御/10生命；历史NaN/Infinity已定位到当时未调用初始化，最新代码已接入，恢复效果待验证。

对象池空栈/路径修正已在已有提交保存；ClearPool仍按用户决定仅清字典。世界空间血条采用Filled Image，LookMe朝向摄像机。当前布局按用户决定保留。本次补记管理器与NPC、对象池/UI、配置与血条、动画恢复四份过程记录，并保存静态证据。没有额外修改玩法代码或模型资源。

[配置与生命值记录](records/17-character-config-health-and-hit.md) · [本轮日志](logs/2026-10-28-config-health.md) · [交付清单](deliveries/2026-10-28.md)

## 2026-10-29 当前补充：任务面板 UI

GMScene 的 Canvas/MissionPanel 已搭建静态 UI：主线/支线页签、8条任务示例、右侧详情、3个奖励占位和1个追踪按钮。所有按钮 OnClick 留空，没有新增任务运行逻辑；用户计划自行实现。Unity 编辑器编译、结构检查及 1920×1080 Game 视图外观核对完成，未进入 Play Mode，不视为任务流程验收。详见 [任务界面记录](records/19-mission-panel-ui.md)。本轮未提交或推送。
