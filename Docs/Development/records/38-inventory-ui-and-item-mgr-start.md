# 背包面板界面与玩家持有道具管理器（第一天）

日期：2026-11-16。工程版本：`08d40f0` 起，本次提交 `be23363`（用户代码）、`e3e0ec4`（Claude Code 的编辑器工具）。

协助工具：Claude Code。

相关 Issue：#25（简单背包与道具表）、#13（任务完成后实际发放奖励，和 #25 一起做）。设计结论见[记录 37](37-inventory-and-reward-design.md)。

分工：背包面板的预制件由 Claude Code 按用户给的参考图拼接（[记录 37](37-inventory-and-reward-design.md) 中约定"UI 可能交给 Claude Code 拼接"）；`PlayerItemSaveData`、`PlayerItemMgr`、`BagPanel`、`BagSlot` 和 `GamePanel` 的改动由用户自己编写，Claude Code 只做检查和解释，没有修改这些脚本。

## 用户观察与目标

按 #25 的设计开始实施：先把背包界面拼出来，再写持有道具的数据层。用户计划今天先写一部分，第二天一口气做完。

## 用户提出的想法

1. **背包界面要求**：结构参考用户提供的崩铁背包截图，也可以参考原神、崩铁的背包。物品区采用自动布局，从左上角开始排列，Content 区域随物品数量自动扩充。大小、比例、布局全部按参考图。右上角只保留信用点（对应游戏中的金币），不要星琼（抽卡物资）。上方分页标签、左下角排序按钮先不做。其他细节做出第一版后再问。
2. **按钮调整**（在第一版预览图上圈画）：参考图"销毁"位置的按钮应该是"销毁"而不是"使用"；"使用"放在右侧详情区右下角，只在消耗品时显示。
3. **关闭按钮点不到**：用户给面板写好 `BagPanel` 并接好 `btnClose` 后，点关闭没有任何反应，"好像永远都点不到这个按钮"，并追问"那个对象上不是有 Button 吗，为什么没有效果"。
4. **数据层第一版**：`PlayerItemSaveData`（`playerID` + `items` 列表）、`ItemCountData`（物品 ID + 数量）、`PlayerItemMgr`（构造时读档，`GetItemCount` / `AddItem` / `RemoveItem` / `GetAllItems`，事件 `OnItemCountChanged`）。用户认为按目前的写法，`PlayerItemSaveData` 里的 `playerID` 似乎没有意义了。
5. **检查后的决定**（逐条回复见下文）：事件第二个参数改为传新数量；`maxCount` 上限、货币单独显示、`BagSlot` 传物品 ID 放到第二天做；`playerID` 暂时不删；`GetAllItems` 改成只读列表后不再往下处理元素字段的可改性；删掉对外暴露整个存档对象的 `SaveData` 属性。

## 辅助分析与建议

### 背包界面（Claude Code 实施）

- 新建编辑器工具 `Assets/Editor/BagPanelSetup.cs`，生成 `Resources/UI/BagPanel.prefab` 和 `BagSlot.prefab`，并在隐藏的预览场景里渲染 1920×1080 预览图，不改动当前场景。做法和 DevConsoleSetup 相同。
- 尺寸从用户的 1920×1080 参考截图上量取，与 CanvasScaler 参考分辨率一致：
  - 格子每行 9 个，116×140，间距 9。
  - `ScrollRect` + `GridLayoutGroup`（从左上角开始横向排列，固定 9 列）+ `ContentSizeFitter`（纵向按内容撑高），右侧放竖向滚动条。
  - 右上角保留金币栏和关闭按钮。
  - 右侧详情区：名称加下划线、图标框（左下角显示数量）、描述。
- 按用户圈画调整：原位置改为"销毁"按钮（深灰，占位，没有功能）；"使用"按钮作为 `DetailPanel` 的子物体放在详情区右下角。预制件里"使用"默认显示，需要由面板脚本按 `itemType == consumable` 控制显隐。
- "销毁"不在 #25 的设计里。要不要做、是否需要二次确认、任务物品能不能销毁，都交给用户决定。
- 第一版向用户提出了几个待定细节：字体（工程里唯一的中文字体是笔画很粗的华文琥珀）、详情区是否加"背景故事 / 获得途径"字段、稀有度底色、背景模糊、左上角标题。用户还没有回复。
- 预览结束时 Unity 报 "Releasing render texture that is set as Camera.targetTexture"。原因是工具在释放 RenderTexture 前没有解除相机绑定，已修改工具，重新渲染后不再报错。

### 关闭按钮点不到（Claude Code 生成的预制件有缺陷）

- 原因在预制件，不在用户代码。`CloseButton` 本身就是"×"这段 TMP 文本，上面挂了 `Button`；但工具的 `NewText` 方法会把所有文本的 `raycastTarget` 关掉，这个按钮上就没有任何能接收射线的 Graphic。
- 用户的 `BagPanel` 已挂在根物体上，`btnClose` 指向 `CloseButton` 的 `Button`，接线正确。
- 修复方式：直接把预制件里 `CloseButton` 文本的 `m_RaycastTarget` 由 0 改为 1，**没有**重新生成预制件，所以用户挂上的组件和引用都还在。同时修改工具，以后生成时开启这一项。
- 向用户解释了 UGUI 的点击流程：
  1. `GraphicRaycaster` 只对 `raycastTarget` 开启的 Graphic 做射线检测，取最上层命中的那个。
  2. `ExecuteEvents` 从命中的物体开始，沿父级向上寻找实现了 `IPointerClickHandler` 的组件。
  3. `Button` 只负责响应，不负责被选中；`targetGraphic` 只用于颜色过渡，不决定能否点中。
- 这次射线穿过关闭按钮打到 `TopBar`，向上查找经过 `TopBar` → `BagPanel` 都没有 `Button`，所以事件丢失。排查方法：在 Play Mode 下选中 `EventSystem`，看 Inspector 预览区里的 `pointerEnter`。
- 以后的约定：用户已经在预制件上挂了脚本、接了引用，再用工具的 overwrite 会把这些清掉。之后调整布局时直接改现有预制件，不再覆盖生成。

### 数据层检查（第一轮）

| 问题 | 位置 | 说明 |
|---|---|---|
| 新存档时 `items` 为 null | `PlayerItemMgr` 构造函数 | `JsonMgr.LoadData` 在文件不存在时返回 `new T()`，`items` 没有初始值，查询或添加道具会抛空引用 |
| `Save()` 没有被调用 | `PlayerItemMgr.Save` | 与"每次数量变化都存盘"的设计不符 |
| 移除不存在的物品也广播 | `RemoveItem` | 订阅方会误以为扣除成功 |
| 数量为 0 的记录不删 | `RemoveItem` | 与"只存持有的物品"不符，背包还会画出数量为 0 的格子 |
| 事件参数含义未定 | `OnItemCountChanged` | 用户当时传的是变化量（`2` / `-2`），类图写的是新数量。建议先统一：只传变化量、只传新数量，或两者都传 `(itemId, delta, newCount)` |
| 打开背包未禁用输入 | `GamePanel.btnBag` | 和 `MissionPanel` 的做法不一致 |
| 对外暴露内部数据 | `SaveData` 属性、`GetAllItems` | 外部可以绕过管理器直接改列表，常见做法是去掉属性、返回 `IReadOnlyList` |

其他建议：`BagSlot.Init` 以后需要知道物品 ID；道具存档文件名可以和 `PlayerMgr` 的 `Character/Player/{id}` 目录风格统一（可选）。

**关于 `playerID`**：Claude Code 认为可以删。存档属于谁，通常由文件名、目录、槽位或数据库主键表示；数据里再存一份 ID 是重复信息，两者可能不一致。只有多名玩家共用一个文件、或者需要服务端校验时才有必要。若想在存档里加字段，数据版本号更有用。现有的 `PlayerSaveData` 也没有 ID 字段。

### 用户追问与解释

- **"我传入的不就是新数量吗"**：Claude Code 用表格说明两种传法的区别。原有 10 个，加 2 个时传 `2` 是变化量，传 `12` 才是新数量；减 2 个时传 `-2` 是变化量，传 `8` 是新数量。之后用户改为传新数量。
- **"SaveData 属性直接返回了内部列表"是什么意思**：属性返回的是引用类型对象本身，任何脚本都可以通过 `SaveData.items.Clear()` 或 `Add` 绕过 `AddItem` / `RemoveItem`，这样既不会存盘、不会广播，也受不到上限限制。工程里没有其他地方用到这个属性，建议直接删掉；以后需要某个字段时，单独开一个值类型的只读属性。
- **"不是改成 `IReadOnlyList` 了吗"**：`IReadOnlyList` 只禁止对列表本身做增删和替换。`ItemCountData` 是 class，`list[0].itemCount = 999` 改的仍然是管理器内部的对象。要彻底防住，可以返回副本、改用 struct 视图，或者把字段改成 `private set`（需要先确认 LitJson 能否正常读写）。单机项目做到只读列表加约定一般就够了。
- **打开背包时输入如何恢复**：用户说明恢复写在 `GamePanel.ShowMe` 里。核对后确认 `btnBag` 打开背包时会 `Disable()`，关闭背包时 `BagPanel.HideMe` 调 `ShowPanel<GamePanel>()`，由 `GamePanel.ShowMe` 重新 `Enable()`，两边是对应的。

### 数据层检查（第二轮）

用户修改后再次检查，第一轮的问题已解决。新出现两个问题：

1. **叠加已有物品不存盘**：找到物品后直接 `return`，执行不到 `Save()`。
2. **在 `foreach` 中删除元素**：`RemoveItem` 在 `foreach` 遍历 `items` 时调用 `items.Remove`，下一次迭代会抛 `InvalidOperationException`；只要物品被扣到 0 就会触发。建议删除后立即 `Save()` 并返回。

用户修改后第三次检查，两处都已修好。

## 用户采纳和独立补充

| 项 | 结果 | 来源 |
|---|---|---|
| 背包布局 | 按参考图拼接，只保留金币，不做分页和排序 | 要求由用户提出，Claude Code 实施 |
| 销毁 / 使用按钮 | 原位置改为"销毁"，"使用"移到详情区右下角，只在消耗品时显示 | 用户在预览图上圈画决定 |
| `items` 初始化 | 字段声明时直接 `new List<ItemCountData>()` | 用户采纳并自己修改 |
| 存盘 | 增加、移除后都调用 `Save()` | 用户采纳并自己修改；第二轮漏掉的叠加分支也由用户补上 |
| 事件参数 | 改为传新数量 | 用户在 Claude Code 解释区别后决定 |
| 移除逻辑 | 只在扣除成功时广播；数量为 0 时删除记录并立即返回 | 用户采纳并自己修改，还在代码里写了 `foreach` 限制的注释 |
| 打开背包禁用输入 | `btnBag` 中 `Disable()`，由 `GamePanel.ShowMe` 恢复 | 用户补充 |
| `SaveData` 属性 | 已删除 | 用户采纳 |
| `GetAllItems` | 返回 `IReadOnlyList`，不再往下处理元素字段 | 用户采纳只读列表；"不再往下改"是用户的决定 |
| `playerID` | 暂时保留 | 用户决定，没有采纳删除的建议 |
| `maxCount` 上限、货币单独显示、`BagSlot` 传 ID、格子生成与详情 | 第二天做 | 用户决定 |

## 当前文件依据

| 文件 | 本次状态 |
|---|---|
| [PlayerItemSaveData](../../../Assets/Scripts/Data/Save/PlayerItemSaveData.cs) | `playerID`；`items` 在声明时初始化 |
| [PlayerItemMgr](../../../Assets/Scripts/Managers/PlayerItemMgr.cs) | 构造时按 `PlayerItemSaveData{currentPlayerId}` 读档。`AddItem` 叠加或新增后广播新数量并存盘；`RemoveItem` 数量不足时返回 false，成功时广播新数量，到 0 时删除记录；`GetAllItems` 返回 `IReadOnlyList`。没有上限判断 |
| [BagPanel](../../../Assets/Scripts/UI/GamePanel/BagPanel.cs) | 绑定关闭按钮；打开时隐藏 GamePanel、`timeScale = 0`、显示光标；关闭时恢复。生成格子还是 TODO |
| [BagSlot](../../../Assets/Scripts/UI/GamePanel/BagSlot.cs) | `Init(Sprite, int)` 设置图标和数量 |
| [GamePanel](../../../Assets/Scripts/UI/GamePanel/GamePanel.cs) | 新增 `btnBag`：打开 BagPanel 并禁用玩家输入 |
| [BagPanelSetup](../../../Assets/Editor/BagPanelSetup.cs) | Claude Code 维护的生成与预览工具 |
| 本地 `Resources/UI/BagPanel.prefab`、`BagSlot.prefab`、`GamePanel.prefab` | 不入库。BagPanel 根物体挂着用户的 `BagPanel`，`bagParent`、`bagSlotPrefab`、`btnClose` 已接线；BagSlot 挂着 `BagSlot`，`icon`、`amount` 已接线；GamePanel 的 `btnBag` 已接线。登记见 [ASSET_RESTORE](../../ASSET_RESTORE.md) |

## 验证与当前状态

- **编译**：通过。依据是 `Library/ScriptAssemblies/Assembly-CSharp.dll` 生成于 21:46:35，晚于 `PlayerItemMgr.cs` 最后一次修改的 21:46:33；Editor.log 中这次编译之后没有新的 `error CS`。编写过程中出现过的几次编译错误都已由用户改掉。
- **界面**：只在离屏预览场景里渲染检查过布局，Claude Code 没有进入 Play Mode。
- **关闭按钮修复**：Claude Code 修改预制件后，由于用户当时在使用电脑，没有切到 Unity 重新导入，也没有实际点击验证，**待复测**。
- **数据层**：只做了静态检查，没有运行测试。存档、读档、扣到 0 的情况都没有实际跑过。
- **未完成**：`maxCount` 上限、货币单独显示、格子生成、选中与详情、使用 / 销毁按钮的逻辑、`RewardMgr` 其余类型的分支、任务发奖。
- **未带入本次提交的本地改动**：`DevConsole.cs` / `DevConsoleSetup.cs` 中的等级调试控件，以及 `EnemyConfig.asset` 中敌人攻击由 5 改为 50 并补了成长字段。文档中查不到这几处的记录，来源和用途待用户确认，暂时留在本地。

## 论文素材与证据入口

- 可用于"系统实现"中背包模块的数据层：单一修改入口、内存为权威、只存持有的物品、数量变化事件。
- UGUI 事件射线与 `raycastTarget` 的问题，可以作为界面实现中的问题与解决记录。原因是工具生成的预制件有缺陷，不能写成用户代码的问题。
- 本记录不包含运行测试结果。
