using Godot;
using System.Linq;

// Test-only input. Movement, aiming, bullets and damage run in production scripts.
public static class SurvivalTestInputDriver
{
    private static readonly Vector3[] Patrol = { new(7, 0.5f, 4), new(7, 0.5f, -4), new(-7, 0.5f, -4), new(-7, 0.5f, 4) };
    public static void PatrolAndAim(Game3D game, Player3D player, ref int waypoint, bool shoot)
    {
        Vector3 offset = Patrol[waypoint] - player.GlobalPosition;
        if (offset.Length() < 0.4f) { waypoint = (waypoint + 1) % Patrol.Length; offset = Patrol[waypoint] - player.GlobalPosition; }
        Vector3 direction = offset.Normalized();
        Axis("move_right", "move_left", direction.X); Axis("move_down", "move_up", direction.Z);
        var enemy = game.GetNode<Node3D>("Entities").GetChildren().OfType<Enemy3D>()
            .Where(e => !e.IsQueuedForDeletion()).OrderBy(e => e.GlobalPosition.DistanceSquaredTo(player.GlobalPosition)).FirstOrDefault();
        if (!shoot || enemy == null) { Input.ActionRelease("fire"); return; }
        Vector2 screen = game.GetNode<Camera3D>("Camera3D").UnprojectPosition(enemy.GlobalPosition);
        game.GetViewport().PushInput(new InputEventMouseMotion { Position = screen }, true);
        Input.ActionPress("fire");
    }

    private static void Axis(string positive, string negative, float amount)
    {
        Input.ActionRelease(positive); Input.ActionRelease(negative);
        if (amount > 0) Input.ActionPress(positive, amount);
        else if (amount < 0) Input.ActionPress(negative, -amount);
    }

    public static void Release()
    {
        foreach (string action in new[] { "move_left", "move_right", "move_up", "move_down", "fire" }) Input.ActionRelease(action);
    }
}
