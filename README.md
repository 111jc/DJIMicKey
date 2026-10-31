# DJI Mic Key

当前源码版本：**v1.0.2**。将 DJI Mic Mini TX 配对键映射为自定义快捷键，适合切换微信输入法语音输入。用户已确认在本次 Win11 测试电脑上，语音启停正常且不再弹出音量条。

这是非 DJI 官方工具，已适配的接收器为 USB **2CA3:4011**。不保证其他型号、蓝牙直连或所有系统环境。GitHub 源码更新不会自动替换旧 Release 安装包，下载前请核对版本。

![界面示意，电平为演示数据](docs/images/window.png)

## 使用

运行环境：Windows 10/11 x64、.NET Framework 4.8。便携包中的 **DJIMicKey.exe 和 DJIMicKey.Shell.dll 必须保持在同一目录**，无需安装设备驱动、AutoHotkey 或 GCC。

1. 完全退出旧实例，解压新版并启动 EXE，确认标题为 v1.0.2。
2. 连接 RX，确认 TX 已配对、未静音。可在界面查看麦克风电平；USB 已连接不等于无线链路正常。
3. 点击“开始录制”，保持修饰键按住，再按下并松开主键，最后松开修饰键。点击“应用并保存”后生效。
4. 输入法设置完全相同的组合。默认是 **左 Ctrl + 左 Alt + F9**；在文本框先用真实键盘验证，再用配对键切换语音。
5. × 可选择退出或最小化到托盘。托盘保留映射、停止电平采集；暂停映射或退出会释放音量拦截。

支持自适应双栏/单栏、窗口内无双向滚动条、逐显示器 DPI、实时电平趋势、按键灯反馈、开机启动和独立诊断弹窗。设置保存在 `%LOCALAPPDATA%\DJIMicKey\settings.ini`。

## 映射与拦截范围

通过目标 RX 的 Consumer Raw HID 及实际设备描述解析 Volume Increment，松开触发一次快捷键。键盘事件与 Raw 输入不会各触发一次。Raw 注册失败时保留旧键盘路径，日志会注明。

为阻止原生音量动作，键盘钩子和用户态 Shell 钩子保留音量增加拦截。映射启用且 RX 就绪时，**同桌面其他未处理的 Volume Up 也可能被阻止**；Shell 命令不能可靠区分原设备。音量减、静音和其他命令不处理。暂停、录制快捷键、设备断开或退出会停用 Shell 拦截，不会通过减音量补偿。

此机制受桌面、权限和位数等边界约束，不能保证所有应用环境。电源键的独立映射尚未证实，详见 [测试记录](docs/POWER_BUTTON_FINDINGS.md)。

## 构建与测试

开发需要 Windows x64、.NET Framework 4.8 C# 编译器和 PATH 中的 **MinGW-w64 x64 gcc**。GCC 仅构建自带用户态 DLL；无 NuGet 依赖。

```bat
Build.cmd
Test.cmd
Test.cmd --device
Release.cmd
```

- Build.cmd 编译 C DLL/C# EXE 并运行 73 项基础检查；默认输出 dist/。
- Test.cmd 验证 DPI、布局、录制、保存、托盘、生命周期、诊断及视觉反馈，需要交互式桌面。
- --device 额外检查当前 RX/音频设备；没有设备时不能视为硬件通过。
- Release.cmd 在独立构建目录生成便携包，只打包当前 EXE、DLL、许可证与使用文档。

输出位于忽略跟踪的 `dist/` 和 `.build/`。私人日志放在忽略跟踪的 `log/`，不提交录音、设备日志、设置或构建产物。

## 源码职责

- src/App.cs：界面、布局、录制/保存、诊断和生命周期。
- src/Core.cs：快捷键、键盘钩子、设备与音频监测。
- src/RawInputMapping.cs：常驻设备来源筛选及 HID 描述解码。
- src/ShellVolumeGuard.cs、src/ShellVolumeHook.c：用户态 Shell 音量增加拦截。
- src/InputDiagnostics.cs、src/PowerWatch.cs：诊断日志及独立电源键观察。
- tests/SelfTests.cs：基础检查和测试辅助。

[Windows 使用说明](docs/WINDOWS.md) · [验证范围](docs/VALIDATION.md) · [Win11 排查](docs/WIN11_TROUBLESHOOTING.md) · [Shell 拦截机制](docs/SHELL_VOLUME_FILTER.md) · [贡献指南](CONTRIBUTING.md)

## License

[MIT](LICENSE)。DJI 和微信输入法名称仅说明兼容对象，不表示官方隶属或授权关系。
