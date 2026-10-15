> 整理于 2026-10-15。本文保留执行当时的对象名称、方案和验证范围；当前入口见 [开发状态](../STATUS.md)。原始记录位于本地 `工作记录/工程搬迁_20261014/用户启动验收_20261014.md`。资源、截图和备份未上传。

# 英文路径启动验收

日期：2026年10月14日。

工程从 E:\毕设\AstralEchoes 搬至 E:\UnityProjects\AstralEchoes 后，用户重新打开并反馈：

> 确实好了

据此更新为：用户确认本次 QuickInstaller / Assembly.GetName / GetCodeBase 的 Illegal byte sequence 异常在英文路径下不再出现。搬迁时未修改核心工程文件，60个核心文件哈希核对一致，结果支持原工程路径含非ASCII字符是本次异常的触发因素。尚未进行所有Unity版本和环境的普遍性验证，不推广为所有中文资源名均不可用。

后续工作区分工：Unity工程位于 E:\UnityProjects\AstralEchoes，论文、学校材料、项目协作规则和开发归档继续位于 E:\毕设。两个目录均已直接读取验证。
