$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$publishOutput = Join-Path $projectRoot "artifacts\publish"
$msbuild = (Get-Command MSBuild.exe).Source

New-Item -ItemType Directory -Force $publishOutput | Out-Null

dotnet restore `
    (Join-Path $projectRoot "DesktopAnimatedWallpaper.csproj") `
    --ignore-failed-sources `
    -p:NuGetAudit=false

& $msbuild `
    (Join-Path $projectRoot "DesktopAnimatedWallpaper.csproj") `
    /t:Build `
    /p:Configuration=Release `
    /p:OutDir="$publishOutput\"
