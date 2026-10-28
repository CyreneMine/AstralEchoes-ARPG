# 基础对象池、朝向脚本与世界空间血条选型

归档日期：2026-10-28；Unity 6000.4.8f1，UGUI 2.0.0。

## 用户观察与目标

用户独立编写基础 PoolMgr，随后制作让目标始终面朝摄像机的 LookMe，并询问敌人头顶血条的 Canvas、生成方式、Slider 与填充 Image 的取舍。用户希望了解正式项目的设计依据，而不是只得到临时教学方案。

## 用户提出的想法

- 对象池用 Dictionary<string, Stack<GameObject>> 保存不同预制件的空闲对象。
- 用户表示已修复取用空栈和资源路径问题，ClearPool 暂时不改。
- 用户担心 UGUI 必须在 Canvas 下会导致每帧位置同步、每生成敌人都需另建血条；最终在敌人子级加入 World Space Canvas。
- 用户自行定位 Transform 手柄偏到 Canvas 中心的原因，切换 Center/Pivot 后解决；这个发现归用户，不归助手。
- 用户明确保留当前血条尺寸、锚点与交互配置，暂不采纳助手提出的居中和关闭射线检测建议。

## 辅助分析与建议

对象池 ContainsKey 只能说明池存在，不能说明栈非空；Pop 前需要 Count > 0。Resources 路径需包含 PoolPrefabs/。LookMe 初版判空条件写反，有目标时反而 return；建议改为无目标才退出，并在 LateUpdate 更新朝向。

血条方案比较：World Space Canvas 作为敌人子物体时，位置可依靠父子关系跟随；只需处理朝向。屏幕空间血条则需世界坐标投影、显示与回收管理。根据当前项目采用前者，不将其宣称为所有企业唯一标准。纯展示血条选择 Image Filled，Slider 同样可实现；没有性能实测，不能宣称更换组件带来确定提升。

## 实际采纳与用户调整

- PoolMgr 已保存空栈判断和 PoolPrefabs/ 路径；新建对象将 name 设为 poolName，保持归还键一致。ClearPool 仍只清字典，不销毁场景缓存对象，这是用户明确延期的范围。
- LookMe 已修正判断并改用 LateUpdate；当前仍先访问 Camera.main.transform，再判 targetTrs，且 RectTransform/Transform 旋转赋值重复。主摄像机缺失时的保护仍未加入；不在本轮归档中顺手改代码。
- 当前层级为 EnemyObj / HealthBarPos / Canvas / HealthBar / BK、Fill；Canvas 为 World Space，尺寸600×100，缩放0.005；父级缩放1时约为3×0.5世界单位。LookMe 挂在 Canvas 上。
- imgFill 指向 Fill；Fill 为水平 Filled Image。实际 Fill Origin 以场景保存值为准，原锚点和射线选项按用户要求保留。

## 当前文件依据

- `Assets/Scripts/Managers/PoolMgr.cs`，历史修正提交 c939356。
- `Assets/Scripts/Object/Common/LookMe.cs`
- `Assets/Scripts/UI/GamePanel/HealthBarController.cs`
- `Assets/Scenes/GMScene.unity`

## 验证与当前状态

对象池修正有用户完成声明和静态核对，没有本轮取出/归还实验。用户确认 Pivot 操作解决手柄位置问题。Canvas 层级、Image 设置与引用通过文本核对；未独立测试不同视角、遮挡、复用、分辨率或大量敌人下的性能。

## 论文素材与参考入口

可描述对象复用、UI 展示职责与世界/屏幕空间取舍，不将教程机制写为原创算法。

- [Unity Canvas 模式](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/class-Canvas.html)
- [Unity UI 优化建议](https://unity.com/how-to/unity-ui-optimization-tips)
