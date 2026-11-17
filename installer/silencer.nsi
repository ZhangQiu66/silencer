; silencer NSIS installer
; Per-user install into LocalAppData to keep HKCU Run registration on the
; installing Windows account (avoids elevated-admin HKCU mismatches).
; Does not install into or overwrite "Quiet on the Set".

!define APP_NAME "silencer"
!define APP_VERSION "1.1.0"
!define APP_PUBLISHER "张秋"
!define APP_EXE "silencer.exe"
!define APP_REG_NAME "silencer"
!define APP_DIR_NAME "silencer"

Unicode true
Name "${APP_NAME} ${APP_VERSION}"
OutFile "..\dist\silencer-Setup-${APP_VERSION}.exe"
InstallDir "$LOCALAPPDATA\${APP_DIR_NAME}"
InstallDirRegKey HKCU "Software\${APP_NAME}" "InstallDir"
RequestExecutionLevel user
SetCompressor /SOLID lzma
ShowInstDetails show
ShowUnInstDetails show

VIProductVersion "${APP_VERSION}.0"
VIAddVersionKey "ProductName" "${APP_NAME}"
VIAddVersionKey "FileDescription" "${APP_NAME} installer"
VIAddVersionKey "FileVersion" "${APP_VERSION}"
VIAddVersionKey "ProductVersion" "${APP_VERSION}"
VIAddVersionKey "LegalCopyright" "Copyright (c) 2016 Troy Larson; Copyright (c) 2026 张秋"
VIAddVersionKey "CompanyName" "${APP_PUBLISHER}"

!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "LogicLib.nsh"

!define MUI_ABORTWARNING
!define MUI_ICON "..\QuietOnTheSetUI\Resources\favicon.ico"
!define MUI_UNICON "..\QuietOnTheSetUI\Resources\favicon.ico"

!insertmacro MUI_PAGE_LICENSE "..\LICENSE"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\${APP_EXE}"
!define MUI_FINISHPAGE_RUN_TEXT "安装完成后运行 silencer"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "SimpChinese"
!insertmacro MUI_LANGUAGE "English"

Var AutostartCommand

Section "Install"
  SetOutPath "$INSTDIR"

  ; Payload produced by CI Release build
  File "..\QuietOnTheSetUI\bin\Release\silencer.exe"
  File "..\QuietOnTheSetUI\bin\Release\silencer.exe.config"
  File "..\QuietOnTheSetUI\bin\Release\NAudio.dll"
  File /oname=LICENSE.txt "..\LICENSE"

  WriteUninstaller "$INSTDIR\Uninstall.exe"

  CreateDirectory "$SMPROGRAMS\${APP_NAME}"
  CreateShortCut "$SMPROGRAMS\${APP_NAME}\${APP_NAME}.lnk" "$INSTDIR\${APP_EXE}"
  CreateShortCut "$SMPROGRAMS\${APP_NAME}\卸载 ${APP_NAME}.lnk" "$INSTDIR\Uninstall.exe"

  ; Default: register login autostart for the current (non-elevated) user.
  ; Path is quoted so directories with spaces still work.
  StrCpy $AutostartCommand '"$INSTDIR\${APP_EXE}"'
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "${APP_REG_NAME}" "$AutostartCommand"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "QuietOnTheSet"

  WriteRegStr HKCU "Software\${APP_NAME}" "InstallDir" "$INSTDIR"
  WriteRegStr HKCU "Software\${APP_NAME}" "Version" "${APP_VERSION}"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayName" "${APP_NAME} ${APP_VERSION}"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayIcon" "$INSTDIR\${APP_EXE}"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "Publisher" "${APP_PUBLISHER}"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayVersion" "${APP_VERSION}"
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "NoModify" 1
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "NoRepair" 1

  ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
  IntFmt $0 "0x%08X" $0
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "EstimatedSize" "$0"
SectionEnd

Section "Uninstall"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "${APP_REG_NAME}"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "QuietOnTheSet"

  Delete "$INSTDIR\${APP_EXE}"
  Delete "$INSTDIR\${APP_EXE}.config"
  Delete "$INSTDIR\NAudio.dll"
  Delete "$INSTDIR\LICENSE.txt"
  Delete "$INSTDIR\Uninstall.exe"

  Delete "$SMPROGRAMS\${APP_NAME}\${APP_NAME}.lnk"
  Delete "$SMPROGRAMS\${APP_NAME}\卸载 ${APP_NAME}.lnk"
  RMDir "$SMPROGRAMS\${APP_NAME}"

  ; Keep user settings/logs under %LOCALAPPDATA%\silencer\... except install dir files.
  RMDir "$INSTDIR"

  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"
  DeleteRegKey HKCU "Software\${APP_NAME}"
SectionEnd
