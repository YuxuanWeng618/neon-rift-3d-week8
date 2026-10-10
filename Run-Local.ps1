param([string]$GodotPath)
$ErrorActionPreference = 'Stop'
if (-not $GodotPath) {
    $taskGodotCommand = Get-Command godot,godot4 -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($taskGodotCommand) { $GodotPath = $taskGodotCommand.Source }
    else { $GodotPath = 'E:\NTU Coding\neon-rift-local-review-20261011\godot\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe' }
}
if (-not (Test-Path -LiteralPath $GodotPath -PathType Leaf)) { throw 'Specify the Godot .NET executable with -GodotPath.' }
Push-Location -LiteralPath $PSScriptRoot
try {
    dotnet build NeonRiftStage1.csproj --nologo
    if ($LASTEXITCODE -ne 0) { throw 'C# build failed.' }
    & $GodotPath --headless --editor --path $PSScriptRoot --import --quit
    if ($LASTEXITCODE -ne 0) { throw 'Godot import failed.' }
    # Run by the user as an interactive local game.
    & $GodotPath --path $PSScriptRoot
}
finally { Pop-Location }
