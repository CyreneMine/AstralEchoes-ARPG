> 本地原始记录：`工作记录/Cyrene_Blender42_BoneCloth_20261015`。原始模型、截图和备份未上传。

# Cyrene_Blender4.2 的 Magica Cloth 2 配置

日期：2026-10-15。Unity 6000.4.8f1，Magica Cloth 2 本地版本 2.18.4。

## 配置对象与交付

用户希望使用可以逐个开关配饰的 `Cyrene_Blender4.2.fbx`。本次为当前 SampleScene 中该模型的实例配置 BoneCloth，另生成独立预制体：

`Assets/ArtRes/Character/Cyrene/Physics/Cyrene_Blender42_BoneCloth.prefab`

场景入口：`Cyrene_Blender4.2/MC2_Clothing`。原 FBX GUID 为 `8a20f58c2d3f4ef46b0e6140851c62db`，未改变。保留现有 CATS 模型及其衣物配置；没有迁移 PlayerObject、摄像机或 Animator Controller。

## 分组与碰撞

| 布料组 | 骨骼数（含固定根） | 根骨与连接 |
|---|---:|---|
| 01_Skirt | 161 | 裙_0_0 至 裙_0_11，按骨盆局部方位排序，SequentialLoopMesh |
| 02_BackSkirt | 8 | 後中裙1-1，Line |
| 03_BackVeil | 51 | 後紗，Line |
| 04_Sleeve_L | 8 | 下袖1-1.L 至 下袖4-1.L，Line |
| 04_Sleeve_R | 8 | 下袖1-1.R 至 下袖4-1.R，Line |
| 05_HeadVeil_Short | 10 | 短头纱.L/R，Line |
| 06_HeadVeil_Long_L | 25 | 长头纱1.L，Line |
| 06_HeadVeil_Long_R | 25 | 长头纱1.R，Line |

合计 8 组、296 根互不重复的衣物骨骼。原模型保留了 CATS 模型中已简化的部分衣物骨骼，所以数量不同。

11 个 Magica 碰撞体覆盖骨盆、头、躯干、左右大腿/小腿/上臂/前臂。腿部使用实际蒙皮对应的足D/ひざD/足首D骨链，未修改 Humanoid 映射。尺寸根据约 1.475m 的骨骼高度适配。使用 Edge 碰撞、低重力、角度恢复与角度限制、最大位移限制，以及瞬移重置；没有启用自碰撞。

头发、脸、身体主骨骼和刚性饰品未加入衣物模拟。根骨固定，AnimatorLinkage 更新，Animation Pose Ratio=0。

## 保留配饰开关

原有 64 个 SkinnedMeshRenderer 仍是独立子节点，未合并或删除。可继续在 Hierarchy 选中某个网格，通过 Inspector 顶部复选框启用或停用；不要停用整棵骨架来隐藏网格。

例如 `057_頭紗` 与 `060_頭紗+` 可分别关闭。本次 Play Mode 测试实际关闭并恢复了这两个对象，保留它们原先的 activeSelf 状态。隐藏网格不会自动关闭对应布料计算；若无需任何头纱物理，可额外停用 MC2_Clothing 下的短头纱和左右长头纱组。

## 原始状态与资源保护

- 比较配置前后结构签名：690 个原有 Transform、64 个 SkinnedMeshRenderer、总计 316 个 BlendShape 条目保持一致。
- 核对原节点父子关系、局部位置/旋转/缩放、启用状态、材质引用、蒙皮骨骼引用、BlendShape 权重、Avatar 与 Controller 引用，签名完全一致。
- 204 个受保护文件的 SHA256 核对无变化，范围包括模型与 meta、原始贴图、两套材质及用户 PlayerObject.cs。
- 先保存并备份 Unity 当前内存场景，再添加本次配置。配置前快照为 `SampleScene.before-bonecloth.unity`；此前磁盘版本另存 `SampleScene.disk-before.unity`。
- 配置结束后场景与独立物理预制体均已保存。

## 实际运行验证

实际在 Unity Play Mode 中让该模型根对象进行小范围水平平移和转向，采样 396 次：

- 8 组均 valid=true、running=true。
- 位置数据均为有效有限数值。
- 固定根局部位置漂移为 0。
- 最大衣物骨骼局部旋转变化约 45.93°。
- 两个独立头纱网格关闭/恢复检查通过，Renderer 总数仍为 64。
- 结果见 `PlayMode验证.json`；完整配置见 `配置结果.json`，结构与文件核对见 `资源与结构核对.json`。

第一次测试器使用编辑态 Prefab 关联在运行态定位对象失败，没有取得运行样本；失败记录保留为 `PlayMode验证.首次定位失败.json`。随后按明确的场景模型与网格数量识别对象，并完成上述测试。调整测试器期间的播放态脚本重载曾触发现有 PlayerObject 的临时空引用；没有修改该用户脚本，最终退出播放后确认 Console 无红色错误，仍有第三方插件过时 API 警告。不能将排查过程中出现的异常写成“全过程无异常”。

测试完成后确认编辑模式，一次性配置/测试脚本及其 meta 已移出 Assets 并归档，执行标记已清理。最终源码为 `CyreneBlender42BoneClothSetup.final.cs`，不在 Unity 中自动执行。

本次验证只覆盖布料构建、根对象移动时的惯性、基本数值稳定性和网格开关。没有验收跑步、攻击、闪避时的所有穿插，也没有性能基准。该模型现有 Animator 未绑定 Controller，当前 WASD/闪避仍由 CATS 模型承担。

## 使用

可直接使用场景中的 `Cyrene_Blender4.2`，或拖入上述预制体。点 Play 后在 Scene 中移动/旋转该模型根对象查看布料摆动。正式接入角色控制时，还需单独绑定所需 Animator Controller、PlayerObject 和相机跟随目标，并验证实际动作；本次未自动替换。

本次未执行 Git 提交或推送；模型和物理预制体仍按仓库规则保留本地。
