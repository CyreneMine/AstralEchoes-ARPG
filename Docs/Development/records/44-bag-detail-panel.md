# 背包详情区：点击格子显示物品信息

日期：2026-11-22。工程版本：基于 `63364bc`。

协助工具：Claude Code（检查与解释）。相关 Issue：#25。上一阶段见[记录 43](43-bag-slot-display-and-refresh.md)。

`BagPanel`、`BagSlot`、`DetailPanel` 以及本地 `DetailPanel.prefab` 都由用户编写和拼接，Claude Code 只做检查和解释，没有修改这些文件。

## 用户观察与目标

点击背包格子时，在右侧详情区显示物品的名称、数量、描述和图标；打开背包时默认显示一个物品。

用户写完第一版后发现：**断点里看数据一切正常，但 DetailPanel 里什么也不显示。**

## 用户的做法

1. 把详情区拆成独立的 `DetailPanel` 脚本，并做成 `Resources/UI/DetailPanel.prefab`，嵌套在 `BagPanel` 里。`BagPanel` 持有 `detailPanel` 引用。
2. `BagSlot.Init` 给 `btn` 添加点击回调：经 `UIManager.GetPanel<BagPanel>()` 调用 `SetDetailById(itemId)`。
3. 第一版的 `DetailPanel` 继承 `BasePanel`，在 `Init` 中默认显示 `items[0]`。
4. 经过几轮检查后的最终结构：
   - `DetailPanel` 继承 `MonoBehaviour`，只负责"按 ID 显示"和 `Clear`。数量通过 `PlayerItemMgr.GetItemCount` 查询；`Clear` 清空文字，并关闭图标组件。
   - `BagPanel.UpdateBag` 生成格子时记下第一个格子的 ID。有格子就显示这个 ID 的详情，没有就 `Clear`。

## 辅助分析与建议（Claude Code）

### 不显示的原因

- `DetailPanel` 继承了 `BasePanel`。`BasePanel.Awake` 会给它自动加上 `CanvasGroup`。
- 之后 `Update` 中 `isShow` 默认为 `false`，每帧都会降低 alpha，大约 0.2 秒后整块区域就变成全透明了。
- 只有 `ShowMe` 能让它显示，而 `ShowMe` 只有 `UIManager.ShowPanel` 会调用，详情区作为面板内部的一块区域，从来不会被调用。
- 所以数据其实都写进去了，只是画面不可见。排查方法：在 Play Mode 中选中 `DetailPanel`，可以看到 `Canvas Group` 的 `Alpha` 为 0。
- 建议的分工：由 UIManager 管理的面板继承 `BasePanel`；面板里的子区域和列表项（格子、详情区）用普通 `MonoBehaviour`，由所属面板持有引用并调用。工程里的 `BagSlot`、`MissionItem`、`RewardSlot` 已经是这样做的。

### 之后几轮检查

| 轮次 | 发现 | 用户处理 |
|---|---|---|
| 2 | `items[0]` 可能是金币；空背包时会越界 | 改为递归跳过 money 和 exp，越界时返回 |
| 3 | 能用，但"跳过哪些类型"的规则在 `BagPanel` 和 `DetailPanel` 里各写了一份；空背包时显示预制件的占位文字；递归加成员字段可以换成普通循环 | 删掉了 `DetailPanel` 里的类型判断，空背包时清空文字、把 sprite 设为 null |
| 4 | 删掉判断后又会默认显示金币；`sprite = null` 会让 `Image` 显示成白色方块 | 用户指出"不是说重复了吗" |
| 5 | Claude Code 承认上次表述不清：本意是把"挑选默认物品"整个移到 `BagPanel`，不是只删掉判断。给出示例：`UpdateBag` 记下第一个格子的 ID，`DetailPanel` 只提供 `SetDetailById` 和 `Clear` | 按示例修改，`Clear` 改为 `imageIcon.enabled = false` |
| 6 | `DetailPanel` 里的 `items` 字段已经没有地方赋值，`SetDetailById` 遍历它会抛空引用，打开背包就会触发。建议直接用 `GetItemCount` 查询 | 采纳，删除了 `items` 字段 |
| 7 | 没有发现会出错的问题 | — |

### 留到以后的

- 背包开着时，只要数量一变，详情区就会跳回第一个格子。做"使用"按钮时，建议 `BagPanel` 记住当前选中的 ID，刷新后如果该物品还在就继续显示它。
- `DetailPanel.cs` 里有两个没用的 `using`，文件头有 BOM；`SetDetailById` 上方的注释是旧写法留下的。

## 用户采纳和独立补充

| 项 | 结果 | 来源 |
|---|---|---|
| 详情区拆成独立脚本和预制件 | `DetailPanel` 嵌套在 `BagPanel` 中 | 用户 |
| 格子点击显示详情 | `BagSlot.btn` 回调到 `BagPanel.SetDetailById` | 用户 |
| 改继承 `MonoBehaviour` | 已改 | 用户采纳 |
| 默认显示由 `BagPanel` 决定 | 已改，规则只保留一份 | 用户采纳（第 5 轮说明之后） |
| 空背包清空并关闭图标 | 已改 | 用户采纳 |
| 数量改为向管理器查询 | 已改 | 用户采纳 |

## 当前文件依据

| 文件 | 内容 |
|---|---|
| [DetailPanel](../../../Assets/Scripts/UI/GamePanel/DetailPanel.cs) | `MonoBehaviour`；`Awake` 中调用 `Clear`；`SetDetailById` 从配置读名称、描述、图标，向 `PlayerItemMgr.GetItemCount` 查数量 |
| [BagPanel](../../../Assets/Scripts/UI/GamePanel/BagPanel.cs) | `UpdateBag` 记录第一个格子的 ID 并设置详情，没有格子时调用 `Clear`；`SetDetailById` 转发给 `detailPanel` |
| [BagSlot](../../../Assets/Scripts/UI/GamePanel/BagSlot.cs) | `btn` 点击后调用 `BagPanel.SetDetailById(itemId)` |
| 本地 `Resources/UI/DetailPanel.prefab` | 新增，已接线 `txtItemName`、`txtCount`、`txtDesc`、`imageIcon`；嵌套在 `BagPanel.prefab` 中，`BagPanel.detailPanel` 指向它。不入库，见 [ASSET_RESTORE](../../ASSET_RESTORE.md) |

## 验证与当前状态

- **编译**：通过。最终版 `Assembly-CSharp.dll` 生成于 19:40:52，晚于三个脚本的最后修改时间；Editor.log 中的报错都是中间版本留下的。
- **运行**：Claude Code 没有进入 Play Mode，用户这次也没有说明是否实测过。以下几项都还没有运行证据：
  - 打开背包时详情区显示第一个格子；
  - 点击格子后详情跟着切换；
  - 清空背包后详情区为空，并且不出现白色方块。
- **未完成**：`maxCount` 上限、格子选中框、记住选中项、"使用"按钮（只对消耗品显示）、"销毁"按钮的功能、任务发奖（#13）。
- **未带入本次提交**：`DevConsole.cs`、`DevConsoleSetup.cs` 中控制台的重构，仍等待负责控制台的会话完成验证并补写记录。

## 论文素材与证据入口

- 可以作为"界面实现中的问题与排查"：数据正确但界面不可见，原因是继承了带淡入淡出逻辑的面板基类。
- 可以说明"面板"和"面板内组件"的职责区分，以及同一条规则只保留一处的好处。
- 本记录不包含运行测试结果。
