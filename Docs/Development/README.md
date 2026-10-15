# 开发记录索引

本目录将历史过程、当前状态和测试证据分别列出，原始文件及撤销备份继续保存在本地。

先读 [当前开发状态](STATUS.md)，再按问题查阅记录。

| 主题 | 记录 | 适用范围 |
|---|---|---|
| 工程路径与启动异常 | [搬迁说明](records/01-project-path.md)、[用户启动验收](records/02-project-path-acceptance.md) | 当前英文路径工程的来源 |
| 白色颗粒与透明叠层 | [PMX 材质记录](records/03-legacy-pmx-materials.md) | 已删除旧工程的历史案例 |
| Avatar、插件与 Shader | [映射和插件处理](records/04-avatar-and-plugins.md) | 旧 FBX 映射、插件清理及方案撤销 |
| WASD 与身体大幅偏移 | [实现和修正](records/05-wasd-and-root-motion.md) | 旧临时控制器与动作副本 |
| 裙摆、袖摆及头纱物理 | [BoneCloth 配置](records/06-bonecloth.md) | 当前 CATS 模型；名称沿革见文首 |
| 重启后面部黑块 | [覆盖层修复](records/07-face-overlay.md) | 当前 Nilo 示例 Shader 的腮红覆盖材质 |

[文本证据目录](evidence/)保留实际测试数据。[来源清单](SOURCES.json)记录原始文档路径与 SHA256。公开副本只调整文档链接与本机路径表达，不改测试数值。

论文可使用这些记录说明排查方法、资源适配和验证过程。旧工程记录应明确为探索过程；当前实现须与最终提交工程重新对应。这里没有完成率、帧率提升、用户实验或最终构建结论。
