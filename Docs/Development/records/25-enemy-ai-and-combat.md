# 敌人导航、攻击与死亡原型

2026-11-05，GMScene。

## 用户观察与目标

用户反馈本日敌人基础功能已基本完成，要求核对代码、场景与现有运行记录，并整理分模块提交。此前用户运行巡逻追击时遇到数组越界，要求定位。

## 用户实现

| 模块 | 当前代码与配置 |
|---|---|
| 巡逻 | `EnemyAI` 使用 `NavMeshAgent` 到四个场景巡逻点，抵达后等待 2 秒，推进下标并回绕。 |
| 追击和脱战 | 发现距离 5，丢失距离 8，离开追击起点的直线距离上限 15。追击中刷新玩家目的地，丢失目标后返回当前巡逻点，返回过程中暂停玩家检测。 |
| 攻击 | AI 请求攻击的距离为 2。敌人测试配置的冷却为 2 秒，伤害判定半径为 1.5，攻击力为 5。Animator 的 `fire` 触发 `Atk`，`TestAtk` 动画事件调用 `EnemyCombat.OnAttackCheck`，对 Player 层进行球形检测。 |
| 受击与死亡 | 敌人测试配置为 10 生命、1 防御，受击后更新世界空间血条。普通伤害归零后调用 `CharacterHealth.Dead`，`EnemyHealth` 重写该方法并预留后续扩展位置。AI 在死亡时停止导航，战斗入口检查死亡状态。 |
| 光标 | 用户在 GamePanel 的基础 Update 之后增加任务面板检查，任务面板存在期间跳过 Alt 光标处理。 |

## 越界分析与采纳

旧版日志记录了 `IndexOutOfRangeException`，调用链为 `Update → CheckPlayer → LoseTarget`。此前最后一个巡逻点被选中后，`currentIndex++` 会暂时留下数值 4。该场景四个巡逻点的合法下标为 0 至 3，`LoseTarget` 直接读数组时绕过了巡逻入口的回绕检查。

Codex 建议让丢失目标复用 `UpdatePatrolPoint()`。当前保存的用户实现采用该入口，并把递增与回绕放在到达巡逻点的分支内。`UpdatePatrolPoint()` 读取当前点并设置导航目的地。旧版日志与新版源码分别作为问题复现及修改依据。

## 已有运行证据与边界

- 11 月 5 日 Unity 编辑器日志中，`EnemyCombat.OnAttackCheck → PlayerHealth.TakeDamage` 的命中记录包含玩家 98/100 和 96/100，原始伤害为 5。
- `PlayerCombat.OnAttackCheck → EnemyObject.TakeDamage` 的后续日志记录敌人 6/10、2/10、0/10，原始伤害为 5。当前敌人防御为 1，与单次实际扣 4 相符。
- 本轮读取现有日志并静态核对代码、预制件及场景。巡逻末点转追击后再脱战、死亡停机、任务面板的 Alt 时序仍待专项 Play Mode 回归。
- 当前 `UpdatePatrolPoint()` 先读 `patrolPoints.Length` 再检查 null，数组为 null 时仍会出错。敌人攻击事件按 Collider 逐个取同对象上的生命组件，后续接入子碰撞体或多碰撞体的角色时，需要核对生命组件查找和单次命中去重。
- `EnemyHealth.Dead` 仍保留任务怪识别、经验和金币的 TODO。此阶段的血量归零证据可用于基础战斗原型，任务和奖励闭环仍待接入。

## 文件依据

- [导航和追击](../../../Assets/Scripts/Object/Enemy/EnemyAI.cs)
- [攻击冷却与命中](../../../Assets/Scripts/Object/Enemy/EnemyCombat.cs)
- [敌人死亡入口](../../../Assets/Scripts/Object/Enemy/EnemyHealth.cs)
- [测试场景](../../../Assets/Scenes/GMScene.unity)
- [敌人预制件及资源恢复](../../ASSET_RESTORE.md)
