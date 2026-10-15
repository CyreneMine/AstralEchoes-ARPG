> 整理于 2026-10-15。本文保留执行当时的对象名称、方案和验证范围；当前入口见 [开发状态](../STATUS.md)。原始记录位于本地 `工作记录/Cyrene_BoneCloth配置_20261015/配置与验证记录.md`。资源、截图和备份未上传。

> 名称更新：配置时的 `untitled.fbx` 已更名为 `Cyrene_cats.fbx`，GUID 仍为 `164b0b633681e7e4a9d2da2153e2157a`。当前场景入口为 `Cyrene_cats/MC2_Clothing`。文末旧 WASD 挂载说明仅对应当时。

# Cyrene 新模型 BoneCloth 配置记录

日期：2026-10-15。工程：`E:\UnityProjects\AstralEchoes`。Unity 6000.4.8f1，Magica Cloth 2 2.18.4。

## 配置目标与交付

用户授权为 CATS 处理后的新模型完整配置衣物 BoneCloth。配置对象是场景中的 `untitled`，其源资源为 `Assets/ArtRes/Character/Cyrene/untitled.fbx`。没有向旧的 Cyrene_Blender4.2 模型添加衣物物理。

- 场景：`Assets/Scenes/SampleScene.unity`，已保存用户原本未保存的新模型和材质设置，并在配置前复制场景备份。
- 物理预制体：`Assets/ArtRes/Character/Cyrene/Physics/Cyrene_BoneCloth.prefab`。源 FBX 不变，独立保存物理配置。
- 场景衣物组件入口：`untitled/MC2_Clothing`。
- 一次性配置和自动测试脚本已移出 Unity 工程，源码归档在本目录。

## 衣物分组

| 分组 | 根骨数量 | 总骨骼数（含固定根骨） | 连接方式 |
|---|---:|---:|---|
| 01_Skirt 主裙摆 | 12 | 143 | Sequential Loop Mesh |
| 02_BackSkirt 后裙 | 1 | 8 | Line |
| 03_BackVeil 后纱与长纱 | 1 | 37 | Line |
| 04_Sleeve_L 左袖摆 | 4 | 8 | Line |
| 04_Sleeve_R 右袖摆 | 4 | 8 | Line |
| 05_HeadVeil 长、短头纱 | 4 | 36 | Line |

共六组，覆盖 240 根互不重叠的衣物骨骼。主裙摆根骨依据骨盆局部空间中的实际方位排序后环状连接。每组根骨固定，`rootRotation=0`；使用 AnimatorLinkage 更新模式；Animation Pose Ratio 为 0，保持衣物初始形状作为恢复参考。

采用较低重力、0.18 阻尼、角度恢复与 30–48 度角度限制、随深度变化的最大位移限制。远距离瞬移或大幅旋转时重置模拟。调参阶段关闭摄像机剔除，以免新模型位于当前 Game 摄像机外时模拟暂停。未启用衣物自碰撞，后续根据具体穿插情况与性能需要再评估。

头发、面部、身体主骨骼、胸部和刚性饰品未加入本次衣物模拟。

## 碰撞体

共 11 个 Magica 专用碰撞体：骨盆与头部两个球形碰撞体，躯干、左右大腿、左右小腿、左右上臂、左右前臂九个胶囊碰撞体。胶囊根据骨骼端点定向，尺寸按模型实测高度约 1.475 米缩放，并补偿 Transform 缩放。所有六组已登记碰撞体并使用 Edge 碰撞模式。

这些是布料求解器使用的碰撞体，不添加 Rigidbody，也不替代角色移动用的 CharacterController。

## 验证证据

- 配置工具使用当前 Unity 与 MagicaCloth 程序集进行编译检查，无编译错误。
- 在 Unity Play Mode 中，六组均获得有效 TeamId，`IsValid` 和 `IsRunning` 均为 true。
- 在不保存运行时变动的前提下，让新模型根对象做小范围平移与转向；共采样 537 次。
- 检测到衣物骨骼摆动，最大局部旋转变化约 38.36 度；固定根骨的局部位置漂移为 0；采样位置全部为有效有限数值。
- 测试结束后自动退出 Play Mode，根对象恢复运行前位置与旋转。
- 配置及测试期间 Unity 日志未出现新的编译错误或异常。
- 验证了 76 个源模型、导入设置、原始贴图和 Mat_Cats 文件的 SHA256，均未变化。

详细记录：`配置结果.json`、`PlayMode验证.json`、`原始资源SHA256.json`。配置前场景：`SampleScene.before-bonecloth.unity`；更早磁盘场景：`SampleScene.disk-before.unity`。

本次验证覆盖组件构建、根对象移动/转向驱动的摆动、数值稳定性及固定根骨保持，不代表已经完成所有跑步、闪避和攻击动作的穿插验收，也未开展性能基准测试。

## 如何查看与调参

在 Hierarchy 中展开 `untitled → MC2_Clothing`，选择六个子对象中的任一个，调整 MagicaCloth 的重力、阻尼、角度恢复和碰撞参数。

Play 后在 Scene 中移动或旋转新模型，可以观察衣物惯性。当前 WASD 测试脚本仍挂在旧模型上，本次没有迁移移动控制、Animator 或修改新模型的 Avatar。

若画面被绿色碰撞体线框遮挡，可通过 Scene 右上角的 Gizmos 开关或对应 Magica 组件的 Gizmo 选项隐藏辅助显示；这些线框不是渲染出来的衣物。

使用依据：[官方 BoneCloth 教程](https://magicasoft.jp/en/mc2_boneclothstartguide/)、[碰撞设置](https://magicasoft.jp/en/mc2_collision_setup/)。
