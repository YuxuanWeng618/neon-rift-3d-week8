using Godot;
using System;
using System.Linq;

// Headless integration tests using real packed scenes, without extra test packages.
public partial class EnemySpawnerTests : Node
{
    private int _checks;
    public override void _Ready() => CallDeferred(nameof(Run));
    private void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        _checks++;
    }

    public void Run()
    {
        try
        {
            var entities = new Node3D(); AddChild(entities);
            var player = new Player3D(); AddChild(player);
            player.ResetPlayer(new Vector3(8.5f, 0.5f, 5.5f));
            var spawner = new EnemySpawner3D(); AddChild(spawner);
            spawner.Configure(player, entities, 42);
            int spawned = 0, strikers = 0;
            spawner.EnemySpawned += enemy =>
            {
                spawned++;
                if (enemy is StrikerEnemy3D) strikers++;
                Vector3 p = enemy.GlobalPosition;
                Check(Math.Abs(p.X) <= 8.5f && Math.Abs(p.Z) <= 5.5f && p.Y == 0.5f, "Arena bounds");
                Check(new Vector2(p.X-player.GlobalPosition.X, p.Z-player.GlobalPosition.Z).Length() >= 4f, "Player clearance");
                foreach (Enemy3D other in entities.GetChildren().OfType<Enemy3D>())
                    if (other != enemy && !other.IsQueuedForDeletion())
                        Check(p.DistanceTo(other.GlobalPosition) >= 1.2f, "Enemy clearance");
            };
            spawner.Start();
            spawner.Tick(2.5); Check(spawned == 0, "Opening delay");
            spawner.Tick(0.5); Check(spawned == 1, "First spawn");
            spawner.Tick(16.5); Check(spawner.CurrentInterval == 3f, "Before 20 seconds");
            spawner.Tick(0.5); Check(spawner.CurrentInterval == 2f, "20-second boundary");
            spawner.Tick(19.5); Check(spawner.CurrentInterval == 2f, "Before 40 seconds");
            spawner.Tick(0.5); Check(spawner.CurrentInterval == 1f, "40-second boundary");
            for (int i=0;i<100;i++) spawner.Tick(1);
            Check(spawner.ActiveEnemyCount == 20, "Active cap reached and respected");
            Check(strikers > 0, "Time unlocks Strikers without kills");
            int atCap = spawned; spawner.Tick(30);
            Check(spawned == atCap, "No spawning at cap");
            entities.GetChildren().OfType<Enemy3D>().First().TakeDamage(1000);
            Check(spawner.ActiveEnemyCount == 19, "Destroyed enemy releases slot");
            spawner.Tick(1); Check(spawner.ActiveEnemyCount == 20, "Slot replenished");
            double elapsed = spawner.ElapsedSeconds;
            spawner.Stop(); spawner.Tick(10);
            Check(spawner.ElapsedSeconds == elapsed, "Stop freezes progress");
            spawner.Start();
            Check(spawner.ActiveEnemyCount == 0 && spawner.ElapsedSeconds == 0 && spawner.CurrentInterval == 3f, "Restart resets enemies and difficulty");
            spawner.Tick(double.NaN); spawner.Tick(-1);
            Check(spawner.ElapsedSeconds == 0, "Invalid delta ignored");
            spawner.MinimumPlayerDistance = 100;
            spawner.Tick(3); Check(spawner.ActiveEnemyCount == 0, "Unsafe arena skips spawn");
            spawner.MinimumPlayerDistance = 4;
            spawner.MaxActiveEnemies = 0;
            spawner.Tick(3); Check(spawner.ActiveEnemyCount == 0, "Zero cap disables spawning");
            spawner.MaxActiveEnemies = 20;
            player.Active = false; spawner.Tick(3);
            Check(!spawner.Running && spawner.ActiveEnemyCount == 0, "Inactive player stops spawns");

            var game = GD.Load<PackedScene>("res://Scenes3D/Main3D.tscn").Instantiate<Game3D>();
            AddChild(game);
            for (int i=0;i<10;i++)
            {
                game.Spawner.Tick(3);
                game.GetNode<Node3D>("Entities").GetChildren().OfType<Enemy3D>().First(e=>!e.IsQueuedForDeletion()).TakeDamage(1000);
            }
            Check(game.Spawner.Running && game.GetNode<Player3D>("Player").Active, "Ten kills do not end survival");
            game.StopGame(); game._PhysicsProcess(5);
            Check(!game.Spawner.Running, "Coordinator stops spawner");
            game.StartGame();
            Check(game.Spawner.Running && game.Spawner.ElapsedSeconds == 0 && game.Spawner.ActiveEnemyCount == 0, "Coordinator restart");
            game.GetNode<Player3D>("Player").TakeDamage(1000);
            Check(!game.Spawner.Running, "Defeat stops spawner");
            GD.Print($"PASS: {_checks} enemy-spawner checks");
            GetTree().Quit(0);
        }
        catch (Exception e)
        {
            GD.PushError(e.ToString());
            GetTree().Quit(1);
        }
    }
}
