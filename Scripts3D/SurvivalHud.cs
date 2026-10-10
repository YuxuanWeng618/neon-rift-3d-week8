#nullable enable
using Godot;
using System;

// Member B: native Godot controls. This view never advances the game clock.
public partial class SurvivalHud : CanvasLayer
{
    private static readonly Color Accent = SurvivalVisualStyle.Accent;
    private static readonly Color Ink = SurvivalVisualStyle.Ink;
    private static readonly Color Muted = SurvivalVisualStyle.Muted;
    private static readonly Color Danger = SurvivalVisualStyle.Danger;
    private Game3D? _game;
    private SurvivalGameManager? _manager;
    private Player3D? _player;
    private Label _time = null!, _health = null!, _phase = null!, _phaseDetail = null!, _kills = null!;
    private Label _resultTitle = null!, _resultText = null!, _resultStats = null!;
    private Label _timeCaption = null!, _healthCaption = null!;
    private ProgressBar _healthBar = null!, _timeBar = null!;
    private Control _result = null!;
    private Button _restart = null!;
    public string DisplayedTime => _time.Text;
    public string DisplayedHealth => _health.Text;
    public bool ResultVisible => _result.Visible;
    public string ResultTitle => _resultTitle.Text;
    public string DisplayedTimeCaption => _timeCaption.Text;
    public string DisplayedHealthCaption => _healthCaption.Text;

    public override void _Ready()
    {
        Layer = 10;
        var root = Add(this, new Control { Name = "Root" });
        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        root.Theme = new Theme { DefaultFont = SurvivalVisualStyle.BodyFont() };

        var brand = Add(root, new VBoxContainer { Name = "Title" });
        Place(brand, Control.LayoutPreset.TopLeft, 34, 28, 330, 100);
        Text(brand, "NEON RIFT", 33, Ink, true);
        Text(brand, "SURVIVAL / 60 SECONDS", 14, Muted);

        var clock = Add(root, new VBoxContainer { Name = "Clock" });
        Place(clock, Control.LayoutPreset.CenterTop, -90, -3, 90, 142);
        clock.AddThemeConstantOverride("separation", 2);
        _time = Text(clock, "60", SurvivalVisualStyle.ClockFontSize, Ink, true); _time.HorizontalAlignment = HorizontalAlignment.Center;
        _timeCaption = Text(clock, "SECONDS LEFT", 13, Ink); _timeCaption.HorizontalAlignment = HorizontalAlignment.Center;
        _timeBar = Bar(clock, Accent, 180, 4); _timeBar.MaxValue = 60; _timeBar.Value = 60;

        var score = Add(root, new VBoxContainer { Name = "Score" });
        Place(score, Control.LayoutPreset.TopRight, -275, 32, -34, 101);
        var scoreTitle = Text(score, "ELIMINATIONS", 13, Muted); scoreTitle.HorizontalAlignment = HorizontalAlignment.Right;
        _kills = Text(score, "0 / 0 PTS", 26, Ink, true); _kills.HorizontalAlignment = HorizontalAlignment.Right;

        var vital = Add(root, new VBoxContainer { Name = "Health" });
        Place(vital, Control.LayoutPreset.BottomLeft, 34, -114, 315, -26);
        vital.AddThemeConstantOverride("separation", 4);
        _healthCaption = Text(vital, "HEALTH", 13, Ink, true);
        _healthBar = Bar(vital, Accent, 280, 16);
        _health = Text(vital, "100 / 100", 23, Ink, true);

        var controls = Add(root, new VBoxContainer { Name = "Controls" });
        Place(controls, Control.LayoutPreset.CenterBottom, -220, -65, 220, -23);
        var move = Text(controls, "WASD MOVE  /  MOUSE AIM", 14, Ink); move.HorizontalAlignment = HorizontalAlignment.Center;
        var fire = Text(controls, "HOLD LEFT CLICK TO SHOOT", 12, Muted); fire.HorizontalAlignment = HorizontalAlignment.Center;

        var phase = Add(root, new VBoxContainer { Name = "Difficulty" });
        Place(phase, Control.LayoutPreset.BottomRight, -302, -104, -34, -27);
        _phase = Text(phase, "CONTACT", 25, Ink, true); _phase.HorizontalAlignment = HorizontalAlignment.Right;
        _phaseDetail = Text(phase, "ENEMY INTERVAL 3s", 13, Muted); _phaseDetail.HorizontalAlignment = HorizontalAlignment.Right;

        _result = Add(root, new Control { Name = "ResultOverlay", Visible = false, MouseFilter = Control.MouseFilterEnum.Stop });
        _result.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var shade = Add(_result, new ColorRect { Color = new Color(SurvivalVisualStyle.Paper, SurvivalVisualStyle.ResultOpacity) });
        shade.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var panel = Add(_result, new PanelContainer { Name = "ResultPanel" });
        Place(panel, Control.LayoutPreset.Center, -330, -213, 330, 213);
        panel.AddThemeStyleboxOverride("panel", new StyleBoxEmpty());
        var content = Add(panel, new VBoxContainer { Name = "Content" }); content.AddThemeConstantOverride("separation", 16);
        var tag = Text(content, "ROUND OVER", 15, Muted, true); tag.HorizontalAlignment = HorizontalAlignment.Center;
        _resultTitle = Text(content, "SURVIVED", SurvivalVisualStyle.ResultFontSize, Ink, true); _resultTitle.HorizontalAlignment = HorizontalAlignment.Center;
        _resultText = Text(content, "You made it through all 60 seconds.", 19, Ink); _resultText.HorizontalAlignment = HorizontalAlignment.Center;
        Add(content, new Control { CustomMinimumSize = new Vector2(0, 8) });
        _resultStats = Text(content, "", 20, Muted); _resultStats.HorizontalAlignment = HorizontalAlignment.Center;
        _resultStats.CustomMinimumSize = new Vector2(0, 67);
        _restart = Add(content, new Button { Name = "RestartButton", Text = "PLAY AGAIN", CustomMinimumSize = new Vector2(340, 60),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter, MouseFilter = Control.MouseFilterEnum.Stop });
        _restart.AddThemeFontSizeOverride("font_size", 19);
        foreach (string colorName in new[] { "font_color", "font_hover_color", "font_pressed_color" })
            _restart.AddThemeColorOverride(colorName, SurvivalVisualStyle.Paper);
        _restart.AddThemeStyleboxOverride("normal", Box(Ink, Ink, 16));
        _restart.AddThemeStyleboxOverride("hover", Box(Accent, Accent, 16));
        _restart.AddThemeStyleboxOverride("pressed", Box(SurvivalVisualStyle.Pressed, SurvivalVisualStyle.Pressed, 16));
        _restart.AddThemeStyleboxOverride("disabled", Box(Muted, Muted, 16));
        _restart.Pressed += RequestRestart;
    }

    public void Bind(Game3D game, SurvivalGameManager manager, Player3D player)
    {
        if (_game != null) throw new InvalidOperationException("Bind the HUD once.");
        _game = game; _manager = manager; _player = player;
        manager.TimeChanged += UpdateTime;
        manager.StateChanged += UpdateState;
        player.HealthChanged += UpdateHealth;
        game.ScoreChanged += UpdateScore;
        UpdateTime(manager.RemainingSeconds); UpdateHealth(player.Health, player.MaxHealth);
        UpdateScore(game.Kills, game.Score); UpdateState(manager.State);
    }

    private void UpdateTime(double seconds)
    {
        _time.Text = Math.Ceiling(seconds).ToString("0");
        bool critical = seconds <= SurvivalVisualStyle.CriticalSeconds;
        _time.AddThemeColorOverride("font_color", critical ? Danger : Ink);
        _timeCaption.Text = critical ? "FINAL SECONDS" : "SECONDS LEFT";
        _timeCaption.AddThemeColorOverride("font_color", critical ? Danger : Ink);
        _timeBar.MaxValue = _manager?.SurvivalDuration ?? 60;
        _timeBar.Value = seconds;
        double elapsed = _game?.Spawner.ElapsedSeconds ?? 0;
        _phase.Text = elapsed < 20 ? "CONTACT" : elapsed < 40 ? "CROSSFIRE" : "OVERLOAD";
        _phaseDetail.Text = $"ENEMY INTERVAL {_game?.Spawner.CurrentInterval ?? 3:0}s";
    }

    private void UpdateHealth(float current, float maximum)
    {
        _health.Text = $"{current:0} / {maximum:0}";
        _healthBar.MaxValue = maximum; _healthBar.Value = current;
        bool critical = current <= maximum * SurvivalVisualStyle.CriticalHealthRatio;
        Color barColor = critical ? Danger : Accent;
        _healthBar.AddThemeStyleboxOverride("fill", Box(barColor, barColor, 0));
        _health.AddThemeColorOverride("font_color", critical ? Danger : Ink);
        _healthCaption.Text = critical ? "LOW HEALTH" : "HEALTH";
        _healthCaption.AddThemeColorOverride("font_color", critical ? Danger : Ink);
    }

    private void UpdateScore(int kills, int score) => _kills.Text = $"{kills} / {score} PTS";

    private void UpdateState(SurvivalState state)
    {
        bool won = state == SurvivalState.Won;
        _result.Visible = won || state == SurvivalState.Lost;
        _restart.Disabled = false;
        if (!_result.Visible || _manager == null || _game == null) return;
        _resultTitle.Text = won ? "SURVIVED" : "SIGNAL LOST";
        _resultTitle.AddThemeColorOverride("font_color", won ? Accent : Danger);
        _resultText.Text = won ? "You made it through all 60 seconds." : "One more try. Keep moving, keep shooting.";
        double duration = Math.Min(_manager.SurvivalDuration, _game.Spawner.ElapsedSeconds);
        _resultStats.Text = $"{duration:0.0} SECONDS ALIVE\n{_game.Kills} ELIMINATIONS  /  {_game.Score} POINTS";
    }

    public void RequestRestart()
    {
        if (_restart.Disabled || _manager == null || !_result.Visible) return;
        _restart.Disabled = true;
        _manager.RestartRound();
    }

    public override void _ExitTree()
    {
        if (_manager != null && IsInstanceValid(_manager))
        { _manager.TimeChanged -= UpdateTime; _manager.StateChanged -= UpdateState; }
        if (_player != null && IsInstanceValid(_player)) _player.HealthChanged -= UpdateHealth;
        if (_game != null && IsInstanceValid(_game)) _game.ScoreChanged -= UpdateScore;
    }

    private static T Add<T>(Node parent, T node) where T : Node
    {
        // Unobtrusive HUD; only result overlay and buttons block mouse input.
        if (node is Control control && node is not Button && node.Name != "ResultOverlay")
            control.MouseFilter = Control.MouseFilterEnum.Ignore;
        parent.AddChild(node); return node;
    }

    private static void Place(Control control, Control.LayoutPreset preset, float left, float top, float right, float bottom)
    {
        control.SetAnchorsAndOffsetsPreset(preset);
        control.OffsetLeft = left; control.OffsetTop = top; control.OffsetRight = right; control.OffsetBottom = bottom;
    }

    private static Label Text(Node parent, string text, int size, Color color, bool bold = false)
    {
        var label = Add(parent, new Label { Text = text });
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        if (bold) label.AddThemeFontOverride("font", SurvivalVisualStyle.BoldFont());
        return label;
    }

    private static ProgressBar Bar(Node parent, Color color, float width, float height)
    {
        var bar = Add(parent, new ProgressBar { CustomMinimumSize = new Vector2(width, height), ShowPercentage = false, MaxValue = 100 });
        bar.AddThemeStyleboxOverride("background", Box(SurvivalVisualStyle.Track, SurvivalVisualStyle.Track, 0));
        bar.AddThemeStyleboxOverride("fill", Box(color, color, 0));
        return bar;
    }

    private static StyleBoxFlat Box(Color background, Color border, int padding) => new()
    {
        BgColor = background, BorderColor = border,
        ContentMarginLeft = padding, ContentMarginRight = padding,
        ContentMarginTop = padding, ContentMarginBottom = padding
    };
}
