# Neon Rift 3D — Week 8

AP6412 Week 8 双人协作项目。使用老师提供的 Neon Rift 3D，通过 GitHub 分支开发一个 Version 2.0 玩法。

## 当前版本

- 当前保存原始 Version 1.0 游戏；Version 2.0 模式待两位成员共同选择。
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

## Version 2.0：选择一个方向

| 模式 | 必做功能 |
| --- | --- |
| Survival Mode | 持续刷敌；存活 60 秒获胜；提前死亡失败；击杀数不再提前结束本模式 |
| Speed Challenge | 开局计时；达到目标击杀数停止计时；显示完成时间 |
| Heavy Enemy Mode | 重装敌人更慢、血量明显更多、伤害更高，实际体验明显不同 |
| Ranged Arena | 禁用玩家射击；远程敌人成为主要威胁并更频繁发射弹丸；躲避并存活指定时长 |

先实现最小可玩版本，再增加可选扩展。

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

## 两人协作

| 成员 | 建议分支 | 开始开发前要确定 |
| --- | --- | --- |
| 成员 A | `feature/member-a` | 姓名、主要功能、主责文件、验收结果 |
| 成员 B | `feature/member-b` | 姓名、主要功能、主责文件、验收结果 |

1. 共同选择模式并填写课程计划表。
2. 从 `main` 各自建立功能分支，尽量先修改不同文件。
3. 各自提交并推送代码。
4. 创建 Pull Request，由另一人检查修改。
5. 合并到 `main`，拉取最新代码并运行测试。
6. 共同修改 `Game3D.cs` 时，先约定负责的方法或接口，再安排合并。

分支、提交和合并记录来自实际开发过程。

## 验收清单

- [ ] 两台电脑都能导入、构建并运行。
- [ ] 所选模式全部必做功能生效。
- [ ] 正常游戏、死亡和胜利条件符合所选模式。
- [ ] 两人的功能合并后都能运行。
- [ ] 说明修改了哪些类、各自职责和修改原因。
- [ ] 完成项目计划文档。
- [ ] 保留清楚的分支、提交和合并记录。

## 课程原始材料

- [Version 2.0 修改要求](docs/course-materials/Neon_Rift_3D_Version_2_Change_Requests.docx)
- [双人项目计划表](<docs/course-materials/Week8_Group_Project_Ideas&Planning without hints.docx>)
- [Week 8 项目与提交说明](docs/course-materials/Week8_Game_Context_Slides.pdf)

每组通过课程入口提交一份材料：完成的项目计划/文档和 GitHub 仓库链接。仓库包含最终代码和实际协作历史。截止时间以 NTULearn 公告为准。
