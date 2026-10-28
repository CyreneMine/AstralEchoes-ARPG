# 管理器基类与 NPC 距离交互记录

归档日期：2026-10-28。补记此前讨论与当前源码；不把归档日期当作所有功能的完成日期。

## 用户观察与目标

用户希望靠近 NPC 显示对话按钮，离开区域时隐藏，对话结束后可以再次交互；随后自行借鉴框架，将管理器统一继承一个基类。

## 用户提出的想法

- 对话触发点先通过 UIManager 创建面板，再由 DialogueMgr 加载数据。
- 用户追问初始化时机，提出把管理器相关调用放到面板 ShowMe 中；同步登记面板可避免等待 Start 的问题。
- 用户赶时间，要求先用最小方案处理重复提示和再次对话；罕见淡出重入暂不扩展为大型状态框架。
- 用户独立将普通管理器改为 BaseManager<T>，并要求核对、提交和推送。

## 辅助分析与建议

早期问题涉及 GetComponentsInChildren<GameObject> 的类型错误、场景面板没有登记到 UIManager、Start/Init 时序、重复提示和距离检测提前 return。Codex 建议将提示的重复创建限制与距离查询区分，并在对话存在时停止提示创建；关闭提示采用立即移除，避开复用仍在淡出的面板。用户授权过的离开范围检测修改属于此前操作，本次不再修改玩法代码。

## 实际采纳与用户调整

- 当前 TestDialogue 在半径5的 OverlapSphere 中检测 Player Layer；先检测离开，再用 canOverlap 限制重复打开，设置交互目标后关闭重复创建入口。
- GamePanel 负责打开和立即关闭 InteractPanel；对话期间禁用玩家输入，关闭时恢复。
- 用户后续加入 F 键交互与鼠标锁定切换；当前 InteractPanel.ClickDialogue 同时供按钮与 F 键调用。
- UIManager、PlayerMgr、DialogueMgr、GameDataMgr 使用 BaseManager<T>；JsonMgr 仍保留独立单例。SingletonMono<T> 是另一种 MonoBehaviour 生命周期，本次不把它视为相同实现。

## 当前文件依据

- `Assets/Scripts/Base/BaseManager.cs`
- `Assets/Scripts/Object/NPC/TestDialogue.cs`
- `Assets/Scripts/UI/GamePanel/InteractPanel.cs`、`GamePanel.cs`、`DialoguePanel.cs`
- `Assets/Scripts/Managers/PlayerMgr.cs`
- 历史管理器与场景提交：d45389e、ab88e8b；之后 F 键交互提交见 9e80a8c。

## 验证与当前状态

本次重新核对源码，没有重新执行 Play Mode。此前用户确认基础对话功能完成，不等于多 NPC 选择、全部交互边界或本次新增血条功能已验收。当前仍以简单场景流程为主，任务系统和通用交互目标选择尚未据此宣称完成。

## 论文素材与证据入口

可说明面板生命周期、重复交互控制、距离检测与单例代码复用的取舍。按用户目标、提出方案、辅助排查、实际保存状态分别归属贡献；历史验收与当前静态检查不可混用。
