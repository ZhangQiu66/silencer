# silencer

Windows 音量上限锁定工具。设定最高音量并锁定后，系统音量不能被调到上限以上。

本项目基于 Troy Larson 的开源软件 [Quiet on the Set](https://github.com/troylar/quiet-on-the-set)（MIT 许可证）改进，并更名为 silencer。主要变化包括：

- 更稳定的音量锁定：约每 200 毫秒复查一次，覆盖所有正在使用的播放设备；默认输出设备切换或音频出错时会自动重连
- 中文界面与自定义图标
- 有人把音量调过上限时，可弹出全屏警示（可在设置中关闭）

> 说明：锁定只在本程序运行时生效，退出或删除程序后限制即会停止。它提供的是基础限制，熟悉电脑的人仍可退出程序。

## 使用

1. 在 [Releases](https://github.com/ZhangQiu66/silencer/releases) 下载 `silencer.exe`、`silencer.exe.config` 和 `NAudio.dll`，放到同一个文件夹
2. 运行 `silencer.exe`（需要 Windows，以及 .NET Framework 4.5.2 或更高版本）
3. 拖动滑块设置最高音量，可选填写锁定密码，然后点击「锁定」
4. 关闭窗口后程序会放到托盘继续运行；只有在解锁状态下点击「退出」才会真正结束
5. 如需开机自动运行，勾选「登录时自动启动」
6. 「音量超限时弹出警示」用于打开或关闭超限弹窗，关闭后仍会把音量拉回上限

## 许可证

采用 MIT 许可证。原作版权归 Troy Larson（2016）所有，silencer 中的修改版权归张秋（2026）所有。完整条款见 [LICENSE](LICENSE)。
