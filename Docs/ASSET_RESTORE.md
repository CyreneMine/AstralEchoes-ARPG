# 资源与本地恢复说明

本次按用户要求仅提交自编代码、Input System 配置、场景/Animator/URP/项目文本配置以及开发文档。

## 未上传内容

- `Assets/ArtRes`：角色 FBX/PMX、贴图、材质和物理预制体。
- `Assets/CombatGirlsCharacterPack`、`Assets/KAWAII_ANIMATIOMS_100`：第三方角色与动作资源包。
- `Assets/MagicaCloth2`：本地插件源码与资源。
- `Assets/Shader`：本地导入的第三方 Nilo 示例 Shader。
- `Assets/Resources/Animation`：FBX 动作、动作副本及测试材质。
- 模板 TutorialInfo、截图、音视频、备份、编译产物与 Unity 缓存。

## 恢复时的顺序

1. 使用 Unity 6000.4.8f1；Package Manager 根据 manifest 和 lock 还原公开依赖。
2. 从用户本地备份恢复上述资源目录，连同原 `.meta` 文件一起恢复。不要重新生成 GUID，否则场景、Animator 和渲染配置的引用可能丢失。
3. 当前角色 FBX 为 `Cyrene_cats.fbx`，GUID 为 `164b0b633681e7e4a9d2da2153e2157a`；当前 Animator 为 `Cyrene_Cats.controller`。不能仅靠同名模型代替。
4. 本地 Magica Cloth 2 配置时版本为 2.18.4；当前场景包含该插件组件。补回插件与素材后再打开 SampleScene 检查 Missing 引用、材质和动画事件。
5. 当前面部修复保存在本地 `Mat_Cats/顏2+.mat`，未随资源上传。恢复旧备份时需参照 [面部覆盖层记录](Development/records/07-face-overlay.md)核对。

仓库是源码与配置快照，单独克隆不保证场景完整显示或运行；这不等于项目源代码缺失。素材及插件的使用与再分发要求以各自实际许可为准，本记录不宣称已取得额外授权。
