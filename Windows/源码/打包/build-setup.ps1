param([string]$OutputPath)
$ErrorActionPreference = 'Stop'
$source = Split-Path $PSScriptRoot -Parent
$work = Join-Path $env:TEMP ('MosslightFarm-Package-' + [guid]::NewGuid().ToString('N'))
$zip = $work + '.zip'
New-Item -ItemType Directory -Path $work -Force | Out-Null
try {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    $uninstaller = Join-Path $work 'MosslightFarmUninstall.exe'
    & $compiler /nologo /target:winexe /platform:x64 /optimize+ /utf8output /r:System.Windows.Forms.dll "/win32icon:$source\assets\farm.ico" "/out:$uninstaller" "$PSScriptRoot\Uninstaller.cs"
    if ($LASTEXITCODE -ne 0) { throw 'Uninstaller build failed' }
    foreach ($name in @('MosslightFarm.exe', 'MosslightFarm.exe.config', 'Microsoft.Web.WebView2.Core.dll', 'Microsoft.Web.WebView2.WinForms.dll', 'WebView2Loader.dll', 'assets', 'runtime', 'licenses')) {
        Copy-Item -LiteralPath (Join-Path $source $name) -Destination $work -Recurse -Force
    }
    Compress-Archive -Path (Join-Path $work '*') -DestinationPath $zip -Force
    $output = if ($OutputPath) { [IO.Path]::GetFullPath($OutputPath) } else { Join-Path (Split-Path $source -Parent) '最新版\MosslightFarm-Setup-v1.1-Windows.exe' }
    New-Item -ItemType Directory -Path (Split-Path $output -Parent) -Force | Out-Null
    & $compiler /nologo /target:winexe /platform:x64 /optimize+ /utf8output /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.IO.Compression.dll /r:Microsoft.CSharp.dll "/win32manifest:$source\app.manifest" "/win32icon:$source\assets\farm.ico" "/resource:$zip,payload.zip" "/out:$output" "$PSScriptRoot\Setup.cs"
    if ($LASTEXITCODE -ne 0) { throw 'Setup build failed' }
    Write-Output $output
} finally {
    if (Test-Path -LiteralPath $work) { Remove-Item -LiteralPath $work -Recurse -Force }
    if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
}
