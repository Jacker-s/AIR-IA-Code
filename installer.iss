#define AppName "AIR IA Code"
#define AppVersion "0.14.2"
#define AppPublisher "Codename Jackers"
#define AppExeName "AIRIACode.exe"

[Setup]
AppId={{9B621F43-A1C7-47D6-8D09-72BBEC488BC1}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={localappdata}\Programs\AIR IA Code
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=installer
OutputBaseFilename=AIRIACodeSetup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#AppExeName}
SetupIconFile=Assets\AIR-IACode.ico
VersionInfoCompany=Codename Jackers
VersionInfoDescription=AIR IA Code - IA local para programação e mídia
VersionInfoCopyright=Copyright (C) 2026 Codename Jackers
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Files]
Source: "bin\Release\net9.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na Área de Trabalho"

[Run]
Filename: "{cmd}"; Parameters: "/c winget install --id Microsoft.DotNet.DesktopRuntime.9 --exact --accept-package-agreements --accept-source-agreements --disable-interactivity"; StatusMsg: "Verificando .NET Desktop Runtime..."; Flags: runhidden waituntilterminated
Filename: "{cmd}"; Parameters: "/c winget install --id Microsoft.VCRedist.2015+.x64 --exact --accept-package-agreements --accept-source-agreements --disable-interactivity"; StatusMsg: "Instalando componentes do Windows..."; Flags: runhidden waituntilterminated
Filename: "{cmd}"; Parameters: "/c winget install --id Google.PlatformTools --exact --accept-package-agreements --accept-source-agreements --disable-interactivity"; StatusMsg: "Instalando Android SDK Platform-Tools (ADB e Logcat)..."; Flags: runhidden waituntilterminated
Filename: "{cmd}"; Parameters: "/c winget install --id ggml.llamacpp --exact --accept-package-agreements --accept-source-agreements --disable-interactivity"; StatusMsg: "Instalando motor local Vulkan..."; Flags: runhidden waituntilterminated
Filename: "{cmd}"; Parameters: "/c winget install --id Git.Git --exact --accept-package-agreements --accept-source-agreements --disable-interactivity"; StatusMsg: "Instalando Git para o motor de mídia local..."; Flags: runhidden waituntilterminated
Filename: "{cmd}"; Parameters: "/c winget install --id Python.Python.3.12 --exact --accept-package-agreements --accept-source-agreements --disable-interactivity"; StatusMsg: "Instalando Python para geração local..."; Flags: runhidden waituntilterminated
Filename: "{cmd}"; Parameters: "/c winget install --id Gyan.FFmpeg --exact --accept-package-agreements --accept-source-agreements --disable-interactivity"; StatusMsg: "Instalando FFmpeg para vídeos..."; Flags: runhidden waituntilterminated
Filename: "{app}\{#AppExeName}"; Description: "Abrir {#AppName}"; Flags: nowait postinstall skipifsilent
