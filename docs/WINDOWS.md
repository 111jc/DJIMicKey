# DJI Mic Key v1.0.2 使用说明

Windows 10/11 x64、.NET Framework 4.8。运行包内 EXE，Shell DLL 必须同目录；运行无需编译器、驱动或安装器。先完全退出旧版，避免单实例机制只恢复旧窗口。

## 录制与使用

连接 USB RX，确认 TX 已配对且未静音。对 TX 说话检查电平，停止测试或隐藏窗口会释放音频采集。工具不保存或上传声音。

点击“开始录制”：按住左 Ctrl 和左 Alt，再按下/松开 F9，最后松开修饰键。确认预览三键完整后点击“应用并保存”。默认组合为左 Ctrl + 左 Alt + F9，输入法必须一致。Esc、超时或离开窗口取消录制；保存失败保留草稿和原映射。

文本框中先用真实键盘验证语音组合，再短按配对键开始/结束。工具次数表示快捷键已发送，不等于输入法反馈。用户已确认本次 Win11 电脑上正常启停且无音量条。

## 界面与后台

主界面按窗口空间自动布局，无上下/左右滚动条。DPI 与系统字体变化会重新布局。灯表示检测到按下；不代表正在录音。诊断弹窗独立显示原始控制事件。

× 可退出或最小化到托盘。托盘继续映射、停止电平采集；双击恢复。开机启动在当前用户 Run 项注册固定 EXE 路径，移动程序后需重新设置；任务管理器禁用项需手动恢复。

## 范围与设置

Raw 输入只匹配 2CA3:4011 的 Consumer collection，使用实际 HID 描述解码。输入去重后松开才发送。Raw 模式下键盘 Volume Up 仅抑制，不再额外触发。

键盘/Shell 音量增加抑制有全局范围，可能影响同桌面其他未处理的 Volume Up；不拦截音量减/静音。暂停、断开或退出恢复，不会自动修改音量值。详见 SHELL_VOLUME_FILTER.md。

设置在 %LOCALAPPDATA%\DJIMicKey\settings.ini。损坏时暂停映射并提示；不自动重置成可能误触发的配置。

## 诊断

打开“诊断”，短按配对键三次，再导出。有效日志应有 Raw source mapping: True；Shell volume filter 应 installed=True enabled=True；命中后有 SHELL_BLOCK command=10。没有命中不能宣称已屏蔽音量动作。

SendInput 成功不等于输入法确认。目标应用权限更高时可能阻止注入，不将管理员启动作为默认要求。更详细见 WIN11_TROUBLESHOOTING.md。

独立电源键观察仍保留：DJIMicKey.exe --power-watch report.txt 60；不会映射电源键。
