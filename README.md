# 星渊回响 · AstralEchoes

基于Unity3D的ARPG游戏《星渊回响》设计与实现。Unity 6 / C#，二次元风格第三人称 ARPG 毕业设计原型。

当前处于角色资源适配、移动/冲刺、攻击动画原型与 UI/对话基础搭建阶段。游戏范围为单一剧情探索模式，部分战斗节点加入三选一临时 Buff。

## 开发环境与入口

- Unity：6000.4.8f1；URP：17.4.0；Input System：1.19.0；Cinemachine：3.1.7。
- 当前场景：`Assets/Scenes/GMScene.unity`。
- 当前控制代码：`Assets/Scripts/Object/Player/PlayerObject.cs`；当前 Animator：`Assets/Resources/Animator/Cyrene.controller`。
- 旧 Testing 与配套 Editor 测试工具已清理；历史测试证据保留在开发文档，不能代替当前玩法验收。
- 新增 UI、对话和 JSON 基础代码包含未完成接口；详见 [本阶段总结](Docs/Development/records/12-stage-summary-20261015.md)。

## 文档

- [文档总入口](Docs/README.md)
- [当前开发状态](Docs/Development/STATUS.md)
- [问题处理与验证记录](Docs/Development/README.md)
- [资源补回说明](Docs/ASSET_RESTORE.md)

本仓库不包含角色、贴图、动作素材和第三方插件。完整运行需补回本地资源及其原 `.meta`；仅克隆仓库不是完整可玩成品。历史测试不自动代表最新代码已经验收。
