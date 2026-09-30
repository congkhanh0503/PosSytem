; ==============================================================================
; INNO SETUP SCRIPT ĐÓNG GÓI PHẦN MỀM BÁN HÀNG DIROPOS PRO
; ==============================================================================

#define MyAppName "DiroPos PRO"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "DiroPos Smart Retail OS"
#define MyAppURL "https://diropos.vn"
#define MyAppExeName "DiroPos.exe"
#define MyIconFile "Diropos.ico"

[Setup]
AppId={{8F5B3E69-42E1-4D39-A19F-3DF68E2F2026}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName=C:\DiroPos
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename=DiroPos_Setup_v{#MyAppVersion}
OutputDir=installer_output
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupIconFile={#MyIconFile}
UninstallDisplayIcon={app}\{#MyIconFile}
PrivilegesRequired=lowest
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Tạo biểu tượng ngoài màn hình chính (Desktop)"; GroupDescription: "Tùy chọn bổ sung:"
Name: "startupicon"; Description: "Tự động khởi động DiroPos khi mở máy tính mỗi sáng"; GroupDescription: "Tự động khởi động:"

[Files]
; Copy toàn bộ file đã publish từ thư mục publish/
Source: "backend\DiroPos.Api\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Copy icon nhận diện thương hiệu
Source: "{#MyIconFile}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Shortcut ngoài Desktop
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyIconFile}"; Tasks: desktopicon
; Shortcut trong Thư Mục Startup (Tự chạy khi khởi động Windows)
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyIconFile}"; Tasks: startupicon
; Shortcut trong Start Menu
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyIconFile}"
Name: "{group}\Gỡ Cài Đặt {#MyAppName}"; Filename: "{uninstallexe}"

[Run]
; Tự động chạy phần mềm ngay sau khi cài đặt hoàn tất
Filename: "{app}\{#MyAppExeName}"; Description: "Khởi chạy {#MyAppName} ngay bây giờ"; Flags: nowait postinstall skipifsilent
