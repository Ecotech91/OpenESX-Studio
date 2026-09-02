$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$projectDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$workspaceDirectory = Split-Path -Parent (Split-Path -Parent $projectDirectory)
$buildDirectory = Join-Path $projectDirectory 'build'
$packageDirectory = Join-Path $projectDirectory 'Package\OpenESX-Studio-2.0-RC5-Windows-x64'
$outputDirectory = Join-Path $workspaceDirectory 'outputs'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$html = Join-Path (Split-Path -Parent $projectDirectory) 'OpenESXStudioOffline\OpenESX-Studio-Offline.html'
$manifest = Join-Path $projectDirectory 'app.manifest'
$icon = Join-Path $buildDirectory 'OpenESXStudio.ico'
$portable = Join-Path $buildDirectory 'OpenESXStudio.exe'
$setup = Join-Path $buildDirectory 'OpenESX-Studio-Setup.exe'
$readme = Join-Path $projectDirectory 'README.txt'

if (-not (Test-Path -LiteralPath $csc)) { throw 'Der Windows-x64-C#-Compiler wurde nicht gefunden.' }
if (-not (Test-Path -LiteralPath $html)) { throw 'Die geprüfte Offline-Oberfläche wurde nicht gefunden.' }

New-Item -ItemType Directory -Path $buildDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $packageDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null

Add-Type -AssemblyName System.Drawing
$bitmap = New-Object System.Drawing.Bitmap 64,64
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
try {
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $background = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255,8,18,31))
    $blue = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255,37,99,235))
    $white = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::White)
    $font = [System.Drawing.Font]::new('Segoe UI',14,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Pixel)
    try {
        $graphics.FillRectangle($background,2,2,60,60)
        $graphics.FillRectangle($blue,7,7,50,50)
        $format = [System.Drawing.StringFormat]::new()
        try {
            $format.Alignment = [System.Drawing.StringAlignment]::Center
            $format.LineAlignment = [System.Drawing.StringAlignment]::Center
            $graphics.DrawString('ESX',$font,$white,[System.Drawing.RectangleF]::new(7,7,50,50),$format)
        } finally { $format.Dispose() }
    } finally {
        $font.Dispose(); $white.Dispose(); $blue.Dispose(); $background.Dispose()
    }
    $handle = $bitmap.GetHicon()
    $generatedIcon = [System.Drawing.Icon]::FromHandle($handle)
    $stream = [System.IO.File]::Create($icon)
    try { $generatedIcon.Save($stream) } finally { $stream.Dispose(); $generatedIcon.Dispose() }
} finally {
    $graphics.Dispose(); $bitmap.Dispose()
}

$commonArguments = @(
    '/nologo',
    '/target:winexe',
    '/platform:x64',
    '/optimize+',
    '/debug-',
    ('/win32manifest:' + $manifest),
    ('/win32icon:' + $icon),
    '/reference:System.dll',
    '/reference:System.Core.dll',
    '/reference:System.Windows.Forms.dll',
    '/reference:System.Drawing.dll'
)

$launcherArguments = $commonArguments + @(
    ('/out:' + $portable),
    ('/resource:' + $html + ',OpenESXStudio.Offline.html'),
    (Join-Path $projectDirectory 'Launcher.cs'),
    (Join-Path $projectDirectory 'NativeBridge.cs')
)
& $csc $launcherArguments
if ($LASTEXITCODE -ne 0) { throw 'Die portable Programmdatei konnte nicht erstellt werden.' }

$setupArguments = $commonArguments + @(
    ('/out:' + $setup),
    ('/resource:' + $portable + ',OpenESXStudio.Setup.Portable.exe'),
    ('/resource:' + $readme + ',OpenESXStudio.Setup.Readme.txt'),
    (Join-Path $projectDirectory 'Installer.cs')
)
& $csc $setupArguments
if ($LASTEXITCODE -ne 0) { throw 'Der Installer konnte nicht erstellt werden.' }

$portableOutput = Join-Path $outputDirectory 'OpenESX-Studio-2.0-RC5-Portable.exe'
$setupOutput = Join-Path $outputDirectory 'OpenESX-Studio-2.0-RC5-Setup.exe'
$readmeOutput = Join-Path $outputDirectory 'README-OpenESX-Studio-2.0-RC5-Windows.txt'
Copy-Item -LiteralPath $portable -Destination $portableOutput -Force
Copy-Item -LiteralPath $setup -Destination $setupOutput -Force
Copy-Item -LiteralPath $readme -Destination $readmeOutput -Force
Copy-Item -LiteralPath $portable -Destination (Join-Path $packageDirectory 'OpenESX-Studio-2.0-RC5-Portable.exe') -Force
Copy-Item -LiteralPath $setup -Destination (Join-Path $packageDirectory 'OpenESX-Studio-2.0-RC5-Setup.exe') -Force
Copy-Item -LiteralPath $readme -Destination (Join-Path $packageDirectory 'README.txt') -Force

$zipOutput = Join-Path $outputDirectory 'OpenESX-Studio-2.0-RC5-Windows-x64.zip'
Compress-Archive -LiteralPath $packageDirectory -DestinationPath $zipOutput -CompressionLevel Optimal -Force

$hashes = Get-FileHash -Algorithm SHA256 -LiteralPath @($setupOutput,$portableOutput,$zipOutput)
$hashText = $hashes | ForEach-Object { [System.IO.Path]::GetFileName($_.Path) + '  ' + $_.Hash }
[System.IO.File]::WriteAllLines((Join-Path $outputDirectory 'SHA256-OpenESX-Studio-2.0-RC5-Windows.txt'),$hashText,[System.Text.Encoding]::UTF8)

Get-Item -LiteralPath $setupOutput,$portableOutput,$zipOutput,$readmeOutput | Select-Object Name,Length,LastWriteTime
