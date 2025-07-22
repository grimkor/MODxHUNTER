#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"
$projectFile = "grimbahack_hxh.csproj"
$projectXml = [xml](Get-Content $projectFile)
$version = $projectXml.Project.PropertyGroup.Version
Write-Host "Project version: $version"

Write-Host "Building project..."
dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}
Write-Host "Build completed successfully"

$tempDir = "temp_release"
if (Test-Path $tempDir) {
    Remove-Item -Recurse -Force $tempDir
}
New-Item -ItemType Directory -Path $tempDir | Out-Null
Write-Host "Created temporary directory: $tempDir"

Write-Host "Extracting template zip..."
Expand-Archive -Path "template\release.zip" -DestinationPath $tempDir
Write-Host "Template zip extracted"

$pluginsDir = Join-Path $tempDir "BepInEx\plugins"
if (-not (Test-Path $pluginsDir)) {
    New-Item -ItemType Directory -Path $pluginsDir | Out-Null
    Write-Host "Created plugins directory: $pluginsDir"
}

$dllSource = "bin\Release\net6.0\modxhunter.dll"
$dllDest = Join-Path $pluginsDir "modxhunter.dll"
Copy-Item $dllSource $dllDest
Write-Host "Copied DLL to plugins directory"

$outDir = "out"
if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir | Out-Null
    Write-Host "Created out directory: $outDir"
}
Remove-Item -Force -Path "$outDir\*.zip"
$outputZip = "$outDir/MODxHUNTER-$version.zip"
if (Test-Path $outputZip) {
    Remove-Item -Force $outputZip
}
Write-Host "Creating final zip file: $outputZip"
Compress-Archive -Path "$tempDir\*" -DestinationPath $outputZip
Write-Host "Final zip file created"

Remove-Item -Recurse -Force $tempDir
Write-Host "Cleaned up temporary directory"

Write-Host "Release process completed successfully"
Write-Host "Output file: $outputZip"
