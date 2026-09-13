param(
    [string]$Version = '1.1.7',
    [string]$ReleaseNotes = 'A tecla Esc cancela imediatamente a escolha de janela ou monitor.'
)

$ErrorActionPreference = 'Stop'
$repository = 'firawynix/firaw-snapcopytext'
$projectRoot = Split-Path -Parent $PSScriptRoot
$mainProject = Join-Path $projectRoot 'src\Firaw.SnapCopyText\Firaw.SnapCopyText.csproj'
$launcherProject = Join-Path $projectRoot 'src\Firaw.SnapCopyText.Launcher\Firaw.SnapCopyText.Launcher.csproj'
$installerScript = Join-Path $projectRoot 'installer\firaw-snapcopytext.iss'
$innoCompiler = Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 7\ISCC.exe'
$releaseRoot = Join-Path $projectRoot "release\Firaw-SnapCopyText-$Version"
$installerOutput = Join-Path $releaseRoot 'installers'
$githubOutput = Join-Path $releaseRoot 'github-release'

if (-not (Test-Path -LiteralPath $innoCompiler)) {
    throw "Inno Setup 7 não foi encontrado em: $innoCompiler"
}

# O launcher compara a FileVersion do executável instalado com o manifesto. Sem
# gravar a versão nova no .exe, o 1.1.1 se apresentaria como 1.1.0.0 e seria
# reinstalado a cada abertura.
$numbers = @([regex]::Matches($Version, '\d+') | ForEach-Object { $_.Value })
while ($numbers.Count -lt 4) { $numbers += '0' }
$fileVersion = ($numbers | Select-Object -First 4) -join '.'
$versionProperties = @("-p:Version=$Version", "-p:AssemblyVersion=$fileVersion", "-p:FileVersion=$fileVersion")

$publishDirectories = @{}
foreach ($architecture in @('x64', 'x86')) {
    $runtime = "win-$architecture"
    $appOutput = Join-Path $projectRoot "artifacts\Firaw-SnapCopyText-$architecture"
    $launcherOutput = Join-Path $projectRoot "artifacts\Firaw-SnapCopyText-Launcher-$architecture"
    $publishDirectories[$architecture] = $appOutput

    dotnet publish $mainProject -c Release -r $runtime --self-contained true @versionProperties -o $appOutput
    if ($LASTEXITCODE -ne 0) { throw "Falha ao publicar o aplicativo $architecture." }

    dotnet publish $launcherProject -c Release -r $runtime --self-contained true @versionProperties -p:PublishSingleFile=true -o $launcherOutput
    if ($LASTEXITCODE -ne 0) { throw "Falha ao publicar o launcher $architecture." }

    Copy-Item -LiteralPath (Join-Path $launcherOutput 'Firaw.SnapCopyText.Launcher.exe') -Destination $appOutput -Force
}

New-Item -ItemType Directory -Path $installerOutput -Force | Out-Null
foreach ($architecture in @('x64', 'x86')) {
    & $innoCompiler "/DMyArch=$architecture" "/DMyVersion=$Version" "/DMySourceDir=$($publishDirectories[$architecture])" "/DMyOutputDir=$installerOutput" $installerScript
    if ($LASTEXITCODE -ne 0) { throw "Falha ao compilar o instalador $architecture." }
}

# Tudo que vai para a release do GitHub (tag v$Version): os dois instaladores,
# um .sha256 de cada (o Firawynix Center confere por ele) e o manifesto.
New-Item -ItemType Directory -Path $githubOutput -Force | Out-Null
$packages = [ordered]@{}
foreach ($architecture in @('x64', 'x86')) {
    $name = "Firaw-SnapCopyText-Setup-$architecture.exe"
    $installer = Join-Path $installerOutput $name
    Copy-Item -LiteralPath $installer -Destination $githubOutput -Force
    $hash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLowerInvariant()
    [System.IO.File]::WriteAllText((Join-Path $githubOutput "$name.sha256"), "$hash  $name`n", [System.Text.Encoding]::ASCII)
    $packages[$architecture] = [ordered]@{
        # Mesmo host do manifesto (github.com), na tag desta versão: manifesto e
        # instalador nunca se desencontram quando sair a próxima release.
        url = "https://github.com/$repository/releases/download/v$Version/$name"
        sha256 = $hash
        size = (Get-Item -LiteralPath $installer).Length
    }
}

$manifest = [ordered]@{
    schemaVersion = 1
    product = 'Firaw - SnapCopyText'
    version = $Version
    releaseNotes = $ReleaseNotes
    packages = $packages
}
$utf8 = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText((Join-Path $githubOutput 'update.json'), ($manifest | ConvertTo-Json -Depth 6), $utf8)

$reportPath = Join-Path $projectRoot 'ENTREGA-FIRAW-SNAPCOPYTEXT.md'
if (Test-Path -LiteralPath $reportPath) {
    Copy-Item -LiteralPath $reportPath -Destination $releaseRoot -Force
}

Write-Host "Release pronta em $githubOutput"
Write-Host "Publicar: gh release create v$Version -R $repository --title `"Firaw - SnapCopyText $Version`" --notes `"$ReleaseNotes`" $githubOutput\*"
