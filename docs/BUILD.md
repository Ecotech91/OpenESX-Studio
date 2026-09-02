# Building OpenESX Studio

## Requirements

- Windows 10 or Windows 11 x64
- 64-bit .NET Framework C# compiler at `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`
- Windows PowerShell
- Node.js for the synthetic core smoke test

No network download is required by the build script.

## Build

From the repository root:

```powershell
.\src\OpenESXStudioInstaller\Build.ps1
```

The generated installer, portable executable, ZIP, README, and SHA-256 list are written to `outputs`.

## Public smoke test

```powershell
node .\tests\CoreSmokeTest.js
```

This test generates an empty synthetic ESX-shaped byte array in memory. It checks recognition, table parsing, bit-exact copying, pattern step writing, range protection, and card-manager controls without distributing private samples.

## Executable self-tests

After building, run:

```powershell
$testPath = Join-Path $PWD 'self-test-output'
$process = Start-Process -FilePath '.\outputs\OpenESX-Studio-2.0-RC5-Setup.exe' -ArgumentList ('--self-test "' + $testPath + '"') -Wait -PassThru -WindowStyle Hidden
if ($process.ExitCode -ne 0) { throw 'Installer self-test failed.' }
```

The build is intentionally dependency-light. Microsoft Edge is used as the installed application window at runtime.

