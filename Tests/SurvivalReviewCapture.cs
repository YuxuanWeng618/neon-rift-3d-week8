using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

// Review-only fixture. Stages real game states; never loaded by the main game.
public partial class SurvivalReviewCapture : Node
{
    public override void _Ready() => CallDeferred(nameof(Capture));
    public async void Capture()
    {
        try
        {
            var args = OS.GetCmdlineUserArgs();
            string output = args.Length > 0 ? args[0] : "user://review";
            DirAccess.MakeDirRecursiveAbsolute(output);
            var game = GD.Load<PackedScene>("res://Scenes3D/Main3D.tscn").Instantiate<Game3D>();
            AddChild(game);
            var player = game.GetNode<Player3D>("Player");
            var manager = game.GetNode<SurvivalGameManager>("SurvivalGameManager");
            var entities = game.GetNode<Node3D>("Entities");
            game.SetPhysicsProcess(false); manager.SetPhysicsProcess(false); player.SetPhysicsProcess(false);
            entities.ProcessMode = ProcessModeEnum.Disabled;
            game.Spawner.Configure(player, entities, 618); game.Spawner.Start();
            await Save(output, "01-start.png");

            for (int i = 0; i < 8; i++) game.Spawner.Tick(3);
            int index = 0;
            foreach (Enemy3D enemy in entities.GetChildren().OfType<Enemy3D>())
            {
                float angle = index++ * Mathf.Tau / 8;
                enemy.GlobalPosition = new Vector3(Mathf.Cos(angle) * 5, 0.5f, Mathf.Sin(angle) * 3.5f);
                enemy.LookAt(player.GlobalPosition, Vector3.Up);
            }
            player.TakeDamage(22); manager.CheckRound();
            await Save(output, "02-playing.png");

            game.Spawner.Tick(36); manager.CheckRound();
            await Save(output, "03-victory.png");

            game.StartGame(); entities.ProcessMode = ProcessModeEnum.Disabled;
            for (int i = 0; i < 6; i++) game.Spawner.Tick(3);
            player.TakeDamage(1000);
            await Save(output, "04-defeat.png");

            game.GetNode<SurvivalHud>("SurvivalHud").RequestRestart();
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            entities.ProcessMode = ProcessModeEnum.Disabled;
            await Save(output, "05-restarted.png");
            game.Spawner.Tick(12); player.TakeDamage(12); manager.CheckRound();
            GetWindow().Size = new Vector2I(960, 540);
            await Save(output, "06-compact-window.png");
            GetWindow().Size = new Vector2I(1280, 720);
            game.StartGame(); entities.ProcessMode = ProcessModeEnum.Disabled;
            game.Spawner.Tick(6); player.TakeDamage(75); manager.CheckRound();
            await Save(output, "07-low-health.png");
            game.StartGame(); entities.ProcessMode = ProcessModeEnum.Disabled;
            game.Spawner.Tick(51); manager.CheckRound();
            await Save(output, "08-final-seconds.png");
            player.TakeDamage(80);
            await Save(output, "09-combined-warning.png");
            GetWindow().Size = new Vector2I(960, 720);
            await Save(output, "10-four-three-window.png");
            player.ResetPlayer(player.GlobalPosition); player.TakeDamage(1000);
            await Save(output, "11-four-three-result.png");
            GD.Print("CAPTURE PASS: eleven real Godot viewport images (staged review states)");
            GetTree().Quit(0);
        }
        catch (Exception exception) { GD.PushError(exception.ToString()); GetTree().Quit(1); }
    }

    private async Task Save(string output, string filename)
    {
        // Let containers lay out and the GPU finish before reading the viewport.
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        var image = GetViewport().GetTexture().GetImage();
        Error result = image.SavePng(output.PathJoin(filename));
        if (result != Error.Ok) throw new Exception($"Screenshot failed: {filename}: {result}");
    }
}
