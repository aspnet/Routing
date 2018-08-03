@echo off
setlocal

rem Redirect away from %USER%\.nuget\packages to avoid picking up a stale package.
set NUGET_PACKAGES=%~dp0\..\..\packages-cache

dotnet publish -c Release -r win-x64 --framework netcoreapp3.0 || exit /b 1

%~dp0\bin\Release\netcoreapp3.0\win-x64\publish\Microsoft.AspNetCore.Routing.Performance.exe --config profile JumpTableMultipleEntryBenchmark