# Mosslight Farm — macOS 原生移植开发版 0.8

**此工程尚未跟进 Windows 0.12。当前没有在 Mac 实机完成编译和游玩验证，也没有可直接安装的发行包。Windows 0.12 存档格式为 Version 3，不能直接给此旧版读取。**

**本目录是 macOS 源码和资源，不是已完成实机验收的安装包。** Windows 无法编译、签名或验证这里的 AppKit 桌面宿主。当前未生成可直接分发的 `.app` / `.dmg`。

## 已实现

- Swift + AppKit + WKWebView 原生宿主，不依赖 Windows、Wine、Wallpaper Engine、Node 或 Python 运行时。
- 下层像素背景与上层透明农场窗口；普通应用窗口位于农场上方。鼠标离开农场物品和工具栏时穿透到桌面。
- 单个显示器上的完整农场，可在菜单栏切换显示器；窗口预览和桌面模式。
- 9 种作物、加工队列、订单、建造、扩建、动物生产、宠物漫游、装饰拖动、离线生产、每日奖励。
- 中文 / English，50 / 75 / 85 / 100 / 125 / 150% 界面缩放。
- 本地原子存档与备份、睡眠唤醒、渲染进程恢复、菜单栏保存退出。
- 通过 SMAppService 配置登录启动，显示系统需要用户确认的状态。

## 在 Mac 构建

需要 macOS 13 或更高版本和 Xcode Command Line Tools。

在本目录运行：

```sh
bash build.sh
```

也可双击 `Build.command`。脚本编译 Apple Silicon + Intel Universal 应用，输出到 `build/日期时间/Mosslight Farm.app`。每次使用新目录，保留旧构建。脚本只做本地临时签名，不包含 Developer ID 公证。

构建成功后，将应用复制到 `/Applications` 再开启登录启动。菜单栏的 `♧` 提供设置、窗口/桌面切换、显示器选择、存档文件夹和退出。

## 存档

`~/Library/Application Support/MosslightFarm/save.json`

此旧版使用 Version 1 存档；Windows 0.12 使用 Version 3，不支持直接交换存档。目前没有云同步。Windows 新版存档位于 `%USERPROFILE%\Saved Games\MosslightFarm\save.json`。

## 验证状态与限制

- 已在 Windows 的 Chromium 浏览器验证此目录实际使用的游戏逻辑和画面：33 项检查通过。包括收种、加工出售、订单、防重复奖励、离线生产、拖动与重新载入、中英文切换、640×480 / 1440×900 / 3840×2160 下的缩放。
- 这些结果**不代表 Safari/WebKit、Swift 编译或 Mac 桌面集成通过**。
- Mac 待验收：双架构编译、Retina、Finder 图标点击穿透、快速拖动、Spaces / Mission Control、休眠恢复、登录启动、重新打开后存档一致性。
- 当前每次只在一个显示器展示农场。宠物活动在当前显示器，尚未实现跨显示器行走。
- 菜单栏和错误提示暂时中英并列。核心游戏界面可切换语言。
- 正式公开分发前仍需 Developer ID 签名、公证与 Mac 实机验收。本版尚未上架 Steam。

原生接口依据：[Apple 窗口层级](https://developer.apple.com/documentation/coregraphics/cgwindowlevelkey)、[登录启动](https://developer.apple.com/documentation/servicemanagement/smappservice)。
