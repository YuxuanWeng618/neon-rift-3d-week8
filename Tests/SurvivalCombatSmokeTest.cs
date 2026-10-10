using Godot;
using System;
using System.Diagnostics;
using System.Linq;

// Test driver supplies movement/aim/fire input; all combat remains production code.
// One scripted patrol is evidence of integration, not a human difficulty assessment.
public partial class SurvivalCombatSmokeTest : Node
{
    private Game3D _game = null!;
    private Player3D _player = null!;
    private SurvivalGameManager _manager = null!;
    private readonly Stopwatch _wall = new();
    private readonly Vector3[] _patrol = { new(7, 0.5f, 4), new(7, 0.5f, -4), new(-7, 0.5f, -4), new(-7, 0.5f, 4) };
    private int _waypoint;

    public override void _Ready()
    {
        ProcessPhysicsPriority = -100;
        _game = GD.Load<PackedScene>("res://Scenes3D/Main3D.tscn").Instantiate<Game3D>();
        AddChild(_game);
        _player = _game.GetNode<Player3D>("Player");
        _manager = _game.GetNode<SurvivalGameManager>("SurvivalGameManager");
        _game.Spawner.Configure(_player, _game.GetNode<Node3D>("Entities"), 618);
        _game.StartGame();
        _manager.StateChanged += state =>
        {
            if (state == SurvivalState.Running) return;
            Release();
            GD.Print($"COMBAT RESULT: {state}; game={_game.Spawner.ElapsedSeconds:0.000}s; hp={_player.Health}; kills={_game.Kills}; score={_game.Score}; speed={_player.MoveSpeed}; maxHp={_player.MaxHealth}; wall={_wall.Elapsed.TotalSeconds:0.00}s");
            if (_game.Kills == 0) { GD.PushError("Combat patrol did not destroy any enemy."); GetTree().Quit(1); }
            else { GD.Print("PASS: real combat patrol with default stats and normal physics"); GetTree().Quit(0); }
        };
        _wall.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_manager.State != SurvivalState.Running) return;
        if (_wall.Elapsed.TotalSeconds > 80) { Release(); GD.PushError("Combat watchdog expired."); GetTree().Quit(1); return; }
        Vector3 offset = _patrol[_waypoint] - _player.GlobalPosition;
        if (offset.Length() < 0.4f) { _waypoint = (_waypoint + 1) % _patrol.Length; offset = _patrol[_waypoint] - _player.GlobalPosition; }
        Vector3 direction = offset.Normalized();
        Axis("move_right", "move_left", direction.X); Axis("move_down", "move_up", direction.Z);
        var enemy = _game.GetNode<Node3D>("Entities").GetChildren().OfType<Enemy3D>()
            .Where(e => !e.IsQueuedForDeletion()).OrderBy(e => e.GlobalPosition.DistanceSquaredTo(_player.GlobalPosition)).FirstOrDefault();
        if (enemy == null) { Input.ActionRelease("fire"); return; }
        Vector2 screen = _game.GetNode<Camera3D>("Camera3D").UnprojectPosition(enemy.GlobalPosition);
        GetViewport().PushInput(new InputEventMouseMotion { Position = screen }, true);
        Input.ActionPress("fire");
    }

    private static void Axis(string positive, string negative, float amount)
    {
        Input.ActionRelease(positive); Input.ActionRelease(negative);
        if (amount > 0) Input.ActionPress(positive, amount);
        else if (amount < 0) Input.ActionPress(negative, -amount);
    }

    private static void Release()
    {
        foreach (string action in new[] { "move_left", "move_right", "move_up", "move_down", "fire" }) Input.ActionRelease(action);
    }

    public override void _ExitTree() => Release();
}
