param([string]$OutputPath,[switch]$Maintenance)
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sourceFiles = Get-ChildItem -LiteralPath (Join-Path $projectRoot 'src') -Filter '*.cs' | ForEach-Object { $_.FullName }
$target = if ($OutputPath) { [IO.Path]::GetFullPath($OutputPath) } elseif ($Maintenance) { Join-Path $projectRoot 'MosslightFarm-Maintenance.exe' } else { Join-Path $projectRoot 'MosslightFarm.exe' }
$targetDir = Split-Path -Parent $target
if (-not (Test-Path -LiteralPath $targetDir)) { New-Item -ItemType Directory -Path $targetDir -Force | Out-Null }
$compilerArgs = @('/nologo', '/target:winexe', '/optimize+', '/platform:x64', '/utf8output', '/r:System.Drawing.dll', '/r:System.Windows.Forms.dll', '/r:System.Web.Extensions.dll', "/out:$target")
if ($Maintenance) { $compilerArgs += '/define:MAINTENANCE_BUILD' }
$compilerArgs += "/r:$projectRoot\Microsoft.Web.WebView2.Core.dll"
$compilerArgs += '/r:Microsoft.CSharp.dll'
$compilerArgs += "/r:$projectRoot\Microsoft.Web.WebView2.WinForms.dll"
$iconPath = Join-Path $projectRoot 'assets\farm.ico'
if (Test-Path -LiteralPath $iconPath) { $compilerArgs += "/win32icon:$iconPath" }
$compilerArgs += "/win32manifest:$(Join-Path $projectRoot 'app.manifest')"
& $compiler @compilerArgs @sourceFiles
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
Write-Output "Built $target"
