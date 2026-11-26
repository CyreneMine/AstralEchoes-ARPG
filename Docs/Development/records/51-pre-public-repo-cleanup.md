# 公开前整理仓库：面向面试官的 README、模拟面试记录移出

日期：2026-11-26。工程版本：基于 `84bf729`。只改文档，没有改代码、场景或配置；工作区里用户未提交的 `GMScene.unity` 改动不在本次范围内。

协助工具：Claude Code。

相关 Issue：#20（克隆后缺 LitJson 无法编译，本次未处理）。

## 用户观察与目标

- 学校在催投简历。用户打算基于当前进度写简历，投 Unity 客户端开发实习（求职计划原定 11-30 开始投）。
- 仓库目前是私有的，面试官打不开。用户问是否应该调整仓库后公开。

## 用户提出的想法

1. 用户问："要不要调整github仓库然后把它公开"。
2. 同时问简历上只放这个项目，还是把以前的练手项目也放上；以及专科期间获得的竞赛奖项是否适合写。这部分属于简历内容，不记在仓库里。
3. 听完建议后，用户决定："你先整理仓库我去找比赛的资料"。

## 辅助分析与建议（Claude Code）

**是否公开**：建议公开。面试官一般只看 README 和几个核心脚本，很少克隆运行，私有仓库等于看不到。公开前先做的检查和整理：

- 扫描现有文件、提交历史和 Issues，确认没有学号、姓名、联系方式和密钥；确认第三方素材都被 `.gitignore` 排除。
- README 改成面向面试官的首页：先放能玩到的内容、架构和代码入口，状态表放后面。
- `Docs/Interview` 里是用户的模拟面试答题和点评，放在面试官能看到的地方没有好处，移到私有的面试准备仓库。
- AI 分工保留并写清楚，不藏：主动说明比被面试官自己翻出来要好。

**素材风险**：本地角色素材目录里的使用规则写有"请勿二次配布""请勿用于商业用途"。公开仓库没有问题（模型不在仓库中），但带模型的构建不要公开放到 Releases 或网盘；对外展示用演示视频，并注明版权归属。当前 `Cyrene_Blender4.2.fbx` 具体由哪份原始模型改造而来，仓库文档中没有记录，素材说明里只写了确定的版权归属。

**仓库可见性**：按本工程 AGENTS.md，Claude Code 不改仓库可见性和设置，由用户自己在 GitHub 设置里操作。

## 实际采纳与用户调整

用户采纳"先整理仓库"，可见性的修改留给用户。本次 Claude Code 做的改动：

| 文件 | 改动 |
|---|---|
| `README.md` | 改为面向面试官的首页。新增：演示（视频整理中）、现在能玩到什么、架构图、一次击杀的数据流图、代码导读、开发中的问题与取舍链接、AI 分工表、素材说明。功能状态表、开发环境、克隆后的运行步骤、文档表保留；状态表的调试控制台一行注明由 Claude Code 编写维护 |
| `Docs/Interview/` | 删除，原文移到面试准备仓库的 `模拟面试/`，文中指向开发记录的相对链接改为 GitHub 地址 |
| `Docs/README.md` | 删去模拟面试记录一行 |
| `CHANGELOG.md` | 新增 11-26 条目；11-15 条目中失效的 `Docs/Interview` 链接改为文字说明 |
| `AGENTS.md` | "文档分工"中 README 的职责改为面向面试官的首页 |
| `Docs/Development/README.md` | 索引新增本记录 |

README 中"现在能玩到什么"只列有用户实测或 Play Mode 自动检查记录的内容；11-24 属性改 float 后待回归的升级流程标注了 #33；Buff 目前只能从控制台添加、三选一面板未做，也在同一处写明。"还没有的内容"单独列出。

两张 Mermaid 图的调用关系按当前代码核对：`EnemyHealth.Dead` 调用 `RewardMgr.SettlementReward` 和 `MissionMgr.CheckFinishMission`；`RewardMgr` 的经验走 `PlayerMgr.AddExp` → `PlayerGrowth.AddExp`，升级时 `PlayerMgr.PlayerLevelChanged` 广播 `OnPlayerLevelChange`，由 `PlayerObject.UpdateStats`、`GamePanel` 订阅；`PlayerObject.UpdateStats` 调用 `CharacterHealth.UpdateStats`，后者广播 `OnHealthChange`，由 `PlayerHealthBar` 订阅；道具走 `PlayerItemMgr`，广播 `OnItemCountChanged`，由 `BagPanel` 订阅。

## 验证与当前状态

- **静态核验**：
  - 现有文件、提交历史、全部 Issues 中都没有学号、姓名；现有文件中没有密钥。
  - 仓库中的非代码资源只有自编配置、对白 JSON、Animator 控制器、URP 设置、NavMesh 数据，以及没有动作曲线的 `TestAtk.anim`。
  - README 的说法逐条对照了 STATUS 和代码。
- **语法检查**：两张 Mermaid 图在浏览器里用 mermaid 10.9.1 解析并渲染成功；README、文档入口、CHANGELOG、记录索引和本记录中的相对链接都指向存在的文件。
- **未验证**：Mermaid 图和页内锚点链接在 GitHub 网页上的实际显示效果，要推送后在网页上确认。
- **已知边界**：
  - `Docs/Interview` 仍在提交历史中（`10a98ca` 起）。AGENTS.md 规定不改写已推送的历史，仓库公开后历史里仍能看到这份记录。
  - 克隆后缺 LitJson 无法编译（#20）未处理。
  - 仓库没有许可证文件，公开后默认保留所有权利。是否添加许可证由用户决定。
  - AGENTS.md 中"仓库：…（私有）"等用户改了可见性再更新。

## 论文素材与证据入口

本记录只涉及仓库整理和对外展示，不作为论文的实现或测试证据。
