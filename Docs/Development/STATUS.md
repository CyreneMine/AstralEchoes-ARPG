# 当前开发状态

核验日期：2026-10-15。正式题目：基于Unity3D的ARPG游戏《星渊回响》设计与实现。

## 当前基线

- 本地工程：`E:\UnityProjects\AstralEchoes`；论文材料与原始记录在 `E:\毕设`。
- Unity 6000.4.8f1；URP 17.4.0；Input System 1.19.0；Cinemachine 3.1.7。
- 当前场景：`Assets/Scenes/SampleScene.unity`；角色：CATS 处理后的 `Cyrene_cats.fbx`，原名 `untitled.fbx`，GUID 未变。
- 场景角色绑定 `PlayerObject`、CharacterController、`Cyrene_Cats.controller`，并包含 Cinemachine 摄像机配置及 Magica 衣物组件。
- 本次版本记录是磁盘上已保存文件的快照；不自动保存 Unity 中未保存的编辑内容。

## 实现与证据

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

## 待完成或待验证

- 当前角色移动、摄像机与闪避的完整人工试玩和回归记录。整理期间用户仍在编辑 PlayerObject 的摄像机方向计算；本提交是保存时的开发中快照，不声明镜头相对移动已完成。
- 跑步、闪避、攻击时衣物穿插、碰撞调参与性能采样。
- 普攻连击、技能、受击与死亡、敌人行为、属性与伤害、NPC 对话、任务、场景推进、临时 Buff、UI、音频和存档闭环；未取得本次完整实现或验收证据。
- 最终发布构建与功能测试。不能以素材导入、代码存在或历史测试代替最终版本验收。

游戏范围为单一剧情探索模式，仅部分战斗节点加入三选一临时 Buff，不扩展为独立 Roguelite 模式。
