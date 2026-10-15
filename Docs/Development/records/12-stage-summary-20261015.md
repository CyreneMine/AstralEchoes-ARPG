# 2026-10-15 阶段总结与分模块版本整理

本记录按用户“分批次分模块 commit，总结文档并 push”的要求整理。代码与场景编辑主要由用户完成；Codex 本轮检查文件、整理文档、归类提交并推送，没有代改玩法代码、动画参数或第三方资源。既有三个未推送提交保留，不改写历史。

## 当前实际入口

- Unity 6000.4.8f1，URP 17.4.0，Input System 1.19.0，Cinemachine 3.1.7。
- 场景已从 SampleScene 重命名为 `Assets/Scenes/GMScene.unity`，原 GUID 保留，EditorBuildSettings 已对应新路径。
- 角色为本地 Cyrene_Blender4.2；控制脚本已移入 `Assets/Scripts/Object/Player/`；当前控制器为 `Assets/Resources/Animator/Cyrene.controller`。
- 这是一份源码、文本配置与文档快照，模型、动作、插件、角色与 UI 预制件仍在本地；克隆仓库后需按资源恢复说明补齐。

## 按模块归纳

| 模块 | 当前保存的内容 | 完成与验证边界 |
|---|---|---|
| 角色移动与冲刺 | Input System、CharacterController、独立速度标量、朝向计算、MoveWeight 恢复、冲刺期间禁止转向 | 代码已保存；最新全流程运行验收未开展 |
| 攻击原型 | PlayerCombat 左键触发 Atk/Attacking；PlayerMgr 注册玩家；AttackEnd 清除攻击标记；攻击时暂时跳过普通移动 Update | 已有输入与动画接口，不等于伤害、命中检测、连击窗口完成 |
| Animator | Base Layer 入口调整为 S_Take；Attack 层含三段攻击、收剑与空状态；冲刺 Exit Time 改为 0.3 | 状态配置存在；AvatarMask 文件新增但当前层的 m_Mask 都为空，未据此宣称上下身隔离已实现 |
| 武器适配 | 保留原外层约束容器，仅更换剑模型子物体；右手挂点移入 Cyrene 模型骨骼 | 用户确认跟随恢复；完整收放剑事件链未验收，SwitchSocket 仍为空 |
| UI 基础 | BasePanel 用 CanvasGroup 控制淡入淡出；UIManager 提供面板显示、隐藏、查询接口 | 缓存字典尚未把新建面板写入，因此复用/隐藏/查询链待接通与验证 |
| 对话骨架 | DialogueData、DialogueOptionData 字段；DialoguePanel、OptionsPanel、DialogueMgr | 对话管理入口与选项逻辑仍为空壳；不能描述为已完成分支对话 |
| JSON 封装 | JsonMgr 可选 JsonUtility/LitJson；保存到 persistentDataPath；读取优先 StreamingAssets 再回退保存目录 | 静态可见实现；未进行保存加载回归，读取优先级是否符合最终存档需求待定 |
| 主入口与生命值 | Main、PlayerHealth 文件及相关目录 | 当前为空壳，不计为已实现的主流程或生命值系统 |
| 清理与场景 | 删除旧 Testing 运行脚本及其 Editor 配置工具；SampleScene → GMScene | 删除依赖链已解释；最新日志出现成功编译与重载，不代表玩法全部通过 |

## 用户思路、建议与实际采纳

1. 用户先提出换向时保持走跑速度；Codex 分析二维向量平滑会使中间长度缩短，建议速度标量与方向分离。用户完成修改。
2. 用户进一步要求冲刺恢复速度跟随动画过渡、避免另调计时器；据此采用 MoveWeight 曲线，并由用户独立补充冲刺期间禁止转向条件。详见移动记录。
3. 用户提出复用原 SwordGirl 武器预制件。查到其原有控制器与约束后，建议保留控制容器、更换显示子物体，并重新绑定到 Cyrene 的实际手骨。用户指出手腕/手掌差别后得到局部偏移解释，操作后确认“这下好了”。
4. 用户怀疑包里缺少空手走路、拔剑可能是开场动作。检查显示该包的两套移动均将剑切至右手，未找到独立空手移动。原素材默认 S_Idle，不证明拔剑限定开场；用户当前自己的控制器已调整为 S_Take 入口。KAWAII 普通移动候选已找到，未预览。
5. 用户询问缩短后摇。建议调整 Exit Time、过渡时长并同步结束事件。实际保存为 Exit Time=0.3、过渡时长仍约0.317秒，EndQuick 的 FBX meta 原始 time 改为0.42003903；这不是原建议试调值的照搬，未测试最终手感。
6. 用户删除 Testing 后发现不能添加 Animator Behaviour。日志与引用分析定位到旧 Editor 工具仍使用已删除类型；随后该 Editor 目录也被移除。最新编译日志有成功退出及程序集重载记录。

## 分批提交边界

- 清理旧运行测试与对应 Editor 配置工具。
- 整理玩家脚本目录、注册管理器、攻击输入与动画状态配置。
- 保存 UI/对话/JSON 基础代码与第三方源码排除规则。
- 保存 GMScene 重命名、场景配置与构建入口。
- 汇总移动、武器、动画库存、后摇、编译故障和本阶段文档。

已有 439e72f（平滑移动）、77b552f（冲刺权重与禁转向）、2166c6c（武器 root 调整）作为前序提交一并推送。最终提交哈希与远端核验结果见独立交付清单，不在本记录中预写推送成功。

## 保留的待办

补齐攻击结束事件和命中/伤害验证；验证冲刺结束事件不会因提前退出遗漏；接通收放剑事件；预览空手移动候选；补齐 UI 缓存登记、对话选项与数据加载；验证当前场景完整试玩、布料穿插和最终构建。

本轮只保存开发中状态，不自动修复上述代码，不把原型接口、空壳文件、历史测试或用户单项反馈写成完整系统验收。第三方模型、动作、布料插件和 JSON 库不归为原创技术。

## 证据与历史入口

[阶段静态证据](../evidence/stage-20261015.json)

移动原因与过程见 [移动转向与冲刺衔接](09-movement-and-dash-transitions.md)，用户贡献与实际实施见 [用户设计与实施](10-user-design-and-implementation.md)，武器挂点见 [武器挂点记录](11-weapon-prefab-and-hand-socket.md)，动画与清理补充见 [动画与清理补充](13-animation-stock-dash-and-cleanup.md)。资源恢复见 [工程恢复说明](../../ASSET_RESTORE.md)。
