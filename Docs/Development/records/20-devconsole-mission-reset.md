# 控制台清空任务存档

日期：2026-10-30。Unity 6000.4.8f1；GMScene。

## 用户目标与想法

用户要求为已有 DevConsole 增加一个清空玩家任务存档的按钮，并提供当前保存入口：`JsonMgr.Instance.SaveData(playerMissionProgress,"MissionData")`。

## 文件分析与建议

实际 `JsonMgr.SaveData` 将数据写到 `Application.persistentDataPath/MissionData.json`。`MissionMgr.playerMissionProgress` 同时保存当前内存进度，`MissionProgressData.missions` 默认为空列表。若仅清除磁盘，旧的内存进度还可能被再次保存，因此按钮同时重置磁盘和内存。

采用空进度保存方案：先通过原 JsonMgr 保存新的 `MissionProgressData`，内容为 `{"missions":[]}`；保存成功后再替换管理器中的进度对象。写入异常时给出错误提示并保留原内存进度。当前未发现 `StreamingAssets/MissionData.json`，没有更改 JsonMgr 的加载优先顺序或任务配置表。

## 实际修改

- `Assets/Scripts/UI/DevConsole.cs`：新增公开按钮方法 `ClearMissionSave()`，显示成功或失败消息。该操作不依赖角色生命组件初始化。
- `Assets/Editor/DevConsoleSetup.cs`：同步新建控制台的布局，并增加升级已有控制台的入口；仅运行添加按钮分支，不执行清空和旧的自动 Play Mode 检查。
- `Assets/Resources/UI/Canvas.prefab`：添加 `DevConsole/Window/ClearMissionSave` 按钮及持久回调，窗口高度从 818 增至 874，提示文本下移。更新预制件使运行时动态生成 Canvas 也能得到按钮。
- `Assets/Scenes/GMScene.unity`：保存场景中的控制台布局，保留 Canvas 原有启用状态。

任务系统代码由用户完成，本次没有修改 MissionMgr、JsonMgr、MissionPanel 或任务配置。若任务面板已经展开，清空后需关闭并重新打开以重新生成任务列表；没有额外加入任务 UI 刷新系统。

## 验证状态

Unity 实际编译后执行升级入口，报告显示 Canvas 预制件 1 个控制台、GMScene 1 个控制台均已保存。升级工具检查新按钮有且仅有一个持久回调，方法为 `DevConsole.ClearMissionSave`；序列化文件再次检查一致。

没有进入 Play Mode，没有点击清空按钮，也没有调用 SaveData 清空用户当前存档；因此实际清空和再次接取任务的运行验收由用户执行。未提交、推送。

证据：本轮修改前备份、`mission-reset-button.json` 和 Unity Editor.log 中 `[DevConsole] 已添加清空任务存档按钮；未执行清空操作。`。本案例可作为开发调试入口及内存/持久化状态一致性的过程素材，不代表完整任务系统验收。
