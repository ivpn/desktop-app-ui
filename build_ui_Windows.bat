@ECHO OFF
setlocal
set SCRIPTDIR=%~dp0

cd %SCRIPTDIR%
if not exist ".deps\nuget.exe" (
  if not exist ".deps" (
    mkdir ".deps" || goto :error
  )
  echo [*] Downloading nuget.exe ...
  curl -#fLo .deps\nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe || goto :error
)

echo [*] Restoring nuget packages ...
.deps\nuget restore "IVPN App Windows.sln"

cd %SCRIPTDIR%
echo [*] Building "IVPN App Windows.sln"
msbuild "IVPN App Windows.sln" /verbosity:minimal /t:Clean,Build -p:Configuration=Release -t:restore
goto :success

:success
	echo [*] Build success
	exit /b 0

:error
	echo [!] Build FAILED with error #%errorlevel%.
	exit /b %errorlevel%
