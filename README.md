# Neon Rift 3D — Week 8

AP6412 Week 8 双人协作项目。SUN ZIZHI 与 WENG YUXUAN 使用老师提供的 Neon Rift 3D，通过 GitHub 分支开发 Version 2.0 的 60 秒生存模式。

## 当前版本

- 成员 A（SUN ZIZHI）的敌人/刷怪部分已实现：持续刷怪、时间驱动难度、安全生成位置、20 个敌人上限和启停/重置接口。详见 [成员 A 修改说明](docs/MEMBER_A_CHANGES.md)。
- 成员B已接入60秒胜负、HUD、重启、低HP与最后10秒提示，完成本机验收和实际游戏视频。玩家参数保留原版，人工难度体验仍待试玩。
- 本次开发使用 `feature/survival-ui-timer`，以成员A的 `8458863` 为基线，通过功能分支与PR交付。[完整审阅包](docs/review/2026-10-11-complete/README.md) 包含视频、11张效果图、382项检查与T01–T16记录。
- 当前视觉按 [明亮街机规范](docs/VISUAL_STYLE.md) 制作，参数决策见 [平衡记录](docs/BALANCE_REVIEW.md)。
- 功能分支已推送，已创建[草稿PR #1](https://github.com/YuxuanWeng618/neon-rift-3d-week8/pull/1)。尚未完成事项见[复核后的待办清单](docs/REMAINING_TASKS.md)。
- 正式双语项目计划见 [Neon Rift 3D Survival Plan](docs/Neon_Rift_3D_Survival_Plan_Bilingual.docx)。
- `starter-v1.0` 标签保存原始项目导入提交。
- 原始说明见 [docs/STARTER_README.md](docs/STARTER_README.md)。
- 游戏架构见 [ARCHITECTURE.md](ARCHITECTURE.md)。

## 运行

原材料指定 `Godot.NET.Sdk/4.7.2`，桌面目标框架为 `net8.0`。使用与项目 SDK 兼容的 Godot 4 .NET 编辑器和 .NET SDK。

1. 将仓库克隆到本机。
2. 在 Godot 项目管理器导入根目录的 `project.godot`。
3. 构建 C# 项目。
4. 运行 `Scenes3D/Main3D.tscn`。
5. W/A/S/D 移动，鼠标瞄准，按住鼠标左键射击。

击杀只计分，不会提前结束生存模式。存活 60 秒获胜，提前死亡失败；结束后停止时间、刷怪和现有敌人/弹丸更新，点击 `PLAY AGAIN` 开始新一局。HUD 显示时间、生命值、阶段、击杀和得分。

本机可运行根目录的 `Run-Local.ps1` 启动本地版本；如 Godot 未加入 PATH，使用 `-GodotPath` 指定 .NET 版 Godot 可执行文件。

`NeonRiftStage1` 是原材料的程序集名称。

## Version 2.0：Survival Mode 项目计划

玩家在敌人持续生成的情况下生存 60 秒。倒计时结束时仍然存活则获胜；此前生命值归零则失败。生存模式中，击杀数量不能提前结束游戏。

| 部分 | 计划行为 |
| --- | --- |
| 敌人生成 | 持续刷怪；选择远离玩家的生成位置；逐步缩短刷怪间隔；限制同时存在的敌人数量 |
| 玩家与状态 | 保留移动和战斗；管理开始、进行、胜利和失败状态；调整速度和生命值以平衡难度 |
| 计时与界面 | 显示 60 秒倒计时、生命值和胜负信息 |
| 游戏结束 | 停止计时和生成新敌人 |
| 重新开始 | 重置计时、玩家状态、敌人及生成器状态、界面和难度进度 |

先完成可运行的 60 秒生存循环，再整合逐渐提高刷怪频率、敌人数量上限、HUD 和重启功能。新增文件名可随实际项目结构调整。

## 文件职责

| 文件 | 主要职责 |
| --- | --- |
| `Scripts3D/Game3D.cs` | 游戏启停、调用生成器、连接事件、统计击杀和得分、停止及清理实体 |
| `Scripts3D/EnemySpawner3D.cs` | 持续刷怪、时间难度、安全位置、敌人上限及重置 |
| `Scripts3D/SurvivalGameManager.cs` | 生存规则、胜负、共享时间读取、重启入口 |
| `Scripts3D/SurvivalHud.cs` | 原生 Godot 控件、状态与生命值显示、重启请求 |
| `Scripts3D/SurvivalVisualStyle.cs` | 统一视觉颜色、字体、警告阈值和光照参数 |
| `Scripts3D/Player3D.cs` | 玩家移动、瞄准、射击、血量及变化信号、完整复位 |
| `Scripts3D/ArenaPresentation3D.cs` | 明亮竞技场材质、棋盘地面、边缘标记与光照；不改变碰撞和游戏规则 |
| `Scripts3D/ChaserEnemy3D.cs` | 追击敌人属性与行为 |
| `Scripts3D/StrikerEnemy3D.cs` | 远程敌人属性与射击行为 |
| `Scripts3D/Enemy3D.cs` | 敌人共同能力与伤害处理 |
| `Scripts3D/Bullet3D.cs` | 弹丸移动与碰撞 |
| `Scenes3D/` | 场景、碰撞形状、网格和材质 |

`SurvivalGameManager` 读取生成器的 `ElapsedSeconds`，不自行推进时间。HUD 场景的原生控件由 HUD 脚本集中构建；详细接口和运行检查见本地审阅说明。

## 本机验证与演示

本轮C#构建0警告、0错误；生存集成83项、成员A回归272项、图形窗口27项通过。已运行正常速度的60秒计时与暂停检查、四组平衡测量，并保存完整生存、自然死亡和两次按钮重启的MP4。

详见 [T01–T16验收](docs/review/2026-10-11-complete/ACCEPTANCE.md) 和 [演示视频](docs/review/2026-10-11-complete/survival-demo.mp4)。测试与录制场景仅用于开发验证，正式主场景不加载这些脚本。

## 两人协作

| 成员 | 建议分支 | 主要任务与文件 |
| --- | --- | --- |
| SUN ZIZHI（成员 A） | `feature/enemy-spawning` | 敌人、持续刷怪、生成位置、难度增长和敌人上限；`StrikerEnemy3D.cs`，计划新增 `EnemySpawner3D.cs`，按需调整相关场景 |
| WENG YUXUAN（成员 B） | `feature/survival-ui-timer` | 玩家、60 秒计时、游戏状态、HUD、重启、平衡及主场景集成；`Player3D.cs`，计划新增 `SurvivalGameManager.cs`，HUD/UI 文件 |

1. 按双语项目计划开发；编码前共同约定开始和结束事件接口。
2. 从 `main` 各自建立功能分支，尽量先修改不同文件。
3. 各自提交并推送代码。
4. 创建 Pull Request，由另一人检查修改。
5. 合并到 `main`，拉取最新代码并运行测试。
6. 刷怪参数由生成器管理，玩家参数由玩家脚本管理；主场景由 WENG YUXUAN 集成。双方修改主场景、`Game3D.cs`、README、游戏状态事件或难度接口前先协调。

分支、提交和合并记录来自实际开发过程。

## 验收清单

- [ ] 两台电脑都能导入、构建并运行。
- [x] 本机初始倒计时为 60 秒，生命值与预定玩家配置一致。
- [x] 本机检查刷怪位置、阶段间隔与数量上限通过。
- [x] 本机移动、战斗、受伤和 UI 更新检查通过。
- [x] 存活 60 秒获胜；此前生命值归零失败。
- [x] 未到 60 秒时，原版目标击杀数不会触发生存模式胜利。
- [x] 胜利或失败后停止计时和生成新敌人。
- [x] 重新开始后所有状态与难度进度正确重置。
- [x] 成员A与成员B功能已在同一场景完成本机集成验证。
- [x] 功能分支已提交、推送并创建草稿PR。
- [ ] 人工试玩与难度评价已记录。
- [ ] 队友完成正式代码审查。
- [ ] 审查通过后合入main，双方取得合并版本并复验。
- [x] 说明修改了哪些类、各自职责和修改原因。
- [x] 项目计划文档已上传。
- [x] 已保留成员A和成员B的分支、提交及草稿PR记录。
- [ ] 主分支合并记录与课程提交回执已保存。

## 课程原始材料

- [本组双语生存模式项目计划](docs/Neon_Rift_3D_Survival_Plan_Bilingual.docx)
- [Version 2.0 修改要求](docs/course-materials/Neon_Rift_3D_Version_2_Change_Requests.docx)
- [双人项目计划表](<docs/course-materials/Week8_Group_Project_Ideas&Planning without hints.docx>)
- [Week 8 项目与提交说明](docs/course-materials/Week8_Game_Context_Slides.pdf)

每组通过课程入口提交一份材料：完成的项目计划/文档和 GitHub 仓库链接。仓库包含最终代码和实际协作历史。截止时间以 NTULearn 公告为准。
