# 开发记录索引

每个问题一份记录，按"用户观察与目标 → 用户提出的想法 → 辅助分析与建议 → 实际采纳与用户调整 → 验证与当前状态"组织，模板见 [ISSUE_TEMPLATE.md](ISSUE_TEMPLATE.md)。当前状态先看 [STATUS.md](STATUS.md)，按日期的变化看 [CHANGELOG](../../CHANGELOG.md)。

记录写的是当时的过程，事后发现的更正以"补充"的形式追加在原记录末尾，不改写原文。

## 目录结构

| 目录 | 内容 | 维护状态 |
|---|---|---|
| [records/](records/) | 每个问题的过程记录，按编号递增 | 持续维护 |
| [evidence/](evidence/) | 实际测试导出的 JSON 证据，测试数值不改 | 有新证据时添加 |
| [history/](history/) | 旧版 STATUS 的原样归档 | 只读 |
| [deliveries/](deliveries/) | 10-15 至 10-30 的分模块交付清单 | 11-05 起停用，交付信息改写在 CHANGELOG 和提交信息里 |
| [logs/](logs/) | 10-16 至 10-30 的开发日志 | 11-05 起停用，内容并入 records |
| [SOURCES.json](SOURCES.json) | evidence 对应的本地原始文件路径和 SHA256 | 有新证据时更新 |

## 记录列表

| 编号 | 日期 | 主题 | 当前适用范围 |
|---|---|---|---|
| [01](records/01-project-path.md)、[02](records/02-project-path-acceptance.md) | 10-15 | 工程路径搬迁与启动异常 | 历史；工程现位于 G 盘 |
| [03](records/03-legacy-pmx-materials.md) | 10-15 | 旧 PMX 白色颗粒与透明叠层 | 历史；旧工程已删除 |
| [04](records/04-avatar-and-plugins.md) | 10-15 | Avatar 映射、插件与 Shader | 历史；旧 FBX |
| [05](records/05-wasd-and-root-motion.md) | 10-15 | 临时 WASD 与身体偏移 | 历史；旧测试控制器 |
| [06](records/06-bonecloth.md) | 10-15 | CATS 模型 BoneCloth | 历史模型 |
| [07](records/07-face-overlay.md) | 10-15 | 面部黑块（旧材质） | 历史；当前材质见 21 |
| [08](records/08-blender42-bonecloth.md) | 10-15 | Cyrene_Blender4.2 衣物物理 | 当前模型 |
| [09](records/09-movement-and-dash-transitions.md)、[10](records/10-user-design-and-implementation.md) | 10-15 | 换向减速、冲刺衔接、MoveWeight | 当前移动代码 |
| [11](records/11-weapon-prefab-and-hand-socket.md) | 10-15 | 武器预制件与右手挂点 | 收放剑切换待完成 |
| [12](records/12-stage-summary-20261015.md)、[13](records/13-animation-stock-dash-and-cleanup.md) | 10-15 | 阶段总结、动画库存与编译清理 | 历史快照 |
| [14](records/14-dialogue-system-implementation.md) | 10-16 | 基础对话与动态选项 | 用户测试确认 |
| [15](records/15-manager-and-npc-interaction.md) | 10-28 | 管理器基类与 NPC 距离交互 | 当前 |
| [16](records/16-pool-billboard-and-world-ui.md) | 10-28 | 对象池、朝向脚本、世界空间 UI | 对象池后续见 27 |
| [17](records/17-character-config-health-and-hit.md) | 10-28 | 角色配置、生命值与血条 | 当前 |
| [18](records/18-animation-lock-and-recovery.md) | 10-28 | 动画锁定与恢复 | 当前 |
| [19](records/19-mission-panel-ui.md) | 10-29 | 任务面板 UI 框架 | 逻辑后续见 22 |
| [20](records/20-devconsole-mission-reset.md) | 10-30 | 控制台清空任务存档 | 待复测 |
| [21](records/21-face-overlay-material-set.md) | 10-30 | 当前模型面部覆盖材质 | 当前 |
| [22](records/22-mission-system-design-and-prototype.md) | 10-30 | 任务系统设计与原型 | 当前 |
| [23](records/23-dev-console-and-player-hud.md) | 10-29 | 控制台与玩家 HUD | 配套部分已在 29 撤回 |
| [24](records/24-first-interaction-ui-warmup.md) | 10-29 | 首次交互提示预热 | 当前 |
| [25](records/25-enemy-ai-and-combat.md) | 11-05 | 敌人导航、攻击与死亡 | 当前 |
| [26](records/26-npc-dialogue-and-mission-turn-in.md) | 11-06 | NPC 对话与两阶段任务结算 | 当前 |
| [27](records/27-pool-capacity-and-usage-contract.md) | 11-08 | 缓存池容量与使用约定 | 当前 |
| [28](records/28-dialogue-input-and-finished-dialogue.md) | 11-09 | 对话输入控制与完成后对白 | 待回归 |
| [29](records/29-console-support-code-removal.md) | 11-10 | 撤回控制台配套代码与待办清单 | 待办已迁到 Issues |
| [30](records/30-player-growth-and-save-design.md) | 11-11 | 玩家成长、存档与属性计算 | 待验证 |
| [31](records/31-player-health-bar-refresh.md) | 11-14 | 玩家血条：显示时拉取与生命事件 | 待回归 |
| [32](records/32-cursor-mode-camera-and-panel-pause.md) | 11-14 | Alt 光标模式、镜头锁定与任务面板暂停 | 待回归 |
| [33](records/33-minimal-death-flow.md) | 11-15 | 玩家最小死亡流程与控制台适配 | 部分实测，怪物脱战待复测 |
| [34](records/34-reward-level-and-stats.md) | 11-15 | 怪物奖励、升级与属性层 | 用户实测通过，满级未测 |
| [35](records/35-uimanager-prefab-only.md) | 11-16 | UIManager 恢复统一由预制体创建 | 用户实测通过 |
| [36](records/36-devconsole-retest.md) | 11-16 | 控制台复测与自动检查扩充 | 自动检查 49/49，手动复测通过 |
| [37](records/37-inventory-and-reward-design.md) | 11-16 | 玩家持有道具、背包与任务发奖的设计讨论 | 设计定稿，未实施 |
| [38](records/38-inventory-ui-and-item-mgr-start.md) | 11-16 | 背包面板界面与玩家持有道具管理器（第一天） | 编译通过，未运行测试 |
| [39](records/39-devconsole-level-controls.md) | 11-17 | 控制台等级调试与敌人测试攻击力 | 自动检查 56/56 通过 |
| [40](records/40-user-playtest-movement-dialogue-mission.md) | 11-18 | 用户实测：移动、对话与两阶段任务 | 用户实测通过，关闭 #7、#8、#10 |
| [41](records/41-gold-coin-icon.md) | 11-21 | 金币图标设计与定稿 | 本地 PNG 已核验，待绑定和显示验证 |
| [42](records/42-stardust-potion-icon.md) | 11-21 | 星屑药剂图标设计与定稿 | PNG 已核验，背包接入和显示待验证 |
| [43](records/43-bag-slot-display-and-refresh.md) | 11-21 | 背包格子显示与数量变化刷新 | 编译通过，用户声明可显示，刷新待验证 |
| [44](records/44-bag-detail-panel.md) | 11-22 | 背包详情区：点击格子显示物品信息 | 用户实测通过（11-22） |
| [45](records/45-item-cap-reward-and-mission-settlement.md) | 11-22 | 道具上限、奖励进背包与任务结算发奖 | 用户实测通过，关闭 #13 |
| [46](records/46-devconsole-tabs-and-item-debug.md) | 11-22 | 控制台重构为分页界面并加入道具调试 | 自动检查 84/84 通过，开关按钮移位已复测，关闭 #30 |
| [47](records/47-bag-regression-and-remove-event-order.md) | 11-22 | 背包回归与扣除事件的广播顺序 | 用户实测通过，关闭 #29 |
| [48](records/48-temporary-buff-design.md) | 11-23 | 三选一临时 Buff 的设计讨论 | 设计定稿；11-25 补充入口改按钮、到期回到候选池等调整，核心实现见 50 |
| [49](records/49-float-stats-and-hp-ratio.md) | 11-24 | 属性改 float 与生命上限按比例保持；打怪后生命变 0 的排查 | 自动检查 93/93 通过，关闭 #31；实机回归见 #33 |
| [50](records/50-buff-core-timer-and-stats.md) | 11-25 | Buff 核心：MonoMgr / TimerMgr、BuffMgr 与属性加成 | 自动检查 123/123，暂停与自然到期用户实测通过；次数、抽取、面板未实现（#16） |
| [51](records/51-pre-public-repo-cleanup.md) | 11-26 | 公开前整理仓库：面向面试官的 README、模拟面试记录移出 | 只有文档；Mermaid 图在 GitHub 上的显示待确认 |
| [52](records/52-buff-choice-random-and-panel.md) | 11-26 | Buff 三选一：随机抽取、选择次数、面板 UI 与控制台 | 自动检查 146/146（新增三选一 23 项）；面板 4 个流程用户实测通过 |
| [53](records/53-ai-generated-assets-and-item-config.md) | 11-26 | 生图素材的范围与提示词、Buff 与道具图标导入、道具表新增 ID 3～7、场景布局统一 | 静态核验和编辑器导入日志；新道具的背包显示未测（#39） |
| [54](records/54-ai-2d-asset-generation.md) | 11-26 | 2D 素材批量生成、预览与选稿 | 61 张本地候选，核验图像与生成记录 |
| [55](records/55-reward-slot-icon.md) | 11-27 | 任务面板奖励格显示道具图标：去掉 `SetNativeSize`，固定显示框加等比缩放 | 编译通过，用户实测确认没问题（#41 已关闭） |
| [56](records/56-enemy-fsm-state-pattern.md) | 11-27 | 敌人 AI 改为状态模式：五个状态、Enter 和 Exit 的分工、玩家死亡时只让追击中的怪物脱战、死亡事件放在基类 | 用户实测巡逻、脱战返回、死亡流程正常；复用入口未测（#26） |
| [57](records/57-enemy-model-pmx-to-fbx.md) | 11-27 | 敌人人形模型转换：千人律者 PMX 转 Unity FBX（骨骼清理与标准命名、材质节点重建、剥离冗余 Alpha 通道） | Blender 回导与渲染核验通过，Unity 已生成 meta；Humanoid 映射、Scene 显示与预制体接入待验证（#43） |
| [58](records/58-enemy-attack-state-and-ranges.md) | 11-28 | 敌人攻击状态：出招挥完再切换（收招事件 + 超时兜底）、停止距离 / 出招距离 / 命中范围三个数的顺序、配置表改按 id 查找 | 攻击状态四项用户实测通过；超时兜底和按 id 查找后的奖励、任务计数未测；出招距离按敌人配置见 #44 |

## 本机 Git 配置

`.gitattributes` 会把场景、预制件、SO 等 Unity YAML 文件的合并交给 UnityYAMLMerge。这个驱动要在每台机器的 `.git/config` 里单独配置（本机已于 2026-11-14 配好）。换电脑后执行下面的命令，路径换成本机 Unity 的安装位置：

```bash
git config merge.unityyamlmerge.name "Unity SmartMerge (UnityYAMLMerge)"
git config merge.unityyamlmerge.driver "<Unity 安装目录>/Editor/Data/Tools/UnityYAMLMerge.exe merge -h -p --force %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

## 论文使用边界

论文可以用这些记录说明排查方法、资源适配和验证过程。旧工程的记录要写明是探索过程，当前实现要和最终提交的工程重新对应。这里没有完成率、帧率提升、用户实验或最终构建结论。
