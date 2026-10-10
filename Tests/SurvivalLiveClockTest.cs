using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

// No time acceleration: validates 60s through the engine's normal physics loop.
// Actors are disabled in this fixture to isolate timing, not assess combat balance.
public partial class SurvivalLiveClockTest : Node
{
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        CallDeferred(nameof(Run));
    }

    public async void Run()
    {
        try
        {
            var game = GD.Load<PackedScene>("res://Scenes3D/Main3D.tscn").Instantiate<Game3D>();
            game.ProcessMode = ProcessModeEnum.Pausable;
            AddChild(game);
            var manager = game.GetNode<SurvivalGameManager>("SurvivalGameManager");
            var hud = game.GetNode<SurvivalHud>("SurvivalHud");
            game.GetNode<Node3D>("Entities").ProcessMode = ProcessModeEnum.Disabled;
            for (int i = 0; i < 6; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            double beforePause = game.Spawner.ElapsedSeconds;
            GetTree().Paused = true;
            await ToSignal(GetTree().CreateTimer(0.2, true), SceneTreeTimer.SignalName.Timeout);
            if (game.Spawner.ElapsedSeconds != beforePause) throw new Exception("Paused game advanced time.");
            GetTree().Paused = false;
            var wall = Stopwatch.StartNew();
            while (manager.State == SurvivalState.Running)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
                if (wall.Elapsed.TotalSeconds > 75) throw new Exception("Live clock did not finish within watchdog.");
            }
            if (manager.State != SurvivalState.Won || hud.DisplayedTime != "0" || game.Spawner.ElapsedSeconds < 60 || Engine.TimeScale != 1)
                throw new Exception("Formal live clock result invalid.");
            double ended = game.Spawner.ElapsedSeconds;
            await ToSignal(GetTree().CreateTimer(0.2), SceneTreeTimer.SignalName.Timeout);
            if (game.Spawner.ElapsedSeconds != ended) throw new Exception("Finished clock advanced.");
            GD.Print($"PASS: live 60-second clock, pause and end freeze; game={ended:0.000}s, observed wall={wall.Elapsed.TotalSeconds:0.00}s; actors disabled for clock isolation");
            GetTree().Quit(0);
        }
        catch (Exception exception)
        {
            GetTree().Paused = false;
            GD.PushError(exception.ToString()); GetTree().Quit(1);
        }
    }
}
