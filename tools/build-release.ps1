param(
    [string]$Version = '1.1.0'
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$mainProject = Join-Path $projectRoot 'src\Firaw.SnapCopyText\Firaw.SnapCopyText.csproj'
$launcherProject = Join-Path $projectRoot 'src\Firaw.SnapCopyText.Launcher\Firaw.SnapCopyText.Launcher.csproj'
$installerScript = Join-Path $projectRoot 'installer\firaw-snapcopytext.iss'
$innoCompiler = Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 7\ISCC.exe'
$releaseRoot = Join-Path $projectRoot "release\Firaw-SnapCopyText-$Version"
$installerOutput = Join-Path $releaseRoot 'installers'
$updateServerOutput = Join-Path $releaseRoot 'update-server\firaw-snapcopytext'

if (-not (Test-Path -LiteralPath $innoCompiler)) {
    throw "Inno Setup 7 não foi encontrado em: $innoCompiler"
}

$publishDirectories = @{}
foreach ($architecture in @('x64', 'x86')) {
    $runtime = "win-$architecture"
    $appOutput = Join-Path $projectRoot "artifacts\Firaw-SnapCopyText-$architecture"
    $launcherOutput = Join-Path $projectRoot "artifacts\Firaw-SnapCopyText-Launcher-$architecture"
    $publishDirectories[$architecture] = $appOutput

    dotnet publish $mainProject -c Release -r $runtime --self-contained true -p:Version=$Version -o $appOutput
    if ($LASTEXITCODE -ne 0) { throw "Falha ao publicar o aplicativo $architecture." }

    dotnet publish $launcherProject -c Release -r $runtime --self-contained true -p:Version=$Version -p:PublishSingleFile=true -o $launcherOutput
    if ($LASTEXITCODE -ne 0) { throw "Falha ao publicar o launcher $architecture." }

    Copy-Item -LiteralPath (Join-Path $launcherOutput 'Firaw.SnapCopyText.Launcher.exe') -Destination $appOutput -Force
}

New-Item -ItemType Directory -Path $installerOutput -Force | Out-Null
foreach ($architecture in @('x64', 'x86')) {
    & $innoCompiler "/DMyArch=$architecture" "/DMyVersion=$Version" "/DMySourceDir=$($publishDirectories[$architecture])" "/DMyOutputDir=$installerOutput" $installerScript
    if ($LASTEXITCODE -ne 0) { throw "Falha ao compilar o instalador $architecture." }
}

$x64Installer = Join-Path $installerOutput 'Firaw-SnapCopyText-Setup-x64.exe'
$x86Installer = Join-Path $installerOutput 'Firaw-SnapCopyText-Setup-x86.exe'
$manifest = [ordered]@{
    schemaVersion = 1
    product = 'Firaw - SnapCopyText'
    version = $Version
    releaseNotes = 'Launcher com atualização automática, instaladores Inno x64/x86 e melhorias de captura, OCR, paleta e bandeja.'
    packages = [ordered]@{
        x64 = [ordered]@{
            url = 'Firaw-SnapCopyText-Setup-x64.exe'
            sha256 = (Get-FileHash -LiteralPath $x64Installer -Algorithm SHA256).Hash
            size = (Get-Item -LiteralPath $x64Installer).Length
        }
        x86 = [ordered]@{
            url = 'Firaw-SnapCopyText-Setup-x86.exe'
            sha256 = (Get-FileHash -LiteralPath $x86Installer -Algorithm SHA256).Hash
            size = (Get-Item -LiteralPath $x86Installer).Length
        }
    }
}

New-Item -ItemType Directory -Path $updateServerOutput -Force | Out-Null
$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $updateServerOutput 'update.json') -Encoding UTF8
Copy-Item -LiteralPath $x64Installer,$x86Installer -Destination $updateServerOutput -Force

$downloadDirectory = Join-Path $projectRoot 'demo-site\dist\downloads'
New-Item -ItemType Directory -Path $downloadDirectory -Force | Out-Null
Copy-Item -LiteralPath $x64Installer,$x86Installer -Destination $downloadDirectory -Force

$siteRelease = Join-Path $releaseRoot 'demonstracao-local'
Copy-Item -LiteralPath (Join-Path $projectRoot 'demo-site\dist') -Destination $siteRelease -Recurse -Force

$reportPath = Join-Path $projectRoot 'ENTREGA-FIRAW-SNAPCOPYTEXT.md'
if (Test-Path -LiteralPath $reportPath) {
    Copy-Item -LiteralPath $reportPath -Destination $releaseRoot -Force
}

Write-Host "Release pronta em $releaseRoot"
