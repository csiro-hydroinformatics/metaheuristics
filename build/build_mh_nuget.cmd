f:

set MSB=%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
if not exist %MSB% goto MSBuild_not_found

:: ======= NuGet settings
:: Get the nuget tools from nuget.org. There is also one coming with the NuGet plug-on from Visual Studio.
set nuget_exe=f:\bin\NuGet.exe
if not exist %nuget_exe% goto Nuget_not_found

:: Section on NuGet.config for nuget update (NOT YET USED - CAME ACCROSS ISSUE WITH NUGET 2.8)
:: To limit machine specific issues, we will require an explicit nuget config file.
:: Usually you will have a config file %AppData%\NuGet\NuGet.config.
:: A gotcha is that even if you have configured your package feed from visual studio, you may need to also add a key to the activePackageSource
::  <activePackageSource>
::    <add key="per202 nuget tests" value="\\path\to\work\per202\nuget" />
::    <add key="nuget.org" value="https://www.nuget.org/api/v2/" />
::  </activePackageSource>
set nuget_conf_file=%AppData%\NuGet\NuGet.config
:: You can also adapt from the sample NuGet.config.sample in the same directory as this file
:: set nuget_conf_file=%~d0%~p0\NuGet.config
:: if not exist %nuget_exe% goto Nuget_config_not_found

:: ======= 
:: The command to use to delete whole directories, so that we force the update of packages over the build process
:: the DOS rmdir does not work on wildcard. At least, have not found a way to make it work. 
:: set rm_cmd=rmdir /S /Q
:: Instead, using for the time being the 'rm' from MinGW that comes with the RTools 
:: toolchain, used to build R and R packages on Windows. Any MinGW setup should to.
set rm_cmd=rm -rf

:: The target where we will put the resulting nuget packages.
set repo_dir=%~d0%~p0.\output\
if not exist %repo_dir% mkdir %repo_dir%

:: ================== Location of the source code ========================
set mh_dir=%~d0%~p0..\
:: The xcopy options for the nuget packages (and some other build outputs)
set COPYOPTIONS=/Y /R /D

:: ================== code compilation settings
if not "%BuildConfiguration%"=="Release" if not "%BuildConfiguration%"=="Debug" set BuildConfiguration=Release

:: Setting the variable named 'Platform' seems to interfere with the nuget pack command, so 
:: we deliberately set a variable BuildPlatform for use with MSBuild.exe
set BuildPlatform="Any CPU"
set Mode=Rebuild
:: set Mode=Build

:: ================== Start build process ========================
:: EVERYTHING else below this line should use paths relative to the lines above, or environment variables

set build_options=/t:%Mode% /p:Configuration=%BuildConfiguration% /p:Platform=%BuildPlatform%
set common_ng_pack_options=-Verbosity normal -Properties Configuration=%BuildConfiguration%

:: CSIRO.Metaheuristics
:: Check that you have bumped the version number up.
set SLN=%mh_dir%Metaheuristics.sln
set pkg_dir=%mh_dir%packages

cd %pkg_dir%
:: %nuget_exe% restore %SLN%

%MSB% %SLN% %build_options%

if exist %mh_dir%build\packages\*.nupkg del %mh_dir%build\packages\*.nupkg
call %mh_dir%build\cpnuspec.cmd
xcopy %mh_dir%build\packages\*.nupkg %repo_dir% %COPYOPTIONS%

goto completed

:MSBuild_not_found
echo "ERROR: MSBuild.exe not found at the location given"
exit 1

:Nuget_not_found
echo "ERROR: NuGet.exe not found at the location given"
exit 1

:Nuget_config_not_found
echo "ERROR: NuGet.config not found at the location given"
exit 1

:completed
echo "INFO: Batch build completed with no known error"
