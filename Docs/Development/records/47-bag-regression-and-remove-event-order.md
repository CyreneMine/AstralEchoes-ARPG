# 背包回归与扣除事件的广播顺序

日期：2026-11-22。工程版本：基于 `a51b185`。

协助工具：Claude Code（检查与解释）。相关 Issue：#29（背包刷新与关闭按钮的回归）。前序记录：[43](43-bag-slot-display-and-refresh.md)、[44](44-bag-detail-panel.md)、[45](45-item-cap-reward-and-mission-settlement.md)。

`PlayerItemMgr`、`BagSlot`、`DetailPanel` 的修改都由用户完成。Claude Code 只负责定位原因、解释和检查，没有改动这些文件。

## 用户观察与目标

用户按 #29 的四项逐一复测：

1. 背包打开时加减道具，界面当场刷新：**正常**
2. 道具扣到 0 后，格子消失，详情区跟着切换：**异常**。用户先自己尝试修复，结果"格子可以删除，详情页不行"
3. 关闭背包后再改数量，不报错：**正常**
4. 右上角关闭按钮能点击：**正常**

## 用户的第一次修复尝试

- 在 `BagSlot.Init` 中加判断：数量小于等于 0 时，通过 `UIManager.GetPanel<BagPanel>().SetDetailById(-1)` 清空详情，再销毁格子自身。
- 在 `DetailPanel.SetDetailById` 中加判断：`itemId < 0` 时调用 `Clear()`，并打印日志。

## 辅助分析（Claude Code）

- **根因在数据层，不在界面**。`PlayerItemMgr.RemoveItem` 扣到 0 时，先触发 `OnItemCountChanged`，之后才把记录从列表中删除。`BagPanel` 收到事件后立即执行 `UpdateBag`，这时数量为 0 的记录还在列表里，于是照常为它生成格子；如果它排在最前面，还会被选作 `firstSlotId`，详情区就显示这个物品。删除记录发生在事件处理之后，界面不会再收到第二次刷新。
- **补丁为什么只生效一半**：`BagSlot.Init` 清空了详情、销毁了格子，但 `UpdateBag` 后面的代码照样执行：把已销毁的格子登记进字典，把这个物品设为 `firstSlotId`，最后调用 `SetDetailById(firstSlotId)`，把刚清空的详情又设回了这个物品，于是显示"× 0"。
- **建议**：数据层按"改状态 → 存盘 → 广播"的顺序执行，保证订阅方收到事件时看到的已经是最终状态，这是常见做法。修好顺序后，撤掉 `BagSlot` 里的补丁：格子不应该反过来调用父面板去改详情区。
- **说明**：11-16 检查 `RemoveItem` 时，Claude Code 没有指出"先广播后删除"的问题，这是当时的疏漏。

## 用户的理解与采纳

用户总结："我原本是会在物品被删到 0 的时候自动删除格子和详情的，但是被这个顺序覆盖了回去，导致显示了数量 0 的数据。所以只需要在删除操作执行之后再广播。"Claude Code 确认这个理解正确：`UpdateBag` 整表重建的逻辑本来就能处理这种情况。

最终修改：

| 项 | 结果 |
|---|---|
| `RemoveItem` 顺序 | 扣减 → 到 0 时删除记录 → `Save()` → 广播新数量 → 返回 |
| `RemoveItem` 返回值 | 删掉了多余的 `removed` 变量，找不到物品时直接 `return false` |
| `BagSlot` 补丁 | 已撤掉，恢复为提交版本 |
| `DetailPanel` 的 -1 分支和日志 | 已撤掉，恢复为提交版本 |

## 当前文件依据

| 文件 | 内容 |
|---|---|
| [PlayerItemMgr](../../../Assets/Scripts/Managers/PlayerItemMgr.cs) | `RemoveItem`：先删除再存盘，最后广播 |
| [BagPanel](../../../Assets/Scripts/UI/GamePanel/BagPanel.cs) | 未修改。`UpdateBag` 整表重建，没有格子时调用 `Clear` |

## 验证与当前状态

- **编译**：通过。`Assembly-CSharp.dll` 生成于 21:03:45，晚于 `PlayerItemMgr.cs` 最后修改的 21:03:42。
- **用户实测（11-22）**：
  - #29 的第 1、3、4 项正常。
  - 修复后第 2 项复测正常：背包打开时把详情区正在显示的物品扣到 0，格子消失，详情区切换到下一个格子；最后一个格子也扣到 0 后，详情区清空，没有出现白色方块。
- **证据性质**：只有用户确认，没有截图或日志。

## 论文素材与证据入口

可以作为"观察者模式中通知时机"的实例：发布方在状态尚未最终确定时就发出通知，订阅方读到中间状态，导致界面显示错误。修复方法是先改状态、再存盘、最后广播。
