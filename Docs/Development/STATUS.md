# 当前开发状态

核验日期：2026-10-15。正式题目：基于Unity3D的ARPG游戏《星渊回响》设计与实现。

## 当前基线（本轮整理核对）

- 本地工程：`E:\UnityProjects\AstralEchoes`；Unity 6000.4.8f1 / URP 17.4.0 / Input System 1.19.0 / Cinemachine 3.1.7。
- 当前场景为 `Assets/Scenes/GMScene.unity`，从 SampleScene 重命名且 GUID 保留；构建入口已同步。
- 当前角色使用本地 Cyrene_Blender4.2 预制件；控制脚本位于 `Assets/Scripts/Object/Player/`，控制器为 Cyrene.controller。
- 已有移动/冲刺代码、攻击输入与动画接口、UI/对话/JSON 基础代码；对话、生命值、主入口等仍含空壳，未完成全部玩法验收。
- 旧 Testing 和对应 Editor 工具已移除。文中早期“尚未迁移”“尚未提交”等说明只代表各记录当时状态，不是最新基线。
- [本阶段总结](records/12-stage-summary-20261015.md)说明实际保存值、用户贡献、验证范围和待办；本轮分模块提交结果在后续交付清单记录。

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
