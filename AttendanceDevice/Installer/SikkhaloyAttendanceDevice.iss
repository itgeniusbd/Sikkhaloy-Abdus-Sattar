; Professional single-file installer for SIKKHALOY Attendance Device
; Requires Inno Setup 6: https://jrsoftware.org/isinfo.php
;
; Build:
;   1) Build AttendanceDevice in Release (x86)
;   2) Open this script in Inno Setup Compiler -> Compile
;   OR run:  AttendanceDevice\Installer\Build-Installer.ps1

#define MyAppName "SIKKHALOY Attendance Device"
#ifndef MyAppVersion
#define MyAppVersion "4.0.0"
#endif
#define MyAppPublisher "IT Genius"
#define MyAppURL "https://sikkhaloy.com"
#define MyAppExeName "AttendanceDevice.exe"
#define MySourceDir "..\bin\Release"

[Setup]
AppId={{A8F3C2E1-5B4D-4A9E-9C7F-1D2E3F4A5B6C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\SIKKHALOY\AttendanceDevice
DefaultGroupName=SIKKHALOY
DisableProgramGroupPage=no
LicenseFile=
OutputDir=..\Installer\Output
OutputBaseFilename=SIKKHALOY_AttendanceDevice_Setup_{#MyAppVersion}
SetupIconFile=..\Resources\Sikkhaloy.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
; x86 app on 64-bit Windows: use x86compatible (x86 alone blocks 64-bit OS)
ArchitecturesAllowed=x86compatible
MinVersion=6.1sp1

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[Files]
Source: "{#MySourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb,*.vshost.*,*.application,*.manifest,app.publish\*,Application Files\*,setup.exe,*.deploy,SikkhaloyAppDB.db"
Source: "Cleanup-LocalData.ps1"; DestDir: "{app}\Installer"; Flags: ignoreversion
Source: "Cleanup-ProgramFilesDb.ps1"; DestDir: "{app}\Installer"; Flags: ignoreversion
Source: "..\Database Scripts\Run-LocalCleanup.ps1"; DestDir: "{app}\Installer"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "SIKKHALOY biometric attendance"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "SIKKHALOY biometric attendance"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent unchecked

[UninstallDelete]
Type: files; Name: "{app}\SikkhaloyAppDB.db"
Type: files; Name: "{app}\Database\SikkhaloyAppDB.db"

[UninstallRun]
; ExecAsOriginalUser is install-only; runascurrentuser works during uninstall.
Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File ""{tmp}\SikkhaloyCleanup-LocalData.ps1"" -Quiet"; Flags: runascurrentuser waituntilterminated runhidden; RunOnceId: "CleanupLocalDataScript"
Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -Command ""$ErrorActionPreference='SilentlyContinue'; taskkill /F /IM AttendanceDevice.exe /T 2>$null; Remove-Item -Recurse -Force (Join-Path $env:LOCALAPPDATA 'SIKKHALOY\AttendanceDevice') -ErrorAction SilentlyContinue; Remove-Item -Recurse -Force (Join-Path $env:LOCALAPPDATA 'SIKKHALOY') -ErrorAction SilentlyContinue; Remove-Item -Recurse -Force (Join-Path $env:LOCALAPPDATA 'SikkhaloyAttendance') -ErrorAction SilentlyContinue"""; Flags: runascurrentuser waituntilterminated runhidden; RunOnceId: "CleanupLocalDataInline"
Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File ""{app}\Installer\Cleanup-ProgramFilesDb.ps1"" -InstallDir ""{app}"""; Flags: waituntilterminated runhidden; RunOnceId: "CleanupProgramFilesDb"

[Registry]
Root: HKCU; Subkey: "Software\SIKKHALOY\AttendanceDevice"; ValueType: string; ValueName: "LocalDataDirectory"; ValueData: "{localappdata}\SIKKHALOY\AttendanceDevice"; Flags: uninsdeletekey

[Code]
function IsDotNet48Installed: Boolean;
var
  Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release)
    and (Release >= 528040);
  if not Result then
    Result := RegQueryDWordValue(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release)
      and (Release >= 528040);
end;

function InitializeSetup: Boolean;
begin
  Result := True;
  if not IsDotNet48Installed then
  begin
    MsgBox(
      'This PC needs Microsoft .NET Framework 4.8 before SIKKHALOY Attendance can run.' + #13#10#13#10 +
      'Download and install .NET 4.8, restart the PC, then run this installer again.' + #13#10#13#10 +
      'https://dotnet.microsoft.com/download/dotnet-framework/net48',
      mbError, MB_OK);
    Result := False;
  end;
end;

function IsZkSdkInstalled: Boolean;
var
  SdkPath: String;
begin
  if IsWin64 then
    SdkPath := ExpandConstant('{syswow64}\zkemkeeper.dll')
  else
    SdkPath := ExpandConstant('{sys}\zkemkeeper.dll');
  Result := FileExists(SdkPath);
end;

function RegisterZkSdk: Boolean;
var
  ResultCode: Integer;
  HelperPath, SdkSource: String;
begin
  Result := True;
  if IsZkSdkInstalled then
    Exit;

  HelperPath := ExpandConstant('{app}\ZKdllRegistrationApp.exe');
  SdkSource := ExpandConstant('{app}\libs\Zktec 32bit');

  if not FileExists(HelperPath) then
  begin
    if not WizardSilent then
      MsgBox(
        'ZKTeco device SDK files were not found in the install folder.' + #13#10#13#10 +
        'Expected helper:' + #13#10 + HelperPath,
        mbError, MB_OK);
    Result := False;
    Exit;
  end;

  if not DirExists(SdkSource) then
  begin
    if not WizardSilent then
      MsgBox(
        'ZKTeco SDK driver files were not found in the install folder.' + #13#10#13#10 +
        'Expected folder:' + #13#10 + SdkSource + #13#10#13#10 +
        'Rebuild the installer on a machine that has AttendanceDevice\libs\Zktec 32bit.',
        mbError, MB_OK);
    Result := False;
    Exit;
  end;

  if not Exec(HelperPath, AddQuotes(SdkSource), ExpandConstant('{app}'),
    SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if not WizardSilent then
      MsgBox('Failed to launch ZKTeco SDK registration helper.', mbError, MB_OK);
    Result := False;
    Exit;
  end;

  Result := IsZkSdkInstalled;
  if (not Result) and (not WizardSilent) then
    MsgBox(
      'ZKTeco SDK registration did not complete.' + #13#10#13#10 +
      'Run this file as Administrator:' + #13#10 + HelperPath,
      mbError, MB_OK);
end;

procedure KillProcess(const ExeName: String);
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{cmd}'), '/C taskkill /F /IM ' + ExeName + ' /T 2>nul', '',
    SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

procedure TryDeletePath(const Path: String);
begin
  if FileExists(Path) then
    DeleteFile(Path);
  if DirExists(Path) then
    DelTree(Path, True, True, True);
end;

procedure DeleteLegacyInstallDatabase;
begin
  TryDeletePath(ExpandConstant('{app}\SikkhaloyAppDB.db'));
  TryDeletePath(ExpandConstant('{app}\Database\SikkhaloyAppDB.db'));
end;

procedure EnsureInstallId;
var
  Existing: String;
  Id: String;
begin
  if RegQueryStringValue(HKCU, 'Software\SIKKHALOY\AttendanceDevice', 'InstallId', Existing) then
    Exit;

  Id := GetDateTimeString('yyyymmddhhnnss', #0, #0) + '-' + IntToStr(Random(2147483647));
  RegWriteStringValue(HKCU, 'Software\SIKKHALOY\AttendanceDevice', 'InstallId', Id);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    EnsureInstallId;
    DeleteLegacyInstallDatabase;
    RegisterZkSdk;
  end;
end;

procedure StageCleanupScriptForUninstall;
var
  ScriptPath, TempScript: String;
begin
  ScriptPath := ExpandConstant('{app}\Installer\Cleanup-LocalData.ps1');
  TempScript := ExpandConstant('{tmp}\SikkhaloyCleanup-LocalData.ps1');

  if FileExists(TempScript) then
    DeleteFile(TempScript);

  if FileExists(ScriptPath) then
    CopyFile(ScriptPath, TempScript, False);
end;

function InitializeUninstall(): Boolean;
begin
  KillProcess('AttendanceDevice.exe');
  KillProcess('ZKdllRegistrationApp.exe');
  StageCleanupScriptForUninstall();
  DeleteLegacyInstallDatabase;
  Result := True;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    KillProcess('AttendanceDevice.exe');
    KillProcess('ZKdllRegistrationApp.exe');
    StageCleanupScriptForUninstall();
    DeleteLegacyInstallDatabase;
  end;
end;
