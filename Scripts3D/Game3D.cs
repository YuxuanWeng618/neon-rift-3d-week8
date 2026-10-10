// Game3D connects actors, score and game state. EnemySpawner3D owns spawn rules.
// Member B connects the survival timer/HUD and calls StopGame on victory.
using Godot;
using System;

public partial class Game3D : Node3D
{
    public EnemySpawner3D Spawner { get; private set; } = null!;
    public bool IsRunning => _running;
    public int Kills => _kills;
    public int Score => _score;
    public event Action RoundStarted;
    public event Action<int, int> ScoreChanged;
    private SurvivalGameManager _survival = null!;
    private PackedScene _bulletScene = null!;
    private Node3D _entities = null!;
    private Player3D _player = null!;
    private int _kills;
    private int _score;
    private bool _running;

    public override void _Ready()
    {
        Engine.TimeScale = 1.0;
        _bulletScene = GD.Load<PackedScene>("res://Scenes3D/Bullet3D.tscn");
        _entities = GetNode<Node3D>("Entities");
        _player = GetNode<Player3D>("Player");
        Spawner = new EnemySpawner3D { Name = "EnemySpawner" };
        AddChild(Spawner);
        Spawner.Configure(_player, _entities);
        Spawner.EnemySpawned += ConnectEnemy;
        _player.ShotRequested += SpawnPlayerBullet;
        _player.Died += EndWithDefeat;
        _survival = GetNode<SurvivalGameManager>("SurvivalGameManager");
        _survival.Initialize(this, _player);
        GetNode<SurvivalHud>("SurvivalHud").Bind(this, _survival, _player);
        StartGame();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_running) Spawner.Tick(delta);
    }

    public void StartGame()
    {
        Spawner.Stop();
        ClearEntities();
        _entities.ProcessMode = ProcessModeEnum.Inherit;
        _kills = 0;
        _score = 0;
        _player.ResetPlayer(new Vector3(0f, 0.5f, 0f));
        Spawner.Start();
        _running = true;
        ScoreChanged?.Invoke(_kills, _score);
        RoundStarted?.Invoke();
    }

    // Shared end hook for Member B's victory timer and the existing defeat path.
    public void StopGame()
    {
        _running = false;
        Spawner.Stop();
        _player.Active = false;
        _player.Velocity = Vector3.Zero;
        _entities.ProcessMode = ProcessModeEnum.Disabled;
    }

    private void ConnectEnemy(Enemy3D enemy)
    {
        enemy.Destroyed += OnEnemyDestroyed;
        enemy.ShotRequested += SpawnEnemyBullet;
    }

    private void SpawnPlayerBullet(Vector3 position, Vector3 direction)
        => SpawnBullet(position, direction, false, 14f, 24f);

    private void SpawnEnemyBullet(Vector3 position, Vector3 direction)
        => SpawnBullet(position, direction, true, 8f, 11f);

    private void SpawnBullet(Vector3 position, Vector3 direction, bool hostile, float speed, float damage)
    {
        if (!_running) return;
        Bullet3D bullet = _bulletScene.Instantiate<Bullet3D>();
        _entities.AddChild(bullet);
        bullet.GlobalPosition = position;
        bullet.Configure(direction, speed, damage, hostile);
    }

    private void OnEnemyDestroyed(int points)
    {
        if (!_running) return;
        _kills++;
        _score += points;
        ScoreChanged?.Invoke(_kills, _score);
        // Survival kills affect score only; the 60-second win condition belongs to Member B.
        GD.Print($"Kills: {_kills} | Score: {_score}");
    }

    private void EndWithDefeat()
    {
        if (!_running) return;
        _survival.FinishRound(SurvivalState.Lost);
    }

    private void ClearEntities()
    {
        foreach (Node child in _entities.GetChildren())
        {
            _entities.RemoveChild(child);
            child.QueueFree();
        }
    }
}
