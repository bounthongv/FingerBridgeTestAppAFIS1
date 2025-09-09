; FingerprintBridge Installer Script

[Setup]
AppName=Fingerprint Bridge
AppVersion=1.0.0
DefaultDirName={autopf}\FingerprintBridge
DefaultGroupName=Fingerprint Bridge
OutputDir=D:\AratekTrustFinger\FingerBridgeTestAppAFIS1\publish\install
OutputBaseFilename=FingerprintBridgeSetup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
DisableProgramGroupPage=yes
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "D:\AratekTrustFinger\FingerBridgeTestAppAFIS1\publish\*"; \
        DestDir: "{app}"; \
        Excludes: "install\*"; \
        Flags: ignoreversion recursesubdirs createallsubdirs
; Include config.sys explicitly (if it's not already included by wildcard)
Source: "D:\AratekTrustFinger\FingerBridgeTestAppAFIS1\config.sys"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Fingerprint Bridge"; Filename: "{app}\FingerBridge.exe"
Name: "{commondesktop}\Fingerprint Bridge"; Filename: "{app}\FingerBridge.exe"; Tasks: desktopicon
Name: "{group}\Uninstall Fingerprint Bridge"; Filename: "{uninstallexe}"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional icons:"; Flags: checkedonce

[Run]
Filename: "{app}\FingerBridge.exe"; Description: "Launch Fingerprint Bridge"; Flags: nowait postinstall skipifsilent