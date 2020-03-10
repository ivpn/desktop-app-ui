@echo off

setlocal
set SCRIPTDIR=%~dp0

rem ==================================================
rem DEFINE path to NSIS binary here
SET MAKENSIS="C:\Program Files (x86)\NSIS\makensis.exe"
rem ==================================================
SET INSTALLER_OUT_DIR=%SCRIPTDIR%..\bin
set INSTALLER_TMP_DIR=%INSTALLER_OUT_DIR%\temp
SET FILE_LIST=%SCRIPTDIR%release-files.txt

set APPVER=???
set SERVICE_REPO=???

rem Checking if msbuild available
WHERE msbuild >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
	echo [!] 'msbuild' is not recognized as an internal or external command
	echo [!] Ensure you are running this script from Developer Cammand Prompt for Visual Studio
	goto :error
)

rem Checking if NSIS  available
if not exist %MAKENSIS% (
    echo [!] NSIS binary not found [%MAKENSIS%]
	echo [!] Install NSIS [https://nsis.sourceforge.io/] or\and modify MAKENSIS variable of this script
	goto :error
)

call :read_app_version 			|| goto :error
call :read_service_repo_path 	|| goto :error
call :build_service				|| goto :error
call :build_ui					|| goto :error
call :copy_files 				|| goto :error
call :build_installer			|| goto :error

goto :success


:read_app_version
	echo [*] Reading App version ...

	set VERSTR=???
	set AssemblyInfFile=%SCRIPTDIR%..\..\IVPN Application\Properties\AssemblyInfo.cs
	set AssemblyVerRegExp=^\[assembly: AssemblyVersion\(\".*\"\)\]
	set cmd=findstr /R /C:"%AssemblyVerRegExp%" "%AssemblyInfFile%"

	rem Find string in file
	FOR /F "tokens=* USEBACKQ" %%F IN (`%cmd%`) DO SET VERSTR=%%F
	if	"%VERSTR%" == "???" (
		echo "[!] ERROR: The file '%AssemblyInfFile%' shall contain '[assembly: AssemblyVersion("X.X.X")]' string"
		exit /b 1
 	)
	rem Get substring in quotes
	for /f tokens^=2^ delims^=^" %%a in ("%VERSTR%") do (
			set APPVER=%%a
	)

	echo     APPVER: %APPVER%
	goto :eof

:copy_files
	echo [*] Copying files...
	cd %SCRIPTDIR%..\..\IVPN Application\bin\Release || exit /b 1

	set BIN_FOLDER_APP=%cd%\
	set BIN_FOLDER_SERVICE=%SERVICE_REPO%\bin\x86\
	set BIN_FOLDER_SERVICE_REFS=%SERVICE_REPO%\References\Windows\

	cd %SCRIPTDIR%

	IF exist "%INSTALLER_TMP_DIR%" (
		rmdir /s /q "%INSTALLER_TMP_DIR%"
	)
	mkdir "%INSTALLER_TMP_DIR%"

	setlocal EnableDelayedExpansion
	for /f "tokens=*" %%i in (%FILE_LIST%) DO (
		set SRCPATH=???
		if exist "%BIN_FOLDER_SERVICE%%%i" set SRCPATH=%BIN_FOLDER_SERVICE%%%i
		if exist "%BIN_FOLDER_SERVICE_REFS%%%i" set SRCPATH=%BIN_FOLDER_SERVICE_REFS%%%i
		if exist "%BIN_FOLDER_APP%%%i"  set SRCPATH=%BIN_FOLDER_APP%%%i
		if exist "%SCRIPTDIR%%%i" set SRCPATH=%SCRIPTDIR%%%i
		if !SRCPATH! == ??? (
			echo FILE '%%i' NOT FOUND!
			exit /b 1
		)
		echo     !SRCPATH!

		IF NOT EXIST "%INSTALLER_TMP_DIR%\%%i\.." (
			MKDIR "%INSTALLER_TMP_DIR%\%%i\.."
		)

		copy /y "!SRCPATH!" "%INSTALLER_TMP_DIR%\%%i" > NUL
		IF !errorlevel! NEQ 0 (
			ECHO     Error: failed to copy to "%INSTALLER_TMP_DIR%"
			EXIT /B 0
		)
	)
	goto :eof

:build_service
	echo [*] Building IVPN service and dependencies...
	call %SERVICE_REPO%\References\Windows\scripts\build-all.bat %APPVER% || exit /b 1	
	goto :eof

:build_ui
	echo [*] Building UI...
	call %SCRIPTDIR%\..\..\..\build_ui_Windows.bat || exit /b 1
	goto :eof

:build_installer
	echo [*] Building installer...
	cd %SCRIPTDIR%

	SET OUT_FILE="%INSTALLER_OUT_DIR%\IVPN-Client-v%APPVER%.exe"
	%MAKENSIS% /DPRODUCT_VERSION=%APPVER% /DOUT_FILE=%OUT_FILE% /DSOURCE_DIR=%INSTALLER_TMP_DIR% "IVPN Client.nsi"
	IF not ERRORLEVEL 0 (
		ECHO [!] Error: failed to create installer
		EXIT /B 1
	)
	goto :eof

:read_service_repo_path
	echo [*] Checking location of IVPN service local repository...

	cd %SCRIPTDIR%..\config
	set /p repo_path=<service_repo_local_path.txt || goto :read_service_repo_path_error
	cd %repo_path% || goto :read_service_repo_path_error
	set SERVICE_REPO=%cd%

	echo     Service sources: %SERVICE_REPO%
	goto :eof
:read_service_repo_path_error
	echo [!] Failed to get info about location of IVPN service local repository
	echo [!] Please, clone IVPN service repository and modify file '%SCRIPTDIR%..\config\service_repo_local_path.txt'
	goto :eof

:success
	echo [*] SUCCESS
	exit /b 0

:error
	echo [!] IVPN Client installer build FAILED with error #%errorlevel%.
	exit /b %errorlevel%
