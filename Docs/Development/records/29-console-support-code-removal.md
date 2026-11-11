# 撤回控制台对玩法脚本的配套修改

2026-11-10，CharacterObject、CharacterHealth、玩家控制与血条脚本。

## 用户观察与目标

用户开发成长系统时发现不熟悉的 SetRuntimeAttributes 方法，要求查明其来源。核对历史提交和开发记录后，确认它属于 Codex 为控制台加入的属性修改接口。用户要求撤回控制台之外的配套修改，保留自己编写的代码，并列出需要自行接回的功能。

## 用户提出的想法

- 正常玩法只读取 SO 配置，成长和其他变化值由独立数据保存。用户没有计划直接写入共享配置。
- UIManager 接管已有 Canvas 和面板的实现留到调试完成后再看。
- 撤回依据是代码归属，不按整份历史提交回退。用户后续写的死亡、任务、输入权限及成长代码必须保留。
- 用户明确允许调整 CharacterObject.SetRuntimeAttributes，包括该方法中本地新增的计算。

## 辅助分析与建议

Codex 先前用直接修改 Config.atk 举例，未贴合用户的配置只读约定，本轮按用户约定恢复初始化方式。找到本地管理员控制台实施前快照，结合 ffd3bad 和后续死亡流程提交划分归属。

删除角色上的控制台接口后，DevConsole 和专用 Editor 工具仍需适配。为保留按钮入口，调试读写收回 DevConsole 内部，通过反射访问已有私有字段。仅在使用调试属性按钮时，控制台创建完整的配置副本并替换目标角色引用，避免调试按钮写入 SO。正常 CharacterObject.Init 直接读取原配置。

## 实际修改与保留

| 文件 | 本轮处理 |
|---|---|
| CharacterObject | 与实施前快照逐字节一致。移除 Health、SetRuntimeAttributes 和初始化复制逻辑。 |
| CharacterHealth | 撤回事件、直接扣血、回血、属性设置和范围保护等控制台辅助实现，恢复原伤害计算及 protected virtual CalculateDamage。保留用户后加的 Dead 方法和调用。 |
| PlayerHealthBar | 与实施前快照逐字节一致，移除自动绑定及事件订阅，保留用户的字段和刷新方法原文。 |
| PlayerObject | 仅移除 Codex 添加的死亡输入判断。 |
| PlayerCombat | 仅移除 Codex 添加的玩家引用及死亡判断，保留用户的 canInput 检查和 GetAtkValue 伤害调用。 |
| DevConsole | 保留公开按钮方法，使用自身的调试读写逻辑，已有定时刷新替代生命事件订阅。 |
| DevConsoleSetup | 配套调整编译引用。旧检查中的 HUD 自动更新断言及通过项目已移除，后续运行该工具只检查控制台操作。 |

UIManager、PlayerHealth、EnemyHealth、EnemyAI、任务和对话代码、PlayerMgr、PlayerGrowth、PlayerSaveData、CharacterConfig 及数值资产均保持撤回前内容。场景、预制件、素材和 meta 保留。11 月 10 日撤回时没有提交或推送；11 月 11 日用户另行要求更新文档、分批提交并推送，同时保存用户已有的成长原型，见[成长记录](30-player-growth-and-save-design.md)。

## 用户后续需要解决的问题清单

1. **接回玩家血条刷新。** 当前只有 UpdatePlayerHealthBar 方法，需要由用户决定在初始化、受伤及数值变化后如何调用。该原方法使用两个 int 相除，非满血时比例会被截断，maxHp 为 0 时还会除零。本轮按要求保留原文，由用户修正计算与必要判断。
2. **接回死亡后的操作限制。** 原来的两处死亡判断已移除，现有 canInput 用于对话控制，没有随着死亡自动关闭。需要由用户在死亡流程中停止移动、闪避和攻击，并确定复活时的恢复方式。
3. **完成成长属性向生命组件的应用。** CharacterObject.Init 仍按配置基础 maxHp 和 def 初始化。玩家攻击已经使用用户新增的 GetAtkValue，但升级后的生命上限、防御及当前生命保留规则需要用户在自己的成长流程中接入。
4. **后续统一 UI 创建方式。** 按用户决定暂留 UIManager，调试结束后再确定统一从预制件创建还是接管场景对象，届时核对 Canvas、面板和血条引用。
5. **复测控制台与玩法联动。** 本轮仅完成编译和静态检查，需在 Unity 中验证扣血、回血、属性修改、复活、任务存档清理及界面开关。HUD 自动同步已撤回，控制台的实时数值刷新由自身定时刷新及按钮调用负责。

控制台反射依赖私有字段 config、currentHp、maxHp、def 的名字。用户以后修改这些字段时，需要同步调整控制台内部映射，避免重新向玩法脚本添加调试接口。

## 验证与当前状态

- 使用 Unity 6000.4.8f1 生成的编译参数和随编辑器提供的 C# 编译器，分别编译 Assembly-CSharp 与 Assembly-CSharp-Editor。输出写入系统临时目录，两项均无编译错误；存在现有 API 过时等警告。
- 对 137 个受保护文件及 Git 索引执行修改前后 SHA-256 比对，结果一致。比较 PlayerObject 和 PlayerCombat 的前后文本，确认只删除指定的 Codex 判断。用户 Dead 方法和调用仍在。
- CharacterObject、PlayerHealthBar 与实施前快照逐字节一致。控制台原有公开按钮方法名称集合一致，原有持久回调入口保留。
- 本轮未进入 Play Mode、未执行控制台自动运行检查、未构建发布包，没有写入用户存档。
- 修改前七个代码文件已备份到系统临时目录，保留本轮撤回前的完整内容。
- 11 月 11 日交付复核时，血条仍只有手动刷新方法，移动和攻击入口仍未接回死亡判断，生命组件仍按基础配置初始化。这三项继续保留在待办中，UIManager 按用户决定延期。
- 11 月 11 日再次使用当前编译参数独立编译主脚本和 Editor 脚本，均无编译错误。交付只保存工作区源码和更新文档，没有在收尾阶段接回上述玩法功能；实际操作效果仍待用户运行验证。

## 文件依据

- [角色初始化](../../../Assets/Scripts/Base/CharacterObject.cs)、[生命及死亡流程](../../../Assets/Scripts/Object/Common/CharacterHealth.cs)
- [玩家移动](../../../Assets/Scripts/Object/Player/PlayerObject.cs)、[玩家攻击](../../../Assets/Scripts/Object/Player/PlayerCombat.cs)、[原玩家血条](../../../Assets/Scripts/UI/GamePanel/PlayerHealthBar.cs)
- [控制台](../../../Assets/Scripts/UI/DevConsole.cs)、[专用编辑器工具](../../../Assets/Editor/DevConsoleSetup.cs)
- [原控制台实现记录](23-dev-console-and-player-hud.md)
