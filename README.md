# 星渊回响 · AstralEchoes

基于Unity3D的ARPG游戏《星渊回响》设计与实现。Unity 6 / C#，二次元风格第三人称 ARPG 毕业设计原型。

当前处于角色资源适配、移动/动画和衣物物理验证阶段。游戏范围为单一剧情探索模式，部分战斗节点加入三选一临时 Buff。

## 开发环境与入口

- Unity：6000.4.8f1；URP：17.4.0；Input System：1.19.0；Cinemachine：3.1.7。
- 当前场景：`Assets/Scenes/SampleScene.unity`。
- 当前控制代码：`Assets/Scripts/Object/PlayerObject.cs`；当前 Animator：`Assets/Resources/Animator/Cyrene_Cats.controller`。
- 历史临时测试：`Assets/Scripts/Testing/CyreneMovementTest.cs`。旧 Editor 配置工具仅用于旧测试对象，操作前先看适用范围。

## 文档

- [文档总入口](Docs/README.md)
- [当前开发状态](Docs/Development/STATUS.md)
- [问题处理与验证记录](Docs/Development/README.md)
- [资源补回说明](Docs/ASSET_RESTORE.md)

本仓库不包含角色、贴图、动作素材和第三方插件。完整运行需补回本地资源及其原 `.meta`；仅克隆仓库不是完整可玩成品。历史测试不自动代表最新代码已经验收。
