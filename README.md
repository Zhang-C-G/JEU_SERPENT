# 🐍 Snake Duel - 双人对战贪吃蛇游戏

[![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

一个功能完整的双人对战贪吃蛇游戏，支持AI训练模式和本地PvP模式。

## 📋 项目概述

**Snake Duel** 是一个基于 .NET 9.0 和 WinForms 的现代化贪吃蛇对战游戏，包含：
- 🤖 **AI训练模式** - 对战渐进加速的智能AI
- ⚔️ **双人对战模式** - 本地PvP (WASD vs 方向键)
- 🎨 **灵活渲染系统** - 支持颜色方案和精灵图片
- 🔧 **可扩展架构** - 插件化收集物系统

---

## 🎮 游戏特性

### 核心玩法
- ✅ **30x30网格地图**
- ✅ **3条生命系统** + 即时复活
- ✅ **2分钟倒计时**
- ✅ **收集物系统** - 普通方块和稀有方块
- ✅ **完整碰撞检测** - 墙壁、自身、对手
- ✅ **智能胜负判定**

### AI系统
- ✅ **寻路算法** - BFS追踪最近收集物
- ✅ **障碍物避让** - 自动规避危险
- ✅ **渐进加速** - 每10秒增速10%，最高176%

### 双人对战
- ✅ **Player 1**: `W` `S` `A` `D`
- ✅ **Player 2**: `↑` `↓` `←` `→`
- ✅ **防误操作** - 阻止180度掉头
- ✅ **独立控制** - 双方完全独立

---

## 🚀 快速开始

### 环境要求
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Windows 操作系统
- Visual Studio 2022 或 VS Code (可选)

### 运行游戏

#### 方法1：命令行运行
```bash
cd Gauniv.Game
dotnet run
```

#### 方法2：Visual Studio
1. 打开 `TP2.NET.sln`
2. 设置 `Gauniv.Game` 为启动项目
3. 按 `F5` 运行

---

## 🎯 操作指南

### 主菜单
1. **🤖 AI训练模式** - 单人挑战渐进AI
2. **⚔️ 双人对战模式** - 本地双人PvP

### 游戏操作
| 功能 | Player 1 | Player 2 |
|-----|---------|---------|
| 向上 | `W` | `↑` |
| 向下 | `S` | `↓` |
| 向左 | `A` | `←` |
| 向右 | `D` | `→` |
| 退出 | `ESC` | `ESC` |

### 游戏规则
- 🟥 **红色方块** - 普通收集物 (+1节)
- 🟨 **金色方块** - 稀有收集物 (+1节，稀有)
- 💀 **碰撞** → 死亡 (-1生命)
- ❤️ **3条生命** - 耗尽后游戏结束
- ⏱️ **2分钟** - 时间结束判定胜负

---

## 📁 项目结构

```
TP2.NET/
├── Gauniv.Game/              # 🎮 游戏客户端 (WinForms)
│   ├── MenuForm.cs           # 主菜单
│   ├── GameForm.cs           # 游戏窗口
│   ├── LocalGameController.cs # 游戏逻辑控制器
│   └── Rendering/            # 渲染系统
│
├── Gauniv.GameServer/        # 🎲 游戏服务器
│   ├── Engine/               # 游戏引擎
│   ├── Systems/              # 游戏系统
│   ├── AI/                   # AI系统
│   └── Collectibles/         # 收集物系统
│
├── Gauniv.WebServer/         # 🌐 Web分发平台
└── Gauniv.Shared/            # 📦 共享库
```

---

## 🛠️ 技术栈

- **前端**: WinForms (GDI+)
- **后端**: .NET 9.0
- **数据序列化**: MessagePack
- **数据库**: Entity Framework Core
- **架构模式**: MVC + ECS

---

## 📊 完成度

| 模块 | 完成度 | 状态 |
|-----|--------|------|
| 游戏客户端 | 100% | ✅ |
| 游戏服务器 | 95% | ✅ |
| AI系统 | 100% | ✅ |
| Web平台 | 60% | ⚠️ |

**总体完成度：90%** 🎉

---

## 👨‍💻 作者

**Zhang Chenggong**
- GitHub: [@Zhang-C-G](https://github.com/Zhang-C-G)

---

**⭐ 如果觉得这个项目不错，请给个 Star！**