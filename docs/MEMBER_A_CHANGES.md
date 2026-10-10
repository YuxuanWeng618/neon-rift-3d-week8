# Member A implementation — SUN ZIZHI

Branch: `feature/dy992`

## 已完成的功能 / Implemented features

- 持续刷怪：开局 3 秒后出现第一个敌人；0–20 秒间隔 3 秒，20–40 秒间隔 2 秒，40 秒以后间隔 1 秒。
  Continuous spawning: first enemy after 3 seconds; intervals are 3 / 2 / 1 seconds across the three difficulty stages.
- 20 秒前只生成 Chaser；20 秒后每次有 35% 概率生成 Striker，40 秒后为 50%。推进依赖生存时间，不依赖击杀数。
  Chasers only initially; Striker chance becomes 35% at 20 seconds and 50% at 40 seconds, independent of kills.
- 敌人从竞技场边缘生成，距玩家至少 4 个单位，距现存敌人至少 1.2 个单位；找不到安全位置时跳过本次生成。
  Edge spawns stay at least 4 units from the player and 1.2 units from other tracked enemies; unsafe spawns are skipped.
- 活跃敌人上限默认 20；死亡或离开场景后释放名额。达到上限期间不会积累刷怪请求。
  Default active-enemy cap is 20. Death/removal releases slots, and blocked spawns do not build up a backlog.
- 游戏结束可停止刷怪；重启会清除旧敌人并重置刷怪计时、难度和数量。
  Stop and restart hooks stop spawning and reset owned enemies, timing and difficulty.
- 原始的 10 杀胜利已移除，击杀仅增加分数；玩家死亡会停止刷怪。
  Ten kills no longer end survival; kills add score, and player defeat stops spawning.

## Files and responsibilities

| File | Change |
| --- | --- |
| `Scripts3D/EnemySpawner3D.cs` | New component owns enemy scenes, progression, safe locations, active count and lifecycle. |
| `Scripts3D/Game3D.cs` | Delegates spawning, preserves scoring/projectile wiring, exposes start/stop integration hooks, removes kill-based victory. |
| `Tests/EnemySpawnerTests.cs` and `.tscn` | Headless integration tests using actual game and enemy scenes. |
| `README.md`, `ARCHITECTURE.md` | Update current implementation status and class ownership. |

The existing Chaser/Striker movement, attack rates, damage and health are reused. Player and scene files are unchanged.

## Handoff to WENG YUXUAN

成员 B 仍需实现 60 秒胜利判定、HUD、重启按钮及玩家平衡。这次提交只完成成员 A；目前游戏不会在 60 秒自动获胜。

Member B still owns the 60-second victory condition, HUD, restart UI and player balancing. This branch does **not** automatically win at 60 seconds.

- `Game3D.Spawner` exposes `ElapsedSeconds`, `CurrentInterval` and `ActiveEnemyCount` for display/diagnostics.
- `Game3D` currently calls `Spawner.Tick(delta)` once each physics frame. Do not also tick the spawner from another script. A paused tree naturally pauses progression.
- On victory, call `Game3D.StopGame()` to stop spawning and deactivate the player. The existing defeat handler uses this same path.
- On restart, call `Game3D.StartGame()` to clear entities, reset the player and score, and restart the spawner. Reset Member B's timer/HUD alongside it.
- If the coordinator is replaced, use `Configure(player, entities)`, subscribe to `EnemySpawned` to connect score and projectile callbacks, call `Start()`, then `Tick(delta)` once per frame. Use `Stop()` at game end.
- `Stop()` only stops spawning; `Game3D.StopGame()` also deactivates the player, which existing enemy AI observes.
- Optional nonzero `randomSeed` in `Configure` allows reproducible spawn tests. Normal gameplay randomizes the generator.

## Validation commands

Validation on 2026-10-10: `dotnet build --no-restore` succeeded with zero errors;
the headless integration scene passed 272 checks. Existing nullable warnings in
`Player3D.cs`/`Bullet3D.cs` and sandbox certificate/NuGet audit-access warnings
were observed; they did not prevent compilation or execution of the tests.

With Godot 4.7.2 .NET and .NET 8 targeting support installed:

```text
dotnet build
godot --headless --editor --path . --import --quit
godot --headless --path . res://Tests/EnemySpawnerTests.tscn
```

Use the local Godot executable name in place of `godot`. Tests cover stage boundaries, spawning with zero kills, arena bounds, player/enemy clearance, cap and slot release, stop/reset, blocked locations, zero cap, inactive player, ten-kill continuation and coordinator restart/defeat.
