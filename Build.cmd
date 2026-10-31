@echo off
setlocal
pushd "%~dp0"
set "DJI_CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%DJI_CSC%" goto missing
if not exist dist mkdir dist
if not exist ".build\reports" mkdir ".build\reports"
set "DJI_OUT=dist\DJIMicKey.exe"
if not "%~1"=="" set "DJI_OUT=%~1"
for %%D in ("%DJI_OUT%") do if not exist "%%~dpD" mkdir "%%~dpD"
where gcc >nul 2>nul
if errorlevel 1 (
    echo MinGW-w64 gcc is required to build the Shell hook DLL.
    goto failed
)
for %%D in ("%DJI_OUT%") do set "DJI_DLL_DIR=%%~dpD"
if not exist ".build\native" mkdir ".build\native"
gcc -shared -Os -s -static-libgcc -Wall -Wextra -Werror -o ".build\native\DJIMicKey.Shell.dll" "src\ShellVolumeHook.c" -luser32
if errorlevel 1 goto failed
copy /y ".build\native\DJIMicKey.Shell.dll" "%DJI_DLL_DIR%DJIMicKey.Shell.dll" >nul
if errorlevel 1 goto failed
"%DJI_CSC%" /nologo /target:winexe /platform:x64 /optimize+ /debug- /codepage:65001 /utf8output /out:"%DJI_OUT%" /win32manifest:"src\app.manifest" /win32icon:"src\app.ico" /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll "src\Core.cs" "src\App.cs" "src\InputDiagnostics.cs" "src\PowerWatch.cs" "src\RawInputMapping.cs" "src\ShellVolumeGuard.cs" "tests\SelfTests.cs" "src\AssemblyInfo.cs"
if errorlevel 1 goto failed
"%DJI_OUT%" --self-test "%CD%\.build\reports\self-test.txt"
if errorlevel 1 goto failed
echo Build and all checks passed. Output: %DJI_OUT%
popd
exit /b 0
:missing
echo .NET Framework 4.8 compiler was not found on this 64-bit Windows system.
popd
exit /b 1
:failed
echo Build failed. Close DJI Mic Key before rebuilding and read the errors above.
popd
exit /b 1
