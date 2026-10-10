# 第二台电脑验证步骤

目前已有本机验证证据。本指南让另一位组员能够从 GitHub 取得项目并复验；填写结果前必须在该电脑实际运行。

## 环境与取得项目

安装 Git、.NET 8 SDK、与项目 `Godot.NET.Sdk/4.7.2` 兼容的 Godot 4.7.2 **.NET 版**。普通版 Godot 无法运行 C#。仓库不包含引擎或编译缓存。

```powershell
git clone https://github.com/YuxuanWeng618/neon-rift-3d-week8.git
Set-Location neon-rift-3d-week8
git rev-parse HEAD
dotnet --version
# 将这个示例路径替换为本机实际的 .NET 版 Godot 路径。
$taskGodot = 'C:\Tools\Godot\Godot_console.exe'
& $taskGodot --version
.\Run-Local.ps1 -GodotPath $taskGodot -VerifyOnly -IncludeWindowTests
.\Run-Local.ps1 -GodotPath $taskGodot
```

脚本先构建 C#、导入资源，再运行游戏或检查。无窗口检查包含83项生存和272项刷怪检查；`-IncludeWindowTests` 会打开图形窗口，增加27项窗口检查。自动检查必须出现 PASS，且脚本正常结束。脚本按 `-GodotPath`、环境变量 `GODOT_DOTNET_PATH`、PATH 中 `godot`/`godot4` 的顺序寻找引擎，不依赖开发者的盘符和目录。

若系统阻止执行 PS1，可在 Godot .NET 编辑器导入 `project.godot`，构建后按 F6 分别运行两个测试场景和图形测试场景，再按 F5 运行正式游戏。无需更改系统的执行策略。

## 人工操作与记录

1. 开局显示60秒、100HP。WASD移动，鼠标瞄准，按住左键射击。
2. 确認击杀只计分；20秒和40秒时刷怪阶段变化。记录实际存活时间、死亡原因、瞄准和躲避体验。
3. 尝试存活60秒，检查胜利界面；另开一局检查提前死亡的失败界面。
4. 两种结果界面分别点击 PLAY AGAIN，确认时间、生命值、分数、敌人和难度全部复位。
5. 调整窗口大小，检查HUD、瞄准和重启按钮；完成后把实际结果记录到下表。

| 记录项目 | 实际结果（待另一台电脑执行后填写） |
| --- | --- |
| 验证人 / 日期 | 待填写 |
| 系统 / .NET SDK / Godot版本 | 待填写 |
| main提交哈希 | 待填写 |
| 构建、导入和自动检查结果 | 待填写 |
| 60秒生存、提前死亡、两种重启 | 待填写 |
| 人工难度、操作和画面评价 | 待填写 |
| 是否需要调整HP或速度 / 理由 | 待填写 |

同一台电脑重新克隆项目属于“干净检出验证”，不算第二台电脑验收。人工体验也不能由脚本巡逻和自动瞄准的结果替代。
