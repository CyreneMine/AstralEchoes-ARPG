# Blender4.2 模型面部黑块：另一套材质未启用裁剪

日期：2026-10-30。Unity 6000.4.8f1；GMScene。

## 用户观察、目标与想法

用户提供角色额头和嘴部出现黑块的截图，指出此前处理过相同问题，要求再次修复并解释原因。本次用户未另外提出具体技术假设；沿用历史修复记录作对照，不能将排查结论记为用户的独立方案。

## 当前文件证据与辅助分析

历史修复对象是旧 CATS 模型使用的 `Assets/ArtRes/Character/Cyrene/Mat_Cats/顏2+.mat`，GUID `aa0824db18511f24fb8a62a464da1cf7`。本次检查，它仍为 BaseColor Alpha=0、_UseAlphaClipping=1，且裁剪关键字有效。

当前 GMScene 引用 `Assets/Resources/Prefab/Cyrene_Blender4.2.prefab`，它基于 `Cyrene_Blender4.2.fbx`。FBX 原有材质映射将 `顏+`、`顏2+` 指向 `Materials` 文件夹下的两个材质，分别为 GUID `095afbebcf6b2b849865dc53d3788f8a` 和 `23af62e5c76ca7b4b8ab45669ed2640e`。这与旧 Mat_Cats 材质并非同一资源。

当前两个材质的 Alpha 已为 0，但 _UseAlphaClipping=0 且 m_ValidKeywords 为空；BaseMap 都引用 `颜赤.tga`。Shader 源文件 `Assets/Shader/SimpleURPToonLitOutlineExample.shader` 的颜色 Pass 使用 `Blend One Zero` 和 `ZWrite On`，并非透明混合。Shared.hlsl 中的 `DoClipTestToTargetAlphaValue` 只在 `_UseAlphaClipping` 变体启用后执行 `clip(alpha - _Cutoff)`。因此单独设置 Alpha=0 不会隐藏黑底覆盖层。

结论：本次有直接证据支持“当前模型使用另一套未启用裁剪的材质”。不能据此认定 Unity 重启改坏了上次已修复的材质，也没有证据指出最近哪个操作首次触发可见症状。

## 实际修改

仅修改两个材质：

- `Assets/ArtRes/Character/Cyrene/Materials/顏+.mat`
- `Assets/ArtRes/Character/Cyrene/Materials/顏2+.mat`

将 _UseAlphaClipping 从 0 改为 1，并在 m_ValidKeywords 加入同名关键字。保持原 Alpha=0、Cutoff=0.5、贴图引用与其他参数。没有修改 Shader，也没有添加自动修改材质的运行脚本或导入钩子。

这恢复的是默认隐藏的腮红/表情覆盖层。若后续需要动态显示腮红，应另行实现相应透明材质和表情控制；不能只把当前不透明 Shader 的 Alpha 改回 1。

## 验证与当前状态

修复前 Unity Scene 视图现场复现与用户截图相同的黑块。修改后执行 Unity Refresh，额头与嘴部黑块消失。随后再次检查磁盘上的材质参数，确认两者 Alpha=0、裁剪属性=1、关键字有效。

对场景、角色预制件、Cyrene FBX 及其 meta、Shader 文件进行修改前后 SHA256 核对；这些文件在本次修复中保持一致。原始两个材质已备份到本轮 `修改前` 目录。未重新启动 Unity，未进行 Play Mode 或构建测试，不宣称重启回归已经完成。没有提交或推送。

## 论文素材与后续使用

该案例可以说明透明度与渲染状态的区别、覆盖材质的默认可见性，以及排查时按实际资源 GUID 和引用链区分同名材质的重要性。历史记录见 [旧 CATS 覆盖层修复](07-face-overlay.md)。本次证据保存在本轮工作目录的 `修复证据.json`；参考历史记录不能替代本次文件核查。
