# 任务面板奖励格显示道具图标

日期：2026-11-27。工程版本：基于 `754a179`，提交为 `1800b4a`（`RewardSlot` 显示图标，用户编写），文档单独提交。预制体是本地资源，不入库。

协助工具：Claude Code。

相关 Issue：#41（奖励格图标显示待实机验证）。道具名称从道具表读取的部分见[记录 45](45-item-cap-reward-and-mission-settlement.md)，背包格子的图标做法见[记录 43](43-bag-slot-display-and-refresh.md)。

分工：
- 用户编写：`RewardSlot.cs` 中“有图标显示图标，没有图标显示名称”的逻辑，包括删除 `SetNativeSize()`。
- Claude Code 修改：本地 `Assets/Resources/UI/RewardSlot.prefab` 的布局。没有改用户的脚本。

## 用户观察与目标

- 用户想让任务面板的奖励格“有图标的显示图标不显示文字”。
- 现象：用户在 `Init` 里调用 `SetNativeSize()` 设成原始大小后，图标超出奖励格，没法正常显示。用户的问题是“我代码要怎么改才能正常显示大小”，同时让 Claude Code 改奖励格的 UI。

## 用户提出的想法

1. **第一版**：给 `RewardSlot` 加了 `imgIcon` 字段，从道具表取 `itemIcon` 赋给 Image，然后调用 `SetNativeSize()`；先把名字清空，Image 的 sprite 为空时再显示道具名称。
2. **由谁改代码**：Claude Code 改完预制体后，问 `RewardSlot.cs` 由谁改，用户选择自己改。
3. **改后的版本**：删掉 `SetNativeSize()`；改用 `config.itemIcon == null` 判断，没有图标时 `imgIcon.enabled = false`，并显示名称。

## 辅助分析与建议（Claude Code）

### 超出的原因

- 道具图标原图是 512×512（`Icon/Item/1.png` 是 1254×1254）。Sprite 和 Canvas 的 Pixels Per Unit 都是 100，换算后不缩放，所以 `SetNativeSize()` 会把 Icon 设成 512×512；奖励格只有 102×98。
- 原来 Icon 的锚点和轴心都在左上角，变大后一直向右下撑开，会盖住数量文字。

### 建议

- **配置表驱动的图标不用 `SetNativeSize()`**：显示框的大小由预制体布局决定，Image 用 Simple 加 Preserve Aspect，在框里等比缩放。美术统一按同样大小的画布出图，并在四周留透明边距，这样每个图标看起来一样大。背包 `BagSlot` 的 Icon（96×96，Preserve Aspect）已经是这种做法。`SetNativeSize()` 适合按显示尺寸画好的界面图，例如装饰边框、按钮贴图。
- **没有图标时关闭 Image**：sprite 为空的 Image 会显示白色方块，所以要设 `imgIcon.enabled = false`。
- **NameText 移出 Icon**：原来 NameText 是 Icon 的子物体，以后如果隐藏整个 Icon 物体，名字会一起被隐藏，所以移到同级。
- **可选、没有做**：图标显示时只有 60～96 像素，可以把图标贴图导入的 Max Size 调到 256，省显存。因为不确定 DetailPanel 会不会用更大的尺寸显示，没有改。

### 预制体改动（Claude Code，本地）

| 节点 | 改动前 | 改动后 |
|---|---|---|
| Icon | 74×45，左上角锚点，内置 UISprite 底图，Sliced | 60×60，顶部居中（y = -6），没有底图，Simple + Preserve Aspect |
| NameText | Icon 的子物体，74×45 | 和 Icon 同级，90×60，位置和 Icon 相同 |
| AmountText | y = -59，高 29 | y = -66，高 28，在图标下方 |

改的是预制体 YAML。改完后用脚本核对过每个节点的父子引用互相一致，Unity 随后重新导入了这个预制体（编辑器日志）。编辑模式下 Icon 没有 sprite，会显示成白块，运行时由代码关闭。

### 审查用户改后的代码

- 在当前用法下是正确的：`SetNativeSize()` 已删除，没有图标时关闭 Icon 并显示名称，有图标时名称为空。
- `imgIcon.enabled` 只会被设成 `false`，没有设回 `true` 的地方。现在 `MissionItem.OnClick` 每次都会销毁旧的奖励格，再重新 `Instantiate`，预制体里 Image 默认是开着的，所以不会出问题。以后如果复用奖励格（比如用对象池），要在有图标时设回 `true`。目前不算 bug，没有开 Issue。

## 实际采纳与用户调整

- 用户采纳了“删除 `SetNativeSize()`”和“没有图标时关闭 Image”两条建议。
- 判断条件用的是 `config.itemIcon == null`，名称先设为 `null`，没有图标时再赋值。Claude Code 示例里的 `hasIcon` 变量和三元表达式写法没有采用。
- 预制体布局由 Claude Code 修改，`txtName`、`txtAmount`、`imgIcon` 三个引用仍指向原来的组件。

## 验证与当前状态

- **用户完成声明**：11-27 用户说“改好了”。
- **静态核验**：Claude Code 读过改后的 `RewardSlot.cs`；预制体的父子引用由脚本核对一致。
- **编译**：编辑器日志里有一次 `CS1061`（第 14 行，`ItemConfig` 没有 `txtName`），是修改过程中的中间状态；之后又重新编译了一次，成功，没有新的报错。
- **实际运行**：没有 Play Mode 截图，也没有用户实测确认。另外，任务 0 的奖励只有道具 0（经验，没有图标），游戏里现在只能看到“显示名称”这一种情况；要看到有图标的情况，需要在任务奖励里加一个带图标的道具。待验证项见 #41。

## 论文素材与证据入口

- 可以用来说明界面中配置驱动的图标怎么处理尺寸：固定显示框加等比缩放，和 `SetNativeSize()` 各自适合什么场景。
- 代码：[RewardSlot.cs](../../../Assets/Scripts/UI/GamePanel/RewardSlot.cs)、[MissionItem.cs](../../../Assets/Scripts/UI/GamePanel/MissionItem.cs)、[BagSlot.cs](../../../Assets/Scripts/UI/GamePanel/BagSlot.cs)。
- 预制体是本地资源，恢复方法和引用要求见 [ASSET_RESTORE](../../ASSET_RESTORE.md)。
