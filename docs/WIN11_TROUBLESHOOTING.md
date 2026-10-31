# Win11 排查

v1.0.2 已由用户确认：配对键可控制微信输入法语音启停，且不再出现原音量条。若其他电脑失败，先核对 EXE 与 Shell DLL 均为同一版本，完整退出旧实例再运行。

1. 工具保存组合与输入法一致；文本框中用真实键盘验证。
2. 确认 RX 已连接、映射启用。开始诊断后短按配对键三次，导出到本地 log/。
3. 不提交私人日志到公开仓库；分享前删除不需要的设备路径/标识。

| 日志 | 判断 |
|---|---|
| HID button ready=False | 接收器未识别，先核对数据连接 |
| Raw source mapping=True | 正在使用新 Raw 触发链路 |
| RAW decoded / RAW DECISION trigger=True | 来源解析并触发，继续检查发送 |
| SEND accepted | Windows 接受注入；不等于输入法确认语音状态 |
| Shell volume filter: installed=True enabled=True | 音量增加拦截已装载并开启 |
| SHELL_BLOCK command=10 | 已接管未处理的 Shell 音量增加命令 |
| Shell filter 错误 | 核对同目录 DLL、版本及完整错误；不要仅凭普通权限推断根因 |
| Raw 有而 LOWLEVEL 无 | Win11 可能只进入 Raw 路径，新版不依赖其产生键盘事件 |
| Shell 已命中仍增加音量 | 可能存在额外处理通道，需要新的实际日志；不能只减回固定音量 |

Shell 事件的 handlerProcessId 只表示执行回调的进程，不代表可靠调用者或物理设备。Shell 拦截范围、限制和文档来源见 SHELL_VOLUME_FILTER.md。
