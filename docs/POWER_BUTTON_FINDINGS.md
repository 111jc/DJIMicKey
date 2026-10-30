# DJI Mic Mini 电源键电脑事件测试

测试日期：2026-10-29；设备：DJI Mic Mini 发射器与 Wireless Mic Rx（USB VID_2CA3 / PID_4011）。

## 设备与监听状态

接收器 HID、Wireless Mic Rx 音频端点、电平读取、Windows 低级键盘钩子与健康探针均正常。测试进程运行在 Windows 10.0.19042、64 位、管理员权限。

## 隔离的短按测试

- 两次 45/60 秒原始 HID/媒体键采集中，健康探针持续通过，接收器连接不变。用户短按发射器 Power 键两次，但未观察到媒体键、WM_APPCOMMAND 或 DJI HID 报告。
- DJI 官方说明：发射器 Power 键短按切换降噪，长按约两秒开关机；配对/Link 键承担另一项功能。参考 [DJI Mic Mini 用户手册](https://dl.djicdn.com/downloads/DJI_Mic_Mini/20251009/DJI_MIC_MINI_UM_V1__EN.pdf) 与 [官方指南](https://repair.dji.com/help/content?customId=01700011680&lang=en&paperDocType=ARTICLE&re=US&spaceId=17)。

## 后续实时测试的歧义

后来运行 v1.7 主界面实时诊断时，输出里出现了一串 `Volume_Up`（VK `0xAF`）和 HID 按下/松开报告。这与之前已验证的配对键事件格式相同，但诊断器只能看到操作系统送来的媒体键和接收器级 HID 报告，不知道报告来自发射器的哪个实体按钮。日志中这串事件多于所述两次短按，且与先前隔离测试矛盾，因此不能可靠归因给 Power 键。

## 当前结论

**尚未证明电脑收到电源键的独立信号，也没有为它添加单独映射。** 可见的 `Volume_Up` 事件即使来自 Power 键，也与配对键无法区分；把它分配到另一功能会同时改变配对键行为。若要进一步区分实体来源，需要在严格隔离 Power/Link 的一次采集中各自短按一次，并按时间戳对照，或获得 DJI 公开的专用按键报告。

本次测试只覆盖上述电脑、USB 连接方式、接收器和固件；没有 Win11 实机结果，不代表其他版本或 DJI Mimo 内部协议。
