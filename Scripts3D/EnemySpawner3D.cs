#nullable enable
using Godot;
using System;
using System.Collections.Generic;

// Member A: owns enemy creation and survival difficulty, not the win timer or HUD.
// Game3D calls Tick once per physics frame; this node does not tick itself.
public partial class EnemySpawner3D : Node
{
    [Export] public int MaxActiveEnemies { get; set; } = 20;
    [Export] public float MinimumPlayerDistance { get; set; } = 4f;
    [Export] public float MinimumEnemyDistance { get; set; } = 1.2f;

    public event Action<Enemy3D>? EnemySpawned;
    public bool Running { get; private set; }
    public double ElapsedSeconds { get; private set; }
    public float CurrentInterval => ElapsedSeconds < 20 ? 3f : ElapsedSeconds < 40 ? 2f : 1f;
    public int ActiveEnemyCount { get { PruneEnemies(); return _enemies.Count; } }

    private const float MaxX = 8.5f;
    private const float MaxZ = 5.5f;
    private readonly RandomNumberGenerator _rng = new();
    private readonly List<Enemy3D> _enemies = new();
    private Player3D _player = null!;
    private Node3D _entities = null!;
    private PackedScene _chaserScene = null!;
    private PackedScene _strikerScene = null!;
    private double _untilNextSpawn;

    public void Configure(Player3D player, Node3D entities, ulong randomSeed = 0)
    {
        Stop();
        ClearEnemies();
        _player = player;
        _entities = entities;
        _chaserScene = GD.Load<PackedScene>("res://Scenes3D/ChaserEnemy3D.tscn");
        _strikerScene = GD.Load<PackedScene>("res://Scenes3D/StrikerEnemy3D.tscn");
        if (randomSeed == 0) _rng.Randomize();
        else _rng.Seed = randomSeed;
    }

    // Start a fresh round, including removing enemies owned by the previous round.
    public void Start()
    {
        if (!IsInstanceValid(_player) || !IsInstanceValid(_entities))
            throw new InvalidOperationException("Configure the spawner before starting a round.");
        ClearEnemies();
        ElapsedSeconds = 0;
        _untilNextSpawn = CurrentInterval;
        Running = true;
    }

    public void Stop() => Running = false;

    public void Tick(double delta)
    {
        if (!Running) return;
        if (!IsInstanceValid(_player) || !_player.Active || !IsInstanceValid(_entities))
        {
            Stop();
            return;
        }
        if (!double.IsFinite(delta) || delta <= 0) return;

        float previousInterval = CurrentInterval;
        ElapsedSeconds += delta;
        _untilNextSpawn -= delta;
        if (CurrentInterval < previousInterval)
            _untilNextSpawn = Math.Min(_untilNextSpawn, CurrentInterval);
        if (_untilNextSpawn > 0) return;

        // At most one enemy per tick. A full/blocked arena never accumulates a burst.
        _untilNextSpawn = CurrentInterval;
        if (ActiveEnemyCount >= Math.Max(0, MaxActiveEnemies)) return;
        if (!TryFindSpawnPosition(out Vector3 position)) return;

        // Unlock ranged enemies by elapsed time, so zero kills cannot stall progression.
        bool spawnStriker = ElapsedSeconds >= 20 && _rng.Randf() < (ElapsedSeconds < 40 ? 0.35f : 0.5f);
        Enemy3D enemy = (spawnStriker ? _strikerScene : _chaserScene).Instantiate<Enemy3D>();
        _entities.AddChild(enemy);
        enemy.GlobalPosition = position;
        enemy.Configure(_player);
        _enemies.Add(enemy);
        enemy.Destroyed += _ => _enemies.Remove(enemy);
        EnemySpawned?.Invoke(enemy);
    }

    private bool TryFindSpawnPosition(out Vector3 position)
    {
        for (int attempt = 0; attempt < 32; attempt++)
        {
            position = EdgePosition(_rng.RandiRange(0, 3), _rng.RandfRange(-1f, 1f));
            if (IsSafe(position)) return true;
        }
        // Bounded fallback search: skip this spawn if every tested edge location is unsafe.
        for (int side = 0; side < 4; side++)
            for (int sample = 0; sample <= 16; sample++)
            {
                position = EdgePosition(side, -1f + sample / 8f);
                if (IsSafe(position)) return true;
            }
        position = Vector3.Zero;
        return false;
    }

    private static Vector3 EdgePosition(int side, float offset) => side switch
    {
        0 => new Vector3(offset * MaxX, 0.5f, -MaxZ),
        1 => new Vector3(MaxX, 0.5f, offset * MaxZ),
        2 => new Vector3(offset * MaxX, 0.5f, MaxZ),
        _ => new Vector3(-MaxX, 0.5f, offset * MaxZ)
    };

    private bool IsSafe(Vector3 position)
    {
        if (HorizontalDistance(position, _player.GlobalPosition) < Math.Max(0, MinimumPlayerDistance))
            return false;
        foreach (Enemy3D enemy in _enemies)
            if (HorizontalDistance(position, enemy.GlobalPosition) < Math.Max(0, MinimumEnemyDistance))
                return false;
        return true;
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
        => new Vector2(a.X - b.X, a.Z - b.Z).Length();

    private void PruneEnemies()
        => _enemies.RemoveAll(enemy => !IsInstanceValid(enemy) || enemy.IsQueuedForDeletion() || !enemy.IsInsideTree());

    private void ClearEnemies()
    {
        foreach (Enemy3D enemy in _enemies)
        {
            if (!IsInstanceValid(enemy)) continue;
            // Detach immediately so a same-frame restart cannot leave old enemies active.
            enemy.GetParent()?.RemoveChild(enemy);
            if (!enemy.IsQueuedForDeletion()) enemy.QueueFree();
        }
        _enemies.Clear();
    }
}
