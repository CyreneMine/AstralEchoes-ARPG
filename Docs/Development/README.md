# 开发记录索引

本目录将历史过程、当前状态和测试证据分别列出，原始文件及撤销备份继续保存在本地。

先读 [当前开发状态](STATUS.md)与 [对话实现和测试确认](records/14-dialogue-system-implementation.md)，再按问题查阅记录。此前 [阶段总结](records/12-stage-summary-20261015.md)保留为历史入口。

| 主题 | 记录 | 适用范围 |
|---|---|---|
| 工程路径与启动异常 | [搬迁说明](records/01-project-path.md)、[用户启动验收](records/02-project-path-acceptance.md) | 当前英文路径工程的来源 |
| 白色颗粒与透明叠层 | [PMX 材质记录](records/03-legacy-pmx-materials.md) | 已删除旧工程的历史案例 |
| Avatar、插件与 Shader | [映射和插件处理](records/04-avatar-and-plugins.md) | 旧 FBX 映射、插件清理及方案撤销 |
| WASD 与身体大幅偏移 | [实现和修正](records/05-wasd-and-root-motion.md) | 旧临时控制器与动作副本 |
| 裙摆、袖摆及头纱物理 | [BoneCloth 配置](records/06-bonecloth.md) | 当前 CATS 模型；名称沿革见文首 |
| 可逐个开关配饰的旧模型衣物 | [Cyrene_Blender4.2 配置](records/08-blender42-bonecloth.md) | 8 组布料、64 个独立网格；运行验证通过 |
| 用户设计思路与实际采纳 | [讨论与实施确认](records/10-user-design-and-implementation.md) | 用户提出保持速度、过渡同步及补充转向锁定；建议与采纳分别归档 |
| 换向减速与冲刺衔接 | [原因与修改过程](records/09-movement-and-dash-transitions.md) | 1D/Velocity、MoveWeight 与冲刺转向条件已保存；运行验收待补充 |
| 重启后面部黑块 | [覆盖层修复](records/07-face-overlay.md) | 当前 Nilo 示例 Shader 的腮红覆盖材质 |
| 武器预制件复用与剑不跟随 | [挂点适配与用户确认](records/11-weapon-prefab-and-hand-socket.md) | 保留外层约束，重新绑定 Cyrene 右手挂点；用户确认随手移动恢复，收放剑切换待验 |
| 动画库存、后摇与编译清理 | [补充记录](records/13-animation-stock-dash-and-cleanup.md) | 空手动作候选、用户实际后摇参数、Testing/Editor 依赖故障 |
| 基础对话与动态选项 | [实现与测试确认](records/14-dialogue-system-implementation.md)、[开发日志](logs/2026-10-16-dialogue.md) | 用户测试确认本轮流程完成，剩余小漏洞次日处理 |

[文本证据目录](evidence/)保留实际测试数据。[来源清单](SOURCES.json)记录原始文档路径与 SHA256。公开副本只调整文档链接与本机路径表达，不改测试数值。

论文可使用这些记录说明排查方法、资源适配和验证过程。旧工程记录应明确为探索过程；当前实现须与最终提交工程重新对应。这里没有完成率、帧率提升、用户实验或最终构建结论。

后续问题记录使用 [记录模板](ISSUE_TEMPLATE.md)，保留用户想法、辅助建议、实际采纳和验证来源。

本轮版本整理见 [分模块交付清单](deliveries/2026-10-15.md)。

本轮收工见 [2026-10-16 分模块交付清单](deliveries/2026-10-16.md)。

## 2026-10-28 最新过程记录

- [管理器基类与NPC距离交互](records/15-manager-and-npc-interaction.md)：补记用户管理器重构、提示恢复、离开范围与F键交互。
- [对象池、朝向脚本与世界空间UI](records/16-pool-billboard-and-world-ui.md)：保留用户暂不改ClearPool、Pivot自查和布局保留决定。
- [角色配置、生命值与血条排查](records/17-character-config-health-and-hit.md)：分SO表、下标ID、枚举预加载、初始化与NaN/Infinity证据。
- [动画锁定与恢复](records/18-animation-lock-and-recovery.md)：区分动画事件、状态退出和MoveWeight。
- [开发日志](logs/2026-10-28-config-health.md) · [交付清单](deliveries/2026-10-28.md)。

## 2026-10-29 任务界面框架

- [任务面板 UI 框架](records/19-mission-panel-ui.md)：按用户参考搭建主线/支线、列表、详情、奖励占位和追踪按钮；仅完成场景 UI，任务逻辑由用户后续实现。

## 2026-10-30 任务系统与其他优化

- [基础任务设计与用户实现](records/22-mission-system-design-and-prototype.md)：SO/JSON、顺序阶段、用户性能疑问与out复用、动态条目依赖、存档/奖励排查及未完成边界。
- [控制台与玩家HUD](records/23-dev-console-and-player-hud.md)：运行时副本、生命事件、历史Play Mode范围和用户独立修改。
- [首次提示预热](records/24-first-interaction-ui-warmup.md)：用户提出并实现启动Show/Hide，用户确认与未测指标分开记录。
- [清空任务存档](records/20-devconsole-mission-reset.md) · [当前模型面部覆盖层](records/21-face-overlay-material-set.md)。
- [本轮日志](logs/2026-10-30-mission-and-ui.md) · [交付清单](deliveries/2026-10-30.md)。
