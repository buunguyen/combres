echo off
set MSBUILD="%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set MSTEST="C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe"

rem ************** Build ************** 
set /P BUILD=Do you want to build now [y/n]? 
if "%BUILD%"=="y" goto BUILD
goto END_BUILD

:BUILD
echo *** Building...
%MSBUILD% /t:Rebuild /p:Configuration=Release "..\Combres.sln"
if errorlevel 1 goto BUILD_FAIL
:END_BUILD

rem ************** NuGet ************** 
set /P NUGET=Do you want to publish to NuGet now [y/n]? 
if /i "%NUGET%"=="y" goto NUGET
goto END

:NUGET
NOTEPAD combres\Combres.nuspec
echo *** Creating Combres NuGet package
xcopy /y ..\Combres\bin\Release\Combres.dll combres\lib\net40\
xcopy /y ..\Combres\bin\Release\Combres.xml combres\lib\net40\
xcopy /y "..\Samples\Sample Data Files\combres_minimal.xml" combres\content\App_Data\
xcopy /y "..\Samples\Sample Data Files\combres_full_with_annotation.xml" combres\content\App_Data\
nuget pack combres\Combres.nuspec
if errorlevel 1 goto PACK_FAIL

NOTEPAD combres.log4net\Combres.Log4Net.nuspec
echo *** Creating Combres.Log4Net NuGet package
xcopy /y ..\Combres.Log4Net\bin\Release\Combres.Log4Net.dll combres.log4net\lib\net40\
xcopy /y ..\Combres.Log4Net\bin\Release\Combres.Log4Net.xml combres.log4net\lib\net40\
nuget pack combres.log4net\Combres.Log4Net.nuspec
if errorlevel 1 goto PACK_FAIL

NOTEPAD combres.mvc\Combres.Mvc.nuspec
echo *** Creating Combres.Mvc NuGet package
xcopy /y ..\Combres.Mvc\bin\Release\Combres.Mvc.dll combres.mvc\lib\net40\
xcopy /y ..\Combres.Mvc\bin\Release\Combres.Mvc.xml combres.mvc\lib\net40\
nuget pack combres.mvc\Combres.Mvc.nuspec
if errorlevel 1 goto PACK_FAIL

:VERSION
set /P VERSION=Enter version: 
if /i "%VERSION%"=="" goto VERSION
set PACKAGE=Combres.%VERSION%.nupkg
echo *** Publishing Combres package...
nuget push %PACKAGE%

set PACKAGE=Combres.Log4Net.%VERSION%.nupkg
echo *** Publishing Combres.Log4Net package...
nuget push %PACKAGE%

set PACKAGE=Combres.Mvc.%VERSION%.nupkg
echo *** Publishing Combres.Mvc package...
nuget push %PACKAGE%
goto END

:BUILD_FAIL
echo *** BUILD FAILED ***
goto END

:PACK_FAIL
echo *** PACKING FAILED ***
goto END

:END