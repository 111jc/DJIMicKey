# v1.0.2 Shell 音量拦截

原版本只在键盘钩子收到 VK_VOLUME_UP 时阻止音量。用户确认 Win11 下 Raw 映射已启动/结束语音，但仍出现系统音量条；此前日志没有对应键盘事件，因此键盘抑制路径没有覆盖实际动作。

本版增加原生用户态 WH_SHELL 钩子：对 HSHELL_APPCOMMAND 中尚未处理的 APPCOMMAND_VOLUME_UP 返回非零，尝试在 Shell 默认处理前接管。不是读取 Raw 后再发送音量减，也没有设置音量、改驱动、安装服务或修改固件。

## 生效范围

- 仅在本工具运行、RX HID 已识别、映射启用、Raw 通道可用且未录制快捷键时开启。
- 暂停映射、RX 不再就绪或退出时关闭；原生回调通过存活的本工具隐藏窗口查询开关，窗口销毁后不再拦截。
- Shell 命令没有可靠设备身份。开启期间会影响同桌面的其他未处理 Volume Up 命令，这是全局范围，不是假称只拦截 DJI。Volume Down、Mute 和其他命令不处理。
- 这是 x64 用户态 Shell 路径适配，不能保证所有应用、权限层级或绕过 Shell 的音量路径都会经过它。若仍增加音量，必须按日志继续验证，不能直接宣布成功。

## 文件与构建

便携包内 DJIMicKey.exe 和 DJIMicKey.Shell.dll 必须放在同一目录。新增 DLL 是本项目自带 C 源码构建的用户态钩子组件，不是设备驱动。构建需要 MinGW-w64 的 gcc（运行不需要编译器）；Build.cmd 在 .build/native 中编译，再复制到 EXE 所在目录。继续使用系统 .NET Framework C# 编译器。

## 验证结果

- 73 项基础检查通过，含真实原生 DLL 的启用/禁用决策、音量减/静音/其他事件放行、所有者窗口销毁后放行。
- 这些策略测试没有向 Windows 发送真实音量命令，没有改变用户音量，也不能代替实体配对键验收。
- 界面、DPI、布局、托盘、快捷键录制、实时诊断和视觉反馈回归通过。
- 2026-10-31 用户确认其 Win11 电脑上实际语音启停正常、不再弹音量条（人工实测 PASS）。此前某轮自动 --device 因 RX 未连接而未通过，与用户后续实测分别记录；不保证其他机器。

## 用户验收和日志

1. 完全退出旧实例，解压新包。两个文件保留在一起，启动后确认标题 v1.0.2。
2. 保持原有快捷键，连接 RX，开启映射，打开诊断。
3. 初始状态应有 Shell volume filter: installed=True enabled=True。若 installed 失败，先保存错误；不要以管理员运行当成已确认修复。
4. 短按配对键 3 次。语音应继续切换，观察系统音量数值和音量条。
5. 若日志出现 SHELL_BLOCK command=10，说明确实经过并被该 Shell 层处理；handlerProcessId 是执行钩子的进程，不等于原始设备或可靠调用者。
6. 如果仍加音量且没有 SHELL_BLOCK，这次没有命中拦截路径；若已有 SHELL_BLOCK 仍加音量，则需要继续检查额外处理路径。请保存完整日志。
7. 暂停映射后普通音量功能应恢复。另在 Win10 回归语音启停，检查没有重复触发。

参考：[微软 ShellProc 返回值](https://learn.microsoft.com/en-us/windows/win32/winmsg/shellproc)、[全局钩子 DLL 与位数要求](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexw)。
