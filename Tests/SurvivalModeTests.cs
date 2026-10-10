using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

// Integration checks against the actual production scene and native HUD controls.
public partial class SurvivalModeTests : Node
{
    private int _checks;
    public override void _Ready() => CallDeferred(nameof(Run));
    private void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        _checks++;
    }

    public async void Run()
    {
        try
        {
            var game = GD.Load<PackedScene>("res://Scenes3D/Main3D.tscn").Instantiate<Game3D>();
            AddChild(game);
            var player = game.GetNode<Player3D>("Player");
            var manager = game.GetNode<SurvivalGameManager>("SurvivalGameManager");
            var hud = game.GetNode<SurvivalHud>("SurvivalHud");
            var entities = game.GetNode<Node3D>("Entities");
            game.SetPhysicsProcess(false); manager.SetPhysicsProcess(false);
            entities.ProcessMode = ProcessModeEnum.Disabled;
            Check(manager.SurvivalDuration == 60 && manager.State == SurvivalState.Running, "Formal duration and initial state");
            Check(hud.DisplayedTime == "60" && hud.DisplayedHealth == "100 / 100" && !hud.ResultVisible, "Initial HUD");
            Check(game.GetChildren().OfType<EnemySpawner3D>().Count() == 1, "Exactly one spawner");
            Check(InputMap.ActionGetEvents("fire").Any(e => e is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left), "Fire action bound to left mouse button");

            Vector3 start = player.GlobalPosition;
            Input.ActionPress("move_right");
            for (int i = 0; i < 8; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            Input.ActionRelease("move_right");
            Check(player.GlobalPosition.X > start.X + 0.3f, "Real physics WASD movement");

            int shots = 0; Vector3 shotDirection = Vector3.Zero;
            player.ShotRequested += (position, direction) => { shots++; shotDirection = direction; };
            Vector2 mouse = game.GetNode<Camera3D>("Camera3D").UnprojectPosition(player.GlobalPosition + Vector3.Right * 5);
            GetViewport().PushInput(new InputEventMouseMotion { Position = mouse }, true);
            Input.ActionPress("fire");
            for (int i = 0; i < 5; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            Input.ActionRelease("fire");
            Check(shots == 1 && shotDirection.Dot(Vector3.Right) > 0.95f, $"Mouse aim and rate-limited shooting: shots={shots}, direction={shotDirection}, mouse={mouse}");
            Check(entities.GetChildren().OfType<Bullet3D>().Any(), "Game creates a real bullet");

            int healthEvents = 0;
            player.HealthChanged += (_, _) => healthEvents++;
            player.TakeDamage(24);
            Check(player.Health == 76 && hud.DisplayedHealth == "76 / 100" && healthEvents == 1, "Damage updates HUD once");
            player.TakeDamage(24); player.TakeDamage(float.NaN); player.TakeDamage(-5);
            Check(player.Health == 76 && healthEvents == 1, "Immunity and invalid damage ignored");
            for (int i = 0; i < 40; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            player.TakeDamage(6);
            Check(player.Health == 70 && healthEvents == 2, "Damage accepted after immunity expires");

            game.StartGame(); entities.ProcessMode = ProcessModeEnum.Disabled;
            for (int i = 0; i < 10; i++)
            {
                game.Spawner.Tick(3);
                entities.GetChildren().OfType<Enemy3D>().First(e => !e.IsQueuedForDeletion()).TakeDamage(1000);
            }
            manager.CheckRound();
            Check(game.Kills == 10 && manager.State == SurvivalState.Running, "Ten kills never end survival");
            Check(hud.DisplayedTime == "30", "HUD reads the shared elapsed time");
            game.Spawner.Tick(29.9); manager.CheckRound();
            Check(manager.State == SurvivalState.Running && hud.DisplayedTime == "1", "Alive at 59.9 seconds");
            game.Spawner.Tick(0.11); manager.CheckRound();
            Check(manager.State == SurvivalState.Won && hud.DisplayedTime == "0", "Expiry wins and displays zero");
            Check(hud.ResultVisible && hud.ResultTitle == "SURVIVED", "Win panel shown");
            double frozen = game.Spawner.ElapsedSeconds;
            game._PhysicsProcess(10); game.Spawner.Tick(10); player.TakeDamage(1000);
            Check(game.Spawner.ElapsedSeconds == frozen && player.Health == 100 && !game.IsRunning, "End freezes clock and guards player");
            Check(entities.ProcessMode == ProcessModeEnum.Disabled, "All existing entities disabled");
            Vector3[] frozenPositions = entities.GetChildren().OfType<Node3D>().Where(n => !n.IsQueuedForDeletion()).Select(n => n.Position).ToArray();
            Vector3 frozenPlayer = player.GlobalPosition; int shotsAtFinish = shots;
            Input.ActionPress("move_right"); Input.ActionPress("fire");
            for (int i = 0; i < 4; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            Input.ActionRelease("move_right"); Input.ActionRelease("fire");
            Check(frozenPositions.SequenceEqual(entities.GetChildren().OfType<Node3D>().Where(n => !n.IsQueuedForDeletion()).Select(n => n.Position)), "Existing actors do not move after finish");
            Check(player.GlobalPosition == frozenPlayer && shots == shotsAtFinish, "Finished round ignores move/fire input");
            manager.FinishRound(SurvivalState.Lost);
            Check(manager.State == SurvivalState.Won, "Repeated result cannot overwrite victory");

            int starts = 0;
            game.RoundStarted += () => starts++;
            var button = hud.GetNode<Button>("Root/ResultOverlay/ResultPanel/Content/RestartButton");
            // Send an actual click through Godot's GUI hit testing, then a duplicate request.
            Vector2 buttonPoint = button.GetGlobalRect().GetCenter();
            GetViewport().PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = buttonPoint, Pressed = true }, true);
            GetViewport().PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = buttonPoint, Pressed = false }, true);
            button.EmitSignal(Button.SignalName.Pressed);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(starts == 1 && manager.State == SurvivalState.Running && !hud.ResultVisible, "Double-click schedules one restart");
            Check(manager.RemainingSeconds == 60 && player.Health == 100 && game.Kills == 0 && game.Score == 0, "Restart resets values");
            Check(entities.GetChildCount() == 0 && game.Spawner.ActiveEnemyCount == 0 && game.Spawner.CurrentInterval == 3, "Restart clears all old actors and difficulty");
            Check(player.GlobalPosition == new Vector3(0, 0.5f, 0) && player.Visible && player.Active, "Player restored completely");
            Check(entities.ProcessMode == ProcessModeEnum.Inherit, "Restart enables entities again");

            Input.ActionPress("fire"); game.StartGame();
            int shotsBeforeHeld = shots;
            for (int i = 0; i < 4; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            Check(shots == shotsBeforeHeld, "Held restart click does not fire in new round");
            Input.ActionRelease("fire");
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            Input.ActionPress("fire");
            for (int i = 0; i < 3; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            Input.ActionRelease("fire");
            Check(shots == shotsBeforeHeld + 1, "Shooting resumes after release barrier");
            game.StartGame();

            game.Spawner.Tick(12); player.TakeDamage(1000);
            Check(manager.State == SurvivalState.Lost && hud.ResultTitle == "SIGNAL LOST" && hud.DisplayedTime == "48", "Early death loses and retains remaining time");
            game.Spawner.Tick(100); manager.CheckRound();
            Check(manager.State == SurvivalState.Lost, "Loss is not changed by future ticks");
            hud.RequestRestart(); await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            game.Spawner.Tick(60); player.TakeDamage(1000); manager.CheckRound();
            Check(manager.State == SurvivalState.Lost, "Death in expiry update takes precedence");

            for (int i = 0; i < 20; i++)
            {
                hud.RequestRestart(); await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                Check(manager.State == SurvivalState.Running && entities.GetChildCount() == 0, "Repeated restart has no state residue");
                player.TakeDamage(1000);
                Check(manager.State == SurvivalState.Lost, "Repeated death subscription remains effective");
            }
            Check(starts == 24, "No duplicate start subscribers after repeated restarts");

            game.StartGame();
            game.Spawner.Tick(19.9); manager.CheckRound();
            Check(game.Spawner.CurrentInterval == 3 && hud.DisplayedTime == "41", "Phase one shares timer");
            game.Spawner.Tick(0.1); manager.CheckRound(); Check(game.Spawner.CurrentInterval == 2, "Twenty-second transition");
            game.Spawner.Tick(19.9); manager.CheckRound(); Check(game.Spawner.CurrentInterval == 2 && hud.DisplayedTime == "21", "Before forty-second transition");
            game.Spawner.Tick(0.1); manager.CheckRound(); Check(game.Spawner.CurrentInterval == 1 && hud.DisplayedTime == "20", "Forty-second transition");
            Check(hud.DisplayedTimeCaption == "SECONDS LEFT" && hud.DisplayedHealthCaption == "HEALTH", "Normal captions before warning thresholds");
            game.Spawner.Tick(10); manager.CheckRound();
            Check(hud.DisplayedTimeCaption == "FINAL SECONDS", "Ten-second threshold uses explicit warning");
            player.TakeDamage(70);
            Check(player.Health == 30 && hud.DisplayedHealthCaption == "LOW HEALTH", "Thirty-percent threshold uses explicit warning");
            game.StartGame();
            Check(hud.DisplayedTimeCaption == "SECONDS LEFT" && hud.DisplayedHealthCaption == "HEALTH", "Restart clears both warnings");
            player.MaxHealth = 200; game.StartGame(); player.TakeDamage(140);
            Check(player.Health == 60 && hud.DisplayedHealthCaption == "LOW HEALTH", "Warning threshold follows configured max health");
            player.MaxHealth = 100; game.StartGame();
            game.Spawner.Tick(40); manager.CheckRound();
            game.StopGame(); game.Spawner.Tick(100); manager.CheckRound();
            Check(manager.State == SurvivalState.Running && manager.RemainingSeconds == 20, "Direct stop cannot manufacture victory");
            game.StartGame(); entities.ProcessMode = ProcessModeEnum.Disabled;
            for (int i = 0; i < 50; i++) game.Spawner.Tick(1);
            manager.CheckRound();
            Check(game.Spawner.ActiveEnemyCount == 20 && manager.State == SurvivalState.Running && hud.DisplayedTime == "10", "Enemy cap does not stop timer/HUD during round");
            entities.GetChildren().OfType<Enemy3D>().First(e => !e.IsQueuedForDeletion()).TakeDamage(1000);
            game.Spawner.Tick(1); manager.CheckRound();
            Check(game.Spawner.ActiveEnemyCount == 20 && hud.DisplayedTime == "9", "Vacated cap slot refills while shared timer continues");
            GD.Print($"PASS: {_checks} survival-mode checks");
            GetTree().Quit(0);
        }
        catch (Exception exception)
        {
            GD.PushError(exception.ToString()); GetTree().Quit(1);
        }
        finally { Input.ActionRelease("move_right"); Input.ActionRelease("fire"); }
    }
}
