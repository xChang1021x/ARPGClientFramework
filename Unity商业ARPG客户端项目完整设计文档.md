# 一、项目定位

## 项目名称

**ARPG Client Framework**

中文：

> 基于Unity的模块化ARPG客户端框架

------

# 二、项目目标

最终Demo：

类似：

- 原神战斗简化版
- 崩铁战斗简化版
- 暗黑ARPG

核心玩法：

一个角色：

- 移动
- 普攻
- 释放技能
- Buff/Debuff
- 击杀怪物
- 关卡结算

但是重点展示：

客户端工程能力。

------

# 三、技术栈选择

## Unity

版本：

推荐：

```
Unity 2022 LTS
```

原因：

公司大量使用LTS版本。

------

## 编程语言

C#

版本：

.NET Standard 2.1

------

## 架构

采用：

```
MVP + Data Driven
```

------

# 四、整体架构设计

最终：

```
ARPGClient

│
├── Framework
│
│   ├── Core
│   │
│   ├── Event
│   │
│   ├── Pool
│   │
│   ├── Timer
│   │
│   └── IOC
│
│
├── Resource
│
│   ├── Addressables
│   ├── AssetLoader
│   └── Cache
│
│
├── UI
│
│   ├── UIManager
│   ├── BasePanel
│   └── WindowStack
│
│
├── Data
│
│   ├── Config
│   ├── Save
│   └── Table
│
│
├── Network
│
│   ├── Socket
│   ├── Protocol
│   └── Message
│
│
├── HotUpdate
│
│   └── HybridCLR
│
│
├── Gameplay
│
│   ├── Character
│   │
│   ├── Combat
│   │
│   ├── Skill
│   │
│   ├── Buff
│   │
│   ├── AI
│   │
│   └── Level
│
│
└── Tools
    │
    ├── Editor
    └── ExcelExporter
```

------

# 五、模块详细设计

------

# Module 1：Framework核心框架 ⭐⭐⭐⭐⭐

这是最重要模块。

## 1. Game入口

设计：

```
GameLauncher
```

负责：

- 初始化框架
- 加载资源
- 初始化管理器

流程：

```
GameLauncher

↓

ResourceManager

↓

UIManager

↓

SceneManager

↓

GameStart
```

------

# 2. Singleton单例系统

目录：

```
Framework/Core/Singleton
```

实现：

```
Singleton<T>
```

用于：

- ResourceManager
- AudioManager
- UIManager

------

# 3. EventBus事件系统

目录：

```
Framework/Event
```

核心：

```
EventBus
```

功能：

发布：

```
PlayerLevelUpEvent
```

监听：

```
UI

Audio

Achievement
```

架构：

```
Player

 |
 |
EventBus

 |
 ├── UI

 ├── Audio

 └── Quest
```

------

# 4. ObjectPool对象池

目录：

```
Framework/Pool
```

应用：

- 子弹
- 技能特效
- 怪物

结构：

```
ObjectPool

↓

PoolItem

↓

GameObject
```

------

# Module 2：Resource资源系统 ⭐⭐⭐⭐⭐

商业项目核心。

------

## AssetManager

负责：

统一资源入口。

```
AssetManager.Load<T>()
```

------

结构：

```
ResourceManager

↓

Addressables

↓

Asset Cache

↓

GameObject
```

支持：

- 同步加载
- 异步加载
- 引用计数

------

# Module 3：数据驱动系统 ⭐⭐⭐⭐⭐

你的旧项目已有基础。

升级。

------

数据：

Excel

↓

Json

↓

ScriptableObject

↓

Runtime

例如：

技能表：

| ID   | Name     | Damage | CD   |
| ---- | -------- | ------ | ---- |
| 101  | FireBall | 100    | 3    |

运行：

```
SkillData

↓

Skill
```

------

# Module 4：UI框架 ⭐⭐⭐⭐⭐

结构：

```
UIManager

↓

BaseWindow

↓

Panel

↓

Component
```

实现：

例如：

打开背包：

```
UIManager.Open("BagUI")

↓

Load Prefab

↓

Instantiate

↓

Bind Data
```

------

功能：

- UI层级
- UI缓存
- UI生命周期

------

# Module 5：角色系统 ⭐⭐⭐⭐⭐

采用MVP。

结构：

```
Player


Model

数据


Presenter

逻辑


View

表现
```

例如：

移动：

```
Input

↓

PlayerPresenter

↓

PlayerModel

↓

PlayerView
```

------

# Module 6：战斗系统 ⭐⭐⭐⭐⭐

核心。

------

## 战斗流程

```
Input

↓

SkillManager

↓

DamageCalculator

↓

BuffSystem

↓

Effect

↓

UI
```

------

# Skill系统

设计：

```
Skill

|

├── SkillData

├── SkillEffect

└── Cooldown
```

支持：

火球：

```
DamageEffect

+

BurnBuff
```

------

# Buff系统

结构：

```
Buff

|

├── Duration

├── Stack

├── Modifier
```

例如：

攻击提升：

```
Attack +=20%

10秒后恢复
```

------

# Module 7：AI系统 ⭐⭐⭐⭐

采用行为树。

结构：

```
BehaviorTree


├── Composite

├── Decorator

└── Action
```

实现：

敌人：

```
Idle

↓

Detect

↓

Chase

↓

Attack

↓

Dead
```

------

# Module 8：网络系统 ⭐⭐⭐⭐

目标：

证明你懂客户端网络。

------

结构：

```
NetworkManager


↓

Socket


↓

Protocol


↓

Handler
```

实现：

登录：

```
Client

发送

LoginRequest


Server

返回

LoginResponse
```

战斗：

```
AttackRequest

↓

AttackResult
```

------

# Module 9：热更新 ⭐⭐⭐⭐

选择：

HybridCLR

结构：

```
Main Game

+

HotUpdate Assembly
```

Demo：

修改：

技能伤害。

不用重新打包。

------

# Module 10：工具系统 ⭐⭐⭐⭐

做：

Excel配置工具。

流程：

```
Excel

↓

Editor工具

↓

Json

↓

Game
```

------

# 六、开发顺序

不要按照模块列表开发。

真实开发顺序：

------

## Phase 1（Day1-30）

基础框架

完成：

```
Framework

+
Resource

+
UI

+
Data
```

成果：

一个空壳商业客户端。

------

## Phase 2（Day31-60）

Gameplay

完成：

```
Character

Combat

Skill

Buff

AI
```

成果：

可以玩的ARPG。

------

## Phase 3（Day61-75）

商业增强

加入：

```
Network

HybridCLR

Save
```

------

## Phase 4（Day76-90）

优化包装

完成：

```
Profiler分析

性能优化

README

Demo视频

简历
```