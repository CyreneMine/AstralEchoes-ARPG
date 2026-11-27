# 敌人人形模型转换：千人律者 PMX 转 Unity FBX

日期：2026-11-27。工程版本：基于 `5bed5b7`（本次新增为 `.gitignore` 排除的本地素材，仅更新文档，未提交代码）。

协助工具：Antigravity。

相关 Issue：#43（千人律者敌人模型 Unity Humanoid 映射与实景渲染待验证）；关联上下文 #42（记录 56 中注明 #42 第二步加攻击状态依赖带动画的敌人模型，本次仅完成模型格式转换与本地导入前置，尚未接入预制体和动画）。

分工：
- 用户：提供模之屋源素材 `G:\毕设\素材\千人律者\千人律者1.0.pmx`、决定目标用途（项目内人形敌人）与输出目录（`Assets/Resources/Prefab/` 下）、批准在本地 Blender 5.2 安装官方 `mmd_tools` 扩展；后续的 Unity Humanoid Avatar 复核、Animator 状态机与动画事件由用户负责。
- Antigravity：在本地 Blender 5.2.5 LTS 安装 `mmd_tools`，编写并执行转换脚本完成骨骼清理/标准命名、材质重建、贴图整理与 FBX 导出，并在空 Blender 场景中回导和渲染核对。

## 用户观察与目标

- 用户计划为项目接入人形敌人模型，询问能否将模之屋下载的非 FBX 模型转换为可直接导入 Unity 且贴图纹理正常的版本（当次聊天）。
- 源素材位于本地目录 `G:\毕设\素材\千人律者\`，包含 `千人律者1.0.pmx`、4 张主贴图（`衣服.png`、`头发.png`、`脸.png`、`搞怪眼.png`）、3 张 MMD 球面/Toon 辅助图（`mc3.png`、`toon2.png`、`toon3.png`）与 `使用规则.txt`（模型编辑：神帝宇，部分版权所属 miHoYo，允许改造与优化骨骼，禁止二次配布与商用）。

## 用户提出的想法

- 提供本机已安装的 Blender 5.2 路径，明确该模型将在项目中用作人形敌人。
- 批准由助手通过命令行在 Blender 5.2 中安装官方 `mmd_tools` 扩展。
- 指定转换结果放到 `G:\Unity\UnityProject\AstralEchoes\Assets\Resources\Prefab` 下。

## 辅助分析与建议（Antigravity）

1. **工具链与渲染边界**：
   - 核查本机快捷方式定位到 `D:\Blender5.2\blender.exe`（Blender 5.2.5 LTS），通过官方扩展仓库安装并启用 `mmd_tools`。
   - 提前说明边界：格式转换可保证主贴图、UV、骨骼与表情完整导入；MMD 专属 `.fx`、`.sph`/`.spa` 球面高光和刚体物理不会自动 1:1 转为 Unity 效果，若要和主角昔涟保持一致的二次元卡通质感，进 Unity 后需换用工程现存的 `SimpleURPToonLitOutlineExample` Shader。
2. **目录结构建议（未强制改变用户选择）**：
   - 对比工程现有目录（`Assets/ArtRes/` 放原始 FBX 与贴图、`Assets/Resources/Prefab/` 只放 `.prefab`），说明 `Resources/` 下所有文件都会被打入包体；用户仍选择放 `Assets/Resources/Prefab/`，按用户指定路径在 `Assets/Resources/Prefab/千人律者/` 下输出。
3. **PMX 静态解析发现的两个导入隐患**：
   - **双套腿骨与辅助骨**：原模型共 112 根 PMX 骨骼（`mmd_tools` 导入后含辅助/阴影骨共 136 根），腿部同时存在 0 权重的 `左足`/`右足` 与实际承载权重的 `左足D`/`右足D`，且 `下半身`（575 顶点）独立于 `腰`；若直接导出，Unity 自动 Humanoid 映射易误绑到无权重腿骨。另外 `左足D`/`右足D` 原末端朝上。
   - **节点与冗余透明通道**：`mmd_tools` 默认使用 `mmd_shader` 自定义节点组，Blender 原生 FBX 导出器不会导出其贴图链接；此外 `脸.png`、`搞怪眼.png` 虽全像素不透明（`min_alpha = 255`），但为 RGBA 格式，首轮回导核验时 Blender FBX 导出器为对应材质多写了一个透明贴图槽，可能让 Unity 误判为半透明材质。

## 实际采纳与用户调整

按上述核查结果完成转换并输出至 `Assets/Resources/Prefab/千人律者/`：

- **文件清单**：`千人律者.fbx`（663,916 字节）、`Textures/`（`衣服.png`、`头发.png`、`脸.png`、`搞怪眼.png`）、`使用规则.txt`。未修改源素材目录原件。
- **骨骼处理**：
  - 将 `下半身`（575 顶点）权重合并入 `Hips`（原 `腰`），校验待删除的 0 权重 IK/辅助/双套非权重骨均不承载任何顶点权重后移除；保留 **63 根骨骼**（51 根人形主骨与手指骨 + 头部下 12 根双马尾发辫骨 `zb_0_1..5`、`yb_0_1..5`）。
  - 51 根人形骨骼重命名为 Unity 标准英文骨名（`Hips`、`Spine`、`Chest`、`Neck`、`Head`、`LeftUpperLeg` 等），并将躯干/四肢骨骼末端朝向对齐子骨；保留原模型 8 个表情 Shape Keys（BlendShapes），高度约 1.53m（与主角昔涟约 1.63m 比例接近）。
- **材质与贴图处理**：
  - 9 个材质（`衣服`、`头饰`、`皮肤`、`头发`、`眼罩`、`脸`、`口腔`、`牙齿`、`舌头`）全部重建为标准 `Principled BSDF` 并绑定对应主贴图，主色设为纯白、粗糙度 1.0。
  - 剥离复制到 `Textures/` 的 `脸.png`、`搞怪眼.png` 中全 255 的冗余 Alpha 通道，转为纯 RGB PNG。

## 验证与当前状态

- **静态核验与 Blender 回导（已完成）**：
  - 导出前脚本断言：8,392 个顶点全部保留有效权重（0 个无权重顶点、0 个顶点权重落到已删除骨骼）。
  - 在空白 Blender 场景重新导入 `千人律者.fbx`：确认 63 根骨骼层级与朝向正常、高度 1.530m、9 个材质各自只关联 1 张有效主贴图（无多余透明槽）、8 个 Shape Keys 保留。
  - 使用 Cycles 渲染回导 FBX 正面图并肉眼检查：服装、头发、眼罩、脸部、关节与鞋袜贴图均正常显示。
- **Unity 自动导入（已核实 `.meta`）**：
  - Unity 编辑器已自动识别并生成 `千人律者.fbx.meta`（GUID `a030a2a8b469aa147926e2a3ff720242`）及 4 张贴图的 `.meta`。
- **待用户验证与后续工作（未完成，见 #43）**：
  - `千人律者.fbx.meta` 当前仍为默认 `animationType: 2`（Generic），尚未在 Unity Inspector 中切换为 **Humanoid** 并校验 Avatar 骨骼映射。
  - 尚未在 Unity Scene / Play Mode 中肉眼核验 URP 渲染效果与体型缩放；尚未决定是否提取材质切换为工程现有的 `SimpleURPToonLitOutlineExample` 卡通 Shader。
  - 尚未迁移到 `EnemyObj.prefab`、尚未配置敌人 Animator 与攻击动画。

## 论文素材与证据入口

- **第三方 MMD 模型适配 Unity URP/Humanoid 管线案例**：可说明 PMX 双套腿骨（`足` 与 `足D`）权重冲突排查、`mmd_shader` 节点重建为标准 Principled BSDF 以兼容 FBX 导出、以及全不透明 RGBA 贴图导致多余透明槽的回导排障过程。
- **证据**：本地资源 `Assets/Resources/Prefab/千人律者/千人律者.fbx`（GUID `a030a2a8b469aa147926e2a3ff720242`）及同目录 `Textures/`、`使用规则.txt`；本地依赖登记见 [ASSET_RESTORE.md](../../ASSET_RESTORE.md)。模型版权归原作者及 miHoYo，不宣称为原创美术。
