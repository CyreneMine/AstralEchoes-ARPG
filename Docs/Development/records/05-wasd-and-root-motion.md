> 整理于 2026-10-15。本文保留执行当时的对象名称、方案和验证范围；当前入口见 [开发状态](../STATUS.md)。原始记录位于本地 `工作记录/WASD移动测试_20261015/实现与验证记录.md`。资源、截图和备份未上传。

> 当前场景已改用用户编写的 `PlayerObject` 与 `Cyrene_Cats.controller`；本记录中的七阶段测试与偏移采样只对应旧测试脚本、旧控制器及四个动作副本。

# Cyrene 临时 WASD 移动测试

- 日期：2026-10-15。
- 当前工程：`E:\UnityProjects\AstralEchoes`，Unity 6000.4.8f1，Input System 1.19.0。
- 用户要求：补齐现有 Idle 和混合树，并编写使用新输入系统的临时移动脚本。

## 已完成

保留现有 Animator 的 `speed`、`HSpeed`、`VSpeed` 参数与 Idle 状态。混合树改为 2D Simple Directional，包含中心 Idle、前进 Run02、后退 Run_Bwd、左移 Run_Left、右移 Run_Right。四个移动动作复制为独立循环 `.anim`，第三方原始动画文件不修改。

Idle → 移动：`speed > 0.1`；移动 → Idle：`speed < 0.1`。两个方向均关闭 Exit Time，固定过渡时间为 0.12 秒。

运行时脚本：`Assets/Scripts/Testing/CyreneMovementTest.cs`。在 Awake 中复制 Input Actions 资源，读取 `Player/Move`；在 OnEnable/OnDisable 中启停本组件的 Move Action；在 Update 中使用 CharacterController 移动、施加重力并更新动画参数。二维输入限制为最大长度 1，避免斜向移动加速。关闭 Animator Root Motion，避免动画与脚本重复移动。测试角色固定朝向，W/S 前后移动，A/D 左右侧移，摄像机平移跟随。

编辑器配置菜单：`Tools/AstralEchoes/配置 WASD 移动测试`。只针对当前打开的 SampleScene，添加 CharacterController、测试移动组件、地面并调整测试摄像机位置。场景已保存。

## 实际验证

Unity 实际加载并编译脚本，确认角色 Avatar 有效且为 Humanoid。在 Play Mode 中添加一次性虚拟 Keyboard，通过 Input System 输入队列分别发送 Idle、W、S、A、D、W+D、释放按键，共七个阶段，每阶段约 1.2 秒。

- W/S/A/D：方向正确，约移动 3 米，进入移动状态。
- W+D：约移动 2.99 米，与单方向速度一致。
- 释放按键：水平位移为 0，回到 Idle。
- 所有阶段 CharacterController 均报告落地。
- 详细原始结果：`PlayMode移动测试结果.json`。
- 自动测试结束后已退出 Play Mode，移除虚拟键盘并恢复输入设置。工程脚本中已移除一次性自动播放与自动按键测试钩子。
- 最终精简脚本使用当前 Unity 与 Input System 程序集进行独立编译检查，无编译错误；这一步与上述 Play Mode 集成测试分别记录。

## 边界与备份

本次是临时移动测试，未实现转向、跳跃、闪避、战斗或衣物物理，也未验证所有复杂动作的美术表现。

FBX、FBX 导入配置、原始 Input Actions、Packages manifest 的 SHA256 均与修改前一致。修改前 Animator 与场景备份保存在 `backup_002341`。完整一次性集成测试工具保存在工作记录中，未作为自动运行代码留在工程内。

## 使用

打开 SampleScene，等待编译完成，点击 Play 后点击 Game 窗口，再使用 WASD。角色根对象上的 Cyrene Movement Test 组件可调整 Move Speed（默认 2.5）、Animation Damping（默认 0.1），也可关闭 Follow Camera。

## 后续修正：前进时身体大幅左右摆动

用户实际试玩反馈：前进时同时快速、大幅左右位移。此前七阶段 Play Mode 检查只记录角色根对象的位置、状态和落地情况，未记录骨骼相对根对象的偏移，因此未发现这一视觉错误；不能把此前通过的结果解释为动作外观已经完整验证。

原因已通过同一模型的隔离预览采样确认：生成独立动作副本时，配置工具错误地强制开启 `loopBlendOrientation`、`loopBlendPositionY`、`loopBlendPositionXZ`。这些字段涉及根运动的 Bake Into Pose，不能当作普通的循环平滑开关。有位移的跑步轨迹与旋转被烘焙进身体姿态，产生了异常的横向摆动。可参考 [Unity 根运动文档](https://docs.unity3d.com/6000.0/Documentation/Manual/RootMotion.html)。

修复：四个测试动作副本完整继承对应源动作的 AnimationClipSettings，只将 `loopTime` 设为 true。配置工具同步改为从源动作读取设置，不再强制开启上述开关。原始 FBX、Avatar、Input Actions、移动脚本及场景配置均未在本次修正中修改。

验证：使用 PlayableGraph 在隔离预览场景中，对同一角色以每秒 60 次、共 120 次求值，记录骨盆与头部相对模型根对象的位置范围。修正前 MoveForward 骨盆横向峰峰范围为 1.0332 米，修正后为 0.0870 米，与原始 Run02 的 0.0870 米完全一致。后退从 0.4136 米降为 0.0666 米，左移从 0.3957 米降为 0.0591 米，右移从 1.0983 米降为 0.0745 米。该数据是隔离动作采样，不能冒充修正后的完整人工试玩验收。

原始记录：`动画身体偏移_修正前.json`、`动画身体偏移_修正后.json`。一次性诊断工具已移出 Unity 工程。用户再次试玩验收待确认。
