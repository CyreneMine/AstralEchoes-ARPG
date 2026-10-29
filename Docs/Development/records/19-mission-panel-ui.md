# 任务面板 UI 框架

日期：2026-10-29。Unity 6000.4.8f1；场景：`Assets/Scenes/GMScene.unity`；对象：`Canvas/MissionPanel`。

## 用户观察与目标

用户计划当晚制作基础任务系统，提供《崩坏：星穹铁道》任务界面截图作为布局参考，并明确只委托搭建 UI，任务逻辑由用户完成。

## 用户提出的想法

- 保留界面框架，上方分类仅主线、支线。
- 右下角仅保留一个“追踪”按钮。
- 不制作左下角入口。
- 使用 UGUI 默认颜色和资源，不复刻参考图的配色、美术或完整界面。
- 在 GMScene 已有 MissionPanel 内搭建。

## 辅助分析与建议

检查确认 MissionPanel 是屏幕空间 Canvas 下的空 RectTransform，已有背景 Image。现有 Canvas 使用 1920×1080 的 Scale With Screen Size 配置。采用上方页签、左列表、右详情的结构，列表使用 UGUI 自带 ScrollRect、RectMask2D、Scrollbar、VerticalLayoutGroup、ContentSizeFitter；按钮使用内置 UISprite，分区使用内置 Background，文字复用工程现有中文 TMP 字体。

搭建工具位于 `Assets/Editor/MissionPanelSetup.cs`，只在编辑器内生成并保存场景对象，不向玩家运行时添加任务代码。再次执行时若面板已有子物体会拒绝覆盖，避免覆盖用户接线后的 UI。

## 实际搭建与后续接线

```
MissionPanel
├─ Header
│  ├─ TitleText / SubtitleText
│  ├─ CategoryTabs
│  │  ├─ MainQuestTab
│  │  └─ SideQuestTab
│  └─ CloseButton
└─ Body
   ├─ MissionListPanel
   │  ├─ ListTitleText
   │  └─ MissionScrollView
   │     ├─ Viewport/Content/MissionItem_01 … MissionItem_08
   │     │                    ├─ TitleText
   │     │                    └─ LocationText
   │     └─ VerticalScrollbar
   └─ MissionDetailPanel
      ├─ DescriptionArea
      │  ├─ LocationText
      │  ├─ MissionTitleText
      │  ├─ ObjectiveRow/ObjectiveText
      │  └─ DescriptionText
      ├─ RewardPreview
      │  ├─ RewardTitleText
      │  └─ RewardSlot1 … RewardSlot3
      │       ├─ Icon/NameText
      │       └─ AmountText
      └─ Footer/TrackButton
```

8 条列表项和经验/金币/道具数量均为排版示例，不是已加载的任务或奖励数据。任务列表项是 Button，可以保留一项作为后续动态创建的模板；Content 的布局组件负责排布数量变化的子项。

共 12 个 Button：2 个分类、8 个任务项、1 个关闭、1 个追踪，OnClick 全部留空。分类筛选、任务选择、详情刷新、追踪、面板关闭和输入控制均由用户后续实现；未添加 MissionPanel 运行脚本或管理器。当前 MissionPanel 为启用状态，便于直接在 Game 视图查看，显示时覆盖其下方的场景 UI。

## 验证与当前状态

- Unity 实际完成编辑器程序集编译和面板搭建，生成 `build-report.json`；本轮工具未产生编译错误。控制台现有 6 条警告来自原有第三方编辑器工具和 DevConsoleSetup 的过时 API。
- 编辑器检查滚动列表引用完整，无缺失组件，12 个按钮持久监听数量全部为 0。
- 已在放大的 1920×1080 Game 视图人工核对文字、分区和奖励/追踪布局，未见文字方框或控件重叠。
- 场景序列化比较：原有 86 个块无删除；原有发生变化的 3 个块均为 MissionPanel 的 GameObject、RectTransform、Image。其余新增块为本面板的 Canvas、GraphicRaycaster 和子控件。
- `Assets/Resources/UI/Canvas.prefab` 与修改前 SHA256 一致，未将界面应用到原 Canvas 预制件。现有中文字体可能由 TMP 自动增加动态字形缓存，未改其字体设置。
- 本轮未进入 Play Mode，未验证鼠标滚动、不同分辨率和任务业务流程；不据此宣称任务系统已实现。未构建、提交或推送。

## 论文素材与证据入口

可用于说明任务界面的列表/详情分区、UGUI 布局组件选择、UI 和任务数据逻辑分工。参考图仅用于布局讨论，不是项目截图，也没有把参考图资源导入工程。后续需要以用户实际接线、运行结果和最终界面补充系统实现证据。

本地备份和证据位于本次工作记录目录：`GMScene_修改前.unity`、`Canvas_修改前.prefab`、`build-report.json`、`scene-diff-audit.json`。

## 后续：用户接入任务逻辑

10月30日收尾检查时，用户已将界面保存为本地MissionPanel/MissionItem/RewardSlot预制件并接入动态列表、详情与奖励预览。本记录的“没有任务逻辑”只对应初次搭建时，最新情况见 [任务实现过程](22-mission-system-design-and-prototype.md)。
