@echo off
echo Running xUnit Tests...
cd /d "%~dp0"
dotnet test --verbosity normal
pause
