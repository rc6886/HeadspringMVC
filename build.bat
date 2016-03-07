@echo off

.\src\.nuget\nuget.exe restore "src\HSMVC.sln" -source "https://nuget.org/api/v2/" -RequireConsent -o "src\packages"

powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%~dp0\src\packages\psake.4.5.0\tools\psake.ps1' %* -parameters @{"packageVersion"="'%2'"}; if ($psake.build_success -eq $false) { write-host "Build Failed!" -fore RED; exit 1 } else { exit 0 }" <nul