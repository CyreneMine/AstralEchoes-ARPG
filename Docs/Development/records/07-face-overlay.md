> 整理于 2026-10-15。本文保留执行当时的对象名称、方案和验证范围；当前入口见 [开发状态](../STATUS.md)。原始记录位于本地 `工作记录/Cyrene面部黑块诊断_20261015/诊断与修复记录.md`。资源、截图和备份未上传。

# Cyrene 面部黑块诊断与修复

日期：2026-10-15
工程：E:\UnityProjects\AstralEchoes
Unity：6000.4.8f1，URP 17.4.0

## 现象与证据

用户反馈重启工程后额头、嘴部出现大片黑块。Unity 当前 Scene 视图中复现了相同现象。

当前模型为 Cyrene_cats.fbx，GUID 164b0b633681e7e4a9d2da2153e2157a（原 untitled 模型的同一资源身份）。其材质重映射包含 Mat_Cats/顏2+.mat。

该材质的 Shader 为 SimpleURPToonLitExample(With Outline)，Base Map 指向颜赤.tga。它是腮红覆盖贴图，不是面部基础皮肤贴图。图像统计：1024×1024；862273 像素 Alpha=0，Alpha 范围为 0..189；透明区 RGB 为黑底。原始 PMX 材质证据 source_evidence.json 中，顏+ 和顏2+ 的 diffuse Alpha 均为 0，即默认隐藏的覆盖层。

修复前材质 Base Color Alpha=1，_UseAlphaClipping=0，m_ValidKeywords 为空。示例 Shader 的颜色 Pass 不进行透明混合；Alpha 仅在启用 _UseAlphaClipping 关键字后通过 clip(alpha-_Cutoff) 剔除。这使原本隐藏的覆盖层以不透明方式绘出，并显示黑色背景。

面部基础图颜.png仍被 Mat_Cats/齿.mat 引用。没有替换基础皮肤贴图。

## 最小修改

仅修改 Assets/ArtRes/Character/Cyrene/Mat_Cats/顏2+.mat：

- _BaseColor.a：1 -> 0，恢复原模型默认隐藏的腮红覆盖层。
- _UseAlphaClipping：0 -> 1。
- m_ValidKeywords：加入 _UseAlphaClipping，确保实际编译变体执行裁剪。
- 保持 _Cutoff=0.5、贴图引用及其他材质设置不变。

原文件备份：本目录的 顏2+.before.mat.txt。

## 验证与边界

Unity 重新导入材质后，直接检查 Scene 视图，额头与嘴部的黑色覆盖区域消失，显示正常面部。参数已保存到 .mat 文件，而非仅修改运行时材质实例。

没有修改 FBX、骨骼、Avatar、BlendShape、动作、场景或 Magica Cloth 设置；没有创建或保留自动执行的 Editor 脚本。

本次未重新关闭并启动 Unity，不能将材质重导入验证写成重启测试。重启触发现象的具体内部缓存过程未单独复现；已证实的问题是磁盘材质中的默认可见性和透明裁剪配置错误。

本次恢复的是正常默认脸部。如果以后需要启用动态腮红表情，应为覆盖层配置支持透明混合的材质，并由表情逻辑驱动透明度；不要仅将本材质 Alpha 调回 1。
