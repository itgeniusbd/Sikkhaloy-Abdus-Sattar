; SIKKHALOY Hybrid — installs to Program Files with Start Menu / Desktop shortcuts
; and Add/Remove Programs uninstall. Requires Inno Setup 6.
;
; Build:  Hybrid\Installer\Build-Installer.ps1

#define MyAppName "SIKKHALOY Hybrid"
#ifndef MyAppVersion
#define MyAppVersion "1.0.0"
#endif
#define MyAppPublisher "IT Genius"
#define MyAppURL "https://sikkhaloy.com"
#define MyAppExeName "SikkhaloyHybrid.exe"
#define MySourceDir "..\dist\SikkhaloyHybrid"

[Setup]
AppId={{9C4E2B71-6A18-4D5F-B8E3-2F7A1C0D9E54}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\SIKKHALOY\Hybrid
DefaultGroupName=SIKKHALOY
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=SIKKHALOY_Hybrid_Setup_{#MyAppVersion}
SetupIconFile=..\src\Sikkhaloy.Client\Assets\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=6.1sp1
CloseApplications=yes
CloseApplicationsFilter=SikkhaloyHybrid.exe
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[Files]
Source: "{#MySourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb,*.xml,_buildcheck\*"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "SIKKHALOY Hybrid school management"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "SIKKHALOY Hybrid school management"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\wwwroot"
Type: filesandordirs; Name: "{app}\runtimes"
