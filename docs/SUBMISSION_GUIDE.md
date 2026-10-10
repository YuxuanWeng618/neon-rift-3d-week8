# 课程提交材料与说明

课程：AP6412 Week8。项目：Neon Rift 3D Version 2.0。成员：SUN ZIZHI、WENG YUXUAN。选择：**Change Request 01 — Survival Mode**。

## 已准备的材料

- 完整GitHub仓库：<https://github.com/YuxuanWeng618/neon-rift-3d-week8>，最终代码从 `main` 取得。
- 已填写的[双语小组计划](Neon_Rift_3D_Survival_Plan_Bilingual.docx)。
- [完成情况](COMPLETION_REPORT.md)、[类职责与调用关系](../ARCHITECTURE.md)、[代码复核](CODE_REVIEW.md)。
- [T01–T16验收记录](review/2026-10-11-complete/ACCEPTANCE.md)、[效果图与演示视频](review/2026-10-11-complete/README.md)。
- [参数试跑记录](BALANCE_REVIEW.md)和[尚待人工完成的事项](REMAINING_TASKS.md)。

课程材料要求每组通过课程入口提交完成的计划/文档及GitHub链接。上述额外记录用于说明实现与验收；是否需要另传源码ZIP或视频，以NTULearn实际提交页面为准。源码也可从GitHub页面选择 Code → Download ZIP。

## 可粘贴的英文项目说明

We extended the existing Neon Rift 3D project with Change Request 01: Survival Mode. The player must survive for 60 seconds while enemies continue spawning. Surviving until expiry wins the round; reaching zero health loses it. Kill count affects score only and cannot end the survival round. The game displays time, health, difficulty phase and results, freezes combat on completion, and fully resets on restart.

SUN ZIZHI implemented enemy spawning, safe spawn positions, time-based difficulty and the active-enemy cap. WENG YUXUAN implemented player reset and health events, survival state management, the HUD, restart, visual presentation, parameter trials and main-scene integration. The repository preserves both members' commits and PR #1.

The original game coordinator previously combined scoring and the kill-target end rule. We removed that end rule and separated survival results into SurvivalGameManager. EnemySpawner3D owns spawning; Player3D owns input and health; SurvivalHud subscribes to gameplay events and displays state. The coordinator wires actors and controls round lifecycle. The existing Enemy3D inheritance remains in use for Chaser and Striker behaviour. Sharing one elapsed-time source prevents timer and difficulty drift; shared visual constants keep presentation changes localized.

Local validation covers 83 survival checks, 272 spawner checks and 27 graphical window checks, plus a normal-speed 60-second clock run and a recorded gameplay demonstration. Scripted trials informed retaining 100 HP and movement speed 7. These results do not certify human playability or a second computer; those records and any teammate approval must be supplied separately.

Repository: https://github.com/YuxuanWeng618/neon-rift-3d-week8

## 提交人最后执行

1. 在NTULearn核对本班截止时间、指定入口和要求的文件格式。
2. 粘贴仓库链接，并上传已填写的计划和页面要求的文档；如需要，附上源码ZIP或演示。
3. 检查上传列表、最终确认并保存提交回执。
4. 把实际截止时间、提交人、时间和回执编号补入下表；没有回执时保持“未提交”。

| 课程记录 | 当前状态 |
| --- | --- |
| NTULearn入口 / 最新截止时间 | 未提供，待核对 |
| 提交人 / 提交时间 / 回执编号 | 未提供 |
| 课程入口提交 | 尚无实际提交证据 |

GitHub推送和PR合并完成代码交付；它们不会自动完成NTULearn课程提交。
