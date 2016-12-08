@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

set version=
if not "%BuildCounter%" == "" (
   set version=--version-suffix ci-%BuildCounter%
)

REM (optional) build.bat is in the root of our repo, cd to the correct folder where sources/projects are
REM cd Hangfire.Private.Redis.StackExchange

call %nuget% install Redis-64 -OutputDirectory packages
REM packages\Redis-64\tools\redis-server.exe --service-install
REM packages\Redis-64\tools\redis-server.exe --service-start

REM Restore
call dotnet restore 
if not "%errorlevel%"=="0" goto failure

REM Build
REM - Option 1: Run dotnet build for every source folder in the project
REM   e.g. call dotnet build <path> --configuration %config%
REM - Option 2: Let msbuild handle things and build the solution
"%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" Hangfire.Private.Redis.StackExchange.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
REM call dotnet build --configuration %config%
if not "%errorlevel%"=="0" goto failure

REM Unit tests
REM call dotnet test .\Hangfire.Redis.Tests -f netcoreapp1.0
REM call dotnet test .\Hangfire.Redis.Tests -f net451
REM if not "%errorlevel%"=="0" goto failure

REM Package
mkdir %cd%\Artifacts
call dotnet pack .\Hangfire.Private.Redis.StackExchange --configuration %config% %version% --output Artifacts
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1