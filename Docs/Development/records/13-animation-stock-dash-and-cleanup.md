# 动画库存、冲刺后摇与删除旧测试脚本的编译故障

日期：2026-10-15。用户观察、辅助建议、实际保存与运行验证分别记录。

## 空手移动动画排查

用户发现当前剑角色动作包可能没有不持剑的走路动画，并推测拔剑动画用于开场。只读枚举 Sword_Girl/Animations 的 Normal、Special、Emotion，核对导入事件：S_Idle、S_Walk、S_Run、S_Sprint，以及 S_Sp_Idle、S_SP_Walk、S_SP_Run 都包含 SwitchSocket → To_Hand_R_Socket；未找到独立的空手/收剑移动套组。

原配套 Sword_Controller 默认 S_Idle，而非 S_Take；故“拔剑只用于开场”不能作为素材的既定要求。可将其用于探索进入战斗，配合另一套空手移动。项目中 KAWAII 包已有 @KA_Idle01_breathing、@KA_Walk01、@KA_Run01，按 Humanoid 导入；只是候选，尚未预览风格和过渡。用户自己的最新 Cyrene 控制器目前已采用 S_Take 入口，与原素材默认状态应分开记录。

## 冲刺后摇调整

用户希望减少冲刺后摇。检查当时配置：Exit Time=0.59473103，固定时长过渡=0.31690288秒；MoveWeight 曲线为0，EndQuick 位于动作后段，代码靠 isQuicking 控制转向和再次冲刺。

Codex 建议优先降低退出时间、缩短过渡，并将 EndQuick 前移以避免状态提前退出后跳过解锁事件。给出的试调值是 Exit Time=0.5、过渡=0.15秒；dashDuration=0.2控制高速位移持续时间，不能直接当作后摇时间。建议仅供试调，没有替用户修改。

本次整理再次检查，用户实际已保存：Exit Time=0.3；过渡仍为0.31690288秒；EndQuick 导入事件原始 time=0.42003903，较此前0.7924528前移。MoveWeight 恢复和 !isQuicking 条件仍在。记录以实际值为准，不将其写成完全采用试调值，也不把 FBX meta 原始 time 直接标为秒。

是否更流畅、连续冲刺和转向能否可靠解锁，仍需实际运行验证。OnStateExit 清理曾作为候选建议，本次未发现添加对应 Behaviour 的证据。

Unity 对 Exit Time 和 Fixed Duration 的含义见 [官方过渡文档](https://docs.unity3d.com/6000.0/Documentation/Manual/class-Transition.html)。文档用于解释参数，不作为本项目运行效果的证据。

## 删除 Testing 后 Add Behaviour 不可用

用户删除 Assets/Scripts/Testing 后，Animator 显示“Please fix compile errors before creating new state machine behaviours”。检查发现旧工具 Assets/Editor/MovementTesting/CyreneMovementTestSetup.cs 的85、86行仍调用 GetComponent<CyreneMovementTest> 与 Undo.AddComponent<CyreneMovementTest>，但运行时类型已删除，日志实际报CS0246。

这说明运行测试脚本与编辑器配置工具存在编译依赖；删除时只移除一端，会使编辑器程序集编译失败。不能据此认定冲刺动画或 Animator 文件损坏。

检查过程中，用户继续移除了 Editor/MovementTesting，最终 Assets/Editor 也不再存在。Codex 当轮只读，没有代删文件。整理时全项目自编脚本中已无该类型引用；最近 Unity 日志出现 ExitCode: 0 与程序集重载完成，Assembly-CSharp.dll 时间晚于当时最后脚本保存时间。这是编译日志观察，不是本轮另行执行构建或确认 Add Behaviour 按钮的实机验收。

日志曾含 PlayerObject.cs 第39行的历史语法错误；最新该行已是合法赋值，因此不将旧日志条目直接视为当前缺陷。

## 论文可用内容与边界

可说明：检查第三方动作事件以判断资源适用范围；将高速位移、动画退出、混合恢复和状态解锁分开调试；清理旧工具时核对运行时与 Editor 编译依赖。当前没有后摇改善的量化数据、完整战斗测试或空手/持剑切换验收，相关结论须待后续真实测试。
