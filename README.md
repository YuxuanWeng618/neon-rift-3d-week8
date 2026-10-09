# Neon Rift 3D — Week 8

AP6412 Week 8 双人协作项目。SUN ZIZHI 与 WENG YUXUAN 使用老师提供的 Neon Rift 3D，通过 GitHub 分支开发 Version 2.0 的 60 秒生存模式。

## 当前版本

- 已确定 Version 2.0 方向为 Survival Mode；当前游戏代码仍是原始 Version 1.0，以下生存模式功能属于待实现计划。
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

原版击败 10 个敌人获胜，血量归零失败。击杀、得分与胜负信息显示在 Godot Output 面板。

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
| `Scripts3D/Game3D.cs` | 开始游戏、生成敌人、统计击杀和得分、胜负判断 |
| `Scripts3D/Player3D.cs` | 玩家移动、瞄准、射击、血量 |
| `Scripts3D/ChaserEnemy3D.cs` | 追击敌人属性与行为 |
| `Scripts3D/StrikerEnemy3D.cs` | 远程敌人属性与射击行为 |
| `Scripts3D/Enemy3D.cs` | 敌人共同能力与伤害处理 |
| `Scripts3D/Bullet3D.cs` | 弹丸移动与碰撞 |
| `Scenes3D/` | 场景、碰撞形状、网格和材质 |

计划新增 `Scripts3D/EnemySpawner3D.cs` 和 `Scripts3D/SurvivalGameManager.cs`。HUD 脚本和 UI 场景优先复用已有可用文件，具体名称在实现时确定。

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
- [ ] 初始倒计时为 60 秒，生命值与预定玩家配置一致。
- [ ] 刷怪位置远离玩家，刷怪间隔随难度变化，活跃敌人数量不超过预定上限。
- [ ] 移动、战斗、受伤和 UI 更新正常。
- [ ] 存活 60 秒获胜；此前生命值归零失败。
- [ ] 未到 60 秒时，原版目标击杀数不会触发生存模式胜利。
- [ ] 胜利或失败后停止计时和生成新敌人。
- [ ] 重新开始后所有状态与难度进度正确重置。
- [ ] 两人的功能合并后都能运行。
- [ ] 说明修改了哪些类、各自职责和修改原因。
- [x] 项目计划文档已上传。
- [ ] 保留清楚的分支、提交和合并记录。

## 课程原始材料

- [本组双语生存模式项目计划](docs/Neon_Rift_3D_Survival_Plan_Bilingual.docx)
- [Version 2.0 修改要求](docs/course-materials/Neon_Rift_3D_Version_2_Change_Requests.docx)
- [双人项目计划表](<docs/course-materials/Week8_Group_Project_Ideas&Planning without hints.docx>)
- [Week 8 项目与提交说明](docs/course-materials/Week8_Game_Context_Slides.pdf)

每组通过课程入口提交一份材料：完成的项目计划/文档和 GitHub 仓库链接。仓库包含最终代码和实际协作历史。截止时间以 NTULearn 公告为准。
