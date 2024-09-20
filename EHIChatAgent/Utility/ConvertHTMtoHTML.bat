@echo off
set "folder=%1"
if "%folder%"=="" (
    echo Please provide a folder path as an argument.
    exit /b 1
)
if not exist "%folder%" (
    echo Folder does not exist.
    exit /b 1
)
cd /d "%folder%"
for %%f in (*.htm) do (
    ren "%%f" "%%~nf.html"
)
echo All .htm files have been renamed to .html
