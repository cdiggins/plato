@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0export-types-context.ps1" %*
exit /b %ERRORLEVEL%
