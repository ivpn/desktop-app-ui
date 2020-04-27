; IVPN Client Installer
; Install script for NSIS 2.x

SetCompressor lzma

; -----------------
; include Modern UI
; -----------------

!include "MUI.nsh"
!include "LogicLib.nsh"
!include "StrFunc.nsh"
!include "x64.nsh"
!include "WinVer.nsh"
; include for some of the windows messages defines
!include "winmessages.nsh"

${StrLoc}

; -------
; general
; -------

; SOURCE_DIR is defined in build.bat

!define PRODUCT_NAME "IVPN Client"
!define PRODUCT_IDENTIFIER "IVPN Client"
!define PRODUCT_PUBLISHER "IVPN Limited"

!define APP_RUN_PATH "$INSTDIR\IVPN Client.exe"
!define PROCESS_NAME "IVPN Client.exe"
!define IVPN_SERVICE_NAME "IVPN Client"

; The following variables will be set from the build.bat script
; !define PRODUCT_VERSION "2.0-b4"
; !define OUT_FILE "bin\${PRODUCT_NAME} ${PRODUCT_VERSION}.exe"

Name "${PRODUCT_NAME}"
OutFile "${OUT_FILE}"
InstallDir "$PROGRAMFILES64\${PRODUCT_IDENTIFIER}"
;InstallDirRegKey HKLM "Software\${PRODUCT_IDENTIFIER}" ""
RequestExecutionLevel admin

; HKLM (all users)
;!define env_hklm 'HKLM "SYSTEM\CurrentControlSet\Control\Session Manager\Environment"'
; HKCU (current user) 
!define env_hkcu 'HKCU "Environment"'

; ---------
; variables
; ---------

var /GLOBAL StartMenuFolder
var /GLOBAL BitDir

Var HEADLINE_FONT

;---------------------------

; StrContains
; This function does a case sensitive searches for an occurrence of a substring in a string. 
; It returns the substring if it is found. 
; Otherwise it returns null(""). 
; Written by kenglish_hi
; Adapted from StrReplace written by dandaman32
Var STR_HAYSTACK
Var STR_NEEDLE
Var STR_CONTAINS_VAR_1
Var STR_CONTAINS_VAR_2
Var STR_CONTAINS_VAR_3
Var STR_CONTAINS_VAR_4
Var STR_RETURN_VAR
 
Function StrContains
  Exch $STR_NEEDLE
  Exch 1
  Exch $STR_HAYSTACK
  ; Uncomment to debug
  ;MessageBox MB_OK 'STR_NEEDLE = $STR_NEEDLE STR_HAYSTACK = $STR_HAYSTACK '
    StrCpy $STR_RETURN_VAR ""
    StrCpy $STR_CONTAINS_VAR_1 -1
    StrLen $STR_CONTAINS_VAR_2 $STR_NEEDLE
    StrLen $STR_CONTAINS_VAR_4 $STR_HAYSTACK
    loop:
      IntOp $STR_CONTAINS_VAR_1 $STR_CONTAINS_VAR_1 + 1
      StrCpy $STR_CONTAINS_VAR_3 $STR_HAYSTACK $STR_CONTAINS_VAR_2 $STR_CONTAINS_VAR_1
      StrCmp $STR_CONTAINS_VAR_3 $STR_NEEDLE found
      StrCmp $STR_CONTAINS_VAR_1 $STR_CONTAINS_VAR_4 done
      Goto loop
    found:
      StrCpy $STR_RETURN_VAR $STR_NEEDLE
      Goto done
    done:
   Pop $STR_NEEDLE ;Prevent "invalid opcode" errors and keep the
   Exch $STR_RETURN_VAR  
FunctionEnd
 
!macro _StrContainsConstructor OUT NEEDLE HAYSTACK
  Push `${HAYSTACK}`
  Push `${NEEDLE}`
  Call StrContains
  Pop `${OUT}`
!macroend
 
!define StrContains '!insertmacro "_StrContainsConstructor"'

;---------------------------
!define StrRepl "!insertmacro StrRepl"
!macro StrRepl output string old new
    Push `${string}`
    Push `${old}`
    Push `${new}`
    !ifdef __UNINSTALL__
        Call un.StrRepl
    !else
        Call StrRepl
    !endif
    Pop ${output}
!macroend
 
!macro Func_StrRepl un
    Function ${un}StrRepl
        Exch $R2 ;new
        Exch 1
        Exch $R1 ;old
        Exch 2
        Exch $R0 ;string
        Push $R3
        Push $R4
        Push $R5
        Push $R6
        Push $R7
        Push $R8
        Push $R9
 
        StrCpy $R3 0
        StrLen $R4 $R1
        StrLen $R6 $R0
        StrLen $R9 $R2
        loop:
            StrCpy $R5 $R0 $R4 $R3
            StrCmp $R5 $R1 found
            StrCmp $R3 $R6 done
            IntOp $R3 $R3 + 1 ;move offset by 1 to check the next character
            Goto loop
        found:
            StrCpy $R5 $R0 $R3
            IntOp $R8 $R3 + $R4
            StrCpy $R7 $R0 "" $R8
            StrCpy $R0 $R5$R2$R7
            StrLen $R6 $R0
            IntOp $R3 $R3 + $R9 ;move offset by length of the replacement string
            Goto loop
        done:
 
        Pop $R9
        Pop $R8
        Pop $R7
        Pop $R6
        Pop $R5
        Pop $R4
        Pop $R3
        Push $R0
        Push $R1
        Pop $R0
        Pop $R1
        Pop $R0
        Pop $R2
        Exch $R1
    FunctionEnd
!macroend
!insertmacro Func_StrRepl ""
!insertmacro Func_StrRepl "un."
;---------------------------

!macro COMMON_INIT
  StrCpy $StartMenuFolder "IVPN"

  ${If} ${RunningX64}
    SetRegView 64
    StrCpy $BitDir "x86_64"
  ${Else}
    SetRegView 32
    StrCpy $BitDir "x86"
  ${EndIf}

  DetailPrint "Running on architecture: $BitDir"
!macroend

Function .onInit
  !insertmacro COMMON_INIT
 
  CreateFont $HEADLINE_FONT "$(^Font)" "12" "600"

  Call CheckOSSupported
  
  ; hack to allow uninstallation of beta
  SetRegView 32
  ReadRegStr $R0 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\IVPN Client" "UninstallString"
  StrCmp $R0 "" done

  ; hack to not prompt for 0.9 release
  ReadRegStr $R1 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\IVPN Client" "DisplayVersion"
  StrCpy $R1 '$R1' 3
  StrCmp $R1 "0.9" done
  StrCmp $R1 "1.0" done
  StrCmp $R1 "1.1" done
  StrCmp $R1 "1.2" done
  StrCmp $R1 "1.3" done
  StrCmp $R1 "1.4" done
  StrCmp $R1 "1.5" done
  StrCmp $R1 "1.6" done
  StrCmp $R1 "1.7" done
  StrCmp $R1 "1.8" done
  StrCmp $R1 "1.9" done
  ; TODO: remove above for 2.x release

  ClearErrors
  DetailPrint "Uninstalling old version..."
  IfSilent uninst is_not_quiet
is_not_quiet:
  MessageBox MB_OKCANCEL|MB_ICONEXCLAMATION "${PRODUCT_NAME} is already installed.$\n$\nClick OK to uninstall the old version." IDOK uninst
  Abort
uninst:
  ExecWait '$R0 _?=$INSTDIR'
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\IVPN Client"
done:

  ${If} ${RunningX64}
    SetRegView 64
    StrCpy $BitDir "x86_64"
  ${Else}
    SetRegView 32
    StrCpy $BitDir "x86"
  ${EndIf}

FunctionEnd

Function un.onInit
  !insertmacro COMMON_INIT
FunctionEnd

; --------------
; user interface
; --------------


!define MUI_HEADERIMAGE 
!define MUI_HEADERIMAGE_RIGHT
!define MUI_HEADERIMAGE_BITMAP "header.bmp"

!define MUI_ICON "application.ico"
!define MUI_UNICON "application.ico"

!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN "$INSTDIR\IVPN Client.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Run ${PRODUCT_NAME} now"
!define MUI_FINISHPAGE_RUN_FUNCTION ExecAppFile

; Checkbox on finish page: create shortcut on desktop 
; using unused 'readme' check box for this 
!define MUI_FINISHPAGE_SHOWREADME ""
!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_FINISHPAGE_SHOWREADME_TEXT "Create a desktop shortcut"
!define MUI_FINISHPAGE_SHOWREADME_FUNCTION finishpageaction
Function finishpageaction
CreateShortcut "$desktop\IVPN Client.lnk" "${APP_RUN_PATH}"
FunctionEnd



LicenseForceSelection checkbox "I Agree"

!define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKLM" 
!define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\${PRODUCT_IDENTIFIER}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"

!define MUI_WELCOMEPAGE_TITLE "Welcome to the ${PRODUCT_NAME} v.${PRODUCT_VERSION} Setup Wizard"

!insertmacro MUI_DEFAULT MUI_WELCOMEFINISHPAGE_BITMAP "startfinish.bmp"
!insertmacro MUI_DEFAULT MUI_UNWELCOMEFINISHPAGE_BITMAP "startfinish.bmp"

!define MUI_ABORTWARNING

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE License.txt
;!insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder
!insertmacro MUI_PAGE_INSTFILES

;===============================
; FINISH page modification
!define MUI_PAGE_CUSTOMFUNCTION_PRE fin_pre
!define MUI_PAGE_CUSTOMFUNCTION_SHOW fin_show
!define MUI_PAGE_CUSTOMFUNCTION_LEAVE fin_leave
;===============================
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

;===============================  
; FINISH page modification handlers
Function fin_show
	ReadINIStr $0 "$PLUGINSDIR\iospecial.ini" "Field 6" "HWND"
	SetCtlColors $0 0x000000 0xFFFFFF
FunctionEnd

Function fin_pre
	WriteINIStr "$PLUGINSDIR\iospecial.ini" "Settings" "NumFields" "6"
	WriteINIStr "$PLUGINSDIR\iospecial.ini" "Field 6" "Type" "CheckBox"
	WriteINIStr "$PLUGINSDIR\iospecial.ini" "Field 6" "Text" "Add IVPN CLI binary to the path"
	WriteINIStr "$PLUGINSDIR\iospecial.ini" "Field 6" "Left" "120"
	WriteINIStr "$PLUGINSDIR\iospecial.ini" "Field 6" "Right" "315"
	WriteINIStr "$PLUGINSDIR\iospecial.ini" "Field 6" "Top" "130"
	WriteINIStr "$PLUGINSDIR\iospecial.ini" "Field 6" "Bottom" "140"
	WriteINIStr "$PLUGINSDIR\iospecial.ini" "Field 6" "State" "0"
FunctionEnd

Function fin_leave
	ReadINIStr $0 "$PLUGINSDIR\iospecial.ini" "Field 6" "State"
	StrCmp $0 "0" end
	 
	; UPDATING %PATH% VARIABLE 
	ReadRegStr $0 ${env_hkcu} "PATH"
	
	; check if PATH already updated
	${StrContains} $1 "$INSTDIR" $0
	StrCmp $1 "$INSTDIR" end ; do nothing	
	
	; set variable for local machine
	StrCpy $0 "$0;$INSTDIR"
	WriteRegExpandStr ${env_hkcu} PATH "$0"

	; make sure windows knows about the change
	SendMessage ${HWND_BROADCAST} ${WM_WININICHANGE} 0 "STR:Environment" /TIMEOUT=100
	
	end:
FunctionEnd
;===============================

; ------------------
; installer sections
; ------------------

!define DEVCON_BASENAME "devcon.exe"
!define PRODUCT_TAP_WIN_COMPONENT_ID "tapivpn"
;!define PRODUCT_WINTUN_COMPONENT_ID "Wintun"

Section "${PRODUCT_NAME}" SecIVPN
  SetOutPath "$INSTDIR"

  ; Stop IVPN service
  stopservcice:  
  Call StopService
  Pop $0 ; 1 - SUCCESS;
  ${if} $0 != 1
		DetailPrint "ERROR: Failed to stop 'IVPN Client' service."
		MessageBox MB_ABORTRETRYIGNORE|MB_ICONEXCLAMATION "Failed to stop 'IVPN Client' service.$\nIgnoring this problem can lead to issues with IVPN Client software in the future." IDRETRY stopservcice IDIGNORE ignoreservicestop
		DetailPrint "Aborted"
		Abort
  ${EndIf}
  ignoreservicestop:
  
  
  ; When service stopping - IVPN Client must also Close automatically
  ; anyway, there could be situations when IVPN Client not connected to service (cannot receive 'service exiting' notification.)
  ; Therefore, here we try to stop IVPN Client process manually.
  ; Stop IVPN Client application
  stopclient:
  Call StopClient
  Pop $0 ; 1 - SUCCESS
  ${if} $0 != 1
		DetailPrint "ERROR: Failed to stop 'IVPN Client' application."
		MessageBox MB_ABORTRETRYIGNORE|MB_ICONEXCLAMATION "Failed to stop 'IVPN Client' application.$\nIgnoring this problem can lead to issues with IVPN Client software in the future." IDRETRY stopclient IDIGNORE ignoreclientstop
		DetailPrint "Aborted"
		Abort
  ${EndIf}
  ignoreclientstop:
    
  ; check is library can be overwritten
  Push "$INSTDIR\IVPN Firewall Native.dll" ; file to check for writting
  Push 15000 ; 15 seconds
  Call WaitFileOpenForWritting
  
  ; check is library can be overwritten
  Push "$INSTDIR\IVPN Helpers Native.dll" ; file to check for writting
  Push 15000 ; 15 seconds
  Call WaitFileOpenForWritting
	
  ; extract all files from source dir (it is important that IVPN Client Application must be stopped on this moment)
  File /r "${SOURCE_DIR}\*.*"

  CreateDirectory "$INSTDIR\log"
  File "dotNetFx45_Full_setup.exe"

  Call InstallDotNet4IfNeeded

  ${If} ${RunningX64}
    SetRegView 64
  ${Else}
    SetRegView 32
  ${EndIf}

  WriteRegStr HKLM "Software\${PRODUCT_IDENTIFIER}" "" $INSTDIR
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_IDENTIFIER}" "DisplayName" "${PRODUCT_NAME}"
  WriteRegExpandStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_IDENTIFIER}" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_IDENTIFIER}" "DisplayIcon" "$INSTDIR\icon.ico"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_IDENTIFIER}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_IDENTIFIER}" "Publisher" "${PRODUCT_PUBLISHER}"

  DeleteRegValue HKLM "Software\Microsoft\Windows\CurrentVersion\Run" "IVPN Client Runtime Warmup"

  ;!insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder

  ;!insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
    CreateShortCut "$SMPROGRAMS\$StartMenuFolder\Uninstall ${PRODUCT_NAME}.lnk" "$INSTDIR\Uninstall.exe"
  ;!insertmacro MUI_STARTMENU_WRITE_END

  ; create shortcut
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\${PRODUCT_NAME}.lnk" "$INSTDIR\IVPN Client.exe"
  
  ; Remove files from old installations
  Delete "$INSTDIR\OpenVPN\x86_64\tap\OemWin2k.inf"
  Delete "$INSTDIR\OpenVPN\x86\tap\OemWin2k.inf"
  Delete "$INSTDIR\up.bat"
  Delete "$INSTDIR\down.bat"
  Delete "$INSTDIR\OpenVPN\x86\libeay32.dll"
  Delete "$INSTDIR\OpenVPN\x86\ssleay32.dll"
  Delete "$INSTDIR\OpenVPN\x86_64\libeay32.dll"
  Delete "$INSTDIR\OpenVPN\x86_64\ssleay32.dll"
  RMDir /r "$INSTDIR\WireGuard\x86\Wintun"
  RMDir /r "$INSTDIR\WireGuard\x86_64\Wintun"
   
  ; Remove files which are not in use for current platform
  ${If} ${RunningX64}
	RMDir /r "$INSTDIR\devcon\x86"
	RMDir /r "$INSTDIR\WireGuard\x86"
	; RMDir /r "$INSTDIR\OpenVPN\x86"
  ${Else}
	RMDir /r "$INSTDIR\devcon\x86_64"
	RMDir /r "$INSTDIR\WireGuard\x86_64"
	; RMDir /r "$INSTDIR\OpenVPN\x86_64" 
  ${EndIf}
  
  Call CheckIsWin7DriverInstalled
  
  ; ============ TAP driver ======================================================================
  DetailPrint "Installing TAP driver..."
  
  ; check if TUN/TAP driver is installed  
  IntOp $R5 0 & 0
  nsExec::ExecToStack '"$INSTDIR\devcon\$BitDir\${DEVCON_BASENAME}" hwids ${PRODUCT_TAP_WIN_COMPONENT_ID}'
  Pop $R0 # return value/error/timeout
  IntOp $R5 $R5 | $R0
  DetailPrint "${DEVCON_BASENAME} hwids returned: $R0"

  ; if output contains the component id, then it's installed already
  Push "${PRODUCT_TAP_WIN_COMPONENT_ID}"
  Push ">"
  Call StrLoc
  Pop $R0

  ; if it's installed, do an update
  ${If} $R5 == 0
    ${If} $R0 == ""
      StrCpy $R1 "install"
    ${Else}
      StrCpy $R1 "update"
    ${EndIf}

    DetailPrint "TAP $R1 (${PRODUCT_TAP_WIN_COMPONENT_ID}) (May require confirmation)"
    nsExec::ExecToLog '"$INSTDIR\devcon\$BitDir\${DEVCON_BASENAME}" $R1 "$INSTDIR\OpenVPN\$BitDir\tap\OemVista.inf" ${PRODUCT_TAP_WIN_COMPONENT_ID}'
    Pop $R0 # return value/error/timeout

    ${If} $R0 == ""
      IntOp $R0 0 & 0
      SetRebootFlag true
      DetailPrint "REBOOT flag set"
    ${EndIf}	

    IntOp $R5 $R5 | $R0
    DetailPrint "${DEVCON_BASENAME} returned: $R0"
  ${EndIf}

  DetailPrint "${DEVCON_BASENAME} cumulative status: $R5"

  ${If} $R5 != 0
    MessageBox MB_OK "An error occurred installing the TAP device driver."
    Abort
  ${EndIf}

  ; ============ Wintun driver ======================================================================
  DetailPrint "Installing Wintun driver..."

  nsExec::ExecToStack '"$SYSDIR\msiexec.exe" /i "$INSTDIR\WireGuard\$BitDir\WintunInstaller_$BitDir.msi" /qn'
  Pop $R0 # return value/error/timeout	
  Pop $R1
  IntOp $R5 0 | $R0
  ${If} $R5 != 0 
	DetailPrint "Error installing Wintun driver [$R0] : $R1"
    MessageBox MB_OK "An error occurred installing the Wintun driver."
    Abort
  ${EndIf}
  
  ; ============ Service ======================================================================
  ; install service
  DetailPrint "Installing IVPN Client service..."
  nsExec::ExecToLog '"$SYSDIR\sc.exe" create "IVPN Client" binPath= "\"$INSTDIR\IVPN Service.exe\"" start= auto'
  nsExec::ExecToLog '"$SYSDIR\sc.exe" sdset "IVPN Client" "D:(A;;CCLCSWRPWPDTLOCRRC;;;SY)(A;;CCDCLCSWRPWPDTLOCRSDRCWDWO;;;BA)(A;;CCLCSWLOCRRC;;;IU)(A;;CCLCSWLOCRRC;;;SU)(A;;RPWPDTLO;;;S-1-1-0)"'

  ; add service to firewall
  ;nsExec::ExecToLog '"$SYSDIR\netsh.exe" firewall add allowedprogram "$INSTDIR\IVPN Service.exe" "IVPN Service" ENABLE'

  ; start service
  DetailPrint "Starting IVPN Client service..."
  nsExec::ExecToLog '"$SYSDIR\sc.exe" start "IVPN Client"'
SectionEnd

; -----------
; uninstaller
; -----------

Section "Uninstall"
  ; stop service
  nsExec::ExecToLog '"$SYSDIR\sc.exe" stop "IVPN Client"'

  ; wait a little (give change for IVPN Client application to stop)
  Sleep 1500
  ; When service stopping - IVPN Client must also Close automatically
  ; anyway, there could be situations when IVPN Client not connected to service (cannot receive 'service exiting' notification.)
  ; Therefore, here we try to stop IVPN Client process manually.
  nsExec::ExecToStack "taskkill /IM $\"${PROCESS_NAME}$\""
  ; give some time to stop the process 
  Sleep 1500

  ; remove service
  nsExec::ExecToLog '"$SYSDIR\sc.exe" delete "IVPN Client"'

  ; removing firewall rules
  nsExec::ExecToLog '"$INSTDIR\ivpncli.exe" firewall disable'

  ; uninstall TUN/TAP driver
  DetailPrint "Removing TUN/TAP device..."

  nsExec::ExecToLog '"$INSTDIR\devcon\$BitDir\${DEVCON_BASENAME}" remove ${PRODUCT_TAP_WIN_COMPONENT_ID}'
  Pop $R0 # return value/error/timeout
  DetailPrint "${DEVCON_BASENAME} remove returned: $R0"

  ; uninstall Wintun
  ;DetailPrint "Removing Wintun device..."
  ;nsExec::ExecToLog '"$INSTDIR\devcon\$BitDir\${DEVCON_BASENAME}" remove ${PRODUCT_WINTUN_COMPONENT_ID}'
  ;Pop $R0 # return value/error/timeout
  ;DetailPrint "${DEVCON_BASENAME} remove returned: $R0"
  
  DetailPrint "Removing files..."
  SetShellVarContext current ; To be able to get environment variables of current user (like "$LOCALAPPDATA")   
  ; remove all
  
  Delete "$desktop\IVPN Client.lnk"
  Delete "$INSTDIR\Uninstall.exe"
  RMDir /r "$INSTDIR\etc"
  RMDir /r "$INSTDIR\log"
  RMDir /r "$INSTDIR\devcon"
  RMDir /r "$INSTDIR\OpenVPN"
  RMDir /r "$INSTDIR\WireGuard" 
  RMDir /r "$INSTDIR\Resources"
  RMDir /r "$INSTDIR\de"
  RMDir /r "$INSTDIR\en"
  RMDir /r "$INSTDIR\es"
  RMDir /r "$INSTDIR\etc"
  RMDir /r "$INSTDIR\fr"
  RMDir /r "$INSTDIR\it"
  RMDir /r "$INSTDIR\ja"
  RMDir /r "$INSTDIR\ko"
  RMDir /r "$INSTDIR\zh-Hans"
  RMDir /r "$INSTDIR\zh-Hant" 

  RMDir /r "$LOCALAPPDATA\IVPN"
  RMDir /r "$LOCALAPPDATA\IVPN_Limited"

  Delete "$INSTDIR\*.config"
  Delete "$INSTDIR\*.xml"
  Delete "$INSTDIR\*.bat"
  Delete "$INSTDIR\*.vbs"
  Delete "$INSTDIR\*.dll"
  Delete "$INSTDIR\*.exe"
  Delete "$INSTDIR\*.ico"
  RMDir "$INSTDIR"

  ;!insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuFolder
  StrCpy $StartMenuFolder "IVPN"

  Delete "$SMPROGRAMS\$StartMenuFolder\Uninstall ${PRODUCT_NAME}.lnk"
  Delete "$SMPROGRAMS\$StartMenuFolder\${PRODUCT_NAME}.lnk"
  RMDir "$SMPROGRAMS\$StartMenuFolder"
  DeleteRegKey /ifempty HKLM "Software\${PRODUCT_IDENTIFIER}"
  DeleteRegValue HKLM "Software\Microsoft\Windows\CurrentVersion\Run" "IVPN Client Runtime Warmup"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_IDENTIFIER}"

  ; UPDATING %PATH% VARIABLE 
  ; read PATH variable value
  ReadRegStr $0 ${env_hkcu} "PATH"
  ; remove all references to $INSTDIR
  ${StrRepl} $1 $0 "$INSTDIR;" "" 
  ${StrRepl} $1 $1 ";$INSTDIR" "" 
  ${StrRepl} $1 $1 "$INSTDIR" "" 
  ${If} $1 != $0
	WriteRegExpandStr ${env_hkcu} PATH "$1"
	; make sure windows knows about the change
	SendMessage ${HWND_BROADCAST} ${WM_WININICHANGE} 0 "STR:Environment" /TIMEOUT=100    
  ${EndIf}
  
SectionEnd

; ----------------
; helper functions
; ----------------

Function CheckOSSupported
    ${If} ${AtLeastWinVista}
        goto end
    ${EndIf}

    MessageBox MB_ICONSTOP|MB_OK "Unsupported Windows Version.$\nThis version of IVPN Client can only be installed on Windows Vista and above."
    Quit
end:
FunctionEnd

; Return values:
;	<0 - Error
;	0 - NOT STOPPED
; 	1 - Stopped (SECCUSS)
Function StopService
	DetailPrint "Checking is IVPN Client service is running..."
	Call IsServiceStopped	
	Pop $0	
	${If} $0 == 1	
		Push 1 ; Stopped OK
		Return 	
	${EndIf}		

	DetailPrint "Stopping IVPN Client service..."

	; stop service
	nsExec::ExecToStack '"$SYSDIR\sc.exe" stop "${IVPN_SERVICE_NAME}"'
	Pop $0 ; Return
	Pop $1 ; Output	
	${If} $0 == '1060'
		DetailPrint "IVPN Client service does not exist as an installed service [1060]"
		Push 1 		; Stopped OK
		Return 	
	${EndIf}	
	${If} $0 != '0'
		DetailPrint "Failed to execute 'sc stop' command: $0; $1"
		Goto killservice 	
	${EndIf}	
	
	; R1 - counter
	StrCpy	$R1 0	
	; waiting to stop 8 seconds (500ms*16) 
	${While} $R1 < 16		
		Sleep 500
		IntOp $R1 $R1 + 1
		
		Call IsServiceStopped 	
		Pop $0		
		${If} $0 < 0
			Goto killservice
		${EndIf}			
		${If} $0 == 1
			Push 1 ; stooped OK
			Return 
		${EndIf}
		
	${EndWhile}
		
	killservice:	
	; if we still here - service still not stopped. Killing it manually
	DetailPrint "WARNING: Unable to stop service. Killing process ..."
	nsExec::ExecToStack 'taskkill /fi "Services eq ${IVPN_SERVICE_NAME}" /F'
	Pop $0 ; Return
	Pop $1 ; Output		
	${If} $0 < 0
		DetailPrint "Failed to execute 'taskkill' command: $0; $1"
		Push -1 ; Error
		Return 	
	${EndIf}	
		
	Sleep 500
		
	Call IsServiceStopped 	
	Pop $0	
	${If} $0 < 0
		Push -1 ; Error
		Return 	
	${EndIf}			
	${If} $0 == 1
		Push 1 ; stooped OK
		Return 
	${EndIf}
	
	Push 0 ; if we are here, service is NOT STOPPED		
FunctionEnd

Function IsServiceStopped
	nsExec::ExecToStack '"$SYSDIR\sc.exe" query "${IVPN_SERVICE_NAME}"'
	Pop $0 ; Return
	Pop $1 ; Output	
	${If} $0 == '1060'
		DetailPrint "IVPN Client service does not exist as an installed service [1060]"
		Push 1 		; Stopped OK
		Return 	
	${EndIf}	
	${If} $0 != '0'
		DetailPrint "Failed to execute 'sc query' command: $0; $1"
		Push -1 ; Error
		Return 	
	${EndIf}	
		
	; An example of an expected result:
	; 	SERVICE_NAME: IVPN Client
    ;    TYPE               : 10  WIN32_OWN_PROCESS
    ;    STATE              : 4  RUNNING
    ;                            (STOPPABLE, NOT_PAUSABLE, ACCEPTS_SHUTDOWN)
    ;    WIN32_EXIT_CODE    : 0  (0x0)
    ;    SERVICE_EXIT_CODE  : 0  (0x0)
    ;    CHECKPOINT         : 0x0
    ;    WAIT_HINT          : 0x0
	
	; Another example:
	;	SERVICE_NAME: [service_name]
    ;    TYPE               : 10  WIN32_OWN_PROCESS
    ;    STATE              : 1  STOPPED
    ;    WIN32_EXIT_CODE    : 0  (0x0)
    ;    SERVICE_EXIT_CODE  : 0  (0x0)
    ;    CHECKPOINT         : 0x0
    ;    WAIT_HINT          : 0x0
		
	${StrContains} $0 "STOPPED" $1
	${If} $0 == "STOPPED"	
		Push 1 		; Stopped OK
		Return 	
	${EndIf}	
	
	Push 0 ; if we are here, service is NOT STOPPED	
FunctionEnd

; Return values:
;	<0 - Error
;	0 - NOT STOPPED
; 	1 - Stopped (SECCUSS)
Function StopClient
	DetailPrint "Checking is IVPN Client application is running..."	
	Call IsClientStopped
	Pop $0	
	${If} $0 == 1	
		Push 1 ; Stopped OK
		Return 	
	${EndIf}		
	
	DetailPrint "Terminating IVPN Client application..."
	
	; stop client
	nsExec::ExecToStack "taskkill /IM $\"${PROCESS_NAME}$\" /T /F"
	Pop $0 ; Return
	Pop $1 ; Output	
	${If} $0 != '0'
		DetailPrint "Failed to execute taskkill command: $0; $1"		
	${EndIf}
	
	; R1 - counter
	StrCpy	$R1 0	
	; waiting to stop 3 seconds (500ms*6) 
	${While} $R1 < 6		
		Sleep 500
		IntOp $R1 $R1 + 1
		
		Call IsClientStopped 	
		Pop $0		
		${If} $0 < 0
			Push -1 ; Error
			Return 	
		${EndIf}			
		${If} $0 == 1	
			Push 1 ; Stopped OK
			Return 	
		${EndIf}	
		
	${EndWhile}
		
	Push 0 ; Not stopped			
FunctionEnd

Function IsClientStopped
	nsExec::ExecToStack 'tasklist /FI "IMAGENAME eq ${PROCESS_NAME}"'
	Pop $0 ; Return
	Pop $1 ; Output			
	${If} $0 != '0'
		DetailPrint "Failed to execute tasklist command: $0; $1"
		Push -1 ; return execution error
		Return
	${EndIf}
			
	${StrContains} $0 "${PROCESS_NAME}" $1
	${If} $0 == ""
		Push 1 ; stopped
		Return
	${EndIf}
	
	Push 0	; running
FunctionEnd

Function WaitFileOpenForWritting
	Pop $1 ; wait milliseconds
	Pop $0 ; filname
	
	StrCpy	$R1 0	
	${While} $R1 < $1	
		FileOpen $4 "$0" w
		FileClose $4
		
		${If} $4 > 0
			Return 	
		${EndIf}			
				
		DetailPrint "File '$0' is in use. Waiting..."
		
		Sleep 1000
		IntOp $R1 $R1 + 1000	
	${EndWhile}
FunctionEnd


Function InstallDotNet4IfNeeded
  Var /GLOBAL dotNetVersion
  Var /GLOBAL installerArgs
  Var /GLOBAL EXIT_CODE

  ; Determine the .NET Framework 4.5 installation as described in document:
  ; http://msdn.microsoft.com/ru-ru/library/hh925568%28v=vs.110%29.aspx#net_b
  ReadRegStr $dotNetVersion HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"

  ${If} $dotNetVersion == ""
    DetailPrint "Running .NET Framework 4.5 installer..."

    IfSilent is_quiet is_not_quiet
    is_quiet:
      StrCpy $installerArgs "/q /norestart"
      Goto run_installer
    is_not_quiet:
      StrCpy $installerArgs "/showrmui /passive /norestart"
      Goto run_installer
    run_installer:
      ExecWait '"$INSTDIR\dotNetFx45_Full_setup.exe" $installerArgs' $EXIT_CODE
      ${If} $EXIT_CODE = 1641
      ${OrIf} $EXIT_CODE = 3010
        SetRebootFlag true
      ${EndIf}
  ${EndIf}


  DetailPrint "Removing temporary installation files..."
  Delete "$INSTDIR\dotNetFx45_Full_setup.exe"
FunctionEnd

Function ExecAppFile    
    Exec "${APP_RUN_PATH}"
    Sleep 500

    StrCpy $R1 0
    ${While} $R1 < 50  ; Wait application launch for 5 seconds max
        IntOp $R1 $R1 + 1
        System::Call user32::GetForegroundWindow()i.r0

        ${If} $0 != $hwndparent
            Return
        ${EndIf}

        Sleep 100
    ${EndWhile}

FunctionEnd  

; For Windows 7 there is requirements:
; - Windows7 SP1 should be installed
; - security update KB3033929 should be installed (info: https://docs.microsoft.com/en-us/security-updates/securityadvisories/2015/3033929 )
Function CheckIsWin7DriverInstalled

	; check is it Windows7
	${WinVerGetMajor} $0
	${WinVerGetMinor} $1
	StrCmp '$0.$1' '6.1' label_win7
	Goto end

	label_win7:	
		; check is driver works fine
		nsExec::ExecToStack '"$INSTDIR\OpenVPN\$BitDir\tap\${DEVCON_BASENAME}" status ${PRODUCT_TAP_WIN_COMPONENT_ID}'
		Pop $0 ; Return
		Pop $1 ; Output
		${If} $0 != '0'
			; command execution failed - do nothing
			Goto end
		${Else}		
			; In case of driver installation problem, 'devcon.exe' returns error. 
			; 	e.g.: 'The device has the following problem: 52'
			${StrContains} $0 "problem" $1
			StrCmp $0 "" end ; do nothing if driver has no problems			
		${EndIf}	
		
		; check service pack version
		${WinVerGetServicePackLevel} $0
		StrCmp $0 '0' win7_SP1_required
		Goto checkRequiredWinUpdate
		
		win7_SP1_required:
			; inform user that Windows7 SP1 required
			MessageBox MB_ICONINFORMATION|MB_OK  "Windows 7 Service Pack 1 is not installed on your PC.$\nPlease, install ServicePack1.$\n$\nProbably, you would need to reinstall the application then.\
				$\n$\nhttps://www.microsoft.com/en-us/download/details.aspx?id=5842" IDOK true ;IDCANCEL next
				true:
					ExecShell "" "iexplore.exe" "https://www.microsoft.com/en-us/download/details.aspx?id=5842"
					;nsExec::ExecToStack 'cmd /Q /C start /Q https://www.microsoft.com/en-us/download/details.aspx?id=5842'
			;	next:
			;Quit
			Goto end
			
		checkRequiredWinUpdate:
			; check is KB3033929 security update installed (if not - notify to user)
			nsExec::ExecToStack 'cmd /Q /C "%SYSTEMROOT%\System32\wbem\wmic.exe qfe get hotfixid"'
			Pop $0 ; Return
			Pop $1 ; Output

			${If} $0 != '0'
				; command execution failed - do nothing
				Goto end
			${Else}
				${StrContains} $0 "KB3033929" $1
				StrCmp $0 "" notfound
					; security update is installed
					Goto end
				notfound:
					; security update not installed				
					${If} ${RunningX64}
						MessageBox MB_ICONINFORMATION|MB_OK  "Security Update for Windows 7 for x64-based Systems (KB3033929) is not installed on your PC.\
							$\nPlease, install Security Update(KB3033929). \
							$\n$\nhttps://www.microsoft.com/en-us/download/details.aspx?id=46148" IDOK yes_x64 ;IDCANCEL quit
							yes_x64:
								ExecShell "" "iexplore.exe" "https://www.microsoft.com/en-us/download/details.aspx?id=46148"
								;nsExec::ExecToStack 'cmd start /Q https://www.microsoft.com/en-us/download/details.aspx?id=46148'
					${Else}
						MessageBox MB_ICONINFORMATION|MB_OK  "Security Update for Windows 7 (KB3033929) is not installed on your PC.\
							$\nPlease, install Security Update(KB3033929). \
							$\n$\nhttps://www.microsoft.com/en-in/download/details.aspx?id=46078" IDOK yes_x32 ;IDCANCEL quit
							yes_x32:
								ExecShell "" "iexplore.exe" "https://www.microsoft.com/en-in/download/details.aspx?id=46078"
								;nsExec::ExecToStack 'cmd start /Q https://www.microsoft.com/en-in/download/details.aspx?id=46078'							
					${EndIf}
				;quit:
				;Quit
				Goto end				
			${EndIf}		   
	end:
FunctionEnd