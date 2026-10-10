#nullable enable
using Godot;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;

// Independent measurement fixture; candidate stats never modify the main scene.
public partial class SurvivalBalanceTrial : Node
{
    private Game3D _game = null!;
    private Player3D _player = null!;
    private SurvivalGameManager _manager = null!;
    private readonly Stopwatch _wall = new();
    private string _mode = "patrol-fire", _output = "";
    private int _waypoint, _damageEvents;
    private float _previousHealth, _damageTaken;

    public override void _Ready() => CallDeferred(nameof(Begin));
    private void Begin()
    {
        try
        {
            ProcessPhysicsPriority = -100;
            var args = OS.GetCmdlineUserArgs();
            _mode = args.Length > 0 ? args[0] : "patrol-fire";
            float speed = args.Length > 1 ? float.Parse(args[1], CultureInfo.InvariantCulture) : 7;
            float health = args.Length > 2 ? float.Parse(args[2], CultureInfo.InvariantCulture) : 100;
            _output = args.Length > 3 ? args[3] : "";
            if (_mode != "patrol-fire" && _mode != "patrol-dodge" && _mode != "stationary") throw new Exception("Unknown trial mode.");
            _game = GD.Load<PackedScene>("res://Scenes3D/Main3D.tscn").Instantiate<Game3D>(); AddChild(_game);
            _player = _game.GetNode<Player3D>("Player"); _manager = _game.GetNode<SurvivalGameManager>("SurvivalGameManager");
            _player.MoveSpeed = speed; _player.MaxHealth = health;
            _game.Spawner.Configure(_player, _game.GetNode<Node3D>("Entities"), 618); _game.StartGame();
            _previousHealth = _player.Health;
            _player.HealthChanged += (current, _) =>
            {
                if (current < _previousHealth) { _damageTaken += _previousHealth - current; _damageEvents++; }
                _previousHealth = current;
            };
            _manager.StateChanged += Finish;
            _wall.Start();
        }
        catch (Exception exception) { GD.PushError(exception.ToString()); GetTree().Quit(1); }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_manager == null || _manager.State != SurvivalState.Running) return;
        if (_wall.Elapsed.TotalSeconds > 80) { SurvivalTestInputDriver.Release(); GD.PushError("Balance watchdog expired."); GetTree().Quit(1); return; }
        if (_mode == "stationary") SurvivalTestInputDriver.Release();
        else SurvivalTestInputDriver.PatrolAndAim(_game, _player, ref _waypoint, _mode == "patrol-fire");
    }

    private void Finish(SurvivalState state)
    {
        if (state == SurvivalState.Running) return;
        try
        {
            SurvivalTestInputDriver.Release();
            var result = new
            {
                Mode = _mode, Seed = 618, Result = state.ToString(), GameSeconds = _game.Spawner.ElapsedSeconds,
                WallSeconds = _wall.Elapsed.TotalSeconds, MaxHealth = _player.MaxHealth, MoveSpeed = _player.MoveSpeed,
                RemainingHealth = _player.Health, Kills = _game.Kills, Score = _game.Score,
                DamageTaken = _damageTaken, DamageEvents = _damageEvents, TimeScale = Engine.TimeScale,
                PhysicsTicks = Engine.PhysicsTicksPerSecond, ScriptedInput = true
            };
            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            if (_output != "")
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_output))!);
                File.WriteAllText(_output, json);
            }
            GD.Print(json);
            if (_mode == "patrol-fire" && _game.Kills == 0) throw new Exception("Firing patrol destroyed no enemies.");
            GD.Print("PASS: balance trial recorded with normal physics; outcome is measurement, not human difficulty approval");
            GetTree().Quit(0);
        }
        catch (Exception exception) { GD.PushError(exception.ToString()); GetTree().Quit(1); }
    }
    public override void _ExitTree() => SurvivalTestInputDriver.Release();
}
