# Avatar 映射与插件处理记录

整理日期：2026-10-15。以下为历史执行结果，不能直接覆盖当前新模型的导入设置。

## 旧 FBX 的可选骨骼误映射

2026-10-14 对 `Cyrene_Blender4.2.fbx.meta` 移除了三条错误的可选映射：LeftEye → 刘海1.L、RightEye → 刘海1-2.R、Jaw → 中刘海1-1。只修改这三条 Human Bone 条目，FBX 哈希不变，其他 meta 内容不变。执行时未进行播放验证。当前 CATS 模型是另一资源，不应套用旧模型的整份 meta。

证据：[修正结果](../evidence/avatar-mapping.json)。原 meta 备份留在本地工作记录。

## 插件清理与 Shader 试用撤销

- 按用户要求移除 `com.autodesk.fbx@5.1.3`，归档 PMX2FBX 安装检查标记与生成的 csproj；核对 218 个资源文件，未发现变化。见 [插件清理结果](../evidence/plugin-cleanup.json)。
- StarRailNPRShader 试用在用户叫停后撤销；manifest 与 lock 文件精确恢复，218 个资源文件不变。见 [撤销结果](../evidence/starrail-rollback.json)。下载副本留在本地工程外，未上传。
- 当前本地 `Assets/Shader` 中存在 Nilo 的示例 Shader，面部黑块处理针对这个实际 Shader。不能将早期“只评估、未导入”的状态当成当前状态，也不能将第三方 Shader 写为项目原创成果。

当前包依赖以仓库 `Packages/manifest.json` 与 `packages-lock.json` 为准。
