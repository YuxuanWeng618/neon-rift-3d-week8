using Godot;
using System;
using System.Diagnostics;

// A real scripted round, a stationary loss, and GUI restarts, captured by Godot.
public partial class SurvivalDemoRecorder : Node
{
    private Game3D _game = null!;
    private Player3D _player = null!;
    private SurvivalGameManager _manager = null!;
    private int _chapter, _waypoint;
    private double _hold;
    private readonly Stopwatch _watchdog = new();

    public override void _Ready() => CallDeferred(nameof(Begin));
    private void Begin()
    {
        ProcessPhysicsPriority = -100;
        _game = GD.Load<PackedScene>("res://Scenes3D/Main3D.tscn").Instantiate<Game3D>(); AddChild(_game);
        _player = _game.GetNode<Player3D>("Player"); _manager = _game.GetNode<SurvivalGameManager>("SurvivalGameManager");
        _game.Spawner.Configure(_player, _game.GetNode<Node3D>("Entities"), 618); _game.StartGame();
        _watchdog.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_manager == null) return;
        if (_watchdog.Elapsed.TotalSeconds > 180) { GD.PushError("Demo watchdog expired."); GetTree().Quit(1); return; }
        if (_chapter == 0)
        {
            if (_manager.State == SurvivalState.Running) SurvivalTestInputDriver.PatrolAndAim(_game, _player, ref _waypoint, true);
            else
            {
                SurvivalTestInputDriver.Release();
                if (_manager.State != SurvivalState.Won) { GD.PushError("Demo first round did not survive."); GetTree().Quit(1); return; }
                _hold += delta;
                if (_hold >= 3) { GD.Print("DEMO: full survival won, GUI restart to stationary round"); ClickRestart(); _chapter = 1; _hold = 0; }
            }
        }
        else if (_chapter == 1)
        {
            SurvivalTestInputDriver.Release();
            if (_manager.State == SurvivalState.Lost)
            {
                _hold += delta;
                if (_hold >= 3) { GD.Print($"DEMO: natural stationary defeat at {_game.Spawner.ElapsedSeconds:0.0}s, GUI restart"); ClickRestart(); _chapter = 2; _hold = 0; }
            }
            else if (_manager.State == SurvivalState.Won) { GD.PushError("Stationary demo unexpectedly won."); GetTree().Quit(1); }
        }
        else if (_manager.State == SurvivalState.Running)
        {
            SurvivalTestInputDriver.Release(); _hold += delta;
            if (_hold >= 3) { GD.Print("PASS: demo records full 60-second win, natural defeat and two GUI restarts"); GetTree().Quit(0); }
        }
    }

    private void ClickRestart()
    {
        var button = _game.GetNode<SurvivalHud>("SurvivalHud").GetNode<Button>("Root/ResultOverlay/ResultPanel/Content/RestartButton");
        Vector2 point = button.GetGlobalRect().GetCenter();
        GetViewport().PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = point, Pressed = true }, true);
        GetViewport().PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = point, Pressed = false }, true);
    }
    public override void _ExitTree() => SurvivalTestInputDriver.Release();
}
