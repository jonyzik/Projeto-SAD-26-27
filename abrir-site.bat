@echo off
setlocal

set "SITE=%~dp0website\index.html"

if not exist "%SITE%" (
    echo Nao foi encontrado: "%SITE%"
    pause
    exit /b 1
)

start "" "%SITE%"
