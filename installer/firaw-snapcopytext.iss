#ifndef MyArch
  #define MyArch "x64"
#endif
#ifndef MyVersion
  #define MyVersion "1.1.0"
#endif
#ifndef MySourceDir
  #error MySourceDir must point to the published application directory.
#endif
#ifndef MyOutputDir
  #define MyOutputDir "."
#endif

#define MyAppName "Firaw - SnapCopyText"
#define MyPublisher "Firawynix"
#define MyLauncherExe "Firaw.SnapCopyText.Launcher.exe"

#if MyArch == "x64"
  #define MyOutputName "Firaw-SnapCopyText-Setup-x64"
#else
  #define MyOutputName "Firaw-SnapCopyText-Setup-x86"
#endif

[Setup]
AppId={{5A12D49F-8448-4CE7-BC1A-53A0C697AC1C}
AppName={#MyAppName}
AppVersion={#MyVersion}
AppVerName={#MyAppName} {#MyVersion}
AppPublisher={#MyPublisher}
AppPublisherURL=https://firawynix.com.br/
AppUpdatesURL=https://snapcopytext.firawynix.com.br/
DefaultDirName={localappdata}\Programs\Firaw SnapCopyText
DefaultGroupName=Firaw
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir={#MyOutputDir}
OutputBaseFilename={#MyOutputName}
SetupIconFile=..\src\Firaw.SnapCopyText\Assets\firaw-eye.ico
UninstallDisplayIcon={app}\{#MyLauncherExe}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern dynamic
CloseApplications=yes
RestartApplications=no
MinVersion=10.0.17763
#if MyArch == "x64"
SetupArchitecture=x64
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
#else
ArchitecturesAllowed=x86compatible
#endif

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar um atalho na área de trabalho"; GroupDescription: "Atalhos adicionais:"; Flags: unchecked

[Files]
Source: "{#MySourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Firaw - SnapCopyText"; Filename: "{app}\{#MyLauncherExe}"
Name: "{autodesktop}\Firaw - SnapCopyText"; Filename: "{app}\{#MyLauncherExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyLauncherExe}"; Description: "Abrir o Firaw - SnapCopyText"; Flags: nowait postinstall skipifsilent; Check: not IsUpdateMode
Filename: "{app}\{#MyLauncherExe}"; Parameters: "--background --no-update"; Flags: nowait runhidden; Check: IsUpdateMode

[Code]
function IsUpdateMode: Boolean;
begin
  Result := ExpandConstant('{param:UPDATE|0}') = '1';
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  { O próprio app grava o início com o Windows (HKCU\...\Run). Sem isto, a
    entrada ficaria apontando para um programa que não existe mais. }
  if CurUninstallStep = usPostUninstall then
    RegDeleteValue(HKEY_CURRENT_USER, 'Software\Microsoft\Windows\CurrentVersion\Run', 'Firaw SnapCopyText');
end;
