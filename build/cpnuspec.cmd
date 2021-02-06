@echo off

echo. 
echo Copying NuGet package spec from CSIRO.Metaheuristics to other assemblies.
echo.

set THIS_DIR=%~d0%~p0
set src_nuspec=%THIS_DIR%..\CSIRO.Metaheuristics\CSIRO.Metaheuristics.nuspec
if not exist "%src_nuspec%" goto noSrcNuspec
if not defined BuildConfiguration set BuildConfiguration=Debug

@REM BuildPlatform may be "Any CPU" from prior compilations, but this messes up, of course, the pack options. 
@REM Just use the damn string without this bothersome space. As of Jan 2021 we now do need to specify platforms
@REM to nuget pack (was not an issue a couple years ago)
if not defined BuildPlatformNupkg set BuildPlatformNupkg=AnyCPU
set COPYOPTIONS=/Y /R /D

xcopy %src_nuspec% %THIS_DIR%..\CSIRO.Utilities\CSIRO.Utilities.nuspec %COPYOPTIONS%
xcopy %src_nuspec% %THIS_DIR%..\CSIRO.Sys\CSIRO.Sys.nuspec %COPYOPTIONS%
:: xcopy %src_nuspec% %THIS_DIR%..\CSIRO.Modelling.Core\CSIRO.Modelling.Core.nuspec %COPYOPTIONS%
xcopy %src_nuspec% %THIS_DIR%..\CSIRO.Metaheuristics.DataModel\CSIRO.Metaheuristics.DataModel.nuspec %COPYOPTIONS%
xcopy %src_nuspec% %THIS_DIR%..\CSIRO.Metaheuristics.Logging\CSIRO.Metaheuristics.Logging.nuspec %COPYOPTIONS%
xcopy %src_nuspec% %THIS_DIR%..\CSIRO.Metaheuristics.Parallel\CSIRO.Metaheuristics.Parallel.nuspec %COPYOPTIONS%
xcopy %src_nuspec% %THIS_DIR%..\CSIRO.Metaheuristics.R\CSIRO.Metaheuristics.R.nuspec %COPYOPTIONS%
xcopy %src_nuspec% %THIS_DIR%..\CSIRO.Metaheuristics.Tests\CSIRO.Metaheuristics.Tests.nuspec %COPYOPTIONS%

set pkg_dir=%THIS_DIR%packages
if not exist "%pkg_dir%" mkdir %pkg_dir%
set pack_options=-IncludeReferencedProjects -Verbosity normal -Properties "Configuration=%BuildConfiguration%;Platform=%BuildPlatformNupkg%" -OutputDirectory %pkg_dir%

nuget pack %THIS_DIR%..\CSIRO.Utilities\CSIRO.Utilities.csproj %pack_options%
nuget pack %THIS_DIR%..\CSIRO.Sys\CSIRO.Sys.csproj %pack_options%
nuget pack %THIS_DIR%..\CSIRO.Modelling.Core\CSIRO.Modelling.Core.csproj %pack_options%
nuget pack %THIS_DIR%..\CSIRO.Metaheuristics\CSIRO.Metaheuristics.csproj %pack_options%
nuget pack %THIS_DIR%..\CSIRO.Metaheuristics.DataModel\CSIRO.Metaheuristics.DataModel.csproj %pack_options%
nuget pack %THIS_DIR%..\CSIRO.Metaheuristics.Logging\CSIRO.Metaheuristics.Logging.csproj %pack_options%
nuget pack %THIS_DIR%..\CSIRO.Metaheuristics.Parallel\CSIRO.Metaheuristics.Parallel.csproj %pack_options%
nuget pack %THIS_DIR%..\CSIRO.Metaheuristics.R\CSIRO.Metaheuristics.R.csproj %pack_options%
nuget pack %THIS_DIR%..\CSIRO.Metaheuristics.Tests\CSIRO.Metaheuristics.Tests.csproj %pack_options%

if errorlevel 1 goto bailOut

goto success 

:noSrcNuspec
echo not found: source nuspec not found
exit /B 1

:bailOut
echo ERROR
exit /B 1

:success
echo Done!
