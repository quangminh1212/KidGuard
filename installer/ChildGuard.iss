; ChildGuard Inno Setup script
; Requires: Build artifacts in ..\out\Agent and ..\out\Service (use scripts\build_installer.ps1)

#define AppName "ChildGuard"
#define AppVersion "1.0.2"
#define AppPublisher "ChildGuard Team"
#define AppUrl "https://example.local/childguard"

[Setup]
AppId={{9F9542F9-6E7E-4D98-8FF7-9E3B5B8A1C8B}}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppUrl}
DefaultDirName={pf}\ChildGuard
DefaultGroupName=ChildGuard
DisableDirPage=no
DisableProgramGroupPage=no
OutputBaseFilename=ChildGuardSetup_{#AppVersion}
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "installservice"; Description: "Install Windows Service (recommended)"; Flags: unchecked
Name: "agentforall"; Description: "Run Agent at logon for all users"; GroupDescription: "Agent autostart mode"; Flags: exclusive
Name: "agentforcurrent"; Description: "Run Agent at logon for current user only"; GroupDescription: "Agent autostart mode"; Flags: exclusive

[Dirs]
Name: "{app}"; Permissions: users-modify
Name: "{app}\Agent"; Permissions: users-modify
Name: "{app}\Service"; Permissions: users-modify
Name: "{app}\tools"; Permissions: users-modify

[Files]
; Agent
Source: "..\out\Agent\*"; DestDir: "{app}\Agent"; Flags: recursesubdirs ignoreversion
; Service
Source: "..\out\Service\*"; DestDir: "{app}\Service"; Flags: recursesubdirs ignoreversion
; Tools scripts
Source: "tools\install_agent_task_allusers.ps1"; DestDir: "{app}\tools"; Flags: ignoreversion
Source: "tools\install_agent_task_currentuser.ps1"; DestDir: "{app}\tools"; Flags: ignoreversion
Source: "tools\ensure_agent_autostart.ps1"; DestDir: "{app}\tools"; Flags: ignoreversion
Source: "tools\uninstall_agent_task.ps1"; DestDir: "{app}\tools"; Flags: ignoreversion
Source: "tools\uninstall_agent_run.ps1"; DestDir: "{app}\tools"; Flags: ignoreversion

[Icons]
Name: "{group}\ChildGuard Agent"; Filename: "{app}\Agent\ChildGuard.Agent.exe"; WorkingDir: "{app}\Agent"
Name: "{group}\Uninstall ChildGuard"; Filename: "{uninstallexe}"

[Run]
; Install Windows service (optional)
Filename: "sc.exe"; Parameters: "create ChildGuardService binPath= '""{app}\Service\ChildGuard.Service.exe""' start= auto DisplayName= 'ChildGuard Service'"; Flags: runhidden; Tasks: installservice
Filename: "sc.exe"; Parameters: "description ChildGuardService 'Child activity monitoring service'"; Flags: runhidden; Tasks: installservice
Filename: "sc.exe"; Parameters: "failure ChildGuardService reset= 86400 actions= restart/5000/restart/5000/restart/5000"; Flags: runhidden; Tasks: installservice
Filename: "sc.exe"; Parameters: "start ChildGuardService"; Flags: runhidden; Tasks: installservice
; Ensure Agent autostart (with fallback to HKCU Run if task creation fails)
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -File '""{app}\tools\ensure_agent_autostart.ps1""' -Mode allusers -TaskName 'ChildGuardAgent' -ExePath '""{app}\Agent\ChildGuard.Agent.exe""'"; Flags: runhidden; Tasks: agentforall
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -File '""{app}\tools\ensure_agent_autostart.ps1""' -Mode current -TaskName 'ChildGuardAgent' -ExePath '""{app}\Agent\ChildGuard.Agent.exe""'"; Flags: runhidden; Tasks: agentforcurrent

[UninstallRun]
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -File '""{app}\tools\uninstall_agent_task.ps1""' -TaskName 'ChildGuardAgent'"; Flags: runhidden
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -File '""{app}\tools\uninstall_agent_run.ps1""' -RunName 'ChildGuardAgent'"; Flags: runhidden
Filename: "sc.exe"; Parameters: "stop ChildGuardService"; Flags: runhidden; Tasks: installservice
Filename: "sc.exe"; Parameters: "delete ChildGuardService"; Flags: runhidden; Tasks: installservice







