#nullable enable
using Godot;
using System;

public enum SurvivalState { Ready, Running, Won, Lost }

// Member B: round rules and results. The spawner supplies the one shared clock.
public partial class SurvivalGameManager : Node
{
    [Export] public double SurvivalDuration { get; set; } = 60.0;
    public SurvivalState State { get; private set; } = SurvivalState.Ready;
    public double RemainingSeconds => Math.Max(0, SurvivalDuration - (_game?.Spawner.ElapsedSeconds ?? 0));
    public event Action<double>? TimeChanged;
    public event Action<SurvivalState>? StateChanged;
    private Game3D? _game;
    private Player3D? _player;
    private bool _restartPending;

    public override void _Ready() => ProcessPhysicsPriority = 100;

    public void Initialize(Game3D game, Player3D player)
    {
        if (_game != null) throw new InvalidOperationException("Initialize the manager once.");
        if (!double.IsFinite(SurvivalDuration) || SurvivalDuration <= 0)
            throw new InvalidOperationException("Survival duration must be positive and finite.");
        _game = game;
        _player = player;
        game.RoundStarted += OnRoundStarted;
    }

    private void OnRoundStarted()
    {
        State = SurvivalState.Running;
        _restartPending = false;
        TimeChanged?.Invoke(RemainingSeconds);
        StateChanged?.Invoke(State);
    }

    public override void _PhysicsProcess(double delta) => CheckRound();

    public void CheckRound()
    {
        if (State != SurvivalState.Running || _game == null || _player == null) return;
        // Death in the same discrete physics update takes precedence over time expiry.
        if (_player.Health <= 0) { FinishRound(SurvivalState.Lost); return; }
        if (!_game.IsRunning) return;
        TimeChanged?.Invoke(RemainingSeconds);
        if (RemainingSeconds <= 0) FinishRound(SurvivalState.Won);
    }

    public void FinishRound(SurvivalState result)
    {
        if (result != SurvivalState.Won && result != SurvivalState.Lost)
            throw new ArgumentOutOfRangeException(nameof(result));
        if (State != SurvivalState.Running || _game == null) return;
        State = result;
        _game.StopGame();
        TimeChanged?.Invoke(RemainingSeconds);
        StateChanged?.Invoke(State);
    }

    public void RestartRound()
    {
        if (_restartPending || (State != SurvivalState.Won && State != SurvivalState.Lost)) return;
        _restartPending = true;
        // Remove collision objects outside an active physics callback.
        CallDeferred(nameof(PerformRestart));
    }

    private void PerformRestart()
    {
        if (_restartPending && _game != null && IsInstanceValid(_game)) _game.StartGame();
    }

    public override void _ExitTree()
    {
        if (_game != null && IsInstanceValid(_game)) _game.RoundStarted -= OnRoundStarted;
    }
}
