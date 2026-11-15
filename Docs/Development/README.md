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

## 本机 Git 配置

`.gitattributes` 会把场景、预制件、SO 等 Unity YAML 文件的合并交给 UnityYAMLMerge。这个驱动要在每台机器的 `.git/config` 里单独配置（本机已于 2026-11-14 配好）。换电脑后执行下面的命令，路径换成本机 Unity 的安装位置：

```bash
git config merge.unityyamlmerge.name "Unity SmartMerge (UnityYAMLMerge)"
git config merge.unityyamlmerge.driver "<Unity 安装目录>/Editor/Data/Tools/UnityYAMLMerge.exe merge -h -p --force %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

## 论文使用边界

论文可以用这些记录说明排查方法、资源适配和验证过程。旧工程的记录要写明是探索过程，当前实现要和最终提交的工程重新对应。这里没有完成率、帧率提升、用户实验或最终构建结论。
