# 背包格子显示与数量变化刷新

日期：2026-11-21。工程版本：基于 `e2fd1c9`。

协助工具：Claude Code（检查与解释）。相关 Issue：#25（简单背包与道具表）。上一阶段见[记录 38](38-inventory-ui-and-item-mgr-start.md)。

本记录中的 `BagPanel`、`BagSlot` 和道具表由用户编写和修改。Claude Code 只做了检查和解释，没有修改这些文件。

## 用户观察与目标

在[记录 38](38-inventory-ui-and-item-mgr-start.md) 的数据层基础上，让背包真正显示玩家持有的道具。同时为"使用 / 销毁 / 详情"等后续功能做准备。

## 用户提出的想法

1. **第三个道具**：需要一个道具来测试背包，并让 GPT 或 Codex 设计图标。用户采纳了 Claude Code 推荐的消耗品"星屑药剂"，图标由 Codex 完成，见[记录 42](42-stardust-potion-icon.md)。
2. **道具表**：
   - 金币的 `itemType` 由 `exp` 改为 `money`，并绑定观星币图标；
   - 新增 2 号星屑药剂：`consumable`，`maxCount` 99，已绑定图标。
3. **第一版显示**：`UpdateBag` 遍历 `GetAllItems`，金币写进金币栏，其余道具逐个生成格子；`BagSlot.Init` 改为传物品 ID，由格子自己查配置取图标。
4. **追问与决定**：
   - 怎样快速清空父物体下的所有子物体，只能用 for 吗？
   - 用列表复用格子时，怎么知道哪个格子代表哪个物品？
   - 面板关闭时会被销毁，复用列表是不是也就没了？
   - 想让 BagPanel 订阅 `PlayerItemMgr.OnItemCountChanged`，但 `UpdateBag` 是无参函数，怎么办？企业也这么做吗？
5. **最终做法（用户选择）**：采用"整表重建"：
   - 数量变化事件到来后调用 `UpdateBag`；
   - 刷新前销毁旧格子、清空字典，先把金币栏置 0，再重新生成；
   - `Init` 中订阅，`OnDestroy` 中取消订阅。
6. **为后续做准备**：
   - 字典改为 `Dictionary<int, BagSlot>`，按物品 ID 记录格子；
   - `BagSlot` 新增 `btn` 字段，预制件已接线，供以后点击选中和显示详情使用。

## 辅助分析与建议（Claude Code）

- **第三个道具**：推荐消耗品。它是第一个进格子的道具，能同时测到：
  - 格子生成；
  - `maxCount` 上限；
  - 扣到 0 后格子消失；
  - "使用"按钮显示。

  "使用按钮隐藏"需要再加一个非消耗品，留到做按钮显隐时再加。
- **第一版检查**：
  - 发现 `Instantiate` 写在货币判断之外，金币会多生成一个格子；
  - 建议按 `itemType` 判断货币，不要写死 ID。
- **清空子物体**：
  - 用 `foreach (Transform child in parent) Destroy(child.gameObject)` 即可，`Destroy` 是延迟执行的，遍历时可以安全调用；
  - 同一帧内 `childCount` 仍包含旧子物体；
  - 运行时不要用 `DestroyImmediate`。
- **企业常见做法**：
  - 列表类界面通常复用格子，或者按 ID 局部更新；
  - 内容很多的列表还会用虚拟滚动；
  - 面板关闭时是销毁还是隐藏缓存，一般按面板分别配置。

  建议本项目保持记录 35 定下的"关闭即销毁"，复用只在打开期间生效。
- **订阅无参方法**：
  - 写一个签名一致的具名处理方法，在里面调用 `UpdateBag`，这是 .NET 和 Unity 中常规的写法；
  - 不建议用 lambda，因为无法用 `-=` 取消；
  - 企业项目在此基础上常用 `IDisposable` 自动取消订阅，或者用脏标记合并一帧内的多次刷新，本项目目前都还用不上。
- **第二版检查**：发现三个问题：
  1. 再次调用 `UpdateBag` 时没有清理旧格子，`Dictionary.Add` 会抛重复键异常；
  2. 没有取消订阅，背包关闭后数量一变就会访问已销毁的面板；
  3. 删除货币分支后，金币栏不再更新。

  另外指出 `UpdateSlotById` 在面板里调用 `RemoveItem` 修改数据，和"背包只负责显示"的设计冲突。
- **第三、四版检查**：三个问题都已解决。唯一遗留的小问题是金币扣到 0 时金币栏不归零，用户已在刷新开头置 0 修复。

## 用户采纳和独立补充

| 项 | 结果 | 来源 |
|---|---|---|
| 第三个道具 | 星屑药剂，消耗品，上限 99 | 用户采纳推荐 |
| 金币类型 | 改为 `money` 并绑定图标 | 用户根据 Claude Code 指出的配置问题修改 |
| 货币判断 | 改为按 `itemType` 判断，经验也跳过 | 用户采纳 |
| 刷新方式 | 整表重建，没有采用逐格更新 | 用户在看过两种方案后自己选择 |
| 订阅 | 具名方法 `OnItemCountChanged`，`Init` 订阅、`OnDestroy` 取消 | 用户采纳 |
| `UpdateSlotById` | 已删除 | 用户采纳"面板不改数据" |
| 金币栏归零 | 刷新开头置 0，并在代码里写了注释说明原因 | 用户采纳 |
| 为后续做准备 | 字典值改为 `BagSlot`，`BagSlot` 加 `btn` | 用户补充 |

## 当前文件依据

| 文件 | 内容 |
|---|---|
| [BagPanel](../../../Assets/Scripts/UI/GamePanel/BagPanel.cs) | `UpdateBag`：金币栏置 0 → 销毁旧格子 → 清空字典 → 遍历持有道具，跳过经验，货币写金币栏，其余生成格子并登记；`Init` 订阅、`OnDestroy` 取消订阅 |
| [BagSlot](../../../Assets/Scripts/UI/GamePanel/BagSlot.cs) | `Init(itemId, count)` 按 ID 查图标；新增 `btn` 字段 |
| [ItemDatabase.asset](../../../Assets/Resources/GameData/Item/ItemDatabase.asset) | 金币 `itemType: 1`（money），绑定观星币图标；新增 2 号星屑药剂（`itemType: 2`，`maxCount: 99`，绑定药剂图标） |
| 本地 `Resources/UI/BagPanel.prefab`、`BagSlot.prefab` | `moneyText`、`btn` 已接线，不入库 |
| 本地 `Resources/Icon/Item/1.png`、`2.png` | 道具表引用的图标，不入库，见[记录 41](41-gold-coin-icon.md)、[42](42-stardust-potion-icon.md) |

## 验证与当前状态

- **编译**：通过。`Assembly-CSharp.dll` 生成于 20:29:45，晚于 `BagPanel.cs` 最后修改的 20:29:42。
- **用户声明**：背包里已经可以显示道具。
- **未验证**：Claude Code 没有进入 Play Mode。背包开着时用控制台加减道具的刷新、扣到 0 后格子消失、关闭背包后再改数量不报错，这几项都还没有运行证据。
- **星屑药剂的描述**写着"使用后可以回复 20% 生命值"，但使用效果还没有实现。
- **未完成**：`maxCount` 上限、格子选中与详情、"使用"按钮（只在消耗品时显示）、"销毁"按钮的功能、任务发奖（#13）。
- **未带入本次提交**：`DevConsole.cs` 和 `DevConsoleSetup.cs` 中控制台的重构和道具调试功能，由另一个专门负责控制台的 Claude Code 会话完成，目前还没有在 Unity 中运行验证，开发记录也由那个会话负责补写，所以本次不提交。

## 论文素材与证据入口

- 可以用于说明背包界面如何订阅数据层的数量变化事件，以及订阅和取消订阅为什么要成对出现。
- "整表重建"和"复用 / 逐格更新"的取舍，可以作为界面实现中的设计讨论。
- 本记录没有运行测试结果。
