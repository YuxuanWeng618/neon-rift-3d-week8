# Neon Rift 3D — Architecture and Call Flow

This is a **3D** Godot 4 C# project. `Node3D`, `CharacterBody3D`, `Camera3D`,
`PackedScene`, input handling, collision functions, and the engine lifecycle are
**Godot-provided APIs**. The classes in `Scripts3D/` are **our own C# code**.

## Main flow

```text
Godot loads Scenes3D/Main3D.tscn
  -> Game3D._Ready()                [Godot lifecycle callback]
     -> load reusable enemy/bullet scenes via GD.Load() [Godot]
     -> connect C# gameplay signals; StartGame() [Our logic]
     -> initialize SurvivalGameManager and bind SurvivalHud before starting
Each Godot physics tick:
  -> Game3D._PhysicsProcess()       -> EnemySpawner3D.Tick(delta) [Our spawn rules]
  -> Player3D._PhysicsProcess()     -> WASD, motion, mouse aim, request shot
  -> Enemy3D._PhysicsProcess()      -> select behaviour, move, contact attack
     -> ChaserEnemy3D.UpdateBehaviour() / StrikerEnemy3D.UpdateBehaviour()
  -> Bullet3D._PhysicsProcess()     -> move, collide, damage, expire
  -> SurvivalGameManager._PhysicsProcess() [priority 100, after combat]
     -> read Spawner.ElapsedSeconds, check death/expiry, update HUD events
```

## Responsibilities and signals

| Our class | Responsibility | Godot base class |
| --- | --- | --- |
| Game3D | Game start/stop, actor wiring, score, entity freezing/cleanup | Node3D |
| EnemySpawner3D | Time-based spawns, safe edge positions, active cap and reset | Node |
| SurvivalGameManager | Round state, 60-second rule, single result, restart request | Node |
| SurvivalHud | Native HUD controls, event-driven display and restart button | CanvasLayer |
| SurvivalVisualStyle | Shared colors, fonts, warning thresholds and lighting | Static C# class |
| ArenaPresentation3D | Bright arena materials, checker floor and lighting; original collisions retained | Node3D |
| Player3D | Movement, aim, fire cooldown, HP | CharacterBody3D |
| Enemy3D | Shared enemy health, contact damage, signals | CharacterBody3D |
| ChaserEnemy3D | Chasing movement and Chaser stats | Our Enemy3D |
| StrikerEnemy3D | Ranged-attack AI and Striker stats | Our Enemy3D |
| Bullet3D | Bullet lifetime, movement, collision/damage | CharacterBody3D |

- `Player3D.ShotRequested` -> `Game3D.SpawnPlayerBullet()`
- `Player3D.Died` -> `Game3D.EndWithDefeat()` -> manager single loss result
- `Player3D.HealthChanged` -> HUD health label and bar
- `Game3D.RoundStarted` -> manager running state -> HUD reset
- `Game3D.ScoreChanged` -> HUD kills/score
- manager time/state events -> HUD countdown and result panel
- HUD restart button -> manager deferred restart -> `Game3D.StartGame()`
- `Enemy3D.ShotRequested` -> `Game3D.SpawnEnemyBullet()`
- `Enemy3D.Destroyed` -> `Game3D.OnEnemyDestroyed()`

Godot's `[Signal]` and `EmitSignal()` mechanism is engine-provided; **the events,
their meaning, and the handlers are designed in our code**. The HUD uses native
Godot controls and Godot player signals; manager/coordinator notifications use
C# events. No dash mechanic is introduced by this survival extension.

Member A's spawn integration is retained. Member B's merged survival version adds
the 60-second result, HUD and restart. Kills never trigger victory.
See [Member A changes and integration contract](docs/MEMBER_A_CHANGES.md).

## Timing, finish and restart

`Game3D` advances the spawner exactly once each physics tick. Its elapsed time
drives both difficulty and survival time; the manager and HUD only read it.
The manager checks after ordinary priority-zero combat nodes. A lethal hit in
the same discrete physics update as expiry is resolved as a loss. A result can
only transition from Running, so subsequent events cannot overwrite it.

`StopGame()` disables actor processing on Entities, stops the spawner and
deactivates the player. HUD remains outside Entities and can receive clicks.
`StartGame()` detaches old entities, restores entity processing, resets player
position/HP/aim/cooldowns, resets score and spawner, then emits RoundStarted.
Restart requests are deferred and guarded against repeated clicks. The player
waits for a held fire input to be released before firing in a new round.

Presentation follows [the visual specification](docs/VISUAL_STYLE.md). HP at or
below 30% displays LOW HEALTH; remaining time at or below 10s displays FINAL
SECONDS. Both captions reset with the same start events. HUD, arena and player
share SurvivalVisualStyle values; these do not affect combat rules.

See [current review images, movie and validation](docs/review/2026-10-11-complete/README.md). Test scenes
are isolated fixtures; production Main3D does not load them.

## Collision configuration

| Layer number | Layer bit mask | Object |
| ---: | ---: | --- |
| 1 | 1 | Player |
| 2 | 2 | Enemy |
| 3 | 4 | Projectiles |
| 4 | 8 | Arena / walls |

**Important:** Godot's inspector **layer numbers** are 1, 2, 3, 4;
C# collision mask **bit values** are 1, 2, 4, 8, respectively.
The `.tscn` scenes set the bodies' collision layers. `Bullet3D.Configure()`
sets each bullet's mask dynamically to hit the world plus the opposing team.

## Terminology note

The coordinate system is 3D, with movement restricted to the **XZ plane**;
Y is vertical. `Vector3` and `CharacterBody3D` are appropriate here.
**MD5 is a cryptographic hash algorithm, not a Godot 3D movement/model class.**
There is no MD5 reference in this project; it should not be confused with 3D,
`Node3D`, or `CharacterBody3D`.
