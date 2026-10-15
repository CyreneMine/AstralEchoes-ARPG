# 资源与本地恢复说明

本次按用户要求仅提交自编代码、Input System 配置、场景/Animator/URP/项目文本配置以及开发文档。

## 未上传内容

- `Assets/ArtRes`：角色 FBX/PMX、贴图、材质和物理预制体。
- `Assets/CombatGirlsCharacterPack`、`Assets/KAWAII_ANIMATIOMS_100`：第三方角色与动作资源包。
- `Assets/MagicaCloth2`：本地插件源码与资源。
- `Assets/Shader`：本地导入的第三方 Nilo 示例 Shader。
- `Assets/Resources/Animation`：FBX 动作、动作副本及测试材质。
- `Assets/Resources/Prefab`、`Assets/Resources/UI`：角色/剑和 UI 预制件；本轮 UI/Canvas.prefab、DialoguePanel.prefab、OptionsPanel.prefab、Option.prefab 及其原 meta 需补回。
- `Assets/Scripts/Json/LitJson` 与原目录 meta：本地第三方 JSON 库；自编 JsonMgr 默认使用它，单独克隆缺少库会编译失败，需按原来源合法补回。
- 模板 TutorialInfo、截图、音视频、备份、编译产物与 Unity 缓存。

## 恢复时的顺序

1. 使用 Unity 6000.4.8f1；Package Manager 根据 manifest 和 lock 还原公开依赖。
2. 从用户本地备份恢复上述资源目录，连同原 `.meta` 文件一起恢复。不要重新生成 GUID，否则场景、Animator 和渲染配置的引用可能丢失。
3. 当前角色为 `Cyrene_Blender4.2.fbx`，GUID 为 `8a20f58c2d3f4ef46b0e6140851c62db`；场景使用本地角色预制件，当前 Animator 为 `Cyrene.controller`。旧 Cyrene_cats 属历史版本，不能靠同名模型代替当前引用。
4. 本地 Magica Cloth 2 配置时版本为 2.18.4；当前场景包含该插件组件。补回插件与素材后再打开 GMScene 检查 Missing 引用、材质和动画事件。
5. 当前面部修复保存在本地 `Mat_Cats/顏2+.mat`，未随资源上传。恢复旧备份时需参照 [面部覆盖层记录](Development/records/07-face-overlay.md)核对。

仓库是源码与配置快照，单独克隆不保证场景完整显示或运行；这不等于项目源代码缺失。素材及插件的使用与再分发要求以各自实际许可为准，本记录不宣称已取得额外授权。

## 2026-10-16 对话测试补充

自编 `Assets/StreamingAssets/Dialogue/Test/Test1_1.json` 及必要目录/文件 meta 已纳入本轮提交，其余 StreamingAssets 仍默认留本地。测试入口按 F 调用 ShowPanel<DialoguePanel>() 和 StartDialogue("Test/Test1_1")；需先补回上述本地 UI 预制件、字体等依赖，并保留原 GUID，才能复现完整画面。本轮本地 UI 快照保存在毕业设计工作记录的“对话系统实现与收工归档_20261016/本地资源快照”中，不向仓库上传。
