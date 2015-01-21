mh_src=/home/per202/src/github_jm/metaheuristics/
MSB=xbuild

# Section on NuGet.config for nuget update (NOT YET USED - CAME ACCROSS ISSUE WITH NUGET 2.8)
# To limit machine specific issues, we will require an explicit nuget config file.
# Usually you will have a config file %AppData%\NuGet\NuGet.config.
# A gotcha is that even if you have configured your package feed from visual studio, you may need to also add a key to the activePackageSource
#  <activePackageSource>
#    <add key="per202 nuget tests" value="\\path\to\work\per202\nuget" />
#    <add key="nuget.org" value="https://www.nuget.org/api/v2/" />
#  </activePackageSource>
set nuget_conf_file=%AppData%\NuGet\NuGet.config
# You can also adapt from the sample NuGet.config.sample in the same directory as this file
# set nuget_conf_file=%~d0%~p0\NuGet.config
# if not exist %nuget_exe% goto Nuget_config_not_found

# The target where we will put the resulting nuget packages.
repo_dir=/home/per202/nuget/

# The xcopy options for the nuget packages (and some other build outputs)
# COPYOPTIONS=/Y /R /D

# ================== code compilation settings
# if not "$BuildConfiguration"=="Release" if not "$BuildConfiguration"=="Debug" BuildConfiguration=Release
BuildConfiguration=Release

# Setting the variable named 'Platform' seems to interfere with the nuget pack command, so 
# we deliberately a variable BuildPlatform for use with MSBuild.exe
BuildPlatform="Any CPU"
# Mode=Rebuild
Mode=Build

# ================== Start build process ========================
# EVERYTHING else below this line should use paths relative to the lines above, or environment variables

# build_options="/t:$Mode /p:Configuration=$BuildConfiguration /p:Platform=\"$BuildPlatform\""
build_options="/t:$Mode /p:Configuration=$BuildConfiguration"
common_ng_pack_options="-Verbosity normal -Properties Configuration=$BuildConfiguration"

# CSIRO.Metaheuristics
# Check that you have bumped the version number up.

SLN=$rdotnet_dir/RDotNet.Release.sln

SLN=$mh_src/Metaheuristics.sln
pkg_dir=$mh_src/packages

cd $pkg_dir/
# %nuget_exe% restore %SLN%

$MSB $SLN $build_options

if exist $mh_src/build\packages\*.nupkg del $mh_src/build\packages\*.nupkg
call $mh_src/build\cpnuspec.cmd
xcopy $mh_src/build\packages\*.nupkg %repo_dir% %COPYOPTIONS%

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
