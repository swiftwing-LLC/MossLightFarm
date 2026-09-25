param([string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$source = Split-Path $PSScriptRoot -Parent
$windowsRoot = Split-Path $source -Parent
$releaseRoot = Join-Path $windowsRoot '维护版'
$target = if ($OutputDirectory) { [IO.Path]::GetFullPath($OutputDirectory) } else { Join-Path $releaseRoot 'MosslightFarm-Maintenance-v1.1' }
New-Item -ItemType Directory -Path $target -Force | Out-Null
$exe = Join-Path $target 'MosslightFarm-Maintenance.exe'
& (Join-Path $source 'build.ps1') -Maintenance -OutputPath $exe
if ($LASTEXITCODE -ne 0) { throw 'Maintenance game build failed' }
foreach ($file in @('Microsoft.Web.WebView2.Core.dll','Microsoft.Web.WebView2.WinForms.dll','WebView2Loader.dll')) {
    Copy-Item -LiteralPath (Join-Path $source $file) -Destination $target -Force
}
Copy-Item -LiteralPath (Join-Path $source 'MosslightFarm.exe.config') -Destination ($exe + '.config') -Force
foreach ($directory in @('assets','runtime','licenses')) {
    Copy-Item -LiteralPath (Join-Path $source $directory) -Destination $target -Recurse -Force
}
$readme = @'
Mosslight Farm Maintenance v1.1

Personal test build for Windows 10/11 64-bit. Double-click MosslightFarm-Maintenance.exe to open the farm in a normal game window. It starts with unlimited coins (shown as ∞), and every building or upgrade takes 1 minute.

This build keeps its save separate from the regular game:
%USERPROFILE%\Saved Games\MosslightFarm-Maintenance\save.json

It does not register itself for automatic startup. To use this build as a wallpaper, first close the regular game and launch this executable with --desktop. The desktop and startup controls are kept separate from the regular game.

This is a personal maintenance build, not a balanced release for friends. Use the v1.1 Windows installer for the normal game.
'@
Set-Content -LiteralPath (Join-Path $target 'README.txt') -Value $readme -Encoding UTF8
$zip = Join-Path $releaseRoot 'MosslightFarm-Maintenance-v1.1.zip'
New-Item -ItemType Directory -Path $releaseRoot -Force | Out-Null
Compress-Archive -Path (Join-Path $target '*') -DestinationPath $zip -Force
Write-Output "Built $exe"
Write-Output "Packaged $zip"
