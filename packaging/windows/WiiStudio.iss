#define AppVersion GetEnv("WIISTUDIO_VERSION")
#define SourceDir GetEnv("WIISTUDIO_PUBLISH_DIR")
#define OutputDir GetEnv("WIISTUDIO_OUTPUT_DIR")
#define IconFile GetEnv("WIISTUDIO_ICON_FILE")

[Setup]
AppId={{6D9AE28D-0AB7-4D41-B42A-9CF8764EF48D}
AppName=WiiStudio
AppVersion={#AppVersion}
AppPublisher=WiiStudio
AppPublisherURL=https://github.com/renatoaleixo/WiiStudio
DefaultDirName={autopf}\WiiStudio
DefaultGroupName=WiiStudio
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=WiiStudioSetup-{#AppVersion}-win-x64
SetupIconFile={#IconFile}
UninstallDisplayIcon={app}\WiiStudio.exe
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ChangesAssociations=no

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na area de trabalho"; GroupDescription: "Atalhos adicionais:"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\WiiStudio"; Filename: "{app}\WiiStudio.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\WiiStudio"; Filename: "{app}\WiiStudio.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\WiiStudio.exe"; Description: "Abrir WiiStudio"; Flags: nowait postinstall skipifsilent
