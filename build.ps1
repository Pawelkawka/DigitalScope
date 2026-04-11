param(
    [string]$OutputDir     = (Join-Path $PSScriptRoot 'build'),
    [string]$Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error ".NET SDK not found. Install .NET 8 SDK from https://dotnet.microsoft.com/download and re-run."
}

$SdkVersion = (& dotnet --version 2>&1)
Write-Host "[*] Using .NET SDK $SdkVersion" -ForegroundColor Cyan

$ProjectFile = Join-Path $PSScriptRoot 'src\DigitalScope.csproj'
if (-not (Test-Path $ProjectFile)) {
    Write-Error "Project file not found: $ProjectFile"
}

if (Test-Path $OutputDir) {
    Write-Host "[*] Cleaning previous build..." -ForegroundColor Cyan
    Remove-Item -Recurse -Force $OutputDir
}
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

Write-Host ""
Write-Host "[*] Building DigitalScope..." -ForegroundColor Cyan
Write-Host "    Configuration : $Configuration"
Write-Host "    Output        : $OutputDir"
Write-Host ""

& dotnet publish $ProjectFile `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained false `
    --output $OutputDir `
    /p:Platform=x64 `
    /p:PublishSingleFile=true `
    /p:DebugType=none `
    /p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE."
}

$sourceBinDir = Join-Path $PSScriptRoot "src\bin"
if (Test-Path $sourceBinDir) {
    Write-Host ""
    Write-Host "Removing src/bin folder..." -ForegroundColor Cyan
    Remove-Item -Path $sourceBinDir -Recurse -Force
}

$Exe = Join-Path $OutputDir 'DigitalScope.exe'
if (Test-Path $Exe) {
    Write-Host ""
    Write-Host "[+] Build successful!" -ForegroundColor Green
    Write-Host "    Executable: $Exe" -ForegroundColor Green
    Write-Host ""
    Write-Host "Run DigitalScope.exe from the build folder to start the app."
} else {
    Write-Error "Build completed but DigitalScope.exe was not found in $OutputDir."
}