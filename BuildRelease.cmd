@echo off
setlocal enabledelayedexpansion

set BatchFile=%0
set Root=%~dp0

dotnet build -c Release --no-incremental
dotnet test
dotnet pack -c Release -o _Dist

exit /b %ERRORLEVEL%
