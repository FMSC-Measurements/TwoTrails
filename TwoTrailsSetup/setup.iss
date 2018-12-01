#define MyAppName "TwoTrails"
#define MyAppVersion "1.0.1"
#define MyAppPublisher "Fortest Management Service Center"
#define MyAppURL "https://www.fs.fed.us/forestmanagement/products/measurement/area-determination/twotrails/"
#define MyAppExeName "TwoTrails.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{8DB997F5-656E-450A-AA05-60EABD190D16}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=TwoTrailsPC
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\TwoTrails\TwoTrails\bin\Release\TwoTrails.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\CSUtilSlim.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\CsvHelper.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\CsvHelper.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\FMSC.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\FMSC.Core.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\FMSC.GeoSpatial.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\GeoAPI.CoordinateSystems.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\GeoAPI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\MaterialDesignColors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\Microsoft.Expression.Interactions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\Microsoft.Win32.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\netstandard.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\NetTopologySuite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\NetTopologySuite.Features.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\NetTopologySuite.IO.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\NetTopologySuite.IO.GeoTools.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\NetTopologySuite.IO.ShapeFile.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\NetTopologySuite.IO.ShapeFile.Extended.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\System.Data.SQLite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\TwoTrails.Core.Windows.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\TwoTrails\TwoTrails\bin\Release\TwoTrails.DAL.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

