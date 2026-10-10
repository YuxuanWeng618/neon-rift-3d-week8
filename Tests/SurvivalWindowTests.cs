using Godot;
using System;
using System.IO;
using System.Threading.Tasks;

// Graphical integration test: resize, actual camera aiming, and GUI hit testing.
public partial class SurvivalWindowTests : Node
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
            var game = GD.Load<PackedScene>("res://Scenes3D/Main3D.tscn").Instantiate<Game3D>(); AddChild(game);
            var player = game.GetNode<Player3D>("Player");
            var manager = game.GetNode<SurvivalGameManager>("SurvivalGameManager");
            var hud = game.GetNode<SurvivalHud>("SurvivalHud");
            game.SetPhysicsProcess(false); manager.SetPhysicsProcess(false);
            int shots = 0; Vector3 direction = Vector3.Zero;
            player.ShotRequested += (_, aim) => { shots++; direction = aim; };
            foreach (var size in new[] { new Vector2I(1280, 720), new Vector2I(960, 540), new Vector2I(960, 720) })
            {
                GetWindow().Size = size;
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                game.StartGame(); game.GetNode<Node3D>("Entities").ProcessMode = ProcessModeEnum.Disabled;
                Rect2 view = GetViewport().GetVisibleRect();
                foreach (string node in new[] { "Title", "Score", "Health", "Controls", "Difficulty" })
                    Check(view.Encloses(hud.GetNode<Control>("Root/" + node).GetGlobalRect()), $"{node} outside viewport at {size}");
                Vector2 target = game.GetNode<Camera3D>("Camera3D").UnprojectPosition(player.GlobalPosition + Vector3.Right * 4);
                GetViewport().PushInput(new InputEventMouseMotion { Position = target }, true);
                int before = shots; Input.ActionPress("fire");
                for (int i = 0; i < 3; i++) await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
                Input.ActionRelease("fire");
                Check(shots == before + 1 && direction.Dot(Vector3.Right) > 0.99f, $"Aim/fire changed after resize to {size}");
                player.TakeDamage(1000);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                var button = hud.GetNode<Button>("Root/ResultOverlay/ResultPanel/Content/RestartButton");
                Check(view.Encloses(button.GetGlobalRect()), $"Restart outside viewport at {size}");
                Vector2 point = button.GetGlobalRect().GetCenter();
                GetViewport().PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = point, Pressed = true }, true);
                GetViewport().PushInput(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = point, Pressed = false }, true);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                Check(manager.State == SurvivalState.Running && !hud.ResultVisible, $"GUI restart failed at {size}");
                Vector2I rendered = GetViewport().GetTexture().GetImage().GetSize();
                Check(rendered == size, $"Render size {rendered} did not follow window size {size}");
                GD.Print($"WINDOW PASS: {size}; viewport={view.Size}; rendered={rendered}; aim and GUI restart passed");
            }
            GD.Print($"PASS: {_checks} graphical window checks"); GetTree().Quit(0);
        }
        catch (Exception exception) { GD.PushError(exception.ToString()); GetTree().Quit(1); }
        finally { Input.ActionRelease("fire"); }
    }
}
