param(
    [string]$GodotPath,
    [switch]$VerifyOnly,
    [switch]$IncludeWindowTests
)
$ErrorActionPreference = 'Stop'
if ($IncludeWindowTests -and -not $VerifyOnly) {
    throw '-IncludeWindowTests requires -VerifyOnly.'
}
if (-not $GodotPath) {
    $GodotPath = $env:GODOT_DOTNET_PATH
}
if (-not $GodotPath) {
    $taskGodotCommand = Get-Command godot,godot4 -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($taskGodotCommand) { $GodotPath = $taskGodotCommand.Source }
}
if (-not $GodotPath -or -not (Test-Path -LiteralPath $GodotPath -PathType Leaf)) {
    throw 'Specify the Godot .NET executable with -GodotPath, GODOT_DOTNET_PATH, or PATH. See docs/SECOND_COMPUTER_CHECK.md.'
}
$GodotPath = (Resolve-Path -LiteralPath $GodotPath).Path
if (-not (Get-Command dotnet -CommandType Application -ErrorAction SilentlyContinue)) {
    throw 'Install the .NET 8 SDK before building this project.'
}
$taskGodotVersion = & $GodotPath --version
if ($LASTEXITCODE -ne 0 -or "$taskGodotVersion" -notmatch '\.mono\.') {
    throw 'Use the Godot .NET (mono) editor, not the standard Godot editor.'
}
Write-Output "Godot: $taskGodotVersion"
Push-Location -LiteralPath $PSScriptRoot
try {
    dotnet build NeonRiftStage1.csproj --nologo
    if ($LASTEXITCODE -ne 0) { throw 'C# build failed.' }
    & $GodotPath --headless --editor --path $PSScriptRoot --import --quit
    if ($LASTEXITCODE -ne 0) { throw 'Godot import failed.' }
    if ($VerifyOnly) {
        foreach ($taskTestScene in @('SurvivalModeTests', 'EnemySpawnerTests')) {
            & $GodotPath --headless --path $PSScriptRoot "res://Tests/$taskTestScene.tscn"
            if ($LASTEXITCODE -ne 0) { throw "$taskTestScene failed." }
        }
        if ($IncludeWindowTests) {
            & $GodotPath --path $PSScriptRoot 'res://Tests/SurvivalWindowTests.tscn'
            if ($LASTEXITCODE -ne 0) { throw 'Graphical window tests failed.' }
        }
        Write-Output 'Verification completed successfully.'
    }
    else {
        # Run by the user as an interactive local game.
        & $GodotPath --path $PSScriptRoot
        if ($LASTEXITCODE -ne 0) { throw 'Godot game exited with an error.' }
    }
}
finally { Pop-Location }
