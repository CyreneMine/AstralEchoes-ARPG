# 剑模型挂点与原预制件复用：问题处理记录

日期：2026-10-15。工程：AstralEchoes；Unity 6000.4.8f1，URP。目标角色：Cyrene_Blender4.2；新剑：Dawnbreak；参考素材：CombatGirlsCharacterPack / SwordGirl。

## 用户观察与目标

用户希望让自己的剑适配拔剑、收剑和后续挥剑动作，减少逐个动作手工调整。先后报告：切换选中对象会中断动画预览；动画含 add_weapon 的 Missing 轨道；替换原武器预制件后 Parent Constraint 消失、位置异常；保留外层并放入新剑后，剑仍完全不动。

本记录覆盖武器外观替换与右手跟随问题。收放剑的完整状态切换尚未验收。

## 用户提出的想法与疑问

1. 用户指出剑模型中的握持位置，希望据此对齐右手挂点。
2. 用户发现原动画还控制 add_weapon 节点，追问这些额外位置曲线的作用。
3. 用户主动提出：“就不能用它原本素材的预制件来快速完成剑的所有动作那些的配置吗”。这促使排查从手工挂点方案转向检查并复用素材自带武器系统。
4. 用户实际尝试替换，指出 Parent Constraint 丢失、位置出问题；之后提交新剑作为子物体的截图，再反馈“剑完全没有动”。
5. 用户进一步区分手先.R 是手掌、手首.R 是手腕，要求明确挂点到底应放在哪里。这促使解释骨骼驱动与握持偏移之间的关系。

以上思路与观察来自用户。原素材机制的文件分析和适配建议由 Codex 提供，不能全部归为任一方的独立设计。

## 辅助分析与方案调整

### 1. 原预制件已有武器系统，可以复用

检查原 SwordGirl 预制件及脚本发现：Character_Weapon_Controller 接收 SwitchSocket 动画事件；Weapon_Sword 外层通过 Parent Constraint 在 Hand_R_Socket、Put_Socket_Sword、add_weapon_r 三个来源之间切换。S_Take 动作导入配置已有 To_Put_Socket_Sword 和 To_Hand_R_Socket 事件。

add_weapon_r 属于作者用于控制武器轨迹的辅助节点。Humanoid 标准人体动作可重定向，但这些额外 Transform 轨道还依赖相对于 Animator 的正确路径与坐标。复制节点名称不等于完成所有动作适配。

早期建议是手工创建挂点并自行切换父物体；检查素材后调整为优先复用其现有控制器、约束容器和动画事件，保留 Cyrene 自己的骨骼、Avatar、衣物物理和配饰结构。这是方案调整，不应把早期建议写成已实施方案。

### 2. 换外观时要保留控制容器

原武器预制件的外层与内层都叫 Weapon_Sword，容易选错。外层有 Parent Constraint，内层是原剑的 MeshFilter / MeshRenderer。直接用 Dawnbreak 替换整个外层，会同时失去原约束组件，并使指向旧组件的配置失效。

建议保留外层，将完整 Dawnbreak 作为其子物体，关闭内层原剑的显示；只调整新剑子物体的局部位置、旋转和缩放以匹配握持位置。新剑原点和轴向不同，所以局部变换归零只是对齐起点，不保证立刻握住剑柄。保留新剑自身的网格和骨骼关系，不单独拆走其蒙皮骨骼。

```text
Weapon_Sword                 ← 原控制容器，保留 Parent Constraint
├─ 原剑显示对象              ← 关闭显示；当前文件已关闭对象
└─ Dawnbreak                 ← 新剑模型；用户可重新命名
```

### 3. 剑不动：约束仍引用原素材骨架

检查当时保存的角色预制件发现，用户复制了原素材 root/pelvis/.../hand_r 骨架。Hand_R_Socket 仍在这套 hand_r 下，外层武器的约束来源也指向它。Cyrene 的 Avatar 中 RightHand 实际映射为手首.R，原素材 hand_r 没有因为被复制进来就自动成为 Cyrene 的 Humanoid 右手。

因此角色可播放动作，但武器所跟随的来源并不是实际驱动角色右手的骨骼。这是本次排查发现的连接错误。Codex 此前只确认了新剑子物体层级，没有核对挂点归属，漏查这一点；文档保留此排查遗漏，避免将过程写成一次即正确。

建议把现有 Hand_R_Socket 移到 Cyrene 的手首.R 下，保留同一个挂点对象和约束引用，调整挂点局部偏移以校准掌心位置。跟随测试时，约束启用、手部来源权重为 1，其他来源为 0。

### 4. 手腕与手掌的区别

用户指出手先.R 位于手掌、手首.R 位于手腕。选择手首.R 的依据是当前 Avatar 的 RightHand 映射，挂点的局部偏移负责把剑柄放到掌心。骨骼驱动位置和最终握持位置不必重合。

本次未验证手先.R 在全部动作中的表现，不将其概括为不能使用或必然不参与动画；当前选择手首.R，是采用已核实的 Humanoid 手骨来减少适配不确定性。

## 实际采纳与用户实施

用户保留原武器外层，把新剑作为其子物体并关闭原剑。随后根据建议修改右手挂点归属，在说明手腕与手掌区别后反馈“这下好了”。上述编辑由用户在 Unity 中完成，Codex 本轮未代改模型、预制件、动画、玩法代码或 Avatar。

整理时再次静态检查：Hand_R_Socket 已移出旧 hand_r（Transform fileID 499016035340502183），父引用改为 Cyrene 模型内的 Transform（fileID 4807702949829026997，模型 GUID 8a20f58c2d3f4ef46b0e6140851c62db）。该父对象在 YAML 中是 stripped 引用，不直接保存骨骼名；手首.R 的实施名称依据 Avatar 映射、给出的操作步骤与用户反馈，未额外调用 Unity API 解析 fileID 到名称。

当前挂点局部缩放约为 0.01，可能涉及模型层级的单位换算；本轮没有改写或验证它。不能在后续整理时无依据地将所有相关 Transform 统一 Reset 或强制缩放为 1。

## 验证与当前状态

| 内容 | 证据与结论 | 边界 |
|---|---|---|
| 新剑外观替换层级 | 用户截图、已保存预制件：新剑位于原受约束外层之下，原剑对象已关闭 | 只确认结构，不代表全部动作通过 |
| 原错误挂点 | 排查时静态文件显示 Hand_R_Socket 在复制的 hand_r 下 | 不是对运行时骨骼位置的采样 |
| 目标手骨 | Cyrene FBX meta：RightHand → 手首.R | 以当前模型为准，不套用于任意模型 |
| 修改已保存 | 最新父引用属于 Cyrene 模型，已移出复制的 hand_r | stripped 引用未用 Unity API 反查名称 |
| 剑恢复跟随 | 用户按建议处理后回复“这下好了” | 用户确认；Codex 未独立 Play Mode 测试，无修正后采样数据 |
| 收放剑切换 | 保存的角色预制件和 SampleScene 未发现原 Character_Weapon_Controller；PlayerObject.SwitchSocket 仍为空方法 | 尚未接通与验收，不能写为拔剑、收剑已经完成 |

下一步需把收剑挂点适配到 Cyrene 的实际躯干，配置原控制器或明确的事件转发，核对辅助节点路径及比例，并验证拔剑、收剑、连续挥剑时的挂点切换与跳变。以上均为待办，不是已完成成果。

## 论文素材与证据入口

本案例可以用于说明第三方动作与自有角色组合时的资源适配：先检查已有机制，区分外观对象与控制容器，按 Avatar 映射绑定实际骨骼，再通过挂点局部偏移处理握持差异。问题定位依据包括层级关系、约束 Sources、Avatar 映射和事件接收函数，不应只靠对象名称或画面位置判断。

可用表述：“在武器跟随适配过程中，发现约束仍引用原动作素材的手部骨骼。将手部挂点重新绑定至角色 Avatar 对应的右手骨骼，并通过局部偏移校准握持位置后，用户反馈武器跟随恢复正常。”不能写为所有武器动作完成、测得性能提升或进行了完整自动化验收。

Parent Constraint、Humanoid 与素材自带武器控制器属于既有机制；项目工作是资源复用、绑定适配和故障排查，不将其描述为原创算法。素材授权范围未在本轮核实，不宣称已经获得公开发布授权。

文件依据：

- `Assets/Resources/Prefab/Cyrene_Blender4.2.prefab`
- `Assets/ArtRes/Character/Cyrene/Cyrene_Blender4.2.fbx.meta`
- `Assets/CombatGirlsCharacterPack/Sword_Girl/Prefab/Parts/Weapon_Sword.prefab`
- `Assets/CombatGirlsCharacterPack/Biperworks_Tools/CombatGirls_Weapon_Control/Character_Weapon_Controller.cs`
- `Assets/Resources/Animation/Charecter/Normal/S_Take.fbx.meta`
- `Assets/Scripts/Object/PlayerObject.cs` 与 `Assets/Scenes/SampleScene.unity`

静态核对和用户验收摘录见 [文本证据](../evidence/weapon-hand-socket.json)。修正前用户截图仅在本地工作记录目录存档，不上传素材图片。本记录与索引本次未提交或推送 Git。
