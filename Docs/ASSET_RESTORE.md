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
5. 旧CATS面部修复在 `Mat_Cats/顏2+.mat`；当前Blender4.2另使用 `Materials/顏+.mat`、`Materials/顏2+.mat`，两者都要保持Alpha=0并启用裁剪属性及关键字。材质未上传，参照 [当前覆盖层修复](Development/records/21-face-overlay-material-set.md)核对。

仓库是源码与配置快照，单独克隆不保证场景完整显示或运行；这不等于项目源代码缺失。素材及插件的使用与再分发要求以各自实际许可为准，本记录不宣称已取得额外授权。

## 2026-10-16 对话测试补充

自编 `Assets/StreamingAssets/Dialogue/Test/Test1_1.json` 及必要目录/文件 meta 已纳入本轮提交，其余 StreamingAssets 仍默认留本地。测试入口按 F 调用 ShowPanel<DialoguePanel>() 和 StartDialogue("Test/Test1_1")；需先补回上述本地 UI 预制件、字体等依赖，并保留原 GUID，才能复现完整画面。本轮本地 UI 快照保存在毕业设计工作记录的“对话系统实现与收工归档_20261016/本地资源快照”中，不向仓库上传。

## 2026-10-28 配置表与血条补充

- 自编 PlayerConfig.asset、EnemyConfig.asset 及原meta已加入窄范围白名单，替代旧Enemy_Test测试配置；这是作者编写的数值配置，不包括美术素材。不要恢复成旧单条SO字段结构，当前类包含 characterDatas 列表。
- World Space Canvas、HealthBar、BK、Fill、HealthBarController 与 LookMe 引用保存在 GMScene 中。场景引用的血条Sprite仍是本地资源，需连同原meta恢复；参考本轮静态证据中的GUID，不重新生成素材GUID。
- 当前F键入口在 InteractPanel 中，仅靠近NPC提示时触发；旧“直接F键开始对话”说明属于历史阶段。
- 运行前补回模型、动作动画事件、Shader、MagicaCloth2、UI预制件与LitJson。本轮没有重新发布或上传这些资源，也没有发布可独立运行构建。

## 2026-10-30 任务、HUD与控制台补充

- 自编 `Assets/Resources/GameData/Mission/MissionConfig.asset`、文件meta和Mission目录meta随任务模块提交。当前只有任务0的一段战斗目标和物品0数量100的测试奖励，不能当作完整任务内容包。
- 任务运行依赖本地 `Resources/UI/GamePanel.prefab`、`MissionPanel.prefab`、`MissionItem.prefab`、`RewardSlot.prefab` 及原meta。任务面板持有右侧目标/简介/奖励/Content/关闭按钮引用，条目仅保存自己的标题/地点/按钮，外部详情引用由Init传入；奖励槽须绑定两个文本字段。
- 恢复 `Resources/UI/Canvas.prefab`（包含DevConsole）、`Resources/DevConsole/DevConsoleFont.asset`、工程中文字体源及原meta。场景Canvas当前可处于用户设置的关闭状态，UIManager会在缺少活动场景Canvas时加载Resources版本；不要为修复引用随意重建GUID或替换布局。
- 清空按钮通过JsonMgr写入 `Application.persistentDataPath/MissionData.json` 并同步内存；这个文件是玩家存档，不上传，不应作为SO配置恢复。历史 `Mission/MissionData` 子路径错误不代表当前仍使用子目录。
- Editor搭建工具仅生成界面框架/控制台，拒绝覆盖已有MissionPanel子对象；不会重建用户已接线的完整任务UI。当前报告写到工程 `Temp/DevConsoleReports` 或 `Temp/MissionPanelReports`，旧本机历史报告另保留本地。
- 本轮界面与材质的修改前快照保留在毕业设计工作记录目录。仓库仍是需要补回本地资源的开发快照，未提供独立发布构建。

## 2026-11-05 敌人原型

- 恢复本地 `Assets/Resources/Prefab/EnemyObj.prefab` 及原 `.meta`，GUID 为 `0d0b28005492b914989176f4beaec0ea`。GMScene 的两个敌人实例依赖该预制件，保留其中的 EnemyObject、EnemyHealth、EnemyAI、EnemyCombat、NavMeshAgent、Animator 和血条引用。
- 场景中的 EnemyAI 实例覆盖了四个巡逻点，另一个 EnemyObj 实例关闭了 EnemyAI。还原预制件后检查这些场景覆盖。
- 仓库包含自编 EnemyAIConfig 数值表、EnemyAnimator 控制器、仅承载攻击事件的 TestAtk 测试片段和 GMScene 的 NavMesh 烘焙数据。测试片段没有动作曲线。

## 2026-11-06 NPC 对话与任务补充

- 恢复本地 `Assets/Resources/GameData/Npc/NpcConfig.asset` 和原 `.meta`。`ConfigMgr` 从 `GameData/Npc/NpcConfig` 加载该资产。测试 NPC `0` 的 `missionIds` 含任务 `0`。
- 恢复 `Assets/StreamingAssets/Dialogue/Npc/0/` 下的 `Mission_0_Accept.json`、`Mission_0_InProgress.json`、`Mission_0_Stage_1.json` 及必要的 `.meta`。这些文件当前由 `.gitignore` 排除。目录名 `Npc` 与加载路径一致。
- `MissionConfig.asset` 中任务 `0` 当前有两段，先击败一只 `0` 号怪，再和 `0` 号 NPC 对话。对白 UI 继续依赖本地预制件、TMP 字体资产和原始字体。复现时使用自己的测试存档，原有玩家存档保留在本地。
- 恢复本地 `Assets/Resources/UI/MissionPanel.prefab` 后，检查 `MissionPanel.txtTitleName` 是否指向详情区域的标题文本。当前本地预制件已保存该引用，旧版预制件需要补绑。

## 2026-11-09 完成后对白

- 任务完成后再次互动依赖 `Assets/StreamingAssets/Dialogue/Npc/0/Mission_0_Finished.json` 和对应 `.meta`。该文件当前受 StreamingAssets 的忽略规则影响，恢复本地对白时一并补回。正文为测试引导员的一句感谢，无选项。
- 当前对白仍按 `Npc/{npcId}/Mission_{missionId}_Accept`、`InProgress`、`Stage_1` 和 `Finished` 命名。交付文件名中的阶段数字暂为 `1`，新增其他阶段的任务时需核对选择规则。
