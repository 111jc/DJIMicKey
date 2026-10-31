@echo off
setlocal
pushd "%~dp0"
set "DJI_VERSION=1.0.2"
set "DJI_STAGE=.build\release-v%DJI_VERSION%-%RANDOM%"
call Build.cmd "%DJI_STAGE%\DJIMicKey.exe"
if errorlevel 1 goto failed
"%WINDIR%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -NonInteractive -Command "$ErrorActionPreference='Stop'; $stage='%DJI_STAGE%'; $files=@('DJIMicKey.exe','DJIMicKey.Shell.dll'); foreach($name in @('WINDOWS.md','WIN11_TROUBLESHOOTING.md','SHELL_VOLUME_FILTER.md','VALIDATION.md')) { Copy-Item -LiteralPath ('docs\'+$name) -Destination (Join-Path $stage $name) -Force; $files+=$name }; Copy-Item -LiteralPath 'LICENSE' -Destination (Join-Path $stage 'LICENSE') -Force; $files+='LICENSE'; Compress-Archive -LiteralPath @($files | ForEach-Object { Join-Path $stage $_ }) -DestinationPath ('dist\DJIMicKey-v%DJI_VERSION%-portable.zip') -Force; try { Copy-Item -LiteralPath (Join-Path $stage 'DJIMicKey.exe') -Destination 'dist\DJIMicKey-v%DJI_VERSION%.exe' -Force; Copy-Item -LiteralPath (Join-Path $stage 'DJIMicKey.Shell.dll') -Destination 'dist\DJIMicKey.Shell.dll' -Force } catch [System.IO.IOException] { Write-Host 'Active executable/DLL retained; use the newly generated ZIP after exiting the app.' }; Set-Content -LiteralPath '.build\latest-release.txt' -Value $stage"
if errorlevel 1 goto failed
echo Portable archive: dist\DJIMicKey-v%DJI_VERSION%-portable.zip
popd
exit /b 0
:failed
echo Packaging failed. Read the errors above; close active versioned EXE/DLL if a destination is locked.
popd
exit /b 1
