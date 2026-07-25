@echo off
setlocal

where npm.cmd >nul 2>nul
if %errorlevel% equ 0 goto run

set "vswhere=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

if not exist "%vswhere%" (
    echo Node.js was not found on PATH and Visual Studio could not be located.
    exit /b 1
)

for /f "usebackq delims=" %%i in (`"%vswhere%" -all -prerelease -products * -property installationPath`) do (
    if exist "%%i\MSBuild\Microsoft\VisualStudio\NodeJs\npm.cmd" (
        set "visualStudio=%%i"
    )
)

if not defined visualStudio (
    echo Node.js was not found on PATH and Visual Studio could not be located.
    exit /b 1
)

set "nodeTools=%visualStudio%\MSBuild\Microsoft\VisualStudio\NodeJs"
set "PATH=%nodeTools%;%PATH%"

:run
if not exist "%~dp0..\node_modules\esbuild" (
    call npm.cmd ci

    if errorlevel 1 exit /b %errorlevel%
)

call npm.cmd %*
exit /b %errorlevel%
