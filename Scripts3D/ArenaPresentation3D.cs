using Godot;

// Visual objects only. The starter's dimensions, collisions and actors are reused.
public partial class ArenaPresentation3D : Node3D
{
    public override void _Ready()
    {
        AddChild(new WorldEnvironment
        {
            Environment = new Godot.Environment
            {
                BackgroundMode = Godot.Environment.BGMode.Color,
                BackgroundColor = SurvivalVisualStyle.Paper,
                AmbientLightSource = Godot.Environment.AmbientSource.Color,
                AmbientLightColor = Colors.White,
                AmbientLightEnergy = SurvivalVisualStyle.AmbientEnergy
            }
        });
        var floor = new StandardMaterial3D { AlbedoColor = SurvivalVisualStyle.Floor, Roughness = 1 };
        var walls = new StandardMaterial3D { AlbedoColor = SurvivalVisualStyle.Wall, Roughness = 1 };
        GetNode<MeshInstance3D>("Ground/MeshInstance3D").MaterialOverride = floor;
        foreach (string wall in new[] { "NorthWall", "SouthWall", "EastWall", "WestWall" })
            GetNode<MeshInstance3D>(wall + "/MeshInstance3D").MaterialOverride = walls;

        var tiles = new Node3D { Name = "FloorTiles" }; AddChild(tiles);
        var tileA = new StandardMaterial3D { AlbedoColor = SurvivalVisualStyle.TileA, Roughness = 1 };
        var tileB = new StandardMaterial3D { AlbedoColor = SurvivalVisualStyle.TileB, Roughness = 1 };
        for (int x = 0; x < 10; x++)
            for (int z = 0; z < 7; z++)
                Block(tiles, new Vector3(-9 + x * 2, 0.005f, -6 + z * 2), new Vector3(1.97f, 0.006f, 1.97f),
                    (x + z) % 2 == 0 ? tileA : tileB);
        var rim = new StandardMaterial3D { AlbedoColor = SurvivalVisualStyle.ArenaEdge, ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded };
        Block(tiles, new Vector3(0, 2.02f, -7), new Vector3(20, 0.06f, 0.28f), rim);
        Block(tiles, new Vector3(0, 2.02f, 7), new Vector3(20, 0.06f, 0.28f), rim);
        Block(tiles, new Vector3(-10, 2.02f, 0), new Vector3(0.28f, 0.06f, 14), rim);
        Block(tiles, new Vector3(10, 2.02f, 0), new Vector3(0.28f, 0.06f, 14), rim);
        var light = GetParent().GetNode<DirectionalLight3D>("DirectionalLight3D");
        light.ShadowEnabled = true;
        light.LightEnergy = SurvivalVisualStyle.DirectionalEnergy;
        var player = GetParent().GetNode<Player3D>("Player");
        player.GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverride = new StandardMaterial3D { AlbedoColor = SurvivalVisualStyle.Player };
        player.GetNode<MeshInstance3D>("AimMarker").MaterialOverride = new StandardMaterial3D
            { AlbedoColor = SurvivalVisualStyle.AimMarker, ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded };
    }

    private static void Block(Node3D parent, Vector3 position, Vector3 size, Material material)
        => parent.AddChild(new MeshInstance3D { Position = position, Mesh = new BoxMesh { Size = size }, MaterialOverride = material });
}
