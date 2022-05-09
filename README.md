![icon](https://cdn.jsdelivr.net/gh/nguaduot/TimelineWallpaper/Assets/StoreLogo.scale-200.png)

# 拾光 for Windows 11

[![release](https://img.shields.io/github/v/release/nguaduot/TimelineWallpaper)](https://github.com/nguaduot/TimelineWallpaper/releases)
[![downloads](https://img.shields.io/github/downloads/nguaduot/TimelineWallpaper/total)](https://github.com/nguaduot/TimelineWallpaper/releases)

> 拾光如歌，岁月如诗。拾光，每日一景

`拾光` 是一款壁纸应用，集成丰富图源，支持每日推送桌面/锁屏。使用 UWP 框架开发，遵循 Fluent Design，是原生的 Windows 应用，于 Windows 11 体验最佳，向下兼容 Windows 10。

## 开始

提供以下两种安装方式：

+ 从 Microsoft Store 安装
  
  在 Microsoft Store 搜索 `拾光壁纸` 进行安装。直达链接：[拾光壁纸 - Microsoft Store](https://www.microsoft.com/store/apps/9N7VHQ989BB7)

+ 下载安装包手动安装
  
  打开右侧的 [Release](https://github.com/nguaduot/TimelineWallpaper/releases) 页面，找到最新版本，下载压缩包，然后解压，找到 `install.ps1` 脚本，右键 **使用 PowerShell 运行**，根据提示即可顺利安装。

**Watch** 项目，以获取应用的更新动态。

## 图源

自建图源：

+ [拾光](https://api.nguaduot.cn/timeline/doc) - 时光如歌，岁月如诗

  API 官网：[api.nguaduot.cn/timeline](https://api.nguaduot.cn/timeline/doc)
  
  开源：[github.com/nguaduot/timeline-api](https://github.com/nguaduot/timeline-api)

三方图源：

+ [Microsoft Bing](https://cn.bing.com) - 每天发现一个新地方
+ [NASA](https://apod.nasa.gov/apod) - 每日天文一图
+ [OnePlus](https://photos.oneplus.com) - Shot on OnePlus
+ [ONE · 一个](http://m.wufazhuce.com/one) - 复杂世界里，一个就够了
+ [向日葵8号](https://himawari8.nict.go.jp/) - 实时地球
+ [一梦幽黎](https://www.ymyouli.com) - 8K优质壁纸资源
+ [轻壁纸](https://bz.qinggongju.com) - 壁纸分享站
+ [乌云壁纸](https://www.obzhi.com) - 高清壁纸站
+ [WallHere](https://wallhere.com) - 世界著名的壁纸网站之一
+ [Infinity](http://cn.infinitynewtab.com) - 精选壁纸

*特别注明：三方图源均为来自网络，本应用无权且不提供商用授权，所以请勿用于商业用途，仅供学习交流。欢迎分享图源*

## 进阶

+ 使用快捷键
  + `鼠标右键`：菜单
  + `鼠标左键`（双击） / `Enter` / `Esc`：切换全屏/窗口模式
  + `鼠标滚轮`：回顾前一天/预览下一天
  + `左方向键` / `上方向键`：回顾前一天
  + `右方向键` / `下方向键`：预览下一天
  + `Backspace` / `Delete`：标记“不喜欢”
  + `Space`：切换全图/拉伸
  + `Ctrl` + `B`：用作桌面背景
  + `Ctrl` + `L`：用作锁屏背景
  + `Ctrl` + `S` / `Ctrl` + `D`：保存图片
  + `Ctrl` + `R` / `F5`：刷新
  + `Ctrl` + `F` / `Ctrl` + `G`：跳转至指定日期
  + `Ctrl` + `C`：复制图片元素

+ 如何以2小时/次的周期更换桌面壁纸？
  + 开启目标图源的桌面/锁屏推送
  + 右键菜单点击“**设置**”图标，导航至“**常规**”组，展开“**配置文件**”，点击“**打开**”，即可编辑配置文件
  + 找到当前图源的块 `[xxx]`，将 `desktopperiod` 参数值修改为 `2`，即2h/次，保存即可

## 反馈

你可以在以下渠道联系到我：
+ 邮件 [nguaduot@163.com](mailto:nguaduot@163.com)
+ 酷安 [@南瓜多糖](http://www.coolapk.com/u/474144)
+ Telegram [@nguaduot](https://t.me/nguaduot)

## 截图

![截图1](https://cdn.jsdelivr.net/gh/nguaduot/TimelineWallpaper/screenshot.jpg)

![截图2](https://s3.bmp.ovh/imgs/2021/12/001241e0a14ef263.gif)

自取：[截图1壁纸原图](https://s3.bmp.ovh/imgs/2021/11/5db69c315b1ab3e3.jpg)

*Copyright © 2021-2022 nguaduot. All rights reserved.*
