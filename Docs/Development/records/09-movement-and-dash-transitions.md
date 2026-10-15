> 本地原始记录：`工作记录/移动转向与冲刺衔接_20261015`。历史建议与当前实施状态分别保留。

# 移动换向减速与冲刺收尾提前移动

> 最新状态：用户已实施 MoveWeight 曲线恢复与冲刺转向锁定，已核对保存文件。用户原始思路及采纳过程见 [设计思路与实施确认](10-user-design-and-implementation.md)。本文早期待实施内容属于历史阶段。


记录日期：2026-10-15。Unity 6000.4.8f1，Input System 1.19.0。当前代码为 `Assets/Scripts/Object/PlayerObject.cs`，Animator 为 `Assets/Resources/Animator/Cyrene.controller`。

> 后续需求已扩展为“退出过渡期间逐渐恢复移动”，见第四节。第二节的完整锁定方案保留为过程记录。

## 记录范围与状态

用户反馈了两个现象：按住 Shift 跑动时，从 W 切换到 W+D 等方向，感觉减速再加速；冲刺动作尚未结束时，方向键已开始推动角色。本次先解释与提供修改步骤，再归档过程。没有代改用户玩法代码、Animator、动画资源或场景，也未启动 Play Mode 测试或执行 Git 推送。

已读取用户最新保存代码，确认速度标量改法已写入；冲刺锁定仍待用户实现。不能将建议写成已经修好或已验收。

## 一、转向过程中减速再加速

### 最初的动画参数问题

原代码按 X/Y 分量分别更新 Animator。混合树中的走路与跑步都位于 Y 轴，走路为 0.5、跑步为 1，后退条目使用前进动作倒放。满速 W+D 输入归一化后为约 `(0.707, 0.707)`，总长度仍为 1，但 Y 分量降低，可能退回走跑混合。

第一步建议改用速度长度驱动 1D 混合树：Idle=0、Walk=0.5、Run=1。将代码中的水平速度与 Animator Float 参数区分开，字符串 `Velocity` 只是动画参数名字，不会自动读取包含重力的 Vector3 velocity。

### 为什么只改 magnitude 仍不够

用户修改动画取值后反馈仍有问题。再次读取代码发现，原 `Vector2.MoveTowards(currentInput, targetVelocity, ...)` 依然在对整个二维速度向量进行平滑。

例如满速前进向量 `(0,5)` 变为满速右移 `(5,0)`，中间可能经过 `(2.5,2.5)`，长度约 3.54，而两端长度均为 5。因此改成读取 magnitude 后，参数仍可能经历 `1 -> 0.707 -> 1`。从 W 到 W+D 的夹角较小，长度下降也较小；不能把所有换向都写成下降到同一个数值。

归一化后的斜向输入本身不会降低总速度。Vector2 同时表达方向和速度也没有错误；问题是所采用的向量插值轨迹不满足持续换向保持速度大小的目标。

### 最终建议与用户理解

分别处理速度大小与方向：

- `float currentSpeed` 通过 Mathf.MoveTowards 接近目标速度，处理起步、停步、走跑切换。
- 根据相机方向和输入计算期望方向，用 Quaternion.RotateTowards 平滑角色朝向。
- 普通移动采用 `transform.forward * currentSpeed`，换方向时沿转身形成的路径移动。
- 动画参数采用 `Mathf.Clamp01(currentSpeed / runSpeed)`，不混入垂直重力或冲刺速度。
- 目标速度按输入幅度乘 Walk/Run 速度，保留手柄模拟输入。在保持满幅度、同一走跑状态时，换向不改变目标速度。

用户提出“已在跑动时保持速度”的思路可以实现，但直接保持原值会误锁住起步低速或走路转跑步时的走速。独立速度标量能直接表达这一意图：满速跑动时目标和当前速度均为 5，换向不会让它重新减速。

用户用“一根线”的比喻理解：线长表示速度大小，线的朝向表示移动方向。期望换向时保持线长；原向量插值把端点沿直线移动，中间可能缩短线长。

### 当前核验

最新保存代码已使用 currentSpeed、沿 transform.forward 移动及归一化的 Velocity 参数。该修改属于代码静态确认，未取得修改后实际试玩验收。

磁盘 `Cyrene.controller` 仍是 `m_BlendType: 2`，参数为 XSpeed/YSpeed；不排除用户在 Unity Inspector 中修改但尚未保存。须实际保存并确认 1D + Velocity + 三个正确阈值后再测试。

## 二、冲刺动作没结束就恢复方向键位移

### 原因

当前普通水平移动先生成 `transform.forward * currentSpeed`，只有 dashTimer>0 时才覆盖为冲刺位移。dashTimer 默认 0.2 秒，结束后普通移动立即恢复；普通转向也只检查 dashTimer，未以 isQuicking 锁定。

所以“高速位移计时结束”和“冲刺状态结束”实际上是两条独立时间线：即使 StartQuick 已把 isQuicking 置为 true，只要 dashTimer 归零，仍可正常移动。

当前动画导入配置有 StartQuick 事件和 EndQuick 事件；EndQuick 的 meta 原始 time 字段为 0.7924528，本记录不将其未经转换直接当成秒数。当前冲刺退出过渡 Exit Time 约为 0.594731，固定过渡时长约 0.316903 秒；进入过渡 0.25 秒。这些设置须与期望收尾时机对应。

### 建议步骤 1：整个冲刺状态锁住普通转身

把当前条件：

```csharp
if (dashTimer <= 0f && hasInput)
```

改成：

```csharp
if (!isQuicking && hasInput)
```

### 建议步骤 2：区分冲刺位移与收尾锁定

将当前普通 move 初始化及 dashTimer 覆盖代码替换为：

```csharp
Vector3 move;
if (isQuicking)
{
    // 冲刺收尾期间，普通输入不能产生水平位移。
    move = Vector3.zero;
    if (dashTimer > 0f)
    {
        move = dashDirection * dashSpeed;
        dashTimer = Mathf.Max(0f, dashTimer - Time.deltaTime);
    }
}
else
{
    move = transform.forward * currentSpeed;
}
```

后面的重力与 CharacterController.Move 必须继续执行，不要用 return 直接跳过。可以继续更新 currentSpeed，表示玩家准备恢复的普通移动速度；锁定期间不会将它应用为水平位移。

### 建议步骤 3：在状态退出时解除锁定

若目标是“冲刺状态与退出混合完成后才恢复控制”，移除源动作导入设置中原先的 EndQuick 动画事件，保留 StartQuick；在冲刺 Animator 状态上挂以下 StateMachineBehaviour。以下是建议源码，本次没有在 Assets 中创建它。

文件建议名：`Assets/Scripts/Animation/QuickExitBehaviour.cs`。

```csharp
using UnityEngine;

public sealed class QuickExitBehaviour : StateMachineBehaviour
{
    public override void OnStateExit(
        Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<PlayerObject>()?.EndQuick();
    }
}
```

编译后在 Animator 中选中 S_Quickshift_F 状态，通过 Add Behaviour 添加 QuickExitBehaviour。它挂在动画状态上，不是加到角色 GameObject 的普通 MonoBehaviour。

原 EndQuick 方法建议改为：

```csharp
public void EndQuick()
{
    isQuicking = false;
    dashTimer = 0f;
}
```

不能同时保留提前解锁的原 EndQuick 动画事件，否则它仍会提前清除锁定。OnStateExit 将清理与实际状态退出对应；后续若加入受击/死亡等中断状态，还应由新的状态决定普通移动是否允许，不能仅靠这一个 bool 管理全部战斗状态。

若还要求完整播放冲刺片段后才开始退出，建议把冲刺返回移动过渡的 Has Exit Time 保持开启，Exit Time 设为 1，再用较短的 Transition Duration（例如 0.1 秒）作为试玩起点。该时长是调参建议，不是实测最优值；OnStateExit 在过渡完成时解除锁定。若动作设计允许提前取消，则另行设计可恢复帧，而非一律等到结束。

## 三、待执行的检查

| 检查 | 期望 | 当前状态 |
|---|---|---|
| 保存后的混合树 | 1D，Velocity，Idle/Walk/Run=0/0.5/1 | 磁盘仍为旧配置，待核对 |
| 持续 Shift，W→W+D→D | 达到满速后换向保持 currentSpeed，Velocity 不因方向分量降低 | 新代码已存在，试玩待验证 |
| 松开方向键 | 平滑减速到 0 | 待验证 |
| 按住方向键冲刺 | 高速位移阶段结束后，收尾阶段水平位移为 0 | 代码修改方案待实施 |
| 冲刺收尾时尝试转向 | 收尾锁定期间不转向，状态退出后恢复 | 待实施与验证 |
| 冲刺状态退出 | 清理 isQuicking 和 dashTimer，无提前解锁事件 | 待实施与验证 |
| 收尾期间重力 | 继续执行，不悬空 | 待验证 |

后续应记录实际角色位移、currentSpeed、isQuicking、dashTimer 和 Animator 状态/过渡，区分正常恢复控制与仍处于收尾动作时的错误位移。本次不生成虚构采样结果或完成结论。

## 官方依据

- [1D 混合树](https://docs.unity3d.com/6000.0/Documentation/Manual/BlendTree-1DBlending.html)
- [动画过渡](https://docs.unity3d.com/6000.0/Documentation/Manual/class-Transition.html)
- [StateMachineBehaviour.OnStateExit](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/StateMachineBehaviour.OnStateExit.html)

本次静态来源与 SHA256 见同目录 `静态检查.json`。这些是工程排查记录，不能当作已经完成的性能优化或正式实验结果。


## 四、后续需求：退出冲刺时随动画混合逐渐恢复移动

用户进一步指出，过渡也有持续时间，希望冲刺退出到行走或跑步的过程中，位移速度与动作衔接，不希望另行反复调整恢复时间。前述“锁到 OnStateExit 才恢复”的方案适用于完整锁定需求，不能单独满足此次逐渐恢复控制的目标。

建议采用动画曲线提供普通移动权重（尚未实施、未运行验证）：

1. Animator 添加 Float 参数 `LocomotionWeight`，默认值 1。
2. 冲刺片段添加同名常量曲线，值为 0；Idle、Walk、Run 片段添加同名常量曲线，值为 1。所有相关片段明确设置曲线，避免依赖其他片段缺省值。
3. 动画过渡会混合曲线值，代码用 Animator.GetFloat 读取 0..1 的结果。不在脚本里 SetFloat 覆盖这个由曲线驱动的参数。
4. 冲刺主体的快速位移仍由动作自身的位移阶段管理；收尾/退出阶段改为按权重恢复普通移动，不再一律因 isQuicking 为 true 就强制水平速度为 0。
5. 设普通移动的准备速度为 currentSpeed，冲刺收尾速度为 dashRecoverySpeed，则实际水平速度可按 `Mathf.Lerp(dashRecoverySpeed, currentSpeed, weight)` 混合。举例收尾速度为 0、准备跑速为 5，则权重 0/0.5/1 对应速度 0/2.5/5；准备走速为 2.5，则对应 0/1.25/2.5；没有输入则普通目标为 0。这些是公式示例，不是实测数据。
6. Velocity 仍表示普通走跑状态所需的速度，不能将已经乘恢复权重的实际速度反过来驱动普通混合树，否则混合树可能退回 Idle/Walk 并形成不必要的反馈。Animator 曲线宜在动画本帧求值后读取；实际接入时应安排动画参数输入、曲线读取和 CharacterController 位移的更新顺序，不在多个回调里重复执行移动。
7. OnStateExit 可继续作为状态结束清理，但不再独自决定何时开始恢复移动；原提前 EndQuick 事件同样需要避免与新的阶段/权重机制冲突。

Unity 会随实际动画混合处理同名曲线，因此调整 Animator 过渡时间时，恢复权重可随之变化，不必同步维护另一份恢复计时。它解决控制恢复与动画过渡不同步的问题，不会自动推算素材脚步对应的真实地面速度。基础走跑速度、动作播放倍率与步幅仍须校准；若采用 Root Motion，还须核验素材有可用根位移、排除脚本重复位移并通过 CharacterController 正确处理碰撞。

依据：[Unity 导入动画曲线与 Animator 参数混合说明](https://docs.unity3d.com/6000.0/Documentation/Manual/AnimationCurvesOnImportedClips.html)。同名曲线驱动 Animator 参数；多个片段同时驱动时，Unity 混合曲线值。

截至本次说明，仅更新记录，没有添加曲线、修改原动作或落实上述速度混合逻辑，不能将其描述为已完成修复。
