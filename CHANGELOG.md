# 变更记录

按日期倒序记录每次交付的内容、对应提交和验证范围，每条一两句话，细节放在链接的开发记录里。"待回归"表示还没有在 Play Mode 里重新验证。

## 2026-11-21

- **金币图标定稿**（仅文档入库）：用户选定 A 版观星币，透明底 PNG 已保存到本地资源目录。界面绑定和显示验证列在 [记录 41](Docs/Development/records/41-gold-coin-icon.md) 与 #25。

## 2026-11-18

- **用户实测登记：移动、对话与两阶段任务**（只有文档）：用户在 `aeda051` 上按检查点自测移动与冲刺、对话输入与完成后对白、任务 0 的完整流程，全部通过，关闭 #10、#8、#7。Shift 先冲刺、冲刺后转入奔跑按用户说明记为设计意图。没有截图或日志，只有用户确认。[记录 40](Docs/Development/records/40-user-playtest-movement-dialogue-mission.md)

## 2026-11-17

- **控制台等级调试与敌人测试攻击力**（`4bff8ce`、`1af6688`）：按用户 11-16 的建议，控制台加入"设置等级 / 重置为初始等级"，写入存档并调用用户的 `PlayerLevelChanged`；Play Mode 自动检查 56 项全部通过（11-16 运行）。敌人攻击力按用户确认提交为测试值 50。[记录 39](Docs/Development/records/39-devconsole-level-controls.md)

## 2026-11-16

- **背包与持有道具开始实施**（`be23363`、`e3e0ec4`）：用户编写 `PlayerItemMgr`（读档、增删查、每次变化存盘、广播新数量、数量为 0 时删除记录）和 BagPanel、BagSlot 脚本，GamePanel 新增背包按钮。Claude Code 按参考图拼接背包预制件并提供生成工具，修复了关闭按钮文字没有开启射线检测导致点不到的问题。只做了编译和静态检查，关闭按钮待复测；上限、货币、格子生成与详情还没做。[记录 38](Docs/Development/records/38-inventory-ui-and-item-mgr-start.md)
- **背包与发奖设计定稿**（只有文档）：用户指出要先做背包再做发奖，并完成了设计：新建玩家持有道具管理器负责数据和存档，背包面板只负责显示；只存持有的物品；`maxCount` 为 -1 表示无上限，货币单独显示。结论和参考类图写在 #25，尚未实施，没有代码改动。[记录 37](Docs/Development/records/37-inventory-and-reward-design.md)
- **控制台复测**（`9209b36`、`9dbf9b7`）：控制台改用玩家的光标模式，消除与 CameraCursorLock 的冲突；Play Mode 自动检查扩充到 49 项并全部通过，用户手动复测按键、伤害、清空任务存档正常。撤回清单五项至此全部完成。[记录 36](Docs/Development/records/36-devconsole-retest.md)
- **UIManager 恢复统一由预制体创建**（`c166a09`、`d345388`）：删除 10-30 为适配调试场景加入的两段接管逻辑，GMScene 删除调试用的 Canvas 和 MissionPanel，界面全部由 UIManager 在运行时创建，关闭时销毁。用户实测通过。[记录 35](Docs/Development/records/35-uimanager-prefab-only.md)

## 2026-11-15

- **模拟面试记录**：新增 [Docs/Interview](Docs/Interview/README.md)，针对当天的死亡流程、奖励与升级提交出了 6 题，保留用户回答和点评；记录 34 追加用户补充：主流程在 Codex 阶段就已设计，属性计算的改法是用户自己实现的。
- **怪物奖励、升级与属性层**（`c42cf15`、`84042d5`）：新增道具表、怪物奖励表和统一发奖方法；杀怪获得经验，升级后由 CharacterStats 重新计算攻击、防御、生命上限并更新生命组件；等级显示与玩家存档。用户实测通过，满级未测。[记录 34](Docs/Development/records/34-reward-level-and-stats.md)
- **控制台属性适配**（`accde67`）：属性改为读写 CharacterStats，显示等级。

- **玩家最小死亡流程**（`a22e86e`、`c11d394`）：血量归零后广播死亡，锁住移动、闪避、攻击，播放 S_Die；怪物在玩家死亡和复活时都脱战；复活后回满并刷新血条。用户实测确认了死亡和复活，怪物脱战待复测。[记录 33](Docs/Development/records/33-minimal-death-flow.md)
- **控制台适配**（`8117965`）：改为调用玩家的 `Dead()`、`Respawn()`，改血后刷新血条。控制台从这次起由 Claude 维护，不改动玩法脚本。

## 2026-11-14

- **Alt 光标模式与任务面板暂停**（`2b95693`、`41c9683`）：PlayerMgr 统一管理光标模式并通过事件通知；按住 Alt 或对话时不出刀，CameraCursorLock 锁住镜头；任务面板打开时暂停，UI 淡入淡出不受暂停影响。只做了静态审查和编译，最终版本待回归。[记录 32](Docs/Development/records/32-cursor-mode-camera-and-panel-pause.md)
- **文档与仓库管理**（`e284f9f`、`1672b06` 和随后的文档提交）：README 改为项目首页和功能状态表；新增本文件；STATUS 压缩为当前状态，原全文归档到 [history](Docs/Development/history/STATUS-2026-10-15_2026-11-14.md)；资源补回说明改为清单表；自编的 NPC 配置和 NPC 对白改为入库；待办迁到 GitHub Issues。协作规则改为 Codex 与 Claude Code 共用。
- **场景接线**：一起提交 GMScene 中 GamePanel 实例的 `playerHealthBar` 引用和 TestDialogue 的序列化字段，这两处是接血条时 Unity 保存下来的，经用户确认。
- **Issue 决定**：#12 演示用的重复配置只在本地存在过，已关闭；#20 用户决定暂时继续使用 LitJson。
- **玩家血条刷新**（`e305f25`、`3b6f590`）：血条在显示时拉取当前血量，并订阅 `OnHealthChange` 事件，修复了面板重建后显示满血、面板隐藏时受伤报空引用两个问题。只做了静态审查和编译，待回归。[记录 31](Docs/Development/records/31-player-health-bar-refresh.md)
- **协作规则**（`328bf7d`）：增加"提交推送默认流程"，CLAUDE.md 导入 AGENTS.md。

## 2026-11-11

- **玩家成长原型**（`3369c24`）：在 SO 里配置成长参数，新增 PlayerGrowth 和 PlayerSaveData（等级、经验），攻击力按等级计算。升级和存档恢复没有运行验证。[记录 30](Docs/Development/records/30-player-growth-and-save-design.md)
- **撤回控制台配套代码**（`9722ca3`、`a535623`）：恢复角色初始化和原血条实现，列出需要用户接回的待办。[记录 29](Docs/Development/records/29-console-support-code-removal.md)

## 2026-11-09

- **对话输入控制和完成后对白**（`bd2696a`、`32172ad`、`d366cf3`）：对话期间禁止攻击，淡出后恢复输入；新增任务完成后的对白；交付对白按当前阶段加载。待回归。[记录 28](Docs/Development/records/28-dialogue-input-and-finished-dialogue.md)

## 2026-11-08

- **缓存池**（`1834220`）：增加容量配置和满额轮换，还没有接入方。[记录 27](Docs/Development/records/27-pool-capacity-and-usage-contract.md)
- **剧情草案**（`0ac49a0`）：归档世界观与主线剧情 V1。[草案](Docs/Design/世界观与主线剧情-V1.md)

## 2026-11-06

- **NPC 对话与两阶段任务**（`c42e414`、`038f334`、`e02c2b3`）：击杀推进任务阶段、按进度播放对白、交付时结算存档。本机存档已写入 `isFinished`，完整流程待复测，实际发奖未接入。[记录 26](Docs/Development/records/26-npc-dialogue-and-mission-turn-in.md)

## 2026-11-05

- **敌人原型**（`3d628f8`、`5efe3b0`、`8e4b812`）：NavMesh 巡逻、追击、脱战返回、攻击冷却、死亡停机。编辑器日志记录了双方命中。[记录 25](Docs/Development/records/25-enemy-ai-and-combat.md)

## 2026-10-30

- **任务系统和调试 UI**（`9378e3f`、`ffd3bad`、`0111d2b`、`0759a89`）：任务 SO、玩家任务进度 JSON、任务面板、生命 HUD、交互提示预热、管理员控制台。[记录 22–24](Docs/Development/records/22-mission-system-design-and-prototype.md) · [交付清单](Docs/Development/deliveries/2026-10-30.md)

## 2026-10-26 至 10-28

- **配置、生命值与血条**（`d45389e` 至 `ac1be45`）：统一管理器单例、NPC 距离交互、对象池、角色属性 SO、生命组件、世界空间血条和球形命中原型。[记录 15–18](Docs/Development/records/17-character-config-health-and-hit.md) · [交付清单](Docs/Development/deliveries/2026-10-28.md)

## 2026-10-16

- **基础对话**（`f98e78f` 至 `6fce39e`）：线性对话推进和动态选项，用户测试确认本轮流程完成。[记录 14](Docs/Development/records/14-dialogue-system-implementation.md)

## 2026-10-15

- **首次入库与阶段整理**（`d09e52d` 至 `d6811ce`）：建立源码与配置基线；移动平滑、冲刺权重、武器挂点；清理旧测试脚本；加入攻击状态接口、UI 和对话骨架；场景重命名为 GMScene。[阶段总结](Docs/Development/records/12-stage-summary-20261015.md)
