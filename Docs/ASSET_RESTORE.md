# 资源与本地恢复说明

仓库只提交自编代码、Input System 配置、场景、Animator、URP 和项目文本配置、自编数据（`Resources/GameData/` 下的全部 SO 配置、`StreamingAssets/Dialogue/` 下的测试对白和 NPC 对白）以及开发文档。下面的清单是**当前**需要从本地备份补回的内容，新增本地依赖时直接修改这张表。

## 补回清单（2026-11-14 整理）

所有资源都要**连同原 `.meta` 一起恢复**，不要重新生成 GUID，否则场景、Animator 和材质的引用会断开。

| 本地路径 | 内容 | 谁依赖它 | 缺失的后果 |
|---|---|---|---|
| `Assets/Scripts/Json/LitJson/` 及 `LitJson.meta` | 第三方 JSON 库 | `JsonMgr`（对话、任务、存档） | **编译失败** |
| `Assets/ArtRes/` | 角色 FBX/PMX、贴图、材质、物理预制体；当前角色 `Cyrene_Blender4.2.fbx`（GUID `8a20f58c2d3f4ef46b0e6140851c62db`） | GMScene 玩家、Animator | 角色缺失 |
| `Assets/MagicaCloth2/` | 衣物物理插件 2.18.4 | 场景里的 BoneCloth 组件 | Missing Script |
| `Assets/Shader/` | 第三方 Nilo 示例 Shader | 角色材质 | 材质粉色 |
| `Assets/CombatGirlsCharacterPack/`、`Assets/KAWAII_ANIMATIOMS_100/` | 第三方角色和动作资源包 | 武器、动作 | 部分动作缺失 |
| `Assets/Resources/Animation/` | FBX 动作、动作副本（含 StartQuick/EndQuick、攻击判定等动画事件）；11-15 新增死亡动作 `Charecter/Normal/S_Die.fbx`（GUID `2151d986a78dd47459ae5f82490b93cd`）。`Enemy/` 下为僵尸动作（127 个 FBX / anim 文件，**来源和授权尚未登记，待用户补充**）：敌人 Animator 用到 `Zombie_Idle_03.FBX`、`Zombie_Run_01_Forward_InPlace.fbx`、`Zombie_Attack_Forward01.fbx`（GUID `09037c04ccc7e3b428004a210b5a37be`）；后者的 `.meta` 里保存了动画事件 `OnAttackCheck`（0.527）和 `OnAttackEnd`（0.780，11-28 新增） | `Cyrene.controller`、`Animator/Enemy/EnemyAnimator.controller` | 动画缺失，动画事件失效；缺少攻击动作的 `.meta` 时，敌人不造成伤害，出招标记只能靠 3 秒超时清掉 |
| `Assets/Resources/Prefab/` | 角色预制件 `Cyrene_Blender4.2.prefab`、剑 `Dawnbreak.prefab`、胶囊敌人 `Enemy/EnemyObj.prefab`（GUID `0d0b28005492b914989176f4beaec0ea`）、人形敌人 `Enemy/千人律者.prefab`（GUID `2102f15014b5fe34ea4a39eeef9ac521`，见下方 11-28 补充） | GMScene 中的玩家、两个千人律者实例和两个已停用的胶囊敌人实例 | 敌人缺失 |
| `Assets/ArtRes/Character/千人律者/` | 人形敌人模型 `千人律者.fbx`（GUID `a030a2a8b469aa147926e2a3ff720242`，导入类型 Humanoid）、`Textures/` 下 4 张 PNG（`衣服.png`、`头发.png`、`脸.png`、`搞怪眼.png`）及 `使用规则.txt`。2026-11-27 由 Antigravity 通过 Blender 5.2 + `mmd_tools` 从本地 `G:\毕设\素材\千人律者\千人律者1.0.pmx` 转换（模之屋素材，模型编辑：神帝宇，部分版权属 miHoYo，禁止二配与商用），原输出在 `Assets/Resources/Prefab/千人律者/`，11-28 已位于当前目录。恢复时连同原 `.meta` 一起保留 | `Resources/Prefab/Enemy/千人律者.prefab` | 千人律者敌人没有模型 |
| `Assets/Resources/UI/` | Canvas（含挂 `DevConsole` 组件的 DevConsole 物体；11-22 起控制台界面在运行时由代码生成，预制件里只需要这个物体，缺失时可在编辑模式执行 `Tools/AstralEchoes/Dev Console/Add to Canvas Prefab` 补上，旧版 `Window`、`ToggleButton` 可用 `Remove Legacy Layout` 删除）、GamePanel（11-16 新增背包按钮 `btnBag`，11-26 新增 Buff 按钮 `btnBuff` 和次数文字 `txtBuffCount`）、DialoguePanel、OptionsPanel、Option、InteractPanel、MissionPanel、MissionItem、RewardSlot（11-27 起绑定 `txtName`、`txtAmount`、`imgIcon`；Icon 固定 60×60，Simple + Preserve Aspect，NameText 和 Icon 同级）、BagPanel、BagSlot、DetailPanel（11-22 新增，嵌套在 BagPanel 中）、BuffPanel、BuffOption（11-26 新增）共 14 个预制件，以及 `Sprites/BuffPanel/` 下 9 张 Buff 面板图片。BuffPanel、BuffOption 和面板图片由 Claude Code 用 `Assets/Editor/BuffPanelSetup.cs` 生成：执行 `Tools/AstralEchoes/Buff UI/Build BuffPanel Prefabs` 会补齐缺少的图片，两个预制件都不存在时重新生成布局，但不带用户挂的脚本和引用（`BuffPanel` 的 `optionObj`、`optionParent`、`btnClose`，`BuffOption` 的 `txtName`、`txtDesc`、`imgIcon`、`btnThis`），优先从备份恢复。BagPanel、BagSlot 可以用 `Tools/AstralEchoes/Bag UI` 重新生成布局，但生成结果不带用户挂的脚本和引用（`BagPanel` 的 `bagParent`、`bagSlotPrefab`、`btnClose`、`moneyText`、`detailPanel`，`BagSlot` 的 `icon`、`amount`、`btn`；DetailPanel 不由工具生成，`txtItemName`、`txtCount`、`txtDesc`、`imageIcon` 需从备份恢复），优先从备份恢复 | `UIManager.ShowPanel` | 面板无法打开 |
| `Assets/Resources/DevConsole/DevConsoleFont.asset` 及中文字体源 | TMP 字体 | 对话、任务、控制台文字 | 中文显示为方框 |
| `Assets/TextMesh Pro/` | TMP Essential Resources，也可以从 Window → TextMeshPro 菜单重新导入，但重新导入的 GUID 可能和原来不同，优先用备份 | 所有 TMP 文本 | 文字不显示 |
| `Assets/Resources/Mat/` | 测试用颜色材质 | 场景中的占位物体 | 材质缺失 |
| `Assets/Resources/Icon/Item/1.png` | 金币图标，用户选定的观星币 A 版。备份 PNG 时一并保留 Unity 导入后生成的 `.meta` | 道具 ID `1`，计划用于背包金币栏 | 绑定后还原工程时，缺少该资源会导致金币图标不可用。当前背包使用占位图 |
| `Assets/Resources/Icon/Item/2.png` | 星屑药剂图标，用户选定的青蓝圆肚瓶 A 版，连同 `2.png.meta` 保存和恢复 | 计划道具 ID `2`，用于背包格子和详情区 | 当前仅有图标。道具表接入后缺少该资源会导致药剂图标不可用 |
| `Assets/Resources/Icon/Buff/0.png`～`2.png` 及 `Buff.meta` | 三选一 Buff 图标，按 Buff id 命名：`0` 星锋（剑）、`1` 星盾（盾）、`2` 星息（心）。白色图形、透明底，256 × 256，显示时由 Image 颜色染成 `#9EE6FF`。2026-11-26 由 Codex 生图生成（B1-1 v2，原图、prompt 和生成记录保存在本地毕设素材目录 `素材/AI生成/`），Claude Code 切图、把亮度转成透明度并统一视觉大小；导入设置与 `Icon/Item/1.png` 相同。连同各自的 `.meta` 保存和恢复 | 计划用于 `BuffOption` 的 `Icon`；`BuffConfig` 暂时没有图标字段 | 当前没有代码或预制件引用，缺失不影响运行。接入后缺失会导致 Buff 卡片图标为空 |
| `Assets/Resources/Icon/Item/` 下的 `EngravedKey.png`、`EchoShard.png`、`RepairRope.png`、`LargeStardustPotion.png`、`StarChartScrap.png` | 候选道具图标，按道具英文名命名：刻名钥匙、回响残片、维修绞索、大瓶星屑药剂、星图残页。道具名称以及是否加入道具表由用户决定。512 × 512 透明 PNG。2026-11-26 由 Codex 生图生成（B2-1 v1、B2-3 v1、B2-4 v1、B2-5 v1、B2-6 v2，原图、prompt 和生成记录保存在本地毕设素材目录 `素材/AI生成/`），Claude Code 去除抠图残留并统一留白和尺寸；导入设置与 `1.png` 相同。连同各自的 `.meta` 保存和恢复 | `ItemDatabase` 中道具 ID `3`–`7`（11-26 绑定） | 背包格子和详情区中对应道具的图标为空 |
| 血条 Sprite 等 UI 图片 | 世界空间血条和 HUD 图片 | GMScene、UI 预制件 | 图片缺失 |

恢复步骤：

1. 用 Unity 6000.4.8f1 打开工程，Package Manager 会按 `manifest.json` 和 `packages-lock.json` 还原公开依赖。
2. 关闭 Unity，把上表中的目录从本地备份复制回原位置。
3. 重新打开工程，打开 GMScene 检查 Missing 引用、材质和动画事件。
4. 面部覆盖材质 `Materials/顏+.mat`、`Materials/顏2+.mat` 需要保持 Alpha=0 并开启裁剪，参见[记录 21](Development/records/21-face-overlay-material-set.md)。
5. 玩家存档（`Application.persistentDataPath` 下的 `MissionData.json` 等）不属于工程文件，复现时使用自己的测试存档。

仓库是源码与配置快照，单独克隆不保证场景能完整显示或运行，但这不等于源代码缺失。素材和插件的使用与再分发以各自的许可为准，本文不宣称取得了额外授权。

---

以下是各轮交付时写下的补充说明，原样保留作为历史；其中"由 .gitignore 排除、需要补回"的 NPC 配置和 NPC 对白，从 2026-11-14 起已经入库。

## 首次整理时的未上传内容（2026-10-15）

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
- Editor搭建工具仅生成任务面板框架，拒绝覆盖已有MissionPanel子对象（控制台自 11-22 起改为运行时由代码生成，见记录 46）；不会重建用户已接线的完整任务UI。当前报告写到工程 `Temp/DevConsoleReports` 或 `Temp/MissionPanelReports`，旧本机历史报告另保留本地。
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
- 当前对白按 `Npc/{npcId}/Mission_{missionId}_Accept`、`InProgress`、`Stage_{currentStageIndex}` 和 `Finished` 命名。交付文件名使用从 `0` 开始的阶段索引；当前测试任务的交付阶段为 `1`，继续使用 `Mission_0_Stage_1.json`。新增对话阶段时需准备对应索引的 JSON。

## 2026-11-28 千人律者敌人与攻击动作

- 恢复本地 `Assets/Resources/Prefab/Enemy/千人律者.prefab` 及原 `.meta`（GUID `2102f15014b5fe34ea4a39eeef9ac521`）。预制体包含 EnemyObject（`enemyId` 1）、EnemyHealth、EnemyAI、EnemyCombat、血条（HealthBarController、LookMe）、CapsuleCollider、NavMeshAgent 和 Animator。NavMeshAgent 的停止距离为 **1.3**（11-28，要小于 `EnemyAIConfig.attackRange` 1.4，见[记录 58](Development/records/58-enemy-attack-state-and-ranges.md)），速度 3、加速度 100、角速度 720；Animator 使用 `EnemyAnimator.controller`，关闭 Apply Root Motion。
- GMScene 中有两个千人律者实例：第一只覆盖了四个巡逻点；第二只"千人律者 (1)"关闭了 EnemyAI，用户留作木桩。原来的两个 `EnemyObj` 实例为停用状态。还原预制体后检查这些场景覆盖。
- 攻击动作 `Zombie_Attack_Forward01.fbx` 的 `.meta` 要连同 FBX 一起恢复，里面保存了 `OnAttackCheck`（0.527）和 `OnAttackEnd`（0.780）两个动画事件；`OnAttackEnd` 要早于 Animator 中攻击回待机过渡的 Exit Time 0.8125。
- 僵尸动作包的来源和授权还没有记录，用户补充前不要写成"已获授权"。
