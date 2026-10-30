@echo off
setlocal
pushd "%~dp0"
call Build.cmd ".build\tests\DJIMicKey.exe"
if errorlevel 1 goto failed
set "DJI_TEST=%CD%\.build\tests\DJIMicKey.exe"
"%DJI_TEST%" --layout-smoke "%CD%\.build\reports\layout"
if errorlevel 1 goto failed
"%DJI_TEST%" --lifecycle-smoke "%CD%\.build\reports\lifecycle.txt"
if errorlevel 1 goto failed
"%DJI_TEST%" --record-smoke "%CD%\.build\reports\recording.txt"
if errorlevel 1 goto failed
"%DJI_TEST%" --diagnostic-ui-smoke "%CD%\.build\reports\diagnostic-screen.txt"
if errorlevel 1 goto failed
"%DJI_TEST%" --feedback-smoke "%CD%\.build\reports\feedback"
if errorlevel 1 goto failed
if /i "%~1"=="--device" (
    "%DJI_TEST%" --diagnose "%CD%\.build\reports\device.txt"
    if errorlevel 1 goto failed
)
echo All checks passed. Reports: .build\reports
popd
exit /b 0
:failed
echo Validation failed. Read the report in .build\reports.
popd
exit /b 1
