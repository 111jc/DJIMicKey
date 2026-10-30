@echo off
setlocal
pushd "%~dp0"
call Build.cmd
if errorlevel 1 goto failed
"%WINDIR%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -NonInteractive -Command "$ErrorActionPreference='Stop'; Copy-Item -LiteralPath 'docs\WINDOWS.md' -Destination 'dist\README.txt' -Force; Copy-Item -LiteralPath 'LICENSE' -Destination 'dist\LICENSE' -Force; Copy-Item -LiteralPath 'docs\WIN11_TROUBLESHOOTING.md' -Destination 'dist\WIN11-TROUBLESHOOTING.txt' -Force; Copy-Item -LiteralPath 'docs\POWER_BUTTON_FINDINGS.md' -Destination 'dist\POWER-BUTTON-FINDINGS.txt' -Force; Compress-Archive -LiteralPath 'dist\DJIMicKey.exe','dist\README.txt','dist\LICENSE','dist\WIN11-TROUBLESHOOTING.txt','dist\POWER-BUTTON-FINDINGS.txt' -DestinationPath 'dist\DJIMicKey-portable.zip' -Force"
if errorlevel 1 goto failed
echo Portable archive: dist\DJIMicKey-portable.zip
popd
exit /b 0
:failed
echo Packaging failed. Close DJI Mic Key before rebuilding.
popd
exit /b 1
